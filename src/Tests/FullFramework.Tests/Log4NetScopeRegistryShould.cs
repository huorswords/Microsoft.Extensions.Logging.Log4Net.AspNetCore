namespace FullFramework.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Extensions.Logging.Scope;
    using Microsoft.Extensions.Logging.Scope.Registers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    [TestClass]
    public class Log4NetScopeRegistryShould
    {
        private MockRepository mockRepository;

        [TestInitialize]
        public void Setup()
        {
            this.mockRepository = new MockRepository(MockBehavior.Default);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Return_ExpectedTypeAndFunc_When_ItIsPreviouslyRegistered()
        {
            var expected = this.mockRepository.Create<Func<object, IEnumerable<IDisposable>>>()
                                              .Object;
            var sut = new Log4NetScopeRegistry();
            sut.SetRegister(typeof(string), expected);

            var result = sut.GetRegister(typeof(string));
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void Return_ExpectedRegister_When_ItIsPreviouslyRegistered()
        {
            var mock = this.mockRepository.Create<Log4NetScopedRegister>();
            mock.Setup(x => x.Type).Returns(typeof(string));

            var expected = mock.Object;
            var sut = new Log4NetScopeRegistry();
            sut.SetRegister(expected);

            var result = sut.GetRegister(typeof(string));

            Assert.IsNotNull(result);
            Assert.AreEqual(result, mock.Object.AddToScope);
        }

        [TestMethod]
        public void Return_ObjectRegister_When_NoAdditionalRegistersAreAdded()
        {
            var sut = new Log4NetScopeRegistry();

            var result = sut.GetRegister(typeof(string));
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Method.Name, nameof(Log4NetObjectScopedRegister.AddToScope));
            Assert.AreEqual(result.Method.ReflectedType.FullName, typeof(Log4NetObjectScopedRegister).FullName);
        }
    }
}