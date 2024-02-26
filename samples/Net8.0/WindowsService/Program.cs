using Sample.Windows.Service.Net80;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<WeatherService>();
        services.AddWindowsService(options => options.ServiceName = "Sample");
        services.AddHostedService<Worker>();
    })
    .UseWindowsService(options => options.ServiceName = "Sample")
    .ConfigureLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddLog4Net(log4NetConfigFile: "log4net.config");
        loggingBuilder.AddConsole();
    });

var host = builder.Build();
host.Run();
