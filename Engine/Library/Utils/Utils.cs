namespace SimEngine.Utils;

public static class NetworkUtils
{
    public static bool IsSubnetwork(string parent, string child)
    {
        return child == parent || child.StartsWith(parent + ".");
    }

    public static bool IsParentNetwork(string parent, string child)
    {
        return child != parent && child.StartsWith(parent + ".");
    }
}