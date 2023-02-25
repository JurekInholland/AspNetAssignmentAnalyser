namespace Models;

public class TestReport
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public IEnumerable<TestResult> Results { get; set; }
}
