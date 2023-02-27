namespace Models;

public class TestReport
{
    public Guid Id { get; set; }
    public DateTime SubmissionTime { get; set; }

    public string StudentEmail { get; set; } = "";

    public IEnumerable<TestResult> Results { get; set; } = null!;

    public string Name => $"Test Report - {StudentEmail.Split("@")[0]} : {Score}%";
    public int Score => (int) Math.Round(Results.Count(x => x.Passed) / (float) Results.ToList().Count * 100, 0);
}
