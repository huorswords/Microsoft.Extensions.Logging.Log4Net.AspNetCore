namespace Sample.Windows.Service.Net80;

internal class Worker : BackgroundService
{
    private readonly WeatherService _weatherService;
    private readonly ILogger<Worker> _logger;

    public Worker(WeatherService weatherService, ILogger<Worker> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting worker");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var forecast = _weatherService.GetWeatherForecast();
                _logger.LogWarning("{forecast}", forecast.First().Summary);

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }
}
