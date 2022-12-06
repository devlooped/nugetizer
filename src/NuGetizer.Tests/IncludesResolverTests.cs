using System.IO;
using Xunit;

namespace NuGetizer;

public class IncludesResolverTests
{
    [Fact]
    public void ResolveIncludes()
    {
        var content = IncludesResolver.Process("Content/readme.md");

        Assert.Contains("the-header", content);
        Assert.Contains("the-footer", content);

        Assert.Contains("section#1", content);
        Assert.DoesNotContain("section#2", content);
        Assert.Contains("section#3", content);
        Assert.Contains("@kzu", content);
    }

    [Fact]
    public void ResolveUrlInclude()
    {
        var content = IncludesResolver.Process("Content/url.md");

        Assert.Contains("Daniel Cazzulino", content);
        Assert.Contains("Sponsors", content);
    }

    [Fact]
    public void ResolveNonExistingInclude()
    {
        var path = Path.GetTempFileName();
        var include = "<!-- include foo.md#bar -->";
        File.WriteAllText(path, include);

        string failed = default;
        var content = IncludesResolver.Process(path, s => failed = s);

        Assert.NotNull(failed);
        Assert.Contains("foo.md#bar", failed);
    }
}
