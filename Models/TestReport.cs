namespace Models;

public class TestReport
{
    public Guid Id { get; set; }
    public string Name => $"Test Report - {DateTime.Now:dd.MM.yy HH:mm:ss}";
    public string StudentEmail { get; set; } = "";
    public int StudentId { get; set; }
    public float Score => Results.Count(x => x.Passed) / (float) Results.ToList().Count * 100;
    public IEnumerable<TestResult> Results { get; set; } = null!;
}
