namespace Sample.Windows.Service.Net80;

internal class WeatherService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    public WeatherForecast[] GetWeatherForecast()
    {
        var weatherForecasts = Enumerable.Range(1, 5)
                                         .Select(index => new WeatherForecast(DateTime.Now.AddDays(index),
                                                                              Random.Shared.Next(-20, 55),
                                                                              Summaries[Random.Shared.Next(Summaries.Length)]))
                                         .ToArray();

        _logger.LogCritical(new Exception("ExceptionText"), "This is {weather}", weatherForecasts.First().Summary);
        _logger.LogTrace("Weather forecast ready!");
        return weatherForecasts;
    }
}

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}