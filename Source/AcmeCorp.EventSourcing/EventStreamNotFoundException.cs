namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EventStreamNotFoundException : EventSourcingException
    {
        public EventStreamNotFoundException()
        {
        }

        public EventStreamNotFoundException(string message)
            : base(message)
        {
        }

        public EventStreamNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EventStreamNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
