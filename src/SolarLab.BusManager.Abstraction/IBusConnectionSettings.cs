using System.Collections.Generic;

namespace SolarLab.BusManager.Abstraction
{
    /// <summary>
    /// Настройки для подключения к шине
    /// </summary>
    public interface IBusConnectionSettings
    {
        /// <summary>
        /// Массив адресов нод кластера шины
        /// </summary>
        List<string> NodesAddresses { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        string UserPassword { get; set; }

        /// <summary>
        /// Адрес кластера
        /// </summary>
        string RabbitClusterAddress { get; set; }

        /// <summary>
        /// Использовать шедулер InMemory
        /// </summary>
        bool UseInMemoryScheduler { get; set; }

        /// <summary>
        /// Имя очереди для шедулера
        /// </summary>
        string ScheduleMessageQueueName { get; set; }
    }
}
