namespace SolarLab.Common.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWithQueueNameAndProcedureType : IWithQueueName
    {
        /// <summary>
        /// Тип процедуры - используется в имени очереди
        /// </summary>
        string ProcedureType { get; set; }
    }
}
