namespace Services.SnakeTestService;

public interface ISnakeTestService
{
    public Task RunTests(Guid id, string conId);
}
