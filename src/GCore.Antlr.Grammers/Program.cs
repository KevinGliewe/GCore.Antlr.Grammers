using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GCore.Antlr.Grammers
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
            var antlrPath = Path.Combine(workSpace, "antlr.jar");
            
            
            if(Directory.Exists(workSpace))
                Directory.Delete(workSpace, true);

            Directory.CreateDirectory(workSpace);
            Directory.CreateDirectory(grammersPath);
            Directory.CreateDirectory(projectsPath);
            Directory.CreateDirectory(packagePath);

            var wc = new WebClient();
            wc.DownloadFile("https://www.antlr.org/download/antlr-4.9.2-complete.jar", antlrPath);

            $"git clone https://github.com/antlr/grammars-v4 {grammersPath}".Sh();

            var repoVersion = "git rev-list --count HEAD".Sh().Replace("\n", "").Trim();
            var grammerVersion = "git rev-list --count HEAD".Sh(grammersPath).Replace("\n", "").Trim();

            var repoContext = new RepoContext(repoPath, repoVersion, grammerVersion, packagePath, workSpace,
                args.Length > 0 ? args[0] : null
            );

            var solutionPath = Path.Combine(workSpace, repoContext.PackagePrefix + ".sln");

            var projects = new List<ProjectHandler>();
            foreach(var dir in Directory.GetDirectories(grammersPath)) {
                var dirName = dir.Split(Path.DirectorySeparatorChar).Last();
                if(!(dirName.StartsWith("_") || dirName.StartsWith("."))) {
                    var proj = new ProjectHandler(antlrPath, dir, projectsPath, repoContext);
                    if(proj.DoIt())
                    projects.Add(proj);
                }
            }
            
            new SolutionHandler(repoContext, projects.Select(p=>p.Context)).DoIt();
        }
    }
}
