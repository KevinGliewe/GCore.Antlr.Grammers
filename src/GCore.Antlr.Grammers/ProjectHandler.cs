using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GCore.Logging;

namespace GCore.Antlr.Grammers
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

        public async Task<bool> DoIt() {
            Log.Info("Processing "+Context.Name);
            bool result = true;

            foreach(Func<Task<bool>> f in new Func<Task<bool>>[] {CopyData, GenerateParser, GenerateProject, BuildProject, Publish}) {
                
                if(!result) 
                    break;
                //Log.Info("-> " + f.Method.Name);
                try {
                    result = await f();
                    if(!result) {
                        Log.Warn($"#> {Context.Name} -> {f.Method.Name} FAILED");
                        Log.Debug(_lastprocess);
                    }
                } catch (Exception ex) {
                    Log.Exception(f.Method.Name, ex);
                    result = false;
                }
            }

            if(result)
                Log.Success("Done " + Context.Name);
            return result;
        }

        private async Task<bool> CopyData() {
            Directory.CreateDirectory(Context.DestPath);

            foreach(var file in Directory.GetFiles(Context.SrcPath, "*.g4"))
                await Helper.CopyFileAsync(file, Path.GetFullPath(Path.Combine(Context.DestPath, Path.GetFileName(file))));
            return true;
        }

        private async Task<bool> GenerateParser()
        {
            foreach (var file in Directory.GetFiles(Context.DestPath, "*.g4"))
            {
                var result = await $"java -jar {Context.Antlr4Path} -Dlanguage=CSharp -visitor -o {Context.DestPath} {file}".Sh2();

                _lastprocess = result.Item2;

                if (result.Item1 != 0)
                    return false;
            }

            foreach(var csfile in Directory.GetFiles(Context.DestPath, "*.cs"))
                await File.WriteAllTextAsync(csfile, $"namespace {Repo.PackagePrefix}.{Context.Name}\n{{\n" + File.ReadAllText(csfile).Replace(".compareTo(", ".CompareTo(") + "\n}");
            return true;
        }

        private async Task<bool> GenerateProject() {
            await File.WriteAllTextAsync(Context.ProjectFile ,$@"

            <Project Sdk=""Microsoft.NET.Sdk"">

                <PropertyGroup>
                    <PackageId>{Repo.PackagePrefix}.{Context.Name}</PackageId>
                    <Version>{Repo.Version}</Version>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <Authors>Kevin Gliewe</Authors>
                    <Company>Kevin Gliewe</Company>
                    <Description>Prebuild Antlr4 grammer for {Context.Name}</Description>
                    <RepositoryUrl>https://github.com/KillerGoldFisch/GCore.Antlr4.Grammers</RepositoryUrl>
                    <PackageProjectUrl>https://kevingliewe.github.io/GCore.Antlr.Grammers/api/GCore.Antlr.Grammers.{Context.Name}.html</PackageProjectUrl>
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

        private async Task<bool> BuildProject()
        {
            var result = await $"dotnet publish -c Release {Context.ProjectFile}".Sh2();
            _lastprocess = result.Item2;
            if (result.Item1  != 0)
                return false;

            var src = Path.Combine(Context.DestPath, "bin", "Release", PackageName);
            var dest = Path.Combine(Repo.PackagePath, PackageName);

            if(!File.Exists(src))
                return false;
            File.Move(src, dest);

            return true;
        }

        private async Task<bool> Publish() {
            if(Repo.ApiToken is null)
                return true;

            var dest = Path.Combine(Repo.PackagePath, PackageName);

            var result =
                await $"dotnet nuget push {dest} -k {Repo.ApiToken} -s https://api.nuget.org/v3/index.json".Sh2();
            _lastprocess = result.Item2;
            if (result.Item1 != 0)
               return false;

            return true;
        }
    }
}