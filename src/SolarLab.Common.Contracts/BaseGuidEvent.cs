using System;

namespace SolarLab.Common.Contracts
{
    /// <summary>
    /// Базовое событие для всех, у кого есть Guid
    /// </summary>
    public class BaseGuidEvent
    {
        /// <summary>
        /// Идентификатор (уникальный идентификатор для саги)
        /// </summary>
        public Guid Id { get; set; }
    }
}
