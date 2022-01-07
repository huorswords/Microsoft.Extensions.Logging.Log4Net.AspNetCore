using log4net.Core;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;

namespace Unit.Tests.Fixtures
{
    internal class TestLogMethodData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { LogLevel.Critical, Level.Fatal };
            yield return new object[] { LogLevel.Error, Level.Error };
            yield return new object[] { LogLevel.Warning, Level.Warn };
            yield return new object[] { LogLevel.Information, Level.Info };
            yield return new object[] { LogLevel.Debug, Level.Debug };
            yield return new object[] { LogLevel.Trace, Level.Trace };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}