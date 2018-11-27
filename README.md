# BusManager
BusManager helps you to connect to bus. Implementation for RabbitMq and MassTransit

SolarLab.Common.Contracts - projects for main contracts. We are using this project to store separatly IWithQueueName class.
IWithQueueName is using for Abstraction, Implementation and also for all contracts between microservices.
nuget package - https://www.nuget.org/packages/SolarLab.Common.Contracts/

SolarLab.BusManager.Abstraction - abstraction for bus manager. Use this abstraction to inject into your business logic.
nuget package - https://www.nuget.org/packages/SolarLab.BusManager.Abstraction/

SolarLab.BusManager.Implementation - implementation for SolarLab.BusManager.Abstraction. Use class MassTransitBusManager to inject into your container as implementation for your SolarLab.BusManager.Abstraction.IBusManager
nuget package - https://www.nuget.org/packages/SolarLab.BusManager.Implementation/
