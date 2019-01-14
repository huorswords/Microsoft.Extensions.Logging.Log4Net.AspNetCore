# Microsoft.Extensions.Logging.Log4Net.AspNetCore

## Add the log4net.config file

 Here you will found an example of how your `log4net.config` file should look like.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<log4net>
  <appender name="DebugAppender" type="log4net.Appender.DebugAppender" >
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>
  <appender name="RollingFile" type="log4net.Appender.RollingFileAppender">
    <file value="example.log" />
     <appendToFile value="true" />
    <maximumFileSize value="100KB" />
    <maxSizeRollBackups value="2" />
    <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%date %5level %logger.%method [%line] - MESSAGE: %message%newline %exception" />
    </layout>
  </appender>
  <root>
    <level value="ALL"/>
    <appender-ref ref="DebugAppender" />
    <appender-ref ref="RollingFile" />
  </root>
</log4net>
```

For more information about log4net configuration files, please take a look into the [configuration files section on the oficial documentation for Log4Net](https://logging.apache.org/log4net/release/manual/configuration.html)

## Basic configuration

* Install the package or reference the project into your ASP .Net Core application.
* Add the `AddLog4Net()` call into your `Configure` method of the `Startup` class.

```csharp
using Microsoft.Extensions.Logging;

public class Startup
{
    //...

    public void Configure(
        IApplicationBuilder app,
        IHostingEnvironment env,
        ILoggerFactory loggerFactory)
    {
        //...

        loggerFactory.AddLog4Net(); // << Add this line
        app.UseMvc();

        //...
    }
}
```

## Custom configuration using `Log4NetProviderOptions`

* Install the package or reference the project into your ASP .Net Core 2.x application.
* Add a new `Log4NetCore` section on your `appsettings.json` file.

```json
{
    "Log4NetCore": {
        "Name": "Test",
        "LoggerRepository": "Fantastic",
        "OverrideCriticalLevelWith": "Fatal",
        "Watch": false,
        "PropertyOverrides": [
            {
                "XPath": "/log4net/appender[@name='RollingFile']/file",
                "Attributes": {
                    "Value": "overridedFileName.log"
                }
            },
            {
                "XPath": "/log4net/appender[@name='RollingFile']/maximumFileSize",
                "Attributes": {
                    "Value": "200KB"
                }
            },
            {
                "XPath": "/log4net/appender[@name='RollingFile']/file"
            }
        ]
    }
}
```

* Reference the `appsettings.json` file into your configuration building at your `Program` class (note: you should require to install  **Microsoft.Extensions.Configuration.Json* package)

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
        => CreateWebHostBuilder(args).Build().Run();

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        return WebHost.CreateDefaultBuilder(args)
            .UseConfiguration(config)
            .UseStartup<Startup>();
    }
}
```

* Add the `AddLog4Net()` call into your `Configure` method of the `Startup` class.

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Add these lines
        var loggingOptions = this.Configuration.GetSection("Log4NetCore")
                                               .Get<Log4NetProviderOptions>();
        loggerFactory.AddLog4Net(loggingOptions);

        app.UseMvc();
    }
}
```

### Overwriting the native log4net XML configuration using `Log4NetProviderOptions`

Sometimes we might want to modify the value of an appender, for example, the file name of our log. This might be interesting if we want to use a different name for each environment deployed. To do this, this package includes the possibility of overwriting the information of a node or the attributes of that node using the Net Core configuration system.

To do this, you will need to do the following:

* Add a property on your log4net configuration section named `PropertyOverrides`. Here, you have got an example:

```json
{
    "Log4NetCore": {
        "Name": "Test",
        "LoggerRepository": "Fantastic",
        "OverrideCriticalLevelWith": "Fatal",
        "Watch": false,
        "PropertyOverrides": [
            {
                "XPath": "/log4net/appender[@name='RollingFile']/file",
                "Attributes": {
                    "Value": "overridedFileName.log"
                }
            },
            {
                "XPath": "/log4net/appender[@name='RollingFile']/maximumFileSize",
                "Attributes": {
                    "Value": "200KB"
                }
            },
            {
                "XPath": "/log4net/appender[@name='RollingFile']/file"
            }
        ]
    }
}
```

As you can see, the property is an array, where you can define overri elements.

Each element on this array can be defined as an object with `XPath` property, which will contain the XPath expression to find the node on log4net configuration file from which we want to overwrite its information, and `Attributes` property, that should contain a list of all the attributes you want to overwrite. In our case, we will almost always add the attribute `value`, followed by the value we want that attribute to take.

The `NodeContent` property will contain the text to be included inside the node, removing any information that was previously on the original node.