namespace AcmeCorp.EventSourcing.Configuration
{
    internal static class ExpectedStreamRevision
    {
        public const int Any = -2;

        public const int Empty = New;

        public const int End = -1;

        public const int New = 0;
    }
}
