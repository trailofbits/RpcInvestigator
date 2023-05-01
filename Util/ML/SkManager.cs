//
// Copyright (c) 2023-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
// Copyright (c) Microsoft. All rights reserved.
//
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.KernelExtensions;
using System.Diagnostics;
using System.Collections;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json.Linq;

namespace RpcInvestigator.Util.ML
{
    using static TraceLogger;

    public class SkManager : IDisposable
    {
        private readonly MLLogger m_Logger = new MLLogger();
        private LLMBackend m_Backend;
        private PythonEnvironment m_Python;
        private static readonly string s_SkillsPath =
            Path.Combine(Directory.GetCurrentDirectory(), "Skills");
        private IKernel m_Kernel;
        private Action m_ProgressUpdate;
        private Action<string> m_UpdateStatus;
        public static readonly int s_InitializeSteps =
            PythonEnvironment.s_Steps;
        public static readonly int s_RunSkillSteps = LLMBackend.s_Steps;
        private readonly Settings m_Settings;

        //
        // Whenever a new semantic skill or function is added, this mapping
        // must be updated. It is used to retrieve default input and other
        // properties of the semantic function. Not all entries in this mapping
        // will be instantiating at runtime, given what the user selects.
        //
        private static readonly Dictionary<string, Type> s_SemanticFunctionHelperMapping =
            new Dictionary<string, Type>{
                { "Client/CodeGenerator", typeof(CodeGeneratorSemanticFunction) }
            };

        //
        // This dictionary keeps a mapping of skill monikers
        // (e.g., <skill>/<semantic_function>) to instances of the class that can tell
        // us default input and other properties.
        //
        private Dictionary<string, SemanticFunctionHelper> m_SemanticFunctionHelpers;

        //
        // This variable contains input data to be used as arguments to skill instance
        // classes for things like determining default input. An example is the RpcClient
        // skill needs an RpcServer object to craft prompts.
        //
        private ArrayList m_SkillArguments;

        public SkManager (
            Action ProgressUpdate,
            Action<string> UpdateStatus,
            ArrayList SkillArguments,
            Settings Settings
            )
        {
            m_SemanticFunctionHelpers = new Dictionary<string, SemanticFunctionHelper>();
            m_SkillArguments = SkillArguments;
            m_ProgressUpdate = ProgressUpdate;
            m_UpdateStatus = UpdateStatus;
            m_Backend = new LLMBackend(null, null, UpdateStatus, ProgressUpdate, Settings);
            m_Settings = Settings;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_Python != null)
                {
                    m_Python.Dispose();
                }
            }
        }

        public void Initialize()
        {
            Trace(TraceLoggerType.ML,
                  TraceEventType.Verbose,
                  "Initializing Semantic Kernel");
            m_Kernel = Kernel.Builder.WithLogger(m_Logger).Build();
            var factory = new Func<IKernel, ITextCompletion>(GetBackend);
            m_Kernel.Config.AddTextCompletionService("tmp", factory);
            //
            // Load native skills manually. Currently there are none.
            //
            // var someNativeSkill = new SomeNativeSkill();
            // m_Kernel.ImportSkill(someNativeSkill, "SomeNativeSkill");
            // m_SkillInstances.Add("NativeSkill/NativeFunction", someNativeSkill);
            //

            Trace(TraceLoggerType.ML,
                  TraceEventType.Verbose,
                  "Imported native skill RpcClientSkill");
            //
            // Load semantic skills from folder.
            //
            foreach (var skillDirectory in Directory.GetDirectories(s_SkillsPath))
            {
                var skillName = Path.GetFileName(skillDirectory);
                foreach (var functionDirectory in Directory.GetDirectories(skillDirectory))
                {
                    if (!File.Exists(Path.Combine(functionDirectory, "config.json")) ||
                        !File.Exists(Path.Combine(functionDirectory, "skprompt.txt")))
                    {
                        continue;
                    }

                    //
                    // We use an instance of a semantic function helper class to be able
                    // to retrieve default input and other properties of this semantic
                    // function. Register that association now.
                    //
                    var funcs = m_Kernel.ImportSemanticSkillFromDirectory(s_SkillsPath, skillName);
                    foreach (var kvp in funcs)
                    {
                        var moniker = skillName + "/" + kvp.Key;
                        if (!s_SemanticFunctionHelperMapping.ContainsKey(moniker))
                        {
                            throw new Exception("Found unknown semantic skill in skills " +
                                "folder: " + moniker);
                        }
                        var helperType = s_SemanticFunctionHelperMapping[moniker];
                        m_SemanticFunctionHelpers.Add(
                            moniker,
                            (SemanticFunctionHelper)Activator.CreateInstance(helperType));
                    }

                    Trace(TraceLoggerType.ML,
                          TraceEventType.Verbose,
                          "Imported semantic skill "+skillName);
                }
            }
            m_Kernel.Config.RemoveTextCompletionService("tmp");
        }

        public bool IsPythonInitialized()
        {
            return m_Python != null;
        }

        public async Task InitializePython(Settings Settings)
        {
            m_Python = new PythonEnvironment(
                    Settings,
                    m_UpdateStatus,
                    m_ProgressUpdate);
            try
            {
                await m_Python.Setup();
            }
            catch (Exception ex)
            {
                m_Python = null;
                throw new Exception(ex.Message);
            }
        }

        public List<string> GetAvailableSkills()
        {
            if (m_Kernel == null)
            {
                throw new Exception("SkManager has not been initialized.");
            }

            var skills = new List<string>();
            var functions = m_Kernel.Skills.GetFunctionsView();

            //
            // Note: native skills are not exposed to the user.
            //

            foreach (var kvp in functions.SemanticFunctions)
            {
                foreach (var func in kvp.Value)
                {
                    skills.Add(kvp.Key + "/" + func.Name);
                }
            }
            skills.Add("Custom");
            return skills;
        }

        public string GetPromptTemplate(string SkillMoniker)
        {
            (var skillName, var functionName) = ParseSkillMoniker(SkillMoniker);
            var path = Path.Combine(s_SkillsPath, skillName, functionName, "skprompt.txt");
            if (!File.Exists(path))
            {
                throw new Exception("skprompt.txt not found");
            }
            return File.ReadAllText(path);
        }

        public string GetDefaultInput(string SkillMoniker)
        {
            if (!s_SemanticFunctionHelperMapping.ContainsKey(SkillMoniker))
            {
                throw new Exception("GetDefaultInput: Skill moniker " + SkillMoniker +
                    " is not valid.");
            }
            return m_SemanticFunctionHelpers[SkillMoniker].GetDefaultInput(m_SkillArguments);
        }

        private (string, string) ParseSkillMoniker(string Moniker)
        {
            var parts = Moniker.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new Exception("Bad skill moniker " + Moniker);
            }
            var skillName = parts[0];
            var functionName = parts[1];
            return (skillName, functionName);
        }

        public async Task<string> RunSkill (
            string SkillMoniker,
            string ModelLocation,
            string GeneratorName,
            string Prompt, // only used for Custom skills
            string Input
            )
        {
            if (m_Kernel == null || m_Python == null)
            {
                throw new Exception("Kernel and/or python not initialized.");
            }

            //
            // If backend wasn't fully initialized, do so now. Also, if a different
            // model was previously loaded, create a new backend for the new one.
            //
            var previousModelLocation = m_Backend.m_ModelLocation;
            if (string.IsNullOrEmpty(previousModelLocation) ||
                previousModelLocation != ModelLocation)
            {
                m_Kernel.Config.RemoveTextCompletionService("LLMBackend");
                m_Backend.m_ModelLocation = ModelLocation;
                m_Backend.m_Generator = GeneratorName;
                var factory = new Func<IKernel, ITextCompletion>(GetBackend);
                m_Kernel.Config.AddTextCompletionService("LLMBackend", factory);
            }

            //
            // Lookup the pre-registered semantic function that was requested.
            //
            (var skillName, var functionName) = ParseSkillMoniker(SkillMoniker);
            var function = m_Kernel.Skills.GetSemanticFunction(skillName, functionName);
            if (function == null)
            {
                throw new Exception("Skill moniker " + SkillMoniker + " is not registered.");
            }

            if (!s_SemanticFunctionHelperMapping.ContainsKey(SkillMoniker))
            {
                throw new Exception("RunSkill: Skill moniker " + SkillMoniker +
                    " is not valid.");
            }

            //
            // Create SK context variables from the input defined by the user. SK will
            // replace the variables in the prompt string with the supplied input.
            //
            var context = m_SemanticFunctionHelpers[SkillMoniker].GetContext(Input);

            //
            // Perform inference using LLM backend.
            //
            var result = await m_Kernel.RunAsync(context, function);
            if (result.LastException != null)
            {
                throw new Exception(result.LastException.Message);
            }
            Trace(TraceLoggerType.ML,
                  TraceEventType.Information,
                  m_Python.GetDebugOutput());
            return result.Result;
        }

        private ITextCompletion GetBackend(IKernel Kernel)
        {
            return m_Backend;
        }
    }

    internal abstract class SemanticFunctionHelper
    {
        abstract public ContextVariables GetContext(string JsonInput);

        abstract public string GetDefaultInput(ArrayList Arguments);
    }

    internal class CodeGeneratorSemanticFunction : SemanticFunctionHelper
    {
        override public ContextVariables GetContext(string JsonInput)
        {
            var context = new ContextVariables();
            try
            {
                var obj = JObject.Parse(JsonInput);
                if (!obj.ContainsKey("function_prototype"))
                {
                    throw new Exception("The key function_prototype is missing");
                }
                var value = (string)obj["function_prototype"];
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception("function_prototype must contain a value");
                }
                context.Set("function_prototype", value);
            }
            catch (Exception ex)
            {
                throw new Exception("Model input is not valid JSON: " +
                    ex.Message);
            }
            return context;
        }

        override public string GetDefaultInput(ArrayList Arguments)
        {
            JObject jsonObject = new JObject
            {
                ["function_prototype"] = "",
            };
            return jsonObject.ToString();
        }
    }

    internal class CustomSemanticFunction : SemanticFunctionHelper
    {
        override public ContextVariables GetContext(string JsonInput)
        {
            //
            // Parse JSON
            //

            var context = new ContextVariables();
            context.Set("function_prototype", JsonInput);
            return context;
        }

        override public string GetDefaultInput(ArrayList Arguments)
        {
            return null;
        }
    }
}
