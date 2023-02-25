using Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace Services.SnakeTestService;

public class SnakeTestService : ISnakeTestService
{
    private readonly IWebDriver _driver = null!;
    private readonly IJavaScriptExecutor _js = null!;
    private IWebElement _body = null!;
    private readonly WebDriverWait _wait = null!;


    private long _cellSize;
    private readonly SubmissionHub _hub;

    public List<TestResult> Results { get; set; }


    private readonly ChromeOptions _options = new();

    public SnakeTestService(SubmissionHub hub)
    {
        _hub = hub;
        Results = new();

        _options.AddArgument("--headless");
        _options.AddArgument("--no-sandbox");
        _options.AddArgument("--disable-gpu");
        _options.AddArgument("--disable-dev-shm-usage");
        _options.AddArgument("--window-size=1024x768");

        new DriverManager().SetUpDriver(new ChromeConfig());
        _driver = new ChromeDriver(_options);


        _js = (IJavaScriptExecutor) _driver;
        _wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(15));
    }


    public async Task RunTests(Guid id, string connectionId)
    {
        // var indexPath = Path.Combine(path, "index.html");

        var relativePath = Path.Combine("upload", id.ToString(), "index.html");
        var url = Path.Combine("file://", Path.GetFullPath(relativePath));
        // Guid id = Guid.NewGuid();

        // var url = Path.Combine("file://", "upload", id.ToString(), "index.html");


        // var url = "file://" + Path.GetFullPath(indexPath);
        // Console.WriteLine("CHROME DRIVER URL: " + url);

        _driver.Navigate().GoToUrl(url);
        _body = _driver.FindElement(By.XPath("html/body"));

        _wait.Until(d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete"));


        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(100);
            try
            {
                // var apple = _js.ExecuteScript("return window");
                // Console.WriteLine("AAAAAPPP: " + apple);
                // var test = ExecuteScript("return window.wrappedJSObject.CELL_SIZE");
                //
                // Console.WriteLine("TEST CELL SIZE " + test);

                _cellSize = (long) (ExecuteScript("return CELL_SIZE") ?? 0);
                Console.WriteLine("old cell size " + _cellSize);

                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("caught: " + e.Message);
            }
        }


        Results.Add(new TestResult(1, IsCanvasSizeIncreased()));
        await _hub.SendMessageToClient(connectionId, Results[^1].Number + Results[^1].Passed.ToString());
        Results.Add(new TestResult(2, IsSnakeMovementSpeedDecreased()));
        await _hub.SendMessageToClient(connectionId, Results[^1].Number + Results[^1].Passed.ToString());
        Results.Add(new TestResult(3, IsEdgeCollisionImplemented()));
        await _hub.SendMessageToClient(connectionId, Results[^1].Number + Results[^1].Passed.ToString());
        Results.Add(new TestResult(4, IsScoreIncreasedByApple()));
        await _hub.SendMessageToClient(connectionId, Results[^1].Number + Results[^1].Passed.ToString());
        Results.Add(new TestResult(5, IsSnakeColorRandomized()));
        await _hub.SendMessageToClient(connectionId, Results[^1].Number + Results[^1].Passed.ToString());
        Results.Add(new TestResult(7, SnakeHandlesRightKey()));
        await _hub.SendMessageToClient(connectionId, Results[^1].Number + Results[^1].Passed.ToString());

        Thread.Sleep(100);
        Results.Add(new TestResult(8, SnakeHandlesUpKey()));

        Thread.Sleep(100);
        Results.Add(new TestResult(9, SnakeHandlesDownKey()));

        Thread.Sleep(100);
        Results.Add(new TestResult(6, SnakeHandlesLeftKey()));
        Results.Add(new TestResult(10, IsGoldenAppleImplemented()));
        Results.Add(new TestResult(11, SpeedCanIncrease()));

        _driver.Close();
        _driver.Quit();
    }

    // private string GetIndexUrl(Guid id)
    // {
    //
    // }

    private object? ExecuteScript(string script, params object[] args)
    {
        try
        {
            return _js.ExecuteScript(script, args);
        }
        catch (JavaScriptException jsException)
        {
            Console.WriteLine("JS EXCEPTION: " + jsException.Message);
            return null;
        }
        catch (WebDriverException webDriverException)
        {
            Console.WriteLine("WEB DRIVER EXCEPTION: " + webDriverException.Message);
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
            return ExecuteScript("return Window.Game.getFps()") is long and < 30;
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    private bool IsEdgeCollisionImplemented()
    {
        ExecuteScript("Window.Game.Reset();");
        Thread.Sleep(1);
        ExecuteScript("Window.Game.eatApple();");
        Thread.Sleep(1);


        TriggerKey(Keys.Up);
        try
        {
            _wait.Until(d => ((IJavaScriptExecutor) d).ExecuteScript("return Window.Game.getScore()").ToString()!.Equals("0"));
            return true;
        }
        catch (WebDriverTimeoutException e)
        {
            Console.WriteLine("Edge collision not implemented " + e);
            return false;
        }
    }

    private bool IsScoreIncreasedByApple()
    {
        ExecuteScript("Window.Game.Reset();");
        Thread.Sleep(1);

        var score = ExecuteScript("return Window.Game.getScore()") as long?;

        ExecuteScript("Window.Game.eatApple();");
        Thread.Sleep(1);

        var finalScore = ExecuteScript("return Window.Game.getScore()") as long?;

        return finalScore > score;
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

        for (int i = 0; i < 100; i++)
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


        // snake starts at y = 0, so move it down first to avoid instant death.
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
