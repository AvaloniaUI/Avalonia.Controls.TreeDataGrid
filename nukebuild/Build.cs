#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.VSWhere.VSWhereTasks;

partial class Build : NukeBuild
{
    [Solution("Avalonia.Controls.TreeDataGrid.sln")] readonly Solution Solution;

    static Lazy<string> MsBuildExe = new Lazy<string>(() =>
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        var msBuildDirectory = VSWhere("-latest -nologo -property installationPath -format value -prerelease").FirstOrDefault().Text;

        if (!string.IsNullOrWhiteSpace(msBuildDirectory))
        {
            string msBuildExe = Path.Combine(msBuildDirectory, @"MSBuild\Current\Bin\MSBuild.exe");
            if (!System.IO.File.Exists(msBuildExe))
                msBuildExe = Path.Combine(msBuildDirectory, @"MSBuild\15.0\Bin\MSBuild.exe");

            return msBuildExe;
        }

        return null;
    }, false);

    BuildParameters Parameters { get; set; }
    protected override void OnBuildInitialized()
    {
        Parameters = new BuildParameters(this);
        Information("Building version {0} of Avalonia.Controls.TreeDataGrid ({1}) using version {2} of Nuke.",
            Parameters.Version,
            Parameters.Configuration,
            typeof(NukeBuild).Assembly.GetName().Version.ToString());

        if (Parameters.IsLocalBuild)
        {
            Information("Repository Name: " + Parameters.RepositoryName);
            Information("Repository Branch: " + Parameters.RepositoryBranch);
        }
        Information("Configuration: " + Parameters.Configuration);
        Information("IsLocalBuild: " + Parameters.IsLocalBuild);
        Information("IsRunningOnUnix: " + Parameters.IsRunningOnUnix);
        Information("IsRunningOnWindows: " + Parameters.IsRunningOnWindows);
        Information("IsRunningOnAzure:" + Parameters.IsRunningOnAzure);
        Information("IsPullRequest: " + Parameters.IsPullRequest);
        Information("IsMainRepo: " + Parameters.IsMainRepo);
        Information("IsMasterBranch: " + Parameters.IsMasterBranch);
        Information("IsReleaseBranch: " + Parameters.IsReleaseBranch);
        Information("IsReleasable: " + Parameters.IsReleasable);
        Information("IsMyGetRelease: " + Parameters.IsMyGetRelease);
        Information("IsNuGetRelease: " + Parameters.IsNuGetRelease);

        void ExecWait(string preamble, string command, string args)
        {
            Console.WriteLine(preamble);
            Process.Start(new ProcessStartInfo(command, args) {UseShellExecute = false}).WaitForExit();
        }
        ExecWait("dotnet version:", "dotnet", "--info");
        ExecWait("dotnet workloads:", "dotnet", "workload list");
    }

    IReadOnlyCollection<Output> MsBuildCommon(
        string projectFile,
        Configure<MSBuildSettings> configurator = null)
    {
        return MSBuild(c => c
            .SetProjectFile(projectFile)
            // This is required for VS2019 image on Azure Pipelines
            .When(_ => Parameters.IsRunningOnWindows && Parameters.IsRunningOnAzure, _ => _
                .AddProperty("JavaSdkDirectory", GetVariable<string>("JAVA_HOME_11_X64")))
            .AddProperty("PackageVersion", Parameters.Version)
            .AddProperty("iOSRoslynPathHackRequired", true)
            .SetProcessToolPath(MsBuildExe.Value)
            .SetConfiguration(Parameters.Configuration)
            .SetVerbosity(MSBuildVerbosity.Minimal)
            .Apply(configurator));
    }

    Target Clean => _ => _.Executes(() =>
    {
        Parameters.BuildDirs.ForEach(p => p.DeleteDirectory());
        Parameters.BuildDirs.ForEach(p => p.CreateOrCleanDirectory());
        Parameters.ArtifactsDir.CreateOrCleanDirectory();
        Parameters.NugetIntermediateRoot.CreateOrCleanDirectory();
        Parameters.NugetRoot.CreateOrCleanDirectory();
        Parameters.ZipRoot.CreateOrCleanDirectory();
        Parameters.TestResultsRoot.CreateOrCleanDirectory();
    });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            if (Parameters.IsRunningOnWindows)
                MsBuildCommon(Parameters.MSBuildSolution, c => c
                    .SetProcessAdditionalArguments("/r")
                    .AddTargets("Build")
                );

            else
                DotNetBuild(c => c
                    .SetProjectFile(Parameters.MSBuildSolution)
                    .AddProperty("PackageVersion", Parameters.Version)
                    .SetConfiguration(Parameters.Configuration)
                );
        });

    Target RunCoreLibsTests => _ => _
        .OnlyWhenStatic(() => !Parameters.SkipTests)
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var testProject in (RootDirectory / "tests").GlobFiles("**/*.csproj"))
            {
                Information($"Running tests from {testProject}");
                DotNetTest(c => c
                    .SetProjectFile(testProject)
                    .SetConfiguration(Parameters.Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .When(_ => Parameters.PublishTestResults, _ => _
                        .SetLoggers("trx")
                        .SetResultsDirectory(Parameters.TestResultsRoot)));
            }
        });

    Target ZipFiles => _ => _
        .After(CreateNugetPackages, Compile, RunCoreLibsTests, Package)
        .Executes(() =>
        {
            var data = Parameters;
            var pathToProjectSource = RootDirectory / "samples" / "TreeDataGridDemo";
            var pathToPublish = pathToProjectSource / "bin" / data.Configuration / "publish";

            DotNetPublish(c => c
                .SetProject(pathToProjectSource / "TreeDataGridDemo.csproj")
                .EnableNoBuild()
                .SetConfiguration(data.Configuration)
                .AddProperty("PackageVersion", data.Version)
                .AddProperty("PublishDir", pathToPublish));

            Zip(data.ZipCoreArtifacts, data.BinRoot);
            Zip(data.ZipNuGetArtifacts, data.NugetRoot);
            Zip(data.ZipTargetControlCatalogNetCoreDir, pathToPublish);
        });

    Target CreateNugetPackages => _ => _
        .DependsOn(Compile)
        .After(RunTests)
        .Executes(() =>
        {
            if (Parameters.IsRunningOnWindows)

                MsBuildCommon(Parameters.MSBuildSolution, c => c
                    .AddTargets("Pack"));
            else
                DotNetPack(c => c
                    .SetProject(Parameters.MSBuildSolution)
                    .SetConfiguration(Parameters.Configuration)
                    .AddProperty("PackageVersion", Parameters.Version));
        });

    Target RunTests => _ => _
        .DependsOn(RunCoreLibsTests);

    Target Package => _ => _
        .DependsOn(RunTests)
        .DependsOn(CreateNugetPackages);

    Target CiAzureLinux => _ => _
        .DependsOn(RunTests);

    Target CiAzureOSX => _ => _
        .DependsOn(Package)
        .DependsOn(ZipFiles);

    Target CiAzureWindows => _ => _
        .DependsOn(Package)
        .DependsOn(ZipFiles);

    public static int Main() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Execute<Build>(x => x.Package)
            : Execute<Build>(x => x.RunTests);
}

public static class ToolSettingsExtensions
{
    public static T Apply<T>(this T settings, Configure<T> configurator)
    {
        return configurator != null ? configurator(settings) : settings;
    }
}
