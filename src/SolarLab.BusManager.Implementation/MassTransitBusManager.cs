using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarLab.Common.Contracts;
using JetBrains.Annotations;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Serilog;
using SolarLab.BusManager.Abstraction;

namespace SolarLab.BusManager.Implementation
{
    public class MassTransitBusManager : IBusManager
    {
        #region Private fields and properties

        private Uri _schedulerAddress;

        /// <summary>
        /// Контрол шины данных
        /// </summary>
        private IBusControl _bus;

        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfigurationRoot _applicationConfiguration;

        /// <summary>
        /// Конфигурация для шины данных
        /// </summary>
        private Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>> _dictConfiguration;

        /// <summary>
        /// Настройки подключния к шине
        /// </summary>
        private IBusConnectionSettings _settings;

        private readonly object _locker = new object();

        #endregion Private fields and properties

        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        public MassTransitBusManager(IConfigurationRoot applicationConfiguration)
        {
            _applicationConfiguration = applicationConfiguration;
        }

        #endregion Constructor

        #region Implementation IBusManager

        /// <summary>
        /// Стартовать шину данных
        /// </summary>
        public void StartBus<T>(T configuration)
        {
            StartBus(configuration, GetSettings(_applicationConfiguration));
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_applicationConfiguration).CreateLogger(); 
        }

        /// <summary>
        /// Стартовать шину данных
        /// </summary>
        public void StartBus<T>(T configuration, IBusConnectionSettings settings)
        {
            if (configuration != null)
            {
                var configurationType = typeof(T);
                if (configurationType == typeof(Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>))
                {
                    _dictConfiguration = configuration as Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>;
                }
                else
                {
                    throw new Exception(ErrorMessages.IncorrectConfigurationType);
                }
            }
            _settings = settings ?? throw new Exception(ErrorMessages.BusConnectionSettingsIsEmpty);

            InitBusAndThrowOnError();
        }

        /// <summary>
        /// Остановить коннекшон к шине
        /// </summary>
        public void StopBus()
        {
            _bus?.Stop();
        }

        /// <summary>
        /// Послать сообщение в шину
        /// </summary>
        /// <typeparam name="TEvent">Тип события в шине</typeparam>
        /// <param name="eventModel">модель для посылки сообщения в шину</param>
        public async Task Publish<TEvent>(TEvent eventModel) where TEvent : class
        {
            CheckForNull(eventModel);
            InitBusAndThrowOnError();
            await _bus.Publish(eventModel);
        }

        /// <summary>
        /// Послать сообщение в очередь
        /// </summary>
        /// <typeparam name="T">тип сообщения</typeparam>
        /// <param name="message">сообщение</param>
        /// <returns>Task</returns>
        public async Task Send<T>(T message) where T : class, IWithQueueName
        {
            CheckForNull(message);

            var queueName = GetQueueNameOrThrow(message);

            InitBusAndThrowOnError();

            var sendEndpoint = await _bus.GetSendEndpoint(ComposeUri(queueName));
            if (sendEndpoint == null)
            {
                throw new Exception($"Не удалось найти очередь {queueName}");
            }
            await sendEndpoint.Send(message, pc => pc.SetAwaitAck(false));
        }

        /// <summary>
        /// Создает отложенное сообщение в шину
        /// </summary>
        /// <typeparam name="TEvent">Тип события в шине</typeparam>
        /// <param name="scheduledTime">Запланированное время отправки сообщения</param>
        /// <param name="eventModel">модель для посылки сообщения в шину</param>
        /// <returns>Возвращает токен запланированного сообщения</returns>
        public async Task<Guid> SchedulePublish<TEvent>(DateTime scheduledTime, TEvent eventModel) where TEvent : class
        {
            InitBusAndThrowOnError();

            var schedulerEndpoint = await _bus.GetSendEndpoint(_schedulerAddress);
            var scheduledMessage = await _bus.SchedulePublish(schedulerEndpoint, scheduledTime, eventModel);
            return scheduledMessage.TokenId;
        }
        
        /// <summary>
        /// Создает отложенное сообщение в шину
        /// </summary>
        /// <typeparam name="TEvent">Тип события в шине</typeparam>
        /// <param name="scheduledTime">Запланированное время отправки сообщения</param>
        /// <param name="eventModel">модель для посылки сообщения в шину</param>
        /// <returns>Возвращает токен запланированного сообщения</returns>
        public async Task<Guid> ScheduleSend<TEvent>(DateTime scheduledTime, TEvent eventModel) where TEvent : class, IWithQueueName 
        {
            InitBusAndThrowOnError();

            var destinationAddress = new Uri($"rabbitmq://{_settings.RabbitClusterAddress}/{((IWithQueueName)eventModel).QueueName}");
            var scheduledMessage = await _bus.ScheduleSend(destinationAddress, scheduledTime, eventModel);
            return scheduledMessage.TokenId;
        }

        /// <summary>
        /// Отменяет запланированное сообщение
        /// </summary>
        /// <param name="tokenId">Токен запланированного сообщения</param>
        public Task CancelSchedulePublish(Guid tokenId)
        {
            InitBusAndThrowOnError();

            return _bus.CancelScheduledSend(tokenId);
        }

        /// <summary>
        /// Послать сообщение в очередь и получить сообщение в ответ
        /// </summary>
        /// <typeparam name="TRequest">тип запроса</typeparam>
        /// <typeparam name="TResponse">тип ответа</typeparam>
        /// <param name="request">запрос</param>
        /// <returns>Task</returns>
        public Task<TResponse> Request<TRequest, TResponse>(TRequest request) where TRequest : class, IWithQueueName where TResponse : class
        {
            return Request<TRequest, TResponse>(request, 30);
        }

        /// <summary>
        /// Послать сообщение в очередь и получить сообщение в ответ
        /// </summary>
        /// <typeparam name="TRequest">тип запроса</typeparam>
        /// <typeparam name="TResponse">тип ответа</typeparam>
        /// <param name="request">запрос</param>
        /// <param name="requestTimeOutInSeconds">тайм-аут запроса в секундах</param>
        /// <param name="ignoreTimeoutException">Нужно ли игнорировать ошибку тайм аута. Если игнорируем, то возвращаем null</param>
        /// <returns>Task</returns>
        public async Task<TResponse> Request<TRequest, TResponse>(TRequest request, double requestTimeOutInSeconds, bool ignoreTimeoutException = false) where TRequest : class, IWithQueueName where TResponse : class
        {
            CheckForNull(request);

            var queueName = GetQueueNameOrThrow(request);

            InitBusAndThrowOnError();

            if (requestTimeOutInSeconds <= 0)
            {
                throw new Exception($"Таймаут для запроса должен быть больше нуля. Текущее значение = {requestTimeOutInSeconds}");
            }

            var address = ComposeUri(queueName);
            var requestTimeout = TimeSpan.FromSeconds(requestTimeOutInSeconds);

            var client = new MessageRequestClient<TRequest, TResponse>(_bus, address, requestTimeout);
            try
            {
                return await client.Request(request, CancellationToken.None);
            }
            catch (RequestTimeoutException e)
            {
                if (ignoreTimeoutException)
                {
                    return null;
                }
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Получить настройки для рэббита
        /// </summary>
        /// <param name="configuration">Конфигурация</param>
        /// <returns>Настройки для подключения к шине</returns>
        public IBusConnectionSettings GetSettings(IConfigurationRoot configuration)
        {
            var settings = new BusConnectionSettings
            {
                UserName = GetSettingsFromConfig("RabbitMQConfig","RabbitUserName"),
                UserPassword = GetSettingsFromConfig("RabbitMQConfig", "RabbitUserPassword")
            };

            var adr = GetSettingsFromConfig("RabbitMQConfig", "RabbitAddress");
            var addresses = adr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (addresses == null || addresses.Count < 1)
            {
                throw new Exception("Необходимо задать хотя бы один адрес сервиса.");
            }
            settings.NodesAddresses = addresses;

            var rabbitClusterAddress = GetSettingsFromConfig("RabbitMQConfig", "RabbitClusterAddress");
            if (string.IsNullOrEmpty(rabbitClusterAddress))
            {
                rabbitClusterAddress = settings.NodesAddresses[0];
            }
            settings.RabbitClusterAddress = rabbitClusterAddress;

            var useInMemorySchedulerFromConfig = GetSettingsFromConfig("RabbitMQConfig", "UseInMemoryScheduler");
            if (!bool.TryParse(useInMemorySchedulerFromConfig, out var useInMemoryScheduler))
            {
                useInMemoryScheduler = true;
            }
            settings.UseInMemoryScheduler = useInMemoryScheduler;

            var scheduleMessageQueueName = GetSettingsFromConfig("RabbitMQConfig", "ScheduleMessageQueueName");
            if (string.IsNullOrEmpty(scheduleMessageQueueName))
            {
                scheduleMessageQueueName = "quartz.ScheduleMessage";
            }
            settings.ScheduleMessageQueueName = scheduleMessageQueueName;

            return settings;

            string GetSettingsFromConfig(string settingNameFirst, string settingNameSecond)
            {
                var result = configuration[$"{settingNameFirst}:{settingNameSecond}"];
                if (string.IsNullOrEmpty(result))
                {
                    result = configuration[$"{settingNameFirst}_{settingNameSecond}"];
                }

                return result;
            }
        }

        #endregion Implementation IBusManager

        [AssertionMethod]
        private void CheckForNull<T>(T innerParameter)
        {
            if (innerParameter == null)
            {
                throw new Exception(ErrorMessages.MessageIsNull);
            }
        }

        private string GetQueueNameOrThrow<T>(T innerParameter) where T : IWithQueueName
        {
            if (string.IsNullOrEmpty(innerParameter.QueueName))
            {
                throw new Exception(ErrorMessages.InvalidQueueName);
            }

            return innerParameter.QueueName;
        }

        private void InitBusAndThrowOnError()
        {
            if (_settings == null)
            {
                throw new Exception(ErrorMessages.BusConnectionSettingsIsEmpty);
            }

            if (!InitAndStartBus())
            {
                throw new Exception(ErrorMessages.BusNotInitialized);
            }
            if (_bus == null)
            {
                throw new Exception(ErrorMessages.BusNotInitialized);
            }
        }

        private bool InitAndStartBus()
        {
            lock (_locker)
            {
                // If bus not configured - then exit
                if (_bus != null)
                {
                    return true;
                }

                _bus = ConfigureBus();
                try
                {
                    _bus.StartAsync().Wait();
                    return true;
                }
                catch (RabbitMqConnectionException)
                {
                    _bus = null;
                    //Error connect to RabbitMq
                    return false;
                }
                catch (Exception)
                {
                    _bus = null;
                    //Critical error connect to RabbitMq
                    return false;
                }
            }
        }

        /// <summary>
        /// Составить Uri адреса
        /// </summary>
        /// <param name="address">адрес</param>
        /// <returns>URI</returns>
        private Uri ComposeUri(string address)
        {
            return new Uri($"rabbitmq://{_settings.RabbitClusterAddress}/{address}");
        }

        /// <summary>
        /// Configure bus
        /// </summary>
        /// <returns>IBusControl</returns>
        private IBusControl ConfigureBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://" + _settings.RabbitClusterAddress), h =>
                {
                    h.Username(_settings.UserName);
                    h.Password(_settings.UserPassword);
                    h.Heartbeat(600); //10 minutes
                    if (_settings.NodesAddresses.Count > 1)
                    {
                        h.UseCluster(c =>
                        {
                            foreach (var serviceHost in _settings.NodesAddresses)
                            {
                                c.Node(serviceHost);
                            }
                        });
                    }
                });
                if (_dictConfiguration != null && _dictConfiguration.Any())
                {
                    foreach (var kvp in _dictConfiguration)
                    {
                        // TODO: UseMessageScope
                        // cfg.UseMessageScope();
                        cfg.ReceiveEndpoint(host, kvp.Key, kvp.Value);
                    }
                }

                if (_settings.UseInMemoryScheduler)
                {
                    cfg.UseInMemoryScheduler();
                }
                else
                {
                    _schedulerAddress = new Uri($"rabbitmq://{_settings.RabbitClusterAddress}/{_settings.ScheduleMessageQueueName}");
                    cfg.UseMessageScheduler(_schedulerAddress);
                }

                cfg.UseSerilog();
            });
        }
    }
}
