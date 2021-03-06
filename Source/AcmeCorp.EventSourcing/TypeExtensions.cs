﻿namespace AcmeCorp.EventSourcing
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensions
    {
        internal static bool TryGetAggregateUpdateStateMethodForMessage(this Type type, Type messageType, out MethodInfo method)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type[] interfaces = type.GetInterfaces();
            Type interfaceType = typeof(IHandleEvent<>).MakeGenericType(messageType);
            if (interfaces.All(x => x != interfaceType))
            {
                // No interface means aggregate doesn't process this event type
                method = null;
                return false;
            }

            InterfaceMapping map = type.GetInterfaceMap(interfaceType);
            method = map.TargetMethods.Single();
            return true;
        }
    }
}
