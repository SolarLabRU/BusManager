using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMqTransport;
using MassTransit.Scheduling;
using Moq;
using Xunit;

namespace SolarLab.BusManager.Implementation.UnitTests
{
    public partial class MassTransitBusManagerTests
    {
        [Fact]
        public async Task ScheduleSendShouldThrowExceptionWhenMessageNull()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => _manager.ScheduleSend(DateTime.Now, (WithQueueName)null));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.MessageIsNull, exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ScheduleSendShouldThrowExceptionWhenQueueNameNullOrEmpty(string queueName)
        {
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _manager.ScheduleSend(DateTime.Now, new WithQueueName {QueueName = queueName}));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.InvalidQueueName, exception.Message);
        }

        [Fact]
        public async Task ScheduleSendCorrectCallWithQueueNameThrowExceptionOnInitBus()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _manager.ScheduleSend(DateTime.Now, new WithQueueName {QueueName = "notNullQueueName"}));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.BusConnectionSettingsIsEmpty, exception.Message);
        }

        [Fact]
        public async Task ScheduleSendCorrectCallPassesParametersToBusClient()
        {
            var scheduledTime = DateTime.Now;

            _managerWithSettableBus.StartBus(new Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>(), new BusConnectionSettings());
            await _managerWithSettableBus.ScheduleSend(scheduledTime, new WithQueueName { QueueName = "notNullQueueName" });

            _busControlMock.Verify(x => x.Publish(It.IsAny<ScheduleMessage<WithQueueName>>(), 
                It.IsAny<ScheduleMessageContextPipe<WithQueueName>>(),
                It.IsAny<CancellationToken>()), Times.Once);

            _busControlMock.Verify(x =>
                x.Publish(
                    It.Is<ScheduleMessage<WithQueueName>>(y => y.ScheduledTime == scheduledTime.ToUniversalTime()),
                    It.IsAny<ScheduleMessageContextPipe<WithQueueName>>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}