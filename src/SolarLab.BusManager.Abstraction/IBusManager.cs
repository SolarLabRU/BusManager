using System;
using System.Threading.Tasks;
using SolarLab.Common.Contracts;
using Microsoft.Extensions.Configuration;

namespace SolarLab.BusManager.Abstraction
{
    /// <summary>
    /// Менеджер работы с шиной данных
    /// </summary>
    public interface IBusManager
    {
        /// <summary>
        /// Стартовать шину данных
        /// </summary>
        void StartBus<T>(T configuration);

        /// <summary>
        /// Стартовать шину данных
        /// </summary>
        void StartBus<T>(T configuration, IBusConnectionSettings settings);

        /// <summary>
        /// Остановить коннекшон к шине
        /// </summary>
        void StopBus();

        /// <summary>
        /// Послать сообщение в шину
        /// </summary>
        /// <typeparam name="TEvent">Тип события в шине</typeparam>
        /// <param name="eventModel">модель для посылки сообщения в шину</param>
        Task Publish<TEvent>(TEvent eventModel) where TEvent : class;

        /// <summary>
        /// Создает отложенное сообщение в шину
        /// </summary>
        /// <typeparam name="TEvent">Тип события в шине</typeparam>
        /// <param name="scheduledTime">Запланированное время отправки сообщения</param>
        /// <param name="eventModel">модель для посылки сообщения в шину</param>
        /// <returns>Возвращает токен запланированного сообщения</returns>
        Task<Guid> SchedulePublish<TEvent>(DateTime scheduledTime, TEvent eventModel) where TEvent : class;
        
        /// <summary>
        /// Создает отложенное сообщение в шину
        /// </summary>
        /// <typeparam name="TEvent">Тип события в шине</typeparam>
        /// <param name="scheduledTime">Запланированное время отправки сообщения</param>
        /// <param name="eventModel">модель для посылки сообщения в шину</param>
        /// <returns>Возвращает токен запланированного сообщения</returns>
        Task<Guid> ScheduleSend<TEvent>(DateTime scheduledTime, TEvent eventModel) where TEvent : class, IWithQueueName;

        /// <summary>
        /// Отменяет запланированное сообщение
        /// </summary>
        /// <param name="tokenId">Токен запланированного сообщения</param>
        Task CancelSchedulePublish(Guid tokenId);

        /// <summary>
        /// Послать сообщение в очередь
        /// </summary>
        /// <typeparam name="T">тип сообщения</typeparam>
        /// <param name="message">сообщение</param>
        /// <returns>Task</returns>
        Task Send<T>(T message) where T : class, IWithQueueName;

        /// <summary>
        /// Послать сообщение в очередь и получить сообщение в ответ
        /// </summary>
        /// <typeparam name="TRequest">тип запроса</typeparam>
        /// <typeparam name="TResponse">тип ответа</typeparam>
        /// <param name="request">запрос</param>
        /// <returns>Task</returns>
        Task<TResponse> Request<TRequest, TResponse>(TRequest request)
            where TRequest : class, IWithQueueName where TResponse : class;

        /// <summary>
        /// Послать сообщение в очередь и получить сообщение в ответ
        /// </summary>
        /// <typeparam name="TRequest">тип запроса</typeparam>
        /// <typeparam name="TResponse">тип ответа</typeparam>
        /// <param name="request">запрос</param>
        /// <param name="requestTimeOutInSeconds">тайм-аут запроса в секундах</param>
        /// <param name="ignoreTimeoutException">Нужно ли игнорировать ошибку тайм аута. Если игнорируем, то возвращаем null</param>
        /// <returns>Task</returns>
        Task<TResponse> Request<TRequest, TResponse>(TRequest request, double requestTimeOutInSeconds,
            bool ignoreTimeoutException = false) where TRequest : class, IWithQueueName where TResponse : class;

        /// <summary>
        /// Получить настройки для рэббита
        /// </summary>
        /// <param name="configuration">Конфигурация</param>
        /// <returns>Настройки для подключения к шине</returns>
        IBusConnectionSettings GetSettings(IConfigurationRoot configuration);
    }
}
