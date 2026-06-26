using Projektor.Core.Models;

namespace Projektor.Core.Services;

public sealed class ProjectFilter
{
    /// <summary>
    /// Filters projects by query. Empty query returns all. Matching is case-insensitive Contains.
    /// </summary>
    public IReadOnlyList<Project> Filter(IEnumerable<Project> projects, string query)
    {
        ArgumentNullException.ThrowIfNull(projects);

        if (string.IsNullOrWhiteSpace(query))
            return projects.ToList();

        return projects
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
