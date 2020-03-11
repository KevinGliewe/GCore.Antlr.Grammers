using System.IO;

namespace GCore.Antlr.Grammers
{
    public class RepoContext
    {
        public string RepoPath { get; private set; }
        public string PackagePrefix { get; private set;} = "GCore.Antlr.Grammers";
        public string RepoVersion { get; private set; }
        public string GrammerVersion { get; private set; }
        public string PackagePath { get; private set; }
        public string WorkspacePath { get; private set; }
        public string DocFxPath { get; private set; }
        public string GhPagesPath { get; private set; }
        public string GithubUser { get; private set; }
        public string GithubToken { get; private set; }
        public string ApiToken { get; private set; }
        public string DocFxTool { get; private set; }

        public string SolutionPath => Path.Combine(WorkspacePath, PackagePrefix +".sln");
        public string Version => $"1.{GrammerVersion}.{RepoVersion}";

        public RepoContext(
            string repoPath, 
            string repoVersion, 
            string grammerVersion, 
            string packagePath, 
            string workspacePath, 
            string docFxPath,
            string ghPagesPath, 
            string apiToken, 
            string githubUser,
            string githubToken, 
            string docFxTool
            ) {
            RepoPath = repoPath;
            RepoVersion = repoVersion;
            GrammerVersion = grammerVersion;
            PackagePath = packagePath;
            WorkspacePath = workspacePath;
            DocFxPath = docFxPath;
            GhPagesPath = ghPagesPath;
            ApiToken = apiToken;
            GithubUser = githubUser;
            GithubToken = githubToken;
            DocFxTool = docFxTool;
        }
    }
}