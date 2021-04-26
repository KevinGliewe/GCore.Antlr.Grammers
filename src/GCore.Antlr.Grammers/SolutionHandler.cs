using System;
using System.Linq;
using System.Collections.Generic;
using GCore.Logging;
using System.IO;

namespace GCore.Antlr.Grammers
{
    public class SolutionHandler
    {
        private string _lastprocess = null;
        public RepoContext Repo {get; private set; }
        public IEnumerable<ProjectContext> Projects;

        public SolutionHandler(RepoContext repo, IEnumerable<ProjectContext> projects) {
            Repo = repo;
            Projects = projects;
        }

        public bool DoIt() {
            Log.Info("Processing Solution");
            bool result = true;

            foreach(Func<bool> f in new Func<bool>[] {InitSolution, AddProjects, CloneGhPages, GenerateDocumentation, PublishDocumentation }) {
                
                if(!result) 
                    break;
                Log.Info("-> " + f.Method.Name);
                try {
                    result = f();
                    if(!result) {
                        Log.Warn("#> " + f.Method.Name + " FAILED");
                        Log.Debug(_lastprocess);
                    }
                } catch (Exception ex) {
                    Log.Exception(f.Method.Name, ex);
                    result = false;
                }
            }

            if(result)
                Log.Success("Done");
            return result;
        }

        private bool InitSolution()
        {
            var result = $"dotnet new sln --name {Repo.PackagePrefix}".Sh2(Repo.WorkspacePath).Result;
            _lastprocess = result.Item2;
            return result.Item1 == 0;
        }

        private bool AddProjects() {
            var relativeSolutionPath = Path.GetRelativePath(Repo.WorkspacePath, Repo.SolutionPath);

            foreach(var proj in Projects) {
                
                var relativeProjectPath = Path.GetRelativePath(Repo.WorkspacePath, proj.ProjectFile);
                Log.Info("--> Add project " + relativeProjectPath);

                var result = $"dotnet sln {relativeSolutionPath} add {relativeProjectPath}".Sh2(Repo.WorkspacePath).Result;
                _lastprocess = result.Item2;
                if (result.Item1 != 0)
                    return false;
            }
            
            return true;
        }

        private bool CloneGhPages()
        {
            return Sh($"git clone -b gh-pages --single-branch https://{Repo.GithubUser}:{Repo.GithubToken}@github.com/KevinGliewe/GCore.Antlr.Grammers --depth 1 {Repo.GhPagesPath}");
        }

        private bool GenerateDocumentation()
        {
            return Sh(Repo.DocFxTool, Repo.GhPagesPath);
        }

        private bool PublishDocumentation() {
            // https://help.github.com/en/github/authenticating-to-github/creating-a-personal-access-token-for-the-command-line
            return 
                Sh($"git add .", Repo.GhPagesPath) &&
                Sh($"git commit -m '{Repo.Version}'", Repo.GhPagesPath) &&
                Sh($"git push origin gh-pages", Repo.GhPagesPath);
        }

        bool Sh(string cmd, string workingDirectory = ".")
        {
            var result = cmd.Sh2(Repo.WorkspacePath).Result;
            _lastprocess = result.Item2;
            return result.Item1 == 0;
        }
    }
}