namespace AcmeCorp.EventSourcing.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DomainEventListExtensions
    {
        public static bool FirstEventIs(this IList<DomainEvent> domainEvents, Type eventType)
        {
            return EventAtIndexIs(domainEvents, eventType, 0);
        }

        public static bool SecondEventIs(this IList<DomainEvent> domainEvents, Type eventType)
        {
            return EventAtIndexIs(domainEvents, eventType, 1);
        }

        public static bool ThirdEventIs(this IList<DomainEvent> domainEvents, Type eventType)
        {
            return EventAtIndexIs(domainEvents, eventType, 2);
        }

        public static bool FourthEventIs(this IList<DomainEvent> domainEvents, Type eventType)
        {
            return EventAtIndexIs(domainEvents, eventType, 3);
        }

        public static bool FifthEventIs(this IList<DomainEvent> domainEvents, Type eventType)
        {
            return EventAtIndexIs(domainEvents, eventType, 4);
        }

        public static bool LastEventIs(this IList<DomainEvent> domainEvents, Type eventType)
        {
            return EventAtIndexIs(domainEvents, eventType, domainEvents.Count - 1);
        }

        public static bool EventAtIndexIs(this IList<DomainEvent> domainEvents, Type eventType, int index)
        {
            Type type = domainEvents[index].Body.GetType();
            if (type == eventType)
            {
                return true;
            }

            if (type.IsSubclassOf(eventType))
            {
                return true;
            }

            if (eventType.IsInterface && type.GetInterfaces().Contains(eventType))
            {
                return true;
            }

            return false;
        }
    }
}
