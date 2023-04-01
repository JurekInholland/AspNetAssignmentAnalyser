using Microsoft.Extensions.Logging;
using Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Services.SnakeTestService;

public class SnakeTestService : ISnakeTestService
{
    private readonly ILogger<SnakeTestService> _logger;
    private readonly SubmissionHub _hub;

    private IWebDriver _driver = null!;
    private IJavaScriptExecutor _js = null!;
    private IWebElement _body = null!;
    private WebDriverWait _wait = null!;

    private readonly ChromeOptions _options = new();
    private readonly Func<bool>[] _testMethods;
    private long _cellSize;

    public SnakeTestService(ILogger<SnakeTestService> logger, SubmissionHub hub)
    {
        _logger = logger;
        _hub = hub;
        _testMethods = new[]
        {
            IsCanvasSizeIncreased, IsSnakeMovementSpeedDecreased, SnakeHandlesUpKey, SnakeHandlesDownKey, SnakeHandlesLeftKey,
            SnakeHandlesRightKey, IsEdgeCollisionImplemented, IsScoreIncreasedByApple, SpeedCanIncrease, IsSnakeColorRandomized,
            IsGoldenAppleImplemented,
        };

        _options.AddArgument("--no-sandbox");
        _options.AddArgument("--headless");
        _options.AddArgument("--disable-gpu");
        _options.AddArgument("--disable-dev-shm-usage");
        _options.AddArgument("--window-size=1024x768");
        _options.AddArgument("--log-level=3");

        new DriverManager().SetUpDriver(new ChromeConfig());
    }


    public async Task<TestReport> RunTests(Guid id, string connectionId)
    {
        _driver = new ChromeDriver(_options);
        _js = (IJavaScriptExecutor) _driver;
        _wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(15));

        var url = GetFileUrl(id);
        _logger.LogInformation("Opening {Url}", url);
        _driver.Navigate().GoToUrl(url);
        _body = _driver.FindElement(By.XPath("html/body"));

        _wait.Until(d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete"));

        _cellSize = (long) (ExecuteScript("return CELL_SIZE") ?? 0);

        if (_cellSize == 0)
        {
            await _hub.SendStatus(connectionId, "CELL_SIZE is not defined");
            throw new Exception("CELL_SIZE is not defined");
        }

        List<TestResult> results = new();
        for (int i = 0; i < _testMethods.Length; i++)
        {
            var methodName = _testMethods[i].Method.Name;

            Thread.Sleep(200);
            var passed = _testMethods[i]();
            var testResult = new TestResult(i + 1, passed);
            results.Add(testResult);

            _logger.LogInformation("Test {I} - {MethodName} [{Passed}]", i + 1, methodName, passed);
            await _hub.SendTestResult(connectionId, testResult);
        }

        _driver.Close();
        _driver.Quit();
        return new TestReport
        {
            Id = id,
            SubmissionTime = DateTime.Now,
            Results = results,
        };
    }

    private string GetFileUrl(Guid id)
    {
        var path = Path.GetFullPath(Path.Combine("upload", id.ToString()));
        var filePath = Path.Combine(path, "index.html");
        var url = "file://" + filePath;
        return url;
    }

    private object? ExecuteScript(string script, params object[] args)
    {
        try
        {
            return _js.ExecuteScript(script, args);
        }
        catch (Exception exception) when (exception is JavaScriptException or InvalidCastException)
        {
            _logger.LogError("Error executing script: {Exception}", exception);
            return null;
        }
    }

    private void TriggerKey(string key)
    {
        Thread.Sleep(10);
        _body.SendKeys(key);
    }

    private bool ConfirmSnakeDirection(long x, long y)
    {
        return ExecuteScript("return snake.dx") as long? == x &&
               ExecuteScript("return snake.dy") as long? == y;
    }

    private void RestartGame()
    {
        ExecuteScript("Window.Game.Reset()");
    }

    private bool IsCanvasSizeIncreased()
    {
        return ExecuteScript("return CANVAS_SIZE") is long and > 5;
    }

    private bool IsSnakeMovementSpeedDecreased()
    {
        try
        {
            return ExecuteScript("return Window.Game.getFps()") is long and < 30 and > 0;
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    private bool IsEdgeCollisionImplemented()
    {
        ExecuteScript("Window.Game.Reset();");
        Thread.Sleep(50);
        var appleX = ExecuteScript("return apple.x") as long?;
        var appleY = ExecuteScript("return apple.y") as long?;


        TriggerKey(Keys.Up);
        Thread.Sleep(50);
        var newAppleX = ExecuteScript("return apple.x") as long?;
        var newAppleY = ExecuteScript("return apple.y") as long?;

        return appleX != newAppleX || appleY != newAppleY;
    }

    private bool IsScoreIncreasedByApple()
    {
        ExecuteScript("Window.Game.Reset();");
        Thread.Sleep(50);

        var score = ExecuteScript("return Window.Game.getScore()") as long?;

        ExecuteScript("Window.Game.eatApple();");
        Thread.Sleep(50);

        var finalScore = ExecuteScript("return Window.Game.getScore()") as long?;

        return finalScore > score && finalScore > 0;
    }

    private bool IsSnakeColorRandomized()
    {
        ExecuteScript("Window.Game.Reset();");
        Thread.Sleep(1);
        var startColor = ExecuteScript("return snake.color") as string;


        ExecuteScript("Window.Game.eatApple();");
        Thread.Sleep(1);
        var secondColor = ExecuteScript("return snake.color") as string;

        ExecuteScript("Window.Game.eatApple();");
        Thread.Sleep(1);
        var thirdColor = ExecuteScript("return snake.color") as string;

        return startColor != secondColor && secondColor != thirdColor;
    }


    private bool IsGoldenAppleImplemented()
    {
        ExecuteScript("Window.Game.Reset();");

        var appleColor = ExecuteScript("return apple.color") as string;

        ExecuteScript("Window.Game.eatApple();");
        var pointRewards = ExecuteScript("return Window.Game.getScore()") as long?;

        for (int i = 0; i < 200; i++)
        {
            Thread.Sleep(1);
            ExecuteScript("Window.Game.eatApple();");
            if (ExecuteScript("return apple.color") as string != appleColor)
            {
                Thread.Sleep(1);
                var scoreBefore = ExecuteScript("return Window.Game.getScore()") as long?;
                ExecuteScript("Window.Game.eatApple();");
                Thread.Sleep(1);
                var scoreAfter = ExecuteScript("return Window.Game.getScore()") as long?;

                // Return true if the score increase of eating the golden apple is different from the initial score reward.
                // I explicitly check for unequal instead of greater than for the slight chance that the very first apple is golden.
                if (scoreAfter - scoreBefore != pointRewards)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool SpeedCanIncrease()
    {
        ExecuteScript("Window.Game.Reset();");
        Thread.Sleep(1);

        var startFps = ExecuteScript("return Window.Game.getFps()") as long?;

        for (int i = 0; i < 25; i++)
        {
            Thread.Sleep(1);
            ExecuteScript("Window.Game.eatApple();");

            if (ExecuteScript("return Window.Game.getFps()") as long? > startFps)
            {
                return true;
            }
        }

        return false;
    }

    private bool SnakeHandlesLeftKey()
    {
        RestartGame();

        // snakes cannot do u-turns :(
        ExecuteScript("snake.x = 100; snake.dx = 0; snake.dy = 20;");

        TriggerKey(Keys.Left);

        return ConfirmSnakeDirection(-_cellSize, 0);
    }

    private bool SnakeHandlesRightKey()
    {
        RestartGame();
        ExecuteScript("snake.dx = 0; snake.dy = 20;");
        TriggerKey(Keys.Right);
        return ConfirmSnakeDirection(_cellSize, 0);
    }

    private bool SnakeHandlesUpKey()
    {
        RestartGame();

        // snake starts at y = 0, so move it down first to avoid certain instant death.
        ExecuteScript(" snake.dx=20;snake.dy=0; snake.y = 300; snake.x = 0;");
        TriggerKey(Keys.Up);
        return ConfirmSnakeDirection(0, -_cellSize);
    }

    private bool SnakeHandlesDownKey()
    {
        RestartGame();
        TriggerKey(Keys.Down);
        return ConfirmSnakeDirection(0, _cellSize);
    }
}
