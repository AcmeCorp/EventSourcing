namespace AcmeCorp.EventSourcing
{
    public interface IHandleEvent<in T>
    {
        void Handle(T message);
    }
}