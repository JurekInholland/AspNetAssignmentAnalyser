namespace Models;

public class StatusMessage
{
    public string Status { get; set; }
    public TestResult? TestResult { get; set; }
    public bool Success { get; set; }

    public StatusMessage(string status, bool success = true)
    {
        Status = status;
        Success = success;
    }

    public StatusMessage(TestResult testResult)
    {
        string resultText = testResult.Passed ? "passed" : "failed";
        Status = $"Test {testResult.Number} {resultText}";
        TestResult = testResult;
        Success = true; // Success is always true for TestResult status. TestResult.Passed may be false.
    }
}
