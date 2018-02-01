namespace AcmeCorp.EventSourcing.Configuration
{
    public static class EventStoreMessageHeaderKey
    {
        public const string EventStreamId = "AcmeCorp-OriginalEventStreamId";

        public const string EventId = "AcmeCorp-EventId";

        public const string CausationId = "AcmeCorp-CausationId";

        public const string ConversationId = "AcmeCorp-ConversationId";
    }
}
