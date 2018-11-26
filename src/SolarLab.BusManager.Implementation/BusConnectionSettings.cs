using System.Collections.Generic;
using SolarLab.BusManager.Abstraction;

namespace SolarLab.BusManager.Implementation
{
    /// <summary>
    /// Настройки для присоединения к шине
    /// </summary>
    public class BusConnectionSettings : IBusConnectionSettings
    {
        /// <summary>
        /// Массив адресов нод кластера шины
        /// </summary>
        public List<string> NodesAddresses { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string UserPassword { get; set; }

        /// <summary>
        /// Адрес кластера
        /// </summary>
        public string RabbitClusterAddress { get; set; }

        /// <summary>
        /// Имя очереди для шедулера
        /// </summary>
        public bool UseInMemoryScheduler { get; set; } = true;

        /// <summary>
        /// Имя очереди для управления планировщиком заданий
        /// </summary>
        public string ScheduleMessageQueueName { get; set; } = "quartz.ScheduleMessage";
    }
}
