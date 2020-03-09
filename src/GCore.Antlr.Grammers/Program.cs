using System;
using System.IO;
using System.Net;

namespace Antlr4.Grammers
{
    class Program
    {
        static void Main(string[] args)
        {
            GCore.Logging.Log.LoggingHandler.Add(new GCore.Logging.Logger.ConsoleLogger());
            GCore.Logging.Log.LoggingHandler.Add(new GCore.Logging.Logger.FileLogger("./log.txt"));

            var repoPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../.."));
            var workSpace = Path.Combine(repoPath, "workspace");
            var grammersPath = Path.Combine(workSpace, "grammers");
            var projectsPath = Path.Combine(workSpace, "projects");
            var packagePath = Path.Combine(workSpace, "packages");
            var antlrPath = Path.Combine(workSpace, "antlr-4.8-complete.jar");
            
            if(Directory.Exists(workSpace))
                Directory.Delete(workSpace, true);

            Directory.CreateDirectory(workSpace);
            Directory.CreateDirectory(grammersPath);
            Directory.CreateDirectory(projectsPath);
            Directory.CreateDirectory(packagePath);

            var wc = new WebClient();
            wc.DownloadFile("https://www.antlr.org/download/antlr-4.8-complete.jar", antlrPath);

            $"git clone https://github.com/antlr/grammars-v4 {grammersPath}".Sh();

            var repoVersion = "git rev-list --count HEAD".Sh().Replace("\n", "").Trim();
            var grammerVersion = "git rev-list --count HEAD".Sh(grammersPath).Replace("\n", "").Trim();

            var repoContext = new RepoContext(repoVersion, grammerVersion, packagePath, Environment.GetEnvironmentVariable("NugetToken"));

            foreach(var dir in Directory.GetDirectories(grammersPath))
                if(!(dir.EndsWith("-test") || dir.EndsWith(".git")))
                    new ProjectHandler(antlrPath, dir, projectsPath, repoContext).DoIt(); 
        }
    }
}
