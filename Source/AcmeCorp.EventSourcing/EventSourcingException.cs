namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EventSourcingException : Exception
    {
        public EventSourcingException()
        {
        }

        public EventSourcingException(string message)
            : base(message)
        {
        }

        public EventSourcingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EventSourcingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
