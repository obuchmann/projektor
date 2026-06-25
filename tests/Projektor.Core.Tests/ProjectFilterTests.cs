using Projektor.Core.Models;
using Projektor.Core.Services;

namespace Projektor.Core.Tests;

public sealed class ProjectFilterTests
{
    private readonly ProjectFilter _filter = new();

    private static Project Project(string name) =>
        new(name, $"/projects/{name.ToLowerInvariant()}", ProjectSource.Manual, [], []);

    private static readonly IReadOnlyList<Project> Projects =
    [
        Project("Projektor"),
        Project("ApiGateway"),
        Project("FrontendApp"),
    ];

    [Fact]
    public void Filter_PrefixMatch_ReturnsMatch()
    {
        var result = _filter.Filter(Projects, "Proj");

        Assert.Single(result);
        Assert.Equal("Projektor", result[0].Name);
    }

    [Fact]
    public void Filter_SubstringMatch_ReturnsMatch()
    {
        var result = _filter.Filter(Projects, "end");

        Assert.Single(result);
        Assert.Equal("FrontendApp", result[0].Name);
    }

    [Fact]
    public void Filter_CaseInsensitive_ReturnsMatch()
    {
        var result = _filter.Filter(Projects, "apigateway");

        Assert.Single(result);
        Assert.Equal("ApiGateway", result[0].Name);
    }

    [Fact]
    public void Filter_EmptyQuery_ReturnsAllProjects()
    {
        var result = _filter.Filter(Projects, "");

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Filter_NoMatch_ReturnsEmptyList()
    {
        var result = _filter.Filter(Projects, "zzznomatch");

        Assert.Empty(result);
    }
}
