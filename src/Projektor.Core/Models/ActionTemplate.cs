namespace Projektor.Core.Models;

public sealed record ActionTemplate(
    string Id,
    string Name,
    string? CommandWindows,
    string? CommandLinux,
    string? Icon = null);
