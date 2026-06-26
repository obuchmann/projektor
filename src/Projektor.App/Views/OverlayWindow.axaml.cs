using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Projektor.App.ViewModels;

namespace Projektor.App.Views;

public partial class OverlayWindow : Window
{
    private bool _activating;

    public OverlayWindow()
    {
        InitializeComponent();
        Deactivated += OnDeactivated;
    }

    private OverlayViewModel? ViewModel => DataContext as OverlayViewModel;

    /// <summary>Shows the overlay centered, forces it to the foreground and focuses the search box.</summary>
    public void ShowOverlay()
    {
        _activating = true;
        ViewModel?.Reset();

        Show();
        Activate();
        WindowActivation.ForceForeground(this);

        // Focus must happen after the window is actually shown/rendered.
        Dispatcher.UIThread.Post(() =>
        {
            var search = this.FindControl<TextBox>("SearchBox");
            search?.Focus();
            search?.SelectAll();
            _activating = false;
        }, DispatcherPriority.Input);
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
        // Click-away / focus loss hides the overlay — but not during our own activation dance.
        if (IsVisible && !_activating)
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

            case Key.Enter when e.KeyModifiers.HasFlag(KeyModifiers.Shift):
                ViewModel?.LaunchAll();
                e.Handled = true;
                return;

            case Key.Enter:
                ViewModel?.LaunchFirst();
                e.Handled = true;
                return;
        }

        // Alt+1 … Alt+9 → launch the n-th action of the selected project.
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) && TryGetDigit(e.Key, out var number))
        {
            ViewModel?.LaunchByIndex(number);
            e.Handled = true;
            return;
        }

        // Down from the search box moves into the project list for arrow navigation.
        if (e.Key == Key.Down &&
            ReferenceEquals(FocusManager?.GetFocusedElement(), this.FindControl<TextBox>("SearchBox")))
        {
            this.FindControl<ListBox>("ProjectList")?.Focus();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private static bool TryGetDigit(Key key, out int number)
    {
        if (key is >= Key.D1 and <= Key.D9)
        {
            number = key - Key.D1 + 1;
            return true;
        }
        if (key is >= Key.NumPad1 and <= Key.NumPad9)
        {
            number = key - Key.NumPad1 + 1;
            return true;
        }
        number = 0;
        return false;
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
