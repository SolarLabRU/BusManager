﻿using System;
using System.Threading.Tasks;
using Xunit;

namespace SolarLab.BusManager.Implementation.UnitTests
{
    public partial class MassTransitBusManagerTests
    {
        [Fact]
        public async Task SendShouldThrowExceptionWhenMessageNull()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => _manager.Send((WithQueueName)null));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.MessageIsNull, exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SendShouldThrowExceptionWhenQueueNameNullOrEmpty(string queueName)
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => _manager.Send(new WithQueueName { QueueName = queueName }));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.InvalidQueueName, exception.Message);
        }

        [Fact]
        public async Task SendCorrectCallWithQueueNameThrowExceptionOnInitBus()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => _manager.Send(new WithQueueName { QueueName = "notNullQueueName" }));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.BusConnectionSettingsIsEmpty, exception.Message);
        }
    }
}
