namespace Tests
{
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class Log4NetProviderExtensionsShould
    {
        /// <summary>
        /// 
        /// 
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void MyTestMethod()
        {
            var provider = new Log4NetProvider("log4net.config");

            var logger = provider.CreateLogger<Log4NetProviderExtensionsShould>();

            Assert.IsNotNull(logger);
        }
    }
}
