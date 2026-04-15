# Microsoft.Extensions.Logging.Log4Net.AspNetCore

## Modifying logging behaviour
The logging behaviour of the provider can be modified with configuration values and by providing custom implementations of certain procedures in the logging process. 

## Changing the log level
The logging provider will try to translate the log levels received by the Microsoft Extensions Logging handler to the best fitting equivalent in the log4net system. The translations are shown below:


| MS LogLevel   | log4net Level |
| ------------- |---------------|
| Critical      | Fatal         |
| Error         | Error         |
| Warning       | Warn          |
| Information   | Info          |
| Debug         | Debug         |
| Trace         | Trace         |

If the default values don't work for you log level translation can be influenced to fit your needs.

### Overriding LogLevel.Critical
Looking at the mapping above you might notice that one mapping sticks out: `LogLevel.Critical` is mapped to `Level.Fatal` instead of `Level.Critical`. Because of the different names, this might not make sense at first. But it's very reasonable, if you already know that log4net considers `DEBUG`, `INFO`, `WARN`, `ERROR` and `FATAL` to be the 5 default levels to log at. 

Nevertheless it can be very confusing if the log level names in your code and in your logs don't match up. Because of this the provider has an easy way to override this behaviour. Setting the configuration value `Log4NetProviderOptions.OverrideCriticalLevelWith` to `"Critical"` will change cause the provider to translate `LogLevel.Critical` to `Level.Critical`.

```csharp
loggerFactory.AddLog4Net(new Log4NetProviderOptions {
    OverrideCriticalLevelWith = "Critical",
});
```
 
 > ⚠️ `OverrideCriticalLevelWith` will only accept `"Critical"` as a value. All other values will result in the default behaviour.

### Overriding all log level translation
If you need even more customization you can override the whole translation process to fit your needs. Log level translation for the logger is provided by implementations of the interface `ILog4NetLogLevelTranslator`.

```csharp
public interface ILog4NetLogLevelTranslator
{
    Level TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options);
}
```

Let's say instead of `Level.Critical` we want `LogLevel.Critical` to translate to our own custom `Level` and instead of `Level.Trace` we want to map to `Level.Debug`. We would start by implementing the interface with our custom mapping:

```csharp
public class CustomLogLevelTranslator : ILog4NetLogLevelTranslator
{
    public Level TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options) {
        return logLevel switch {
            LogLevel.Critical    => new Level(100000, "MyCustomLevel"),
            LogLevel.Error       => Level.Error,
            LogLevel.Warning     => Level.Warn,
            LogLevel.Information => Level.Info,
            LogLevel.Debug       => Level.Debug,
            LogLevel.Trace       => Level.Debug,
            _ => null
        };
    }
}
```

Afterwards we can configure the provider to use this mapping by specifying it `Log4NetProviderOptions.LogLevelTranslator`.

```csharp
loggerFactory.AddLog4Net(new Log4NetProviderOptions {
    LogLevelTranslator = new CustomLogLevelTranslator(),
});
```


## Changing LogEvent creation
Further modifikation of the logging behaviour is possible by changing the creation of log4net LogEvents. This is possible by implementing the interface `ILog4NetLoggingEventFactory`:

```csharp
public interface ILog4NetLoggingEventFactory
{
    LoggingEvent CreateLoggingEvent<TState>(
        MessageCandidate<TState> messageCandidate,
        ILogger logger,
        Log4NetProviderOptions options
    );
}
```

It can for example be used to extract the state information passed to the logging provider and include it in the message object:

```csharp
public class CustomLoggingEventFactory : ILog4NetLoggingEventFactory
{
    LoggingEvent CreateLoggingEvent<TState>(
        MessageCandidate<TState> messageCandidate,
        ILogger logger,
        Log4NetProviderOptions options
    ) 
    {
        // Define the boundary where the logger cuts of stack traces
        // (this is the default)
        Type callerStackBoundaryDeclaringType = typeof(LoggerExtensions);

        // Use the provided formatter to create the log message
        string message = messageCandidate.Formatter(
            messageCandidate.State,
            messageCandidate.Exception
        );
        
        // The log level translator is used even before this factory is called
        // to block log levels that are not activated, so we should reuse the same
        // log levels or log levels with higher severity.
        Level logLevel = options.LogLevelTranslator.TranslateLogLevel(
            messageCandidate.LogLevel,
            options
        );

        // Log level was not translated or is LogLevel.None so let's skip the message
        if (logLevel == null)
            return null;
        
        // If both things are missing we have nothing to log so let's skip the message
        if (string.IsNullOrEmpty(message) && messageCandidate.Exception == null)
            return null;

        // State is always passed in as this type so let's cast to it (as of 2021/09)
        if (messageCandidate.State is IReadOnlyCollection<KeyValuePair<string, object>> stateProperties)
        {
            return new LoggingEvent(
                callerStackBoundaryDeclaringType: callerStackBoundaryDeclaringType,
                repository: logger.Repository,
                loggerName: logger.Name,
                level: logLevel,
                message: new {
                    State = stateProperties.ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value
                    ),
                    Message = message,
                },
                exception: messageCandidate.Exception);
        }

        // Fallback if some other type of state is passed in
        return new LoggingEvent(
            callerStackBoundaryDeclaringType: callerStackBoundaryDeclaringType,
            repository: logger.Repository,
            loggerName: logger.Name,
            level: logLevel,
            message: message,
            exception: messageCandidate.Exception);
    }
}
```

It is also possible to inherit default implementation `LoggingEventFactory` and override `EnrichWithScope()` and/or `EnrichProperties` methods:

```csharp
public class CustomLoggingEventFactory : LoggingEventFactory
{
    protected override void EnrichProperties<TState>(LoggingEvent loggingEvent, in MessageCandidate<TState> messageCandidate)
    {
        base.EnrichProperties(loggingEvent, in messageCandidate);
        loggingEvent.Properties["ApplicationName"] = Assembly.GetEntryAssembly().GetName().Name;
        loggingEvent.Properties["ApplicationVersion"] = Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
```

Afterwards we can configure the provider to use this mapping by specifying it `Log4NetProviderOptions.LoggingEventFactory`.

```csharp
loggerFactory.AddLog4Net(new Log4NetProviderOptions {
    LoggingEventFactory = new CustomLoggingEventFactory(),
});
```
