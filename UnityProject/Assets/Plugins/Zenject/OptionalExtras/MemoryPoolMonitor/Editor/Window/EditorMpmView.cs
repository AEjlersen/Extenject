using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Zenject.EditorWindowTemplate;

namespace Zenject.MemoryPoolMonitor
{
    public sealed class EditorMpmView : EditorTemplateView<IMemoryPool>
    {
        [Inject]
        public EditorMpmView(ZenjectEditorWindow window)
            : base(window)
        {
        }

        protected override Table<IMemoryPool> ConstructTable()
        {
            var columns = new[]
            {
                new ColumnData<IMemoryPool>("Pool Type", OnColumnGetName),
                new ColumnData<IMemoryPool>("Total", OnColumnGetTotal),
                new ColumnData<IMemoryPool>("Active", OnColumnGetActive),
                new ColumnData<IMemoryPool>("Inactive", OnColumnGetInactive)
            };

            var options = new[]
            {
                new OptionData<IMemoryPool>("Clear", OnActionClear),
                new OptionData<IMemoryPool>("Expand", OnActionExpand)
            };

            return new Table<IMemoryPool>(columns, options);
        }

        protected override void PostInitialize()
        {
            StaticMemoryPoolRegistry.PoolAdded += OnPoolListChanged;
            StaticMemoryPoolRegistry.PoolRemoved += OnPoolListChanged;
            drawer.MarkListAsDirty();
        }

        public override void PopulateList(List<IMemoryPool> list)
        {
            list.AddRange(StaticMemoryPoolRegistry.Pools.Where(ShouldIncludePool));
        }
        
        private bool ShouldIncludePool(IMemoryPool pool)
        {
            string filter = drawer.GetFilter();
            return filter.IsEmpty() || GetName(pool).ToLowerInvariant().Contains(filter);
        }
        
        private string GetName(IMemoryPool pool)
        {
            Type type = pool.GetType();
            return "{0}.{1}".Fmt(type.Namespace, type.PrettyName());
        }
        
        private IComparable OnColumnGetName(IMemoryPool pool)
        {
            return GetName(pool);
        }

        private IComparable OnColumnGetTotal(IMemoryPool pool)
        {
            return pool.NumTotal;
        }

        private IComparable OnColumnGetActive(IMemoryPool pool)
        {
            return pool.NumActive;
        }

        private IComparable OnColumnGetInactive(IMemoryPool pool)
        {
            return pool.NumInactive;
        }

        private void OnActionExpand(IMemoryPool pool)
        {
            pool.ExpandBy(5);
        }

        private void OnActionClear(IMemoryPool pool)
        {
            pool.Clear();
        }
        
        private void OnPoolListChanged(IMemoryPool pool)
        {
            drawer.MarkListAsDirty();
        }
    }
}