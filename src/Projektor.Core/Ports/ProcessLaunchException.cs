namespace Projektor.Core.Ports;

/// <summary>
/// Raised by <see cref="IProcessLauncher"/> when an action's process cannot be started
/// (e.g. executable not found, invalid working directory). Carries a user-facing message.
/// </summary>
public sealed class ProcessLaunchException : Exception
{
    public ProcessLaunchException(string message) : base(message) { }
    public ProcessLaunchException(string message, Exception innerException) : base(message, innerException) { }
}
