using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GCore.Antlr.Grammers
{
    class Program
    {
        public static readonly int MAX_CONCURRENT_TASKS = 5;
        static void Main(string[] args)
        {
            GCore.Logging.Log.LoggingHandler.Add(new GCore.Logging.Logger.ConsoleLogger( Logging.LogEntry.LogTypes.All ));
            GCore.Logging.Log.LoggingHandler.Add(new GCore.Logging.Logger.FileLogger("./log.txt"));

            var repoPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../.."));
            var workSpace = Path.Combine(repoPath, "workspace");
            var grammersPath = Path.Combine(workSpace, "grammers");
            var projectsPath = Path.Combine(workSpace, "projects");
            var packagePath = Path.Combine(workSpace, "packages");
            var antlrPath = Path.Combine(workSpace, "antlr.jar");
            var docFxPath = Path.Combine(repoPath, "doc", "docfx");
            var ghPagesPath = Path.Combine(docFxPath, "_site");
            
            
            if(Directory.Exists(workSpace))
                Directory.Delete(workSpace, true);
            if(Directory.Exists(ghPagesPath))
                Directory.Delete(ghPagesPath, true);

            Directory.CreateDirectory(workSpace);
            Directory.CreateDirectory(grammersPath);
            Directory.CreateDirectory(projectsPath);
            Directory.CreateDirectory(packagePath);

            var wc = new WebClient();
            wc.DownloadFile("https://www.antlr.org/download/antlr-4.9.2-complete.jar", antlrPath);

            $"git clone https://github.com/antlr/grammars-v4 {grammersPath}".Sh().Result.ToString();

            var repoVersion = "git rev-list --count HEAD".Sh().Result.Replace("\n", "").Trim();
            var grammerVersion = "git rev-list --count HEAD".Sh(grammersPath).Result.Replace("\n", "").Trim();

            var repoContext = new RepoContext(repoPath, repoVersion, grammerVersion, packagePath, workSpace, docFxPath, ghPagesPath, 
                Environment.GetEnvironmentVariable("NugetToken"),
                Environment.GetEnvironmentVariable("GithubUser"),
                Environment.GetEnvironmentVariable("GithubToken"),
                Environment.GetEnvironmentVariable("DOCFX_TOOL") ?? "docfx build ./" 
            );

            var solutionPath = Path.Combine(workSpace, repoContext.PackagePrefix + ".sln");

            var toProcessProjects = new Stack<ProjectHandler>();

            var projectTasks = new List<(Task<bool> ,ProjectHandler)>();

            var finishedProjects = new List<ProjectHandler>();


            foreach(var dir in Directory.GetDirectories(grammersPath))
            {
                var dirName = dir.Split(Path.DirectorySeparatorChar)[^1];
                if(!(dirName.StartsWith("_") || dirName.StartsWith("."))) {
                    var proj = new ProjectHandler(antlrPath, dir, projectsPath, repoContext);
                    //projectTasks.Add((proj.DoIt(), proj));
                    toProcessProjects.Push(proj);
                }
            }

            /*while (toProcessProjects.Count > 0) {
                var proj = toProcessProjects.Pop();

                var success = proj.DoIt().Result;

                if(success)
                    finishedProjects.Add(proj);
            }*/

            do
            {
                for (int i = 0; i < projectTasks.Count; i++)
                {
                    if (projectTasks[i].Item1.IsCompleted)
                    {
                        if(projectTasks[i].Item1.IsCompletedSuccessfully)
                            finishedProjects.Add(projectTasks[i].Item2);
                        projectTasks.RemoveAt(i);
                        i--;
                    }
                }

                while (projectTasks.Count < MAX_CONCURRENT_TASKS && toProcessProjects.Count > 0)
                {
                    var proj = toProcessProjects.Pop();
                    projectTasks.Add((proj.DoIt(), proj));
                }

            } while (projectTasks.Count > 0);


            new SolutionHandler(repoContext, finishedProjects.Select((p => p.Context))).DoIt();
        }
    }
}
