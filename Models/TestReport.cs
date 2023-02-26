namespace Models;

public class TestReport
{
    public Guid Id { get; set; }
    public string Name => $"Test Report {Id} - {DateTime.Now:dd.MM.yy HH:mm:ss}";
    public int StudentId { get; set; }

    public IEnumerable<TestResult> Results { get; set; }
}
