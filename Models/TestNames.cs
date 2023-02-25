namespace Models;

public static class TestNames
{
    private static readonly Dictionary<int, string> Names = new()
    {
        {1, "Canvas size has been increased"},
        {2, "Snake movement speed has been decreased"},
        {3, "Edge collision has been implemented"},
        {4, "Score is increased by eating apples"},
        {5, "Snake color is randomized"},
        {6, "Golden apple has been implemented"},

    };

    public static string GetName(int testNumber)
    {
        Names.TryGetValue(testNumber, out var testName);
        return testName ?? "Unknown test";
    }
}
