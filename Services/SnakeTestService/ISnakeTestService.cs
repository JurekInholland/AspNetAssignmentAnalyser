namespace Services.SnakeTestService;

public interface ISnakeTestService
{
    public Task RunTests(string path, string conId);
}
