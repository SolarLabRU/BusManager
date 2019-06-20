using SolarLab.Common.Contracts;

namespace SolarLab.BusManager.Implementation.UnitTests
{
    public class WithQueueName : IWithQueueName
    {
        [JetBrains.Annotations.UsedImplicitly]
        public string QueueName { get; set; }
        [JetBrains.Annotations.UsedImplicitly]
        public string Name2 { get; }
        [JetBrains.Annotations.UsedImplicitly]
        string Name3 { get; }
        [JetBrains.Annotations.UsedImplicitly]
        int Name4 { get; }
    }
}