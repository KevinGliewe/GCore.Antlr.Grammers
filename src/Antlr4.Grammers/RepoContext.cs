namespace Antlr4.Grammers
{
    public class RepoContext
    {
        public string RepoVersion { get; private set; }
        public string GrammerVersion { get; private set; }

        public string Version => $"1.{GrammerVersion}.{RepoVersion}";

        public RepoContext(string repoVersion, string grammerVersion) {
            RepoVersion = repoVersion;
            GrammerVersion = grammerVersion;
        }
    }
}