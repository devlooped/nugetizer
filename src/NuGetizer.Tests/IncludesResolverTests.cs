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
}
