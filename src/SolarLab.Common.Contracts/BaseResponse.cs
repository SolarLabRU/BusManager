using System.Collections.Generic;

namespace SolarLab.Common.Contracts
{
    /// <summary>
    /// Описывает базовый ответ на запрос
    /// </summary>
    /// <typeparam name="T">Тип данных ответа</typeparam>
    public class BaseResponse<T> : IBaseResponse<T>
    {
        /// <summary>
        /// Признак успешности выполнения запроса
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Список ошибок
        /// </summary>
        public IList<string> ErrorMessages { get; set; }

        /// <summary>
        /// Результат запроса при успешном выполнении
        /// </summary>
        public T Result { get; set; }
    }
}
