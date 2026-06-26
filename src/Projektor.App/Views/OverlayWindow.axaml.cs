using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Projektor.App.ViewModels;

namespace Projektor.App.Views;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
        Deactivated += OnDeactivated;
    }

    private OverlayViewModel? ViewModel => DataContext as OverlayViewModel;

    /// <summary>Shows the overlay centered, focuses the search box and resets transient state.</summary>
    public void ShowOverlay()
    {
        ViewModel?.Reset();
        Show();
        Activate();
        var search = this.FindControl<TextBox>("SearchBox");
        search?.Focus();
        search?.SelectAll();
    }

    public void ToggleOverlay()
    {
        if (IsVisible)
            Hide();
        else
            ShowOverlay();
    }

    private void OnDeactivated(object? sender, System.EventArgs e)
    {
        // Click-away / focus loss hides the overlay (the process keeps living in the tray).
        if (IsVisible)
            Hide();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Hide();
                e.Handled = true;
                return;

            case Key.Enter:
                ViewModel?.LaunchCommand.Execute(null);
                e.Handled = true;
                return;

            case Key.Down when ReferenceEquals(FocusManager?.GetFocusedElement(), this.FindControl<TextBox>("SearchBox")):
                this.FindControl<ListBox>("ProjectList")?.Focus();
                e.Handled = true;
                return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // The tray app owns the lifetime; closing the window only hides it.
        if (!e.IsProgrammatic)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }
}
