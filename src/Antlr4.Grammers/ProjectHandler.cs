using System;
using System.IO;
using System.Linq;


namespace Antlr4.Grammers
{
    public class ProjectHandler
    {
        public ProjectContext Context { get; private set; }
        public RepoContext Repo {get; private set; }

        public ProjectHandler(string antlr4Path, string srcPath, string destPath, RepoContext repo)
        {
            Repo = repo;
            Context = new ProjectContext(antlr4Path, srcPath, destPath);
        }

        public bool DoIt() {
            bool result = true;

            if(result)
                result = CopyData();
            if(result)
                result = GenerateParser();
            if(result)
                result = GenerateProject();
            if(result)
                result = BuildProject();
            if(result)
                result = Publish();

            return result;
        }

        private bool CopyData() {
            Directory.CreateDirectory(Context.DestPath);

            foreach(var file in Directory.GetFiles(Context.SrcPath, "*.g4"))
                File.Copy(file, Path.GetFullPath(Path.Combine(Context.DestPath, Path.GetFileName(file))));
            return true;
        }

        private bool GenerateParser() {
            foreach(var file in Directory.GetFiles(Context.DestPath, "*.g4"))
                $"java -jar {Context.Antlr4Path} -Dlanguage=CSharp -visitor -o {Context.DestPath} {file}".Sh();
            
            foreach(var csfile in Directory.GetFiles(Context.DestPath, "*.cs"))
                File.WriteAllText(csfile, $"namespace Antlr4.Grammers.{Context.Name}\n{{\n" + File.ReadAllText(csfile) + "\n}");
            return true;
        }

        private bool GenerateProject() {
            File.WriteAllText(Context.ProjectFile ,$@"

            <Project Sdk=""Microsoft.NET.Sdk"">

                <PropertyGroup>
                    <PackageId>Antlr4.Grammers.{Context.Name}</PackageId>
                    <Version>{Repo.Version}</Version>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <Authors>Kevin Gliewe</Authors>
                    <Company>Kevin Gliewe</Company>
                    <Description>Prebuild Antlr4 grammer for {Context.Name}</Description>
                    <RepositoryUrl>https://github.com/KillerGoldFisch/Antlr4.Grammers</RepositoryUrl>
                    <PackageProjectUrl>https://github.com/KillerGoldFisch/Antlr4.Grammers</PackageProjectUrl>
                    <PackageLicenseUrl>http://anak10thn.github.io/WTFPL/</PackageLicenseUrl>
                    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
                    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
                </PropertyGroup>

                <ItemGroup>
                    <PackageReference Include=""Antlr4.Runtime.Standard"" Version=""4.8.0"" />
                </ItemGroup>

            </Project>
            ");
            return true;
        }

        private bool BuildProject() {
            $"dotnet publish -c Release {Context.ProjectFile}".Sh();
            return true;
        }

        private bool Publish() {
            return true;
        }
    }
}