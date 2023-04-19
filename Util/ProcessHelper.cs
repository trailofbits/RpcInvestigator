using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpcInvestigator.Util
{
    public static class ProcessHelper
    {
        public
        static
        int
        LaunchProcess(
            string Program,
            string Args,
            out List<string> Output
            )
        {
            var stdoutData = new List<string>();
            var stderrData = new List<string>();
            int exitCode;
            ProcessStartInfo psi = new ProcessStartInfo();
            Process p = new Process();
            Output = new List<string>();

            //
            // Setup process start parameters to hook its stdout/err
            //
            psi.FileName = Program;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            if (Args != null)
            {
                psi.Arguments = Args;
            }

            //
            // Create process object and start it
            //
            p.StartInfo = psi;
            p.EnableRaisingEvents = true;

            //
            // Create async task to read as much output/err as we can
            //
            p.OutputDataReceived += new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e)
                {
                    stdoutData.Add(e.Data);
                });

            p.ErrorDataReceived += new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e)
                {
                    stderrData.Add(e.Data);
                });

            if (!p.Start())
            {
                return 1;
            }

            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
            p.CancelErrorRead();
            p.CancelOutputRead();
            Output.AddRange(stderrData);
            Output.AddRange(stdoutData);
            exitCode = p.ExitCode;
            p.Close();
            return exitCode;
        }
    }
}
