namespace OpenManta.Core
{
    /// <summary>
    /// Priority for messages in RabbitMQ queues.
    /// </summary>
    public enum RabbitMqPriority : byte
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
}
