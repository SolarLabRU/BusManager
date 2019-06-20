using MassTransit.RabbitMqTransport;
using MassTransit.Scheduling;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SolarLab.BusManager.Implementation.UnitTests
{
    public partial class MassTransitBusManagerTests
    {
        [Fact]
        public async Task ScheduleRecurringSendShouldThrowExceptionWhenMessageNull()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _manager.ScheduleRecurringSend(string.Empty, string.Empty, string.Empty, (WithQueueName) null));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.MessageIsNull, exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ScheduleRecurringSendShouldThrowExceptionWhenQueueNameNullOrEmpty(string queueName)
        {
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _manager.ScheduleRecurringSend(string.Empty, string.Empty, string.Empty,
                    new WithQueueName {QueueName = null}));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.InvalidQueueName, exception.Message);
        }

        [Fact]
        public async Task ScheduleRecurringSendCorrectCallWithQueueNameThrowExceptionOnInitBus()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() => _manager.ScheduleRecurringSend(string.Empty,
                string.Empty, string.Empty, new WithQueueName {QueueName = "notNullQueueName"}));
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
            Assert.Equal(ErrorMessages.BusConnectionSettingsIsEmpty, exception.Message);
        }

        [Theory]
        [InlineData("testScheduleId", "testScheduleGroup", "0 1 0 0 * * ? *")]
        public async Task ScheduleRecurringSendCorrectCallPassesParametersToBusClient(string scheduleId, string scheduleGroup, string cronExpression)
        {
            _managerWithSettableBus.StartBus(new Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>(), new BusConnectionSettings());
            await _managerWithSettableBus.ScheduleRecurringSend(scheduleId, scheduleGroup, cronExpression,
                new WithQueueName {QueueName = "notNullQueueName"});

            _busControlMock.Verify(x =>
                x.Publish(
                    It.Is<ScheduleRecurringMessage<WithQueueName>>(y =>
                        VerifyRecurringSchedule(y.Schedule, scheduleId, scheduleGroup, cronExpression)),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        private bool VerifyRecurringSchedule(RecurringSchedule recurringSchedule, string expectedScheduleId, string expectedScheduleGroup, string expectedCronExpression)
        {
            Assert.NotNull(recurringSchedule);
            Assert.Equal(expectedScheduleId, recurringSchedule.ScheduleId);
            Assert.Equal(expectedScheduleGroup, recurringSchedule.ScheduleGroup);
            Assert.Equal(expectedCronExpression, recurringSchedule.CronExpression);

            return true;
        }
    }
}
