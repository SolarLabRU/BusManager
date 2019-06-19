using System;
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
    }
}
