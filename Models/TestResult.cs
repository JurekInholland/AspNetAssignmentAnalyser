namespace Models;

public class TestResult
{
    public int Number { get; set; }
    public string Name { get; set; }
    public bool Passed { get; set; }

    public TestResult(int number, bool passed)
    {
        Number = number;
        Name = TestNames.GetName(number);
        Passed = passed;
    }
}
