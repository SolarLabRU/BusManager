namespace SolarLab.Common.Contracts
{
    public interface IWithRejectReason
    {
        /// <summary>
        /// Причина отказа
        /// </summary>
        string RejectReason { get; set; }
    }
}
