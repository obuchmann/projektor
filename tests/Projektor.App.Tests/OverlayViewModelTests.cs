using Projektor.App.ViewModels;
using Projektor.Core.Models;
using Projektor.Core.Ports;
using Projektor.Core.Services;

namespace Projektor.App.Tests;

public sealed class OverlayViewModelTests
{
    private sealed class FakeLauncher : IProcessLauncher
    {
        public List<(string Command, string WorkingDir)> Launched { get; } = [];
        public string? FailCommand { get; set; }

        public void Launch(ProjectAction action, string workingDirectory)
        {
            if (FailCommand is not null && action.Command == FailCommand)
                throw new ProcessLaunchException($"boom: {action.Command}");
            Launched.Add((action.Command, workingDirectory));
        }
    }

    private static (OverlayViewModel Vm, FakeLauncher Launcher) Build()
    {
        var launcher = new FakeLauncher();
        var vm = new OverlayViewModel(new ActionResolver(), new ProjectFilter(), launcher);

        // Commands are intentionally identical across OS so assertions hold on Windows + Linux
        // (ActionResolver picks command_windows on Windows, command_linux otherwise).
        var config = new ProjektorConfig(
            ActionTemplates:
            [
                new ActionTemplate("terminal", "Terminal", "term", "term"),
                new ActionTemplate("editor", "Editor", "code {path}", "code {path}"),
                new ActionTemplate("files", "Files", "files", "files"),
            ],
            Projects:
            [
                new Project("MyApp", "/dev/myapp", ProjectSource.Manual, [], []),
            ],
            ScanRoots: [],
            Settings: new AppSettings());

        vm.SetConfig(config);
        return (vm, launcher);
    }

    [Fact]
    public void SetConfig_SelectsFirstProjectAndNumbersActions()
    {
        var (vm, _) = Build();

        Assert.Equal("MyApp", vm.SelectedProject?.Name);
        Assert.Equal(3, vm.Actions.Count);
        Assert.Equal(1, vm.Actions[0].Number);
        Assert.Equal("Alt+1", vm.Actions[0].ShortcutLabel);
    }

    [Fact]
    public void LaunchByIndex_LaunchesNthActionInProjectWorkingDir()
    {
        var (vm, launcher) = Build();

        vm.LaunchByIndex(2);

        var launched = Assert.Single(launcher.Launched);
        Assert.Equal("code /dev/myapp", launched.Command);
        Assert.Equal("/dev/myapp", launched.WorkingDir);
    }

    [Fact]
    public void LaunchByIndex_OutOfRange_DoesNothing()
    {
        var (vm, launcher) = Build();

        vm.LaunchByIndex(0);
        vm.LaunchByIndex(99);

        Assert.Empty(launcher.Launched);
    }

    [Fact]
    public void LaunchFirst_LaunchesActionOne()
    {
        var (vm, launcher) = Build();

        vm.LaunchFirst();

        Assert.Equal("term", Assert.Single(launcher.Launched).Command);
    }

    [Fact]
    public void LaunchAll_LaunchesEveryActionOnce()
    {
        var (vm, launcher) = Build();

        vm.LaunchAll();

        Assert.Equal(3, launcher.Launched.Count);
    }

    [Fact]
    public void Launch_RaisesLaunchRequestedOnSuccess()
    {
        var (vm, _) = Build();
        var raised = false;
        vm.LaunchRequested += (_, _) => raised = true;

        vm.LaunchByIndex(1);

        Assert.True(raised);
    }

    [Fact]
    public void Launch_Failure_SetsStatusMessageAndDoesNotRequestHide()
    {
        var (vm, launcher) = Build();
        launcher.FailCommand = "term"; // action 1
        var raised = false;
        vm.LaunchRequested += (_, _) => raised = true;

        vm.LaunchByIndex(1);

        Assert.False(raised);
        Assert.NotNull(vm.StatusMessage);
        Assert.Contains("boom", vm.StatusMessage);
    }

    [Fact]
    public void Filter_NarrowsProjectsAndRefreshesActions()
    {
        var (vm, _) = Build();

        vm.SearchText = "zzz-no-match";

        Assert.Empty(vm.Projects);
        Assert.Null(vm.SelectedProject);
        Assert.Empty(vm.Actions);
    }
}
