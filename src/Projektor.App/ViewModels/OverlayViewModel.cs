using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Projektor.Core.Models;
using Projektor.Core.Ports;
using Projektor.Core.Services;
using Projektor.Infrastructure;

namespace Projektor.App.ViewModels;

/// <summary>
/// Drives the overlay: filter projects as the user types, resolve the selected project's
/// effective actions (numbered), and launch them in the project's working directory.
/// Launch entry points (click, Alt+N, Enter, Shift+Enter) all funnel through <see cref="LaunchOne"/>.
/// </summary>
public sealed partial class OverlayViewModel : ObservableObject
{
    private readonly ActionResolver _resolver;
    private readonly ProjectFilter _filter;
    private readonly IProcessLauncher _launcher;
    private readonly ILogger<OverlayViewModel> _logger;
    private ProjektorConfig _config = ProjektorConfig.Empty;

    public ObservableCollection<Project> Projects { get; } = [];
    public ObservableCollection<ActionItem> Actions { get; } = [];

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>Raised after at least one successful launch so the host can hide the overlay.</summary>
    public event EventHandler? LaunchRequested;

    public OverlayViewModel(
        ActionResolver resolver,
        ProjectFilter filter,
        IProcessLauncher launcher,
        ILogger<OverlayViewModel>? logger = null)
    {
        _resolver = resolver;
        _filter = filter;
        _launcher = launcher;
        _logger = logger ?? NullLogger<OverlayViewModel>.Instance;
    }

    /// <summary>Replaces the active configuration and refreshes the visible lists.</summary>
    public void SetConfig(ProjektorConfig config)
    {
        _config = config;
        RefreshProjects();
    }

    /// <summary>Clears transient state — called each time the overlay is shown.</summary>
    public void Reset()
    {
        StatusMessage = null;
        SearchText = "";
        RefreshProjects();
    }

    partial void OnSearchTextChanged(string value) => RefreshProjects();

    partial void OnSelectedProjectChanged(Project? value) => RefreshActions();

    private void RefreshProjects()
    {
        Projects.Clear();
        foreach (var project in _filter.Filter(_config.Projects, SearchText))
            Projects.Add(project);

        SelectedProject = Projects.Count > 0 ? Projects[0] : null;
    }

    private void RefreshActions()
    {
        Actions.Clear();

        if (SelectedProject is null)
            return;

        // Resolve against an absolute path so {path} substitution and the working directory match.
        var expanded = SelectedProject with { Path = PathUtil.ExpandHome(SelectedProject.Path) };
        var number = 1;
        foreach (var action in _resolver.Resolve(_config, expanded))
            Actions.Add(new ActionItem(number++, action));
    }

    /// <summary>Click handler binding target — launches the clicked action.</summary>
    [RelayCommand]
    private void Launch(ActionItem? item)
    {
        if (item is null)
            return;
        if (LaunchOne(item.Action))
            LaunchRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Launches the first action (plain Enter).</summary>
    public void LaunchFirst() => LaunchByIndex(1);

    /// <summary>Launches the action at the given 1-based position (Alt+N).</summary>
    public void LaunchByIndex(int oneBasedIndex)
    {
        if (oneBasedIndex < 1 || oneBasedIndex > Actions.Count)
            return;
        if (LaunchOne(Actions[oneBasedIndex - 1].Action))
            LaunchRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Launches every action of the selected project (Shift+Enter).</summary>
    public void LaunchAll()
    {
        if (Actions.Count == 0)
            return;

        var any = false;
        foreach (var item in Actions)
            any |= LaunchOne(item.Action);

        if (any)
            LaunchRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool LaunchOne(ProjectAction action)
    {
        if (SelectedProject is null)
        {
            _logger.LogWarning("Launch von Action '{Action}' ignoriert — kein Projekt ausgewählt.", action.Name);
            return false;
        }

        var workingDirectory = PathUtil.ExpandHome(SelectedProject.Path);
        _logger.LogInformation(
            "Launch angefordert: Action '{Action}' (Command: {Command}) für Projekt '{Project}' in {Cwd}.",
            action.Name, action.Command, SelectedProject.Name, workingDirectory);

        try
        {
            _launcher.Launch(action, workingDirectory);
            return true;
        }
        catch (ProcessLaunchException ex)
        {
            _logger.LogError(ex,
                "Launch von Action '{Action}' für Projekt '{Project}' fehlgeschlagen.",
                action.Name, SelectedProject.Name);
            StatusMessage = ex.Message;
            return false;
        }
    }
}
