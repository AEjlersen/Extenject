using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    #if UNITY_EDITOR
    public static class StaticSignalBusRegistry
    {
        public static event Action<SignalBus> onAdded = delegate {};
        public static event Action<SignalBus> onRemoved = delegate {};

        private static readonly List<SignalBus> LIST = new List<SignalBus>();
        public static IEnumerable<SignalBus> List => LIST;

        public static void Add(SignalBus signalBus)
        {
            LIST.Add(signalBus);
            onAdded(signalBus);
        }

        public static void Remove(SignalBus signalBus)
        {
            LIST.RemoveWithConfirm(signalBus);
            onRemoved(signalBus);
        }
    }
    #endif
}