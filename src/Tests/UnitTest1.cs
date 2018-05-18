using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Tests
{
    [TestClass]
    public class LoggerShould
    {
        [TestMethod]
        public void TestMethod1()
        {
            var provider = new Log4NetProvider("log4net.config");
            var logger = provider.CreateLogger("Test");

            logger.LogCritical("A message");

            Assert.Inconclusive();
        }

        [TestMethod]
        public void UsePatternLayoutOnExceptions()
        {
            var provider = new Log4NetProvider("log4net.config");
            var logger = provider.CreateLogger("Test");

            try
            {
                ThrowException();
            }
            catch (Exception ex)
            {
                logger.LogCritical(10, ex, "Catched message");
            }

            Assert.Inconclusive();
        }

        /// <summary>
        /// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">A message</exception>
        private static void ThrowException() => throw new InvalidOperationException("A message");

        private const string Log4NetConfigFileName = "log4net.config";

        [TestMethod]
        public void LogFileExistsWithFileName0()
        {
            const string fileName = "0.log";
            //Arrange
            DeleteIfExist(fileName);
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var provider = new Log4NetProvider(Log4NetConfigFileName, new Log4NetParams { FileName = fileName });
            var logger = provider.CreateLogger("Test");

            logger.LogCritical("A message");

            Assert.IsTrue(File.Exists(logFilePath));
            provider.Dispose();
        }

        [TestMethod]
        public void LogFileExistsWithFileName1()
        {
            const string fileName = "1.log";

            //Arrange
            DeleteIfExist(fileName);
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var provider = new Log4NetProvider(Log4NetConfigFileName, new Log4NetParams { FileName = fileName });
            var logger = provider.CreateLogger("Test");

            logger.LogCritical("A message");

            Assert.IsTrue(File.Exists(logFilePath));
            provider.Dispose();
        }

        [TestMethod]
        public void LogFileExistsWithFileName2_Watch()
        {
            const string fileName = "2.log";
            const string watchConfigName = "watch-" + Log4NetConfigFileName;

            //Arrange
            DeleteIfExist(watchConfigName);
            DeleteIfExist(fileName);
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var provider = new Log4NetProvider(Log4NetConfigFileName, new Log4NetParams { FileName = fileName, Watch = true });
            var logger = provider.CreateLogger("Test");

            logger.LogCritical("A message");

            Assert.IsTrue(File.Exists(watchConfigName));
            Assert.IsTrue(File.Exists(logFilePath));
            provider.Dispose();
        }

        private static void DeleteIfExist(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}