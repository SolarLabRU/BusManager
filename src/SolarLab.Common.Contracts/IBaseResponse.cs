using System.Collections.Generic;

namespace SolarLab.Common.Contracts
{
    /// <summary>
    /// Интерфейс базового ответа
    /// </summary>
    public interface IBaseResponse<T>
    {
        /// <summary>
        /// Признак успешности выполнения запроса
        /// </summary>
        bool IsSuccess { get; set; }

        /// <summary>
        /// Список ошибок
        /// </summary>
        IList<string> ErrorMessages { get; set; }

        /// <summary>
        /// Результат запроса при успешном выполнении
        /// </summary>
        T Result { get; set; }
    }
}
