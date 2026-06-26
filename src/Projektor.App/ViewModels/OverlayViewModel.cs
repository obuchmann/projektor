using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Projektor.Core.Models;
using Projektor.Core.Ports;
using Projektor.Core.Services;
using Projektor.Infrastructure;

namespace Projektor.App.ViewModels;

/// <summary>
/// Drives the overlay: filter projects as the user types, resolve the selected project's
/// effective actions, and launch the chosen action in the project's working directory.
/// </summary>
public sealed partial class OverlayViewModel : ObservableObject
{
    private readonly ActionResolver _resolver;
    private readonly ProjectFilter _filter;
    private readonly IProcessLauncher _launcher;
    private ProjektorConfig _config = ProjektorConfig.Empty;

    public ObservableCollection<Project> Projects { get; } = [];
    public ObservableCollection<ProjectAction> Actions { get; } = [];

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private ProjectAction? _selectedAction;

    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>Raised after a successful launch so the host can hide the overlay.</summary>
    public event EventHandler? LaunchRequested;

    public OverlayViewModel(ActionResolver resolver, ProjectFilter filter, IProcessLauncher launcher)
    {
        _resolver = resolver;
        _filter = filter;
        _launcher = launcher;
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
        {
            SelectedAction = null;
            return;
        }

        // Resolve against an absolute path so {path} substitution and the working directory match.
        var expanded = SelectedProject with { Path = PathUtil.ExpandHome(SelectedProject.Path) };
        foreach (var action in _resolver.Resolve(_config, expanded))
            Actions.Add(action);

        SelectedAction = Actions.Count > 0 ? Actions[0] : null;
    }

    [RelayCommand]
    private void Launch(ProjectAction? action)
    {
        action ??= SelectedAction;
        var project = SelectedProject;
        if (action is null || project is null)
            return;

        try
        {
            _launcher.Launch(action, PathUtil.ExpandHome(project.Path));
            LaunchRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (ProcessLaunchException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
