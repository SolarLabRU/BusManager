namespace SolarLab.BusManager.Implementation
{
    public class ErrorMessages
    {
        public const string BusConnectionSettingsIsEmpty = "Не заданы настройки для старта шины";
        public const string IncorrectConfigurationType = "Невозможно стартовать шину, так как ожидаемый тип конфигурации - Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>";
        public const string InvalidQueueName = "Не удалось найти имя очереди в сообщении.Укажите публичное свойство QueueName для вашего сообщения";
        public const string MessageIsNull = "Сообщение не может быть пустым";
        public const string BusNotInitialized = "Невозможно послать сообщение в шину, так как она не проинициализированна";
    }
}
