using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Unit.Tests.Target.Netcore31
{
    public class Log4NetProviderOptionsTests
    {
        [Fact]
        public void Ctor_Should_SetDefaultOptions()
        {
            var sut = new Log4NetProviderOptions();

            sut.ExternalConfigurationSetup.Should().BeFalse();
            sut.Log4NetConfigFileName.Should().Be("log4net.config");
            sut.LoggerRepository.Should().BeNull();
            sut.Name.Should().BeEmpty();
            sut.OverrideCriticalLevelWith.Should().Be("");
            sut.PropertyOverrides.Should().BeEmpty();
            sut.ScopeFactory.Should().BeNull();
            sut.Watch.Should().BeFalse();
        }

        [Fact]
        public void Ctor_Should_SetCustomLog4NetFileNameProperty()
        {
            const string Log4NetFileName = "my.log4net.config";

            var sut = new Log4NetProviderOptions(Log4NetFileName);

            sut.ExternalConfigurationSetup.Should().BeFalse();
            sut.Log4NetConfigFileName.Should().Be(Log4NetFileName);
            sut.LoggerRepository.Should().BeNull();
            sut.Name.Should().BeEmpty();
            sut.OverrideCriticalLevelWith.Should().Be("");
            sut.PropertyOverrides.Should().BeEmpty();
            sut.ScopeFactory.Should().BeNull();
            sut.Watch.Should().BeFalse();
        }

        [Fact]
        public void Ctor_Should_SetCustomLog4NetFileName_And_WatchProperties()
        {
            const string Log4NetFileName = "my.log4net.config";

            var sut = new Log4NetProviderOptions(Log4NetFileName, true);

            sut.ExternalConfigurationSetup.Should().BeFalse();
            sut.Log4NetConfigFileName.Should().Be(Log4NetFileName);
            sut.LoggerRepository.Should().BeNull();
            sut.Name.Should().BeEmpty();
            sut.OverrideCriticalLevelWith.Should().Be("");
            sut.PropertyOverrides.Should().BeEmpty();
            sut.ScopeFactory.Should().BeNull();
            sut.Watch.Should().BeTrue();
        }

        [Fact]
        public void UseWebOrAppConfig_Should_BeEditable()
        {
            var sut = new Log4NetProviderOptions
            {
                UseWebOrAppConfig = true
            };

            sut.ExternalConfigurationSetup.Should().BeFalse();
            sut.Log4NetConfigFileName.Should().Be("log4net.config");
            sut.LoggerRepository.Should().BeNull();
            sut.Name.Should().BeEmpty();
            sut.OverrideCriticalLevelWith.Should().Be("");
            sut.PropertyOverrides.Should().BeEmpty();
            sut.ScopeFactory.Should().BeNull();
            sut.Watch.Should().BeFalse();
            sut.UseWebOrAppConfig.Should().BeTrue();
        }
    }
}