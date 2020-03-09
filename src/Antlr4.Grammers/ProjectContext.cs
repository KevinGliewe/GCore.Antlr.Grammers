using System;
using System.IO;
using System.Linq;
using Humanizer;

namespace Antlr4.Grammers
{
    public class ProjectContext
    {
        public string Antlr4Path { get; private set; }
        public string SrcPath { get; private set; }
        public string DestPath { get; private set; }
        public string ProjectFile { get; private set; }

        public string Name { get; private set; }

        public ProjectContext(string antlr4Path, string srcPath, string destPath) {
            Antlr4Path = Path.GetFullPath(antlr4Path);
            SrcPath = Path.GetFullPath(srcPath);
            Name = SrcPath.Split(Path.DirectorySeparatorChar).Last().Transform(To.LowerCase, To.TitleCase);
            DestPath = Path.GetFullPath(Path.Combine(destPath, Name));
            ProjectFile = Path.Combine(DestPath, Name + ".csproj");
        }
    }
}