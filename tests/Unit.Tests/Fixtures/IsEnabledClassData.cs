using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using Unit.Tests.Models;

namespace Unit.Tests.Fixtures
{
    internal class IsEnabledClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Log4NetFileOption.FatalOrHigher, LogLevel.Critical, true };
            yield return new object[] { Log4NetFileOption.FatalOrHigher, LogLevel.Warning, false };
            yield return new object[] { Log4NetFileOption.FatalOrHigher, LogLevel.Trace, false };
            yield return new object[] { Log4NetFileOption.DebugOrHigher, LogLevel.Information, true };
            yield return new object[] { Log4NetFileOption.DebugOrHigher, LogLevel.Debug, true };
            yield return new object[] { Log4NetFileOption.DebugOrHigher, LogLevel.Trace, false };
            yield return new object[] { Log4NetFileOption.TraceOrHigher, LogLevel.Trace, true };
            yield return new object[] { Log4NetFileOption.All, LogLevel.Trace, false };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}