using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GCore.Logging;

namespace GCore.Antlr.Grammers
{
    public static class StringShExtensions {

        public static async Task<string> Sh(this string cmd, string workingDirectory = ".")
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var fileName = "/bin/bash";
            var arguments = $"-c \"{escapedArgs}\"";

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                fileName = "cmd.exe";
                arguments = $"/C \"{escapedArgs}\"";
            }
            
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
            process.Start();
            var stdOut = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();
            return stdOut;
        }

        public static void Sh2(this string cmd, out Process process, string workingDirectory = ".")
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var fileName = "/bin/bash";
            var arguments = $"-c \"{escapedArgs}\"";

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                fileName = "cmd.exe";
                arguments = $"/C \"{escapedArgs}\"";
            }
            
            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
            process.Start();
        }



        public static async Task<(int, string)> Sh2(this string cmd, string workingDirectory = ".")
        {
            Process process;
            cmd.Sh2(out process, workingDirectory);
            await process.WaitForExitAsync();
            return (process.ExitCode, process.StandardOutput.ReadToEnd());
        }

        public static async Task<int> Sh3(this string cmd, Action<string> lineCallback = null, string workingDirectory = ".")
        {
            Process process;
            cmd.Sh2(out process, workingDirectory);
            string line;
            if(lineCallback != null)
                while((line = process.StandardOutput.ReadLine()) != null)
                    lineCallback(line);
            await process.WaitForExitAsync();
            return process.ExitCode;
        }

        public static async Task<int> Sh4(this string cmd, string workingDirectory = ".")
        {
            Process process;
            cmd.Sh2(out process, workingDirectory);
            string line;
            while((line = process.StandardOutput.ReadLine()) != null)
                GCore.Logging.Log.Info($"Process {process.Id}: {line}");
            await process.WaitForExitAsync();
            return process.ExitCode;
        }
            
    }
}