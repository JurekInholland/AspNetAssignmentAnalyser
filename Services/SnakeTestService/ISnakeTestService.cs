using Models;

namespace Services.SnakeTestService;

public interface ISnakeTestService
{
    public Task<TestReport> RunTests(string path, string conId);
}
