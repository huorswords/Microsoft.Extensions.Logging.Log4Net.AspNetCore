# Microsoft.Extensions.Logging.Log4Net.AspNetCore

Allows to configure Log4net as Microsoft Extensions Logging handler on any ASP.NET Core application.

Thanks to [@anuraj](https://github.com/anuraj) for this [original blog post](https://dotnetthoughts.net/how-to-use-log4net-with-aspnetcore-for-logging/).

![Deployment](https://swords.vsrm.visualstudio.com/_apis/public/Release/badge/703fb931-72f4-4d54-9c93-d313144cc22a/1/1)
[![NuGet](https://img.shields.io/nuget/dt/Microsoft.Extensions.Logging.Log4net.AspNetCore.svg)](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Log4Net.AspNetCore/)

## Example of use

* Install the package or reference the project into your asp.net core application.

* Add the `AddLog4Net()` call into your `Configure` method of the `Startup` class.

```csharp
using Microsoft.Extensions.Logging;

public class Startup
{
    //...

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        //...

        loggerFactory.AddLog4Net(); // << Add this line
        app.UseMvc();

        //...
    }
}
```

* Add a `log4net.config` file with the content:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<log4net>
  <appender name="DebugAppender" type="log4net.Appender.DebugAppender" >
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>
  <root>
    <level value="ALL"/>
    <appender-ref ref="DebugAppender" />
  </root>
</log4net>
```

You can found more configuration examples on [configuration documentation](/doc/CONFIG.md).

## FAQ

### Using BeginScope

> Associated issues #45

From version 2.2.7, this nuget allow to use the `BeginScope` method from `Log4NetLogger`.

```csharp
var dictionary = new Dictionary<string, string>() { { "test", "SCOPED_VALUE" } };
using (var scope = logger.BeginScope(dictionary))
{
    logger.LogCritical(message);
}
```

The `BeginScope` method allow any object, but only some of types are handled in an special way. Those types are:

* `string`
* `IEnumerable<KeyValuePair<string, string>>`
* `IEnumerable<KeyValuePair<string, object>>`

By default, any other type will be managed as a conventional `object`.

#### How it works

When you use any of the `IEnumerable<KeyValuePair<,>>` allowed types, the collection will be processed and every item from this collection should be introduced to the `LogicalThreadContext` managed by the log4net library using the `Key` as the property name and the `Value` as the value to replace the placeholder on the Pattern Layout defined.

This example shows how the log4net.config pattern layout could include a `%property{}` placeholder that will be matched within the corresponding scoped value from the collection argument.

```xml
<layout type="log4net.Layout.PatternLayout">
    <!-- Print the date in ISO 8601 format -->
    <conversionPattern value="%date [%thread] %-5level %logger %ndc - scope=%property{scope} - %property{custom_name} - %message%newline" />
</layout>
```

When you use the `BeginScope` method passing a collection that contains a KeyValuePair within the key `custom_name`, the logged message will contain the `SCOPED_VALUE` text on it.

```csharp
var dictionary = new Dictionary<string, string>() { { "custom_name", "SCOPED_VALUE" } };
using (var scope = logger.BeginScope(dictionary))
{
    logger.LogCritical(message); // SCOPED_VALUE will replace the %property{custom_name} placeholder
}
```

At the other hand, if the argument is not from the given `IEnumerable<KeyValuePair<,>>`, the value will be logged on the default `scope` property.

```csharp
using (var scope = logger.BeginScope("SCOPED_VALUE"))
{
    logger.LogCritical(message); // SCOPED_VALUE will replace the %property{scope} placeholder
}

using (var scope = logger.BeginScope(Guid.NewGuid()))
{
    logger.LogCritical(message); // Guid value will replace the %property{scope} placeholder
}
```

And, when you use two chained `BeginScope` calls...

```csharp
using (var scope = logger.BeginScope("SCOPED_VALUE"))
{
    using (var scope = logger.BeginScope(Guid.NewGuid()))
    {
        logger.LogCritical(message); // SCOPED_VALUE and Guid value (both) will replace the %property{scope} placeholder
    }
}
```

For additional information about how the `LogicalThreadContext` works, please visit the [official documentation](https://logging.apache.org/log4net/release/manual/contexts.html)

### .NET Core 2.0 - Logging debug level messages

> Associated issues #34 & #41

In order to be able to register Debug level messages in any of your configured log4net appenders, you should change the ASP .NET Core 2 configuration when you build your `IWebHost` instance as follows.

```csharp
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
           .UseStartup<Startup>()
           .ConfigureLogging((hostingContext, logging) =>
            {
              // The ILoggingBuilder minimum level determines the
              // the lowest possible level for logging. The log4net
              // level then sets the level that we actually log at.
              logging.AddLog4Net();
              logging.SetMinimumLevel(LogLevel.Debug);
            })
            .Build();
```

## Special thanks

Thank you very much to all contributors & users by its collaboration, and specially to:

* [@twenzel](https://github.com/twenzel) by his great job on adapting the library to the new logging recomendations for .NET Core 2.
* [@sBoff](https://github.com/sBoff) by the fix of the mutiple calls to XmlConfigurator.Configure issue.
* [@kastwey](https://github.com/kastwey) by the feature to allow to replace values of log4net.config using the *Microsoft.Extensions.Configuration*.