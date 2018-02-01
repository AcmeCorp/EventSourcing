namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;

    public static class TestObjectThatUsesDomainRepository
    {
        public static void DoSomething(IDomainRepository domainRepository, IAggregate aggregate)
        {
            if (domainRepository == null)
            {
                throw new ArgumentNullException(nameof(domainRepository));
            }

            domainRepository.LoadAsync(aggregate).Wait();
            domainRepository.SaveAsync(aggregate).Wait();
        }
    }
}
