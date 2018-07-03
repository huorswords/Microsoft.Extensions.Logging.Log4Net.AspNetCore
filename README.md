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

## Overwriting the native log4net xml configuration using the Net Core configuration system.

Sometimes we might want to modify the value of an appender, for example, the file name of our log. This might be interesting if we want to use a different name for each environment deployed. To do this, this package includes the possibility of overwriting the information of a node or the attributes of that node using the Net Core configuration system.

To do this, you will need to do the following:

1. Create a section within your `AppSettings.json` file:

  ```json
"Log4net": [
  {
    "XPath": "/log4net/appender[@name='RollingFile']/file",
    "Attributes": {
      "Value": "overrided.log"
      }
    },
    {
      "XPath": "/log4net/appender[@name='RollingFile']/maximumFileSize",
      "Attributes": {
        "Value": "1024KB"
      }
    }
]
```

  As you can see, the section is an array. For each element of the array, an `XPath` key must be included, which will contain the XPath expression to find the node from which we want to overwrite its information.

  The `Attributes` key will contain a list of all the attributes you want to overwrite. In our case, we will almost always add the attribute `value`, followed by the value we want that attribute to take.

  The `NodeContent` key will contain the text to be included inside the node, removing any information that was previously on the original node.

1. Change the call to `loggerFactory.AddLog4Net`. Add as the first parameter the name of your `log4net` configuration file, and specify, as the second parameter, an IConfigurationSection object containing the configuration section you added to your `AppSettings.json` file:

  ```csharp
loggerFactory.AddLog4Net("log4net.config", Configuration.GetSection("Log4net"));
  ```

This way, the package will iterate for each XPath contained in the array, will check if there are any nodes within the XML file that match the expression, and will overwrite the attributes or content of that node, depending on what you have specified in the configuration section.

## Special thanks

Thank you very much to all contributors & users by its collaboration, and specially to:

* [@twenzel](https://github.com/twenzel) by his great job on adapting the library to the new logging recomendations for .NET Core 2.
* [@sBoff](https://github.com/sBoff) by the fix of the mutiple calls to XmlConfigurator.Configure issue.
* [@kastwey](https://github.com/kastwey) by the feature to allow to replace values of log4net.config using the *Microsoft.Extensions.Configuration*.