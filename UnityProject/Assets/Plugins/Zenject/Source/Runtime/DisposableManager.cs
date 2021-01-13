using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public sealed class DisposableManager : IDisposable
    {
        private readonly List<DisposableInfo> disposables = new List<DisposableInfo>();
        private readonly List<LateDisposableInfo> lateDisposables = new List<LateDisposableInfo>();
        private bool disposed;
        private bool lateDisposed;

        [Inject]
        public DisposableManager([Inject(Optional = true, Source = InjectSources.Local)] List<IDisposable> disposables,
                                 [Inject(Optional = true, Source = InjectSources.Local)] List<ValuePair<Type, int>> priorities,
                                 [Inject(Optional = true, Source = InjectSources.Local)] List<ILateDisposable> lateDisposables,
                                 [Inject(Id = "Late", Optional = true, Source = InjectSources.Local)] List<ValuePair<Type, int>> latePriorities)
        {
            foreach (IDisposable disposable in disposables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                int? match = priorities.Where(x => disposable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)x.Second).SingleOrDefault();
                int priority = match.HasValue ? match.Value : 0;

                this.disposables.Add(new DisposableInfo(disposable, priority));
            }

            foreach (ILateDisposable lateDisposable in lateDisposables)
            {
                int? match = latePriorities.Where(x => lateDisposable.GetType().DerivesFromOrEqual(x.First)).Select(x => (int?)x.Second).SingleOrDefault();
                int priority = match.HasValue ? match.Value : 0;

                this.lateDisposables.Add(new LateDisposableInfo(lateDisposable, priority));
            }
        }

        public void Add(IDisposable disposable)
        {
            Add(disposable, 0);
        }

        public void Add(IDisposable disposable, int priority)
        {
            disposables.Add(new DisposableInfo(disposable, priority));
        }

        public void AddLate(ILateDisposable disposable)
        {
            AddLate(disposable, 0);
        }

        public void AddLate(ILateDisposable disposable, int priority)
        {
            lateDisposables.Add(new LateDisposableInfo(disposable, priority));
        }

        public void Remove(IDisposable disposable)
        {
            disposables.RemoveWithConfirm(disposables.Where(x => ReferenceEquals(x.disposable, disposable)).Single());
        }

        public void LateDispose()
        {
            Assert.That(!lateDisposed, "Tried to late dispose DisposableManager twice!");
            lateDisposed = true;

            // Dispose in the reverse order that they are initialized in
            List<LateDisposableInfo> disposablesOrdered = lateDisposables.OrderBy(x => x.priority).Reverse().ToList();

            #if UNITY_EDITOR
            foreach (ILateDisposable disposable in disposablesOrdered.Select(x => x.lateDisposable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate ILateDisposable with type '{0}'".Fmt(disposable.GetType()));
            }
            #endif

            foreach (LateDisposableInfo disposable in disposablesOrdered)
            {
                try
                {
                    disposable.lateDisposable.LateDispose();
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(e, "Error occurred while late disposing ILateDisposable with type '{0}'", disposable.lateDisposable.GetType());
                }
            }
        }

        public void Dispose()
        {
            Assert.That(!disposed, "Tried to dispose DisposableManager twice!");
            disposed = true;

            // Dispose in the reverse order that they are initialized in
            List<DisposableInfo> disposablesOrdered = disposables.OrderBy(x => x.priority).Reverse().ToList();

            #if UNITY_EDITOR
            foreach (IDisposable disposable in disposablesOrdered.Select(x => x.disposable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate IDisposable with type '{0}'".Fmt(disposable.GetType()));
            }
            #endif

            foreach (DisposableInfo disposable in disposablesOrdered)
            {
                try
                {
                    disposable.disposable.Dispose();
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(e, "Error occurred while disposing IDisposable with type '{0}'", disposable.disposable.GetType());
                }
            }
        }

        private readonly struct DisposableInfo
        {
            public readonly IDisposable disposable;
            public readonly int priority;

            public DisposableInfo(IDisposable disposable, int priority)
            {
                this.disposable = disposable;
                this.priority = priority;
            }
        }

        private sealed class LateDisposableInfo
        {
            public readonly ILateDisposable lateDisposable;
            public readonly int priority;

            public LateDisposableInfo(ILateDisposable lateDisposable, int priority)
            {
                this.lateDisposable = lateDisposable;
                this.priority = priority;
            }
        }
    }
}
