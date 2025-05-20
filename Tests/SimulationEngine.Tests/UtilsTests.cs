
using Xunit;
using SimEngine.Utils;

namespace SimEngine.Tests;

public class UtilsTests
{
    [Theory]
    [InlineData("Mall", "Mall", true)]
    [InlineData("Mall", "Mall.Shop", true)]
    [InlineData("Mall", "Mall.Shop.Pizza", true)]
    [InlineData("Mall.Shop", "Mall.Shop.Pizza", true)]
    [InlineData("Mall.Shop", "Mall", false)]
    [InlineData("Mall", "Shop", false)]
    public void IsSubnetwork_WorksAsExpected(string parent, string child, bool expected)
    {
        Assert.Equal(expected, NetworkUtils.IsSubnetwork(parent, child));
    }

    [Theory]
    [InlineData("Mall", "Mall", false)]
    [InlineData("Mall", "Mall.Shop", true)]
    [InlineData("Mall.Shop", "Mall.Shop.Pizza", true)]
    [InlineData("Mall.Shop", "Mall", false)]
    public void IsParentNetwork_WorksAsExpected(string parent, string child, bool expected)
    {
        Assert.Equal(expected, NetworkUtils.IsParentNetwork(parent, child));
    }
}
