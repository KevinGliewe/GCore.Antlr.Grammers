using System;
using System.IO;
using System.Linq;

using GCore.Logging;

namespace Antlr4.Grammers
{
    public class ProjectHandler
    {

        private string _lastprocess = null;
        public ProjectContext Context { get; private set; }
        public RepoContext Repo {get; private set; }

        public string PackageName => $"{Repo.PackagePrefix}.{Context.Name}.{Repo.Version}.nupkg";

        public ProjectHandler(string antlr4Path, string srcPath, string destPath, RepoContext repo)
        {
            Repo = repo;
            Context = new ProjectContext(antlr4Path, srcPath, destPath);
        }

        public bool DoIt() {
            Log.Info("Processing "+Context.Name);
            bool result = true;

            foreach(Func<bool> f in new Func<bool>[] {CopyData, GenerateParser, GenerateProject, BuildProject, Publish}) {
                
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

        private bool CopyData() {
            Directory.CreateDirectory(Context.DestPath);

            foreach(var file in Directory.GetFiles(Context.SrcPath, "*.g4"))
                File.Copy(file, Path.GetFullPath(Path.Combine(Context.DestPath, Path.GetFileName(file))));
            return true;
        }

        private bool GenerateParser() {
            foreach(var file in Directory.GetFiles(Context.DestPath, "*.g4"))
                if($"java -jar {Context.Antlr4Path} -Dlanguage=CSharp -visitor -o {Context.DestPath} {file}".Sh2(out _lastprocess) != 0)
                    return false;
            
            foreach(var csfile in Directory.GetFiles(Context.DestPath, "*.cs"))
                File.WriteAllText(csfile, $"namespace {Repo.PackagePrefix}.{Context.Name}\n{{\n" + File.ReadAllText(csfile).Replace(".compareTo(", ".CompareTo(") + "\n}");
            return true;
        }

        private bool GenerateProject() {
            File.WriteAllText(Context.ProjectFile ,$@"

            <Project Sdk=""Microsoft.NET.Sdk"">

                <PropertyGroup>
                    <PackageId>{Repo.PackagePrefix}.{Context.Name}</PackageId>
                    <Version>{Repo.Version}</Version>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <Authors>Kevin Gliewe</Authors>
                    <Company>Kevin Gliewe</Company>
                    <Description>Prebuild Antlr4 grammer for {Context.Name}</Description>
                    <RepositoryUrl>https://github.com/KillerGoldFisch/GCore.Antlr4.Grammers</RepositoryUrl>
                    <PackageProjectUrl>https://github.com/KillerGoldFisch/GCore.Antlr4.Grammers</PackageProjectUrl>
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
            if($"dotnet publish -c Release {Context.ProjectFile}".Sh2(out _lastprocess) != 0)
                return false;

            var src = Path.Combine(Context.DestPath, "bin", "Release", PackageName);
            var dest = Path.Combine(Repo.PackagePath, PackageName);

            if(!File.Exists(src))
                return false;
            File.Move(src, dest);

            return true;
        }

        private bool Publish() {
            if(Repo.ApiToken is null)
                return true;

            var dest = Path.Combine(Repo.PackagePath, PackageName);
            if($"dotnet nuget push {dest} -k {Repo.ApiToken} -s https://api.nuget.org/v3/index.json".Sh2(out _lastprocess) != 0)
               return false;

            return true;
        }
    }
}