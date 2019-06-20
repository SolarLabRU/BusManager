using MassTransit;
using Microsoft.Extensions.Configuration;

namespace SolarLab.BusManager.Implementation.UnitTests
{
    public class MassTransitBusManagerWithSettableBus : MassTransitBusManager
    {
        public MassTransitBusManagerWithSettableBus(IConfigurationRoot applicationConfiguration) : base(applicationConfiguration)
        {
        }

        public void SetBusControl(IBusControl bus)
        {
            Bus = bus;
        }
    }
}