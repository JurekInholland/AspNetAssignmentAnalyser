using Models;

namespace Services.SnakeTestService;

public interface ISnakeTestService
{
    public Task<TestReport> RunTests(Guid id, string conId);
}
