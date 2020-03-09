namespace Antlr4.Grammers
{
    public class RepoContext
    {
        public string PackagePrefix { get; private set;} = "GCore.Antlr.Grammers";
        public string RepoVersion { get; private set; }
        public string GrammerVersion { get; private set; }
        public string PackagePath { get; private set; }
        public string ApiToken { get; private set; }

        public string Version => $"1.{GrammerVersion}.{RepoVersion}";

        public RepoContext(string repoVersion, string grammerVersion, string packagePath, string apiToken) {
            RepoVersion = repoVersion;
            GrammerVersion = grammerVersion;
            PackagePath = packagePath;
            ApiToken = apiToken;
        }
    }
}