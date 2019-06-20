using System;
using System.Collections.Generic;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace SolarLab.BusManager.Implementation.UnitTests
{
    public partial class MassTransitBusManagerTests
    {
        private readonly Mock<IConfigurationRoot> _configurationRootMock = new Mock<IConfigurationRoot>();
        private readonly Mock<IBusControl> _busControlMock = new Mock<IBusControl>();
        private readonly MassTransitBusManager _manager;
        private readonly MassTransitBusManagerWithSettableBus _managerWithSettableBus;

        public MassTransitBusManagerTests()
        {
            _manager = new MassTransitBusManager(_configurationRootMock.Object);
            _managerWithSettableBus = new MassTransitBusManagerWithSettableBus(_configurationRootMock.Object);
            _managerWithSettableBus.SetBusControl(_busControlMock.Object);
        }

        [Fact]
        public void StartBusShouldThrowExceptionWhenSettingsIsNull()
        {
            var exception = Assert.Throws<Exception>(() =>
                _manager.StartBus((Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>)null, null));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.BusConnectionSettingsIsEmpty, exception.Message);
        }

        [Fact]
        public void StartBusShouldThrowExceptionWhenConfigurationNotNullAndIncorrectType()
        {
            var configurationIncorrectType = "";//configuration must be null or Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>
            var exception = Assert.Throws<Exception>(() => _manager.StartBus(configurationIncorrectType, null));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.IncorrectConfigurationType, exception.Message);
        }
    }
}
