using Xunit;
using SimEngine.Nodes;

public class NodeTests
{
    private class DummyNode : Node
    {
        public DummyNode(string name) : base(name) { }
    }

    [Theory]
    [InlineData("Main.Sub.Queue", "Main.Sub")]
    [InlineData("SoloNode", "SoloNode")]
    public void NodeConstructor_SetsNetworkCorrectly(string input, string expectedNetwork)
    {
        var node = new DummyNode(input);
        Assert.Equal(expectedNetwork, node.Network);
    }

    [Fact]
    public void Node_ShouldHaveNullDefaults()
    {
        var node = new DummyNode("TestNet.Node");
        Assert.Null(node.NextNode);
        Assert.Null(node.NextNodeChoices);
    }
}