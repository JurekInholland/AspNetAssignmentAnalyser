namespace Services;

public class AppConfig
{
    public string SendGridApiKey { get; set; } = "";
    public string SendGridFromEmail { get; set; } = "";
    public string SendGridToEmail { get; set; } = "";
    public string UserHeaderKey { get; set; } = "";
}
