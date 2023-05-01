//
// Copyright (c) 2023-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Microsoft.SemanticKernel.AI.TextCompletion;
using System.Threading.Tasks;
using System.Threading;
using System;
using Python.Runtime;
using System.Diagnostics;

namespace RpcInvestigator.Util.ML
{
    using static TraceLogger;

    public class LLMBackend : ITextCompletion
    {
        public string m_ModelLocation { get; set; }
        public string m_Generator { get; set; }
        private Action<string> m_UpdateStatusRoutine;
        private Action m_ProgressUpdateRoutine;
        public static readonly int s_Steps = 3;
        private dynamic m_ModelConfig;
        private CompleteRequestSettings m_Request;
        private readonly Settings m_Settings;

        public LLMBackend(
            string ModelLocation,
            string Generator,
            Action<string> UpdateRoutine,
            Action ProgressUpdateRoutine,
            Settings Settings
            )
        {
            m_ModelLocation = ModelLocation;
            m_Generator = Generator;
            m_UpdateStatusRoutine = UpdateRoutine;
            m_ProgressUpdateRoutine = ProgressUpdateRoutine;
            m_Settings = Settings;
        }

        public async Task<string> CompleteAsync(
            string text,
            CompleteRequestSettings requestSettings,
            CancellationToken cancellationToken = default)
        {
            if (!PythonEngine.IsInitialized)
            {
                Debug.Assert(PythonEngine.IsInitialized);
                var error = "Python has not been initialized!";
                Trace(TraceLoggerType.ML,
                      TraceEventType.Error,
                      error);
                throw new Exception(error);
            }

            m_Request = requestSettings;

            m_ProgressUpdateRoutine();

            if (m_ModelConfig == null)
            {
                m_UpdateStatusRoutine("Loading model for inference...");
                await LoadModel();
            }

            m_ProgressUpdateRoutine();
            m_UpdateStatusRoutine("Running inference...");
            var output = await Generate(text);
            m_ProgressUpdateRoutine();
            return output;
        }

        private async Task LoadModel()
        {
            await Task.Run(() =>
            {
                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope())
                    {
                        dynamic generator_config = Py.Import("generator_config");
                        dynamic config;
                        if (m_Generator == "dolly")
                        {
                            //
                            // The base llama model has a 2048 output token limit.
                            //
                            int maxTokens = Math.Min(2048, m_Request.MaxTokens);
                            config = generator_config.dolly_config(
                                m_ModelLocation,
                                m_Request.MaxTokens,
                                m_Request.Temperature,
                                !m_Settings.m_UseGpuForInference, // force cpu
                                m_Request.TopP,
                                m_Request.PresencePenalty,
                                m_Request.FrequencyPenalty
                                );
                        }
                        else if (m_Generator == "gpt4all-alpaca-13b-q4")
                        {
                            //
                            // The base llama model has a 2048 output token limit.
                            //
                            int maxTokens = Math.Min(2048, m_Request.MaxTokens);
                            config = generator_config.generator_base_config(
                                m_ModelLocation,
                                maxTokens,
                                m_Request.Temperature,
                                !m_Settings.m_UseGpuForInference, // force cpu
                                m_Request.TopP,
                                m_Request.PresencePenalty,
                                m_Request.FrequencyPenalty
                            );
                        }
                        else if (m_Generator == "stable-lm-alpaca")
                        {
                            //
                            // The base llama model has a 2048 output token limit.
                            //
                            int maxTokens = Math.Min(2048, m_Request.MaxTokens);
                            config = generator_config.stable_lm_config(
                                m_ModelLocation,
                                maxTokens,
                                m_Request.Temperature,
                                !m_Settings.m_UseGpuForInference, // force cpu
                                m_Request.TopP,
                                m_Request.PresencePenalty,
                                m_Request.FrequencyPenalty
                            );
                        }
                        else
                        {
                            throw new Exception("Unknown generator " + m_Generator);
                        }

                        dynamic generator = Py.Import(m_Generator);
                        dynamic init_func = generator.init_model;
                        generator.init_model(config);
                        m_ModelConfig = config;
                    }
                }
            });
        }

        private async Task<string> Generate(string Prompt)
        {
            if (m_ModelConfig == null)
            {
                throw new Exception("Model has not been loaded.");
            }

            dynamic output = null;

            await Task.Run(() =>
            {
                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope())
                    {
                        dynamic generator = Py.Import(m_Generator);
                        dynamic generate_func = generator.generate;
                        output = generate_func(m_ModelConfig, Prompt);
                    }
                }
            });

            return output;
        }
    }
}