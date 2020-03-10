using System.IO;

namespace GCore.Antlr.Grammers
{
    public class RepoContext
    {
        public string PackagePrefix { get; private set;} = "GCore.Antlr.Grammers";
        public string RepoVersion { get; private set; }
        public string GrammerVersion { get; private set; }
        public string PackagePath { get; private set; }
        public string ApiToken { get; private set; }
        public string GithubToken { get; private set; }
        public string WorkspacePath { get; private set; }

        public string SolutionPath => Path.Combine(WorkspacePath, PackagePrefix +".sln");
        public string Version => $"1.{GrammerVersion}.{RepoVersion}";

        public RepoContext(string repoVersion, string grammerVersion, string packagePath, string workspacePath, string apiToken, string githubToken) {
            RepoVersion = repoVersion;
            GrammerVersion = grammerVersion;
            PackagePath = packagePath;
            WorkspacePath = workspacePath;
            ApiToken = apiToken;
            GithubToken = githubToken;
        }
    }
}