using System;
using System.IO;

namespace Antlr4.Grammers
{
    class Program
    {
        static void Main(string[] args)
        {
            var repoPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../.."));
            var workSpace = Path.Combine(repoPath, "workspace");
            var grammersPath = Path.Combine(workSpace, "grammers");
            var projectsPath = Path.Combine(workSpace, "projects");
            
            Directory.Delete(workSpace, true);

            Directory.CreateDirectory(workSpace);
            Directory.CreateDirectory(grammersPath);
            Directory.CreateDirectory(projectsPath);

            $"git clone https://github.com/antlr/grammars-v4 {grammersPath}".Sh();

            var repoVersion = "git rev-list --count HEAD".Sh().Replace("\n", "").Trim();
            var grammerVersion = "git rev-list --count HEAD".Sh(grammersPath).Replace("\n", "").Trim();

            var repoContext = new RepoContext(repoVersion, grammerVersion);

            foreach(var dir in Directory.GetDirectories(grammersPath))
                if(!(dir.EndsWith("-test") || dir.EndsWith(".git")))
                    new ProjectHandler(@"C:\PATH\antlr-4.8-complete.jar", dir, projectsPath, repoContext).DoIt(); 
        }
    }
}
