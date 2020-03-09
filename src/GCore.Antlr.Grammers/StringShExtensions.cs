using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GCore.Logging;

namespace Antlr4.Grammers
{
    public static class StringShExtensions {

        public static string Sh(this string cmd, string workingDirectory = ".")
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
            process.WaitForExit();
            return stdOut;
        }

        public static void Sh2(this string cmd, out Process process)
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
                }
            };
            process.Start();
        }



        public static int Sh2(this string cmd, out string stdOut)
        {
            Process process;
            cmd.Sh2(out process);
            stdOut = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode;
        }

        public static int Sh2(this string cmd, Action<string> lineCallback = null)
        {
            Process process;
            cmd.Sh2(out process);
            string line;
            if(lineCallback != null)
                while((line = process.StandardOutput.ReadLine()) != null)
                    lineCallback(line);
            process.WaitForExit();
            return process.ExitCode;
        }

        public static int Sh2(this string cmd)
        {
            Process process;
            cmd.Sh2(out process);
            string line;
            while((line = process.StandardOutput.ReadLine()) != null)
                GCore.Logging.Log.Info($"Process {process.Id}: {line}");
            process.WaitForExit();
            return process.ExitCode;
        }
            
    }
}