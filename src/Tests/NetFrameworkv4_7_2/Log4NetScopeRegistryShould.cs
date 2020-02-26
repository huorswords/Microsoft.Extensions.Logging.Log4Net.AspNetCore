using Microsoft.Extensions.Logging.Scope;
using Microsoft.Extensions.Logging.Scope.Registers;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace NetFrameworkv4_7_2.Tests
{
    public class Log4NetScopeRegistryShould
    {
        [Fact]
        public void Return_ExpectedTypeAndFunc_When_ItIsPreviouslyRegistered()
        {
            var expected = Substitute.For<Func<object, IEnumerable<IDisposable>>>();
            var sut = new Log4NetScopeRegistry();
            sut.SetRegister(typeof(string), expected);

            var result = sut.GetRegister(typeof(string));
            Assert.Equal(result, expected);
        }

        [Fact]
        public void Return_ExpectedRegister_When_ItIsPreviouslyRegistered()
        {
            var mock = Substitute.For<Log4NetScopedRegister>();
            mock.Type.Returns(typeof(string));

            var expected = mock;
            var sut = new Log4NetScopeRegistry();
            sut.SetRegister(expected);

            var result = sut.GetRegister(typeof(string));

            Assert.NotNull(result);
            Assert.Equal(result, mock.AddToScope);
        }

        [Fact]
        public void Return_ObjectRegister_When_NoAdditionalRegistersAreAdded()
        {
            var sut = new Log4NetScopeRegistry();

            var result = sut.GetRegister(typeof(string));
            Assert.NotNull(result);
            Assert.Equal(result.Method.Name, nameof(Log4NetObjectScopedRegister.AddToScope));
            Assert.Equal(result.Method.ReflectedType.FullName, typeof(Log4NetObjectScopedRegister).FullName);
        }
    }
}