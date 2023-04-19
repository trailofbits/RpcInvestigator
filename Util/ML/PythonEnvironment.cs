//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Python.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace RpcInvestigator.Util.ML
{
    using static TraceLogger;

    public class PythonEnvironment : IDisposable
    {
        private string m_PythonDllLocation;
        private VirtualEnv m_VirtualEnv;
        private Action<string> m_UpdateStatusRoutine;
        private Action m_ProgressUpdateRoutine;
        public static readonly int s_Steps = 2;
        public TextWriter m_Console;
        private nint m_ThreadState;
        private StringWriter m_DebugOutput;
        private bool m_UseGpu;

        public PythonEnvironment(
            Settings Settings,
            Action<string> UpdateRoutine,
            Action ProgressUpdateRoutine
            )
        {
            m_UseGpu = Settings.m_UseGpuForInference;
            m_PythonDllLocation = Settings.m_PythonDllLocation;
            m_VirtualEnv = new VirtualEnv(Settings.m_PythonVenvPath, m_UseGpu);
            m_UpdateStatusRoutine = UpdateRoutine;
            m_ProgressUpdateRoutine = ProgressUpdateRoutine;
            m_DebugOutput = new StringWriter();
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
                if (PythonEngine.IsInitialized)
                {
                    PythonEngine.EndAllowThreads(m_ThreadState);
                    PythonEngine.Shutdown();
                }
            }
        }

        public string GetVirtualEnvPath()
        {
            return m_VirtualEnv.m_Path;
        }

        public string GetDebugOutput()
        {
            return m_DebugOutput.GetStringBuilder().ToString();
        }

        public async Task Setup()
        {
            Trace(TraceLoggerType.PythonWrapper,
                 TraceEventType.Verbose,
                 "PythonWrapper setup");
            m_ProgressUpdateRoutine();
            m_UpdateStatusRoutine("Creating python virtual environment...");
            await Task.Run(m_VirtualEnv.Create);
            m_ProgressUpdateRoutine();
            SetPythonInternals();
            m_UpdateStatusRoutine("Setting up python virtual environment...");
            await Task.Run(m_VirtualEnv.Setup);
            if (m_UseGpu)
            {
                ConfigureGpu();
            }
        }

        private void SetPythonInternals()
        {
            //
            // Set the python DLL
            //
            if (string.IsNullOrEmpty(m_PythonDllLocation) ||
                !File.Exists(m_PythonDllLocation))
            {
                throw new Exception("Python DLL location is not valid.");
            }

            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  $"PythonDLL: {m_PythonDllLocation}");
            Runtime.PythonDLL = m_PythonDllLocation;

            //
            // Set environment variables. Given the hack below, I don't know if these are
            // really necessary, but in the future, they might be if the hack becomes
            // unnecessary.
            //
            var pythonPath = $"{m_VirtualEnv.m_Path};" +
                $"{Path.Combine(m_VirtualEnv.m_Path, "Lib", "site-packages")};" +
                $"{Path.Combine(m_VirtualEnv.m_Path, "Lib")}";
            var path = Environment.GetEnvironmentVariable("PATH").TrimEnd(';');
            path = string.IsNullOrEmpty(path) ? m_VirtualEnv.m_Path :
                $"{m_VirtualEnv.m_Path};" +
                $"{Path.Combine(m_VirtualEnv.m_Path, "Scripts")};" +
                $"{path}";
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", "", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath, EnvironmentVariableTarget.Process);
            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  $"PATH: {Environment.GetEnvironmentVariable("PATH")}");
            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  $"PYTHONHOME: {Environment.GetEnvironmentVariable("PYTHONHOME")}");
            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  $"PYTHONPATH: {Environment.GetEnvironmentVariable("PYTHONPATH")}");

            //
            // Pythonnet hack:  see https://github.com/pythonnet/pythonnet/issues/1478
            //
            _ = PythonEngine.Version;
            PythonEngine.SetNoSiteFlag();
            PythonEngine.Initialize();
            m_ThreadState = PythonEngine.BeginAllowThreads();

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.prefix = m_VirtualEnv.m_Path;
                sys.exec_prefix = m_VirtualEnv.m_Path;
                dynamic site = Py.Import("site");
                site.PREFIXES = new List<PyObject> { sys.prefix, sys.exec_prefix };
                site.main();
            }

            //
            // Prepare a console
            //
            // Because we're running python from a windows GUI app, there is
            // no console for stdin/stderr to point to, so they are NoneType.
            // Because modules will expect these, we'll create one now. See:
            //  https://github.com/pythonnet/pythonnet/issues/370
            //  https://docs.python.org/3/library/sys.html#sys.__stdin
            //
            SetupConsole();

            //
            // Redirect python logger to stdout
            //
            RedirectPyLogger();
        }

        private void SetupConsole()
        {
            //
            // Source:  https://github.com/yagweb/pythonnetLab/blob/master/pynetLab/PySysIO.cs
            //
            var writer = new ConsoleWriter(m_DebugOutput);
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.stdout = sys.stderr = writer;
            }
        }

        private void RedirectPyLogger()
        {
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString(@"
import logging
import sys
root = logging.getLogger()
root.setLevel(logging.DEBUG)
handler = logging.StreamHandler(sys.stdout)
handler.setLevel(logging.DEBUG)
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)
root.addHandler(handler)");
            }
        }

        private void ConfigureGpu()
        {
            //
            // Avoid CUDA out of memory errors
            //
            Environment.SetEnvironmentVariable(
                "PYTORCH_CUDA_ALLOC_CONF",
                "garbage_collection_threshold:0.6,max_split_size_mb: 128",
                EnvironmentVariableTarget.Machine);
            //
            // verify pytorch sees cuda
            //
            using (Py.GIL())
            {
                dynamic torch = Py.Import("torch");
                dynamic is_available = torch.cuda.is_available();
                if (!(bool)is_available)
                {
                    throw new Exception("Cuda not available for GPU support. Double "+
                        "check that the virtual environment was setup correctly "+
                        "with the version of pytorch compiled with CUDA support.");
                }
            }
        }

        public static List<string> GetInstalledPythons()
        {
            var pythons = new List<string>();
            var exitCode = ProcessHelper.LaunchProcess(
                "where",
                "python",
                out List<string> output);
            foreach (var line in output)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                var path = Path.GetDirectoryName(line);
                var info = new DirectoryInfo(path);
                foreach (var f in info.GetFiles("*.dll"))
                {
                    if (f.Name.Contains("python"))
                    {
                        pythons.Add(f.FullName);
                    }
                }
            }
            return pythons;
        }

        public static void DumpDebugInfo()
        {
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    dynamic os = Py.Import("os");
                    dynamic pythonpath = os.getenv("PYTHONPATH");
                    dynamic path = os.getenv("PATH");
                    dynamic pythonhome = os.getenv("PYTHONHOME");
                    dynamic sys = Py.Import("sys");
                    dynamic syspath = sys.path;
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Verbose,
                          $"PATH: {path}");
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Verbose,
                          $"PYTHONPATH: {pythonpath}");
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Verbose,
                          $"PYTHONHOME: {pythonhome}");
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Verbose,
                          $"sys.path: {syspath}");
                }
            }
        }
    }

    sealed class VirtualEnv
    {
        public string m_Path;
        public static readonly string s_DefaultEnv =
            Path.Combine(Directory.GetCurrentDirectory(), "rpci_venv");
        private bool m_Created;
        private bool m_SkipSetup;
        private bool m_UseGpu;

        public VirtualEnv(string ExistingVenvPath, bool UseGpu)
        {
            m_UseGpu = UseGpu;
            m_Path = ExistingVenvPath;
            if (!string.IsNullOrEmpty(m_Path))
            {
                //
                // If the user specified an existing venv in their settings, we'll
                // skip the long-winded python setup entirely.
                //
                m_SkipSetup = true;
            }
        }

        public void Create()
        {
            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  "Creating virtual environment");

            if (string.IsNullOrEmpty(m_Path) || !Directory.Exists(m_Path))
            {
                //
                // Create a new default virtualenv.
                //
                m_Path = s_DefaultEnv;
                try
                {
                    CreateInternal();
                }
                catch (Exception ex)
                {
                    var error = $"Exception in CreateInternal(): {ex.Message}";
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Error,
                          error);
                    throw new Exception(error);
                }
            }
            else
            {
                Trace(TraceLoggerType.PythonWrapper,
                      TraceEventType.Verbose,
                      "Using existing virtualenv at " + m_Path);
                if (m_Path == s_DefaultEnv)
                {
                    //
                    // Reuse a previously-created default virtualenv.
                    //
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Verbose,
                          "Reusing default virtualenv at " + m_Path);
                }
                else
                {
                    //
                    // An existing virtualenv has been provided - nothing to do.
                    //
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Verbose,
                          "Reusing virtualenv at " + m_Path);
                }
            }
            m_Created = true;
        }

        public void Setup()
        {
            if (m_SkipSetup)
            {
                return;
            }

            Trace(TraceLoggerType.PythonWrapper,
                 TraceEventType.Verbose,
                 "Setting up python virtual environment");

            if (!m_Created)
            {
                var error = "Setup() called before Create()";
                Trace(TraceLoggerType.PythonWrapper,
                      TraceEventType.Error,
                      error);
                throw new Exception(error);
            }

            //
            // Extract resources
            //
            try
            {
                var resourceFileNames = new List<(string, string)> {
                    ("RpcInvestigator.Util.ML", "requirements.txt"),
                    ("RpcInvestigator.Util.ML", "requirements-cpu.txt"),
                    ("RpcInvestigator.Util.ML", "requirements-gpu.txt"),
                    ("RpcInvestigator.Util.ML.Generators", "dolly.py"),
                    ("RpcInvestigator.Util.ML.Generators", "gpt4all-alpaca-13b-q4.py"),
                    ("RpcInvestigator.Util.ML.Generators", "alpaca-lora.py"),
                    ("RpcInvestigator.Util.ML.Generators", "generator_config.py"),
                };
                foreach (var tuple in resourceFileNames)
                {
                    var dest = Path.Combine(m_Path, tuple.Item2);
                    ExtractResource(tuple.Item1 + "." + tuple.Item2, dest);
                }
            }
            catch (Exception ex)
            {
                var error = $"Exception during resource extraction: {ex.Message}";
                Trace(TraceLoggerType.PythonWrapper,
                      TraceEventType.Error,
                      error);
                throw new Exception(error);
            }

            //
            // Important: install gpu/cpu requirements first, so we get the right 'torch'
            //
            string requirements;
            if (m_UseGpu)
            {
                requirements = Path.Combine(m_Path, "requirements-gpu.txt");
                if (!InstallPackage(requirements, true))
                {
                    var error = "Failed to install requirements-gpu.txt";
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Error,
                          error);
                    throw new Exception(error);
                }
            }
            else
            {
                requirements = Path.Combine(m_Path, "requirements-cpu.txt");
                if (!InstallPackage(requirements, true))
                {
                    var error = "Failed to install requirements-cpu.txt";
                    Trace(TraceLoggerType.PythonWrapper,
                          TraceEventType.Error,
                          error);
                    throw new Exception(error);
                }
            }
            requirements = Path.Combine(m_Path, "requirements.txt");
            if (!InstallPackage(requirements, true))
            {
                var error = "Failed to install requirements.txt";
                Trace(TraceLoggerType.PythonWrapper,
                      TraceEventType.Error,
                      error);
                throw new Exception(error);
            }
        }

        private void ExtractResource(string Name, string Location)
        {
            Stream resource = null;
            FileStream file = null;

            try
            {
                resource = Assembly.GetExecutingAssembly().
                    GetManifestResourceStream(Name);
                if (resource == null)
                {
                    throw new Exception("Unable to locate resource '" + Name + "'");
                }

                file = new FileStream(Location, FileMode.Create, FileAccess.Write);
                resource.CopyTo(file);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (resource != null)
                {
                    resource.Close();
                }

                if (file != null)
                {
                    file.Close();
                }
            }
        }

        private void CreateInternal()
        {
            //
            // First launch pip to install 'virtualenv' package
            //
            if (!InstallPackage("virtualenv", false))
            {
                var error = "Failed to install python virtualenv module.";
                Trace(TraceLoggerType.PythonWrapper,
                     TraceEventType.Error,
                     error);
                throw new Exception(error);
            }

            var exitCode = ProcessHelper.LaunchProcess(
                "virtualenv",
                $"{m_Path}",
                out List<string> output);

            if (!Directory.Exists(m_Path) || exitCode != 0)
            {
                var error = $"virtualenv appears to have failed (exit code {exitCode})";
                Trace(TraceLoggerType.PythonWrapper,
                     TraceEventType.Error,
                     $"{error}{Environment.NewLine}{string.Join(",", output)}");
                throw new Exception(error);
            }
            Trace(TraceLoggerType.PythonWrapper,
                 TraceEventType.Verbose,
                 "Virtualenv created at " + m_Path);
        }

        private bool InstallPackage(string package, bool IsRequirementsTxt)
        {
            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  $"Installing {package}");

            string args;
            if (IsRequirementsTxt)
            {
                args = $"install -r {package}";
            }
            else
            {
                args = $"install {package}";
            }

            var exitCode = ProcessHelper.LaunchProcess(
                "pip.exe",
                args,
                out List<string> output);

            if (output.Count == 0)
            {
                Trace(TraceLoggerType.PythonWrapper,
                      TraceEventType.Error,
                      "No output from pip process.");
                return false;
            }
            else if (exitCode != 0)
            {
                Trace(TraceLoggerType.PythonWrapper,
                      TraceEventType.Error,
                      $"Pip process failed with exit code {exitCode}:" +
                      $"{Environment.NewLine}{string.Join(",", output)}");
                return false;
            }
            Trace(TraceLoggerType.PythonWrapper,
                  TraceEventType.Verbose,
                  $"Pip install output: {Environment.NewLine}{string.Join(",", output)}");
            return true;
        }
    }

    sealed class ConsoleWriter
    {
        private TextWriter _TextWriter;
        public TextWriter TextWriter
        {
            get
            {
                return _TextWriter == null ? Console.Out : _TextWriter;
            }
            set
            {
                _TextWriter = value;
            }
        }

        public ConsoleWriter(TextWriter writer = null)
        {
            this._TextWriter = writer;
        }

        public void write(String str)
        {
            //str = str.Replace("\n", Environment.NewLine);
            this.TextWriter.Write(str);
        }

        public void writelines(String[] str)
        {
            foreach (String line in str)
            {
                this.write(line);
            }
        }

        public void flush()
        {
            this.TextWriter.Flush();
        }

        public void close()
        {
            if (this._TextWriter != null)
            {
                this._TextWriter.Close();
            }
        }
    }
}
