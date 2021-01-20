using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Zenject.EditorWindowTemplate;

namespace Zenject.MemoryPoolMonitor
{
    public sealed class EditorSignalBusView : EditorTemplateView<SignalBus>
    {
        [Inject]
        public EditorSignalBusView(ZenjectEditorWindow window)
            : base(window)
        {
        }

        protected override Table<SignalBus> ConstructTable()
        {
            var columns = new[]
            {
                new ColumnData<SignalBus>("Type", OnColumnGetName),
                new ColumnData<SignalBus>("Parent", OnColumnGetParent),
                new ColumnData<SignalBus>("Parent Depth", OnColumnGetParentDepth),
                new ColumnData<SignalBus>("Subscribers", OnColumnGetSubscribers)
            };
            
            return new Table<SignalBus>(columns, null);
        }

        protected override void PostInitialize()
        {
            StaticSignalBusRegistry.onAdded += OnListChanged;
            StaticSignalBusRegistry.onRemoved += OnListChanged;
            drawer.MarkListAsDirty();
        }

        public override void PopulateList(List<SignalBus> list)
        {
            list.AddRange(StaticSignalBusRegistry.List.Where(ShouldIncludeSignalBus));
        }

        private bool ShouldIncludeSignalBus(SignalBus signalBus)
        {
            string filter = drawer.GetFilter();
            return filter.IsEmpty() || GetName(signalBus).ToLowerInvariant().Contains(filter);
        }

        private string GetName(SignalBus signalBus)
        {
            Type type = signalBus.GetType();
            return "{0}.{1}".Fmt(type.Namespace, type.PrettyName());
        }

        private IComparable OnColumnGetName(SignalBus signalBus)
        {
            return GetName(signalBus);
        }

        private IComparable OnColumnGetParent(SignalBus signalBus)
        {
            return signalBus.ParentBus != null;
        }

        private IComparable OnColumnGetParentDepth(SignalBus signalBus)
        {
            var count = 0;
            SignalBus temp = signalBus;

            while (temp.ParentBus != null)
            {
                count++;
                temp = temp.ParentBus;
            }

            return count;
        }

        private IComparable OnColumnGetSubscribers(SignalBus signalBus)
        {
            return signalBus.NumSubscribers;
        }

        private void OnListChanged(SignalBus signalBus)
        {
            drawer.MarkListAsDirty();
        }
    }
}