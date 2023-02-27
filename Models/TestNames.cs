namespace Models;

public static class TestNames
{
    private static readonly Dictionary<int, string> Names = new()
    {
        {1, "Canvas size has been increased"},
        {2, "Snake movement speed has been decreased"},
        {3, "Snake can move up"},
        {4, "Snake can move down"},
        {5, "Snake can move left"},
        {6, "Snake can move right"},
        {7, "Edge collision has been implemented"},
        {8, "Score is increased by eating apples"},
        {9, "Snake speed increases"},
        {10, "Snake color is randomized"},
        {11, "Golden apple has been implemented"},
    };

    public static string GetName(int testNumber)
    {
        Names.TryGetValue(testNumber, out var testName);
        return testName ?? "Unknown test";
    }
}
