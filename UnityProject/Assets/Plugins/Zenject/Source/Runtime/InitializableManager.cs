using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    // Responsibilities:
    // - Run Initialize() on all Iinitializable's, in the order specified by InitPriority
    public class InitializableManager
    {
        private List<InitializableInfo> initializables;
        private List<LateInitializableInfo> lateInitializables;
        protected bool hasInitialized;
        protected bool hasLateInitialized;

        [Inject]
        public InitializableManager([Inject(Optional = true, Source = InjectSources.Local)] List<IInitializable> initializables,
                                    [Inject(Optional = true, Source = InjectSources.Local)] List<ValuePair<Type, int>> priorities,
                                    [Inject(Optional = true, Source = InjectSources.Local)] List<ILateInitializable> lateInitializables,
                                    [Inject(Id = "Late", Optional = true, Source = InjectSources.Local)] List<ValuePair<Type, int>> latePriorities)
        {
            this.initializables = new List<InitializableInfo>(initializables.Count);
            this.lateInitializables = new List<LateInitializableInfo>(lateInitializables.Count);

            for (var i = 0; i < initializables.Count; i++)
            {
                IInitializable initializable = initializables[i];

                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                List<int> matches = priorities.Where(x => initializable.GetType().DerivesFromOrEqual(x.First))
                                              .Select(x => x.Second)
                                              .ToList();
                
                int priority = matches.IsEmpty() ? 0 : matches.Distinct().Single();

                this.initializables.Add(new InitializableInfo(initializable, priority));
            }
            
            for (var i = 0; i < lateInitializables.Count; i++)
            {
                ILateInitializable lateInitializable = lateInitializables[i];

                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                List<int> matches = latePriorities.Where(x => lateInitializable.GetType().DerivesFromOrEqual(x.First))
                                                  .Select(x => x.Second)
                                                  .ToList();
                
                int priority = matches.IsEmpty() ? 0 : matches.Distinct().Single();
                this.lateInitializables.Add(new LateInitializableInfo(lateInitializable, priority));
            }
        }

        public void Add(IInitializable initializable)
        {
            Add(initializable, 0);
        }

        public void Add(IInitializable initializable, int priority)
        {
            Assert.That(!hasInitialized);
            initializables.Add(
                new InitializableInfo(initializable, priority));
        }

        public void AddLate(ILateInitializable initializable)
        {
            AddLate(initializable, 0);
        }

        public void AddLate(ILateInitializable initializable, int priority)
        {
            lateInitializables.Add(new LateInitializableInfo(initializable, priority));
        }

        public void Initialize()
        {
            Assert.That(!hasInitialized);
            hasInitialized = true;
            initializables = initializables.OrderBy(x => x.priority).ToList();

            #if UNITY_EDITOR
            foreach (IInitializable initializable in initializables.Select(x => x.initializable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate IInitializable with type '{0}'".Fmt(initializable.GetType()));
            }
            #endif

            foreach (InitializableInfo initializable in initializables)
            {
                try
                {
                    #if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
                    #endif
                    #if UNITY_EDITOR
                    using (ProfileBlock.Start("{0}.Initialize()", initializable.initializable.GetType()))
                    #endif
                    {
                        initializable.initializable.Initialize();
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(e, "Error occurred while initializing IInitializable with type '{0}'", 
                                                 initializable.initializable.GetType());
                }
            }
        }

        public void LateInitialize()
        {
            Assert.That(!hasLateInitialized);
            hasLateInitialized = true;
            lateInitializables = lateInitializables.OrderBy(x => x.priority).ToList();

            #if UNITY_EDITOR
            foreach (ILateInitializable initializable in lateInitializables.Select(x => x.lateInitializable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate ILateInitializable with type '{0}'".Fmt(initializable.GetType()));
            }
            #endif

            foreach (LateInitializableInfo initializable in lateInitializables)
            {
                try
                {
                    #if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
                    #endif
                    #if UNITY_EDITOR
                    using (ProfileBlock.Start("{0}.LateInitialize()", initializable.lateInitializable.GetType()))
                        #endif
                    {
                        initializable.lateInitializable.LateInitialize();
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(e, "Error occurred while late initializing ILateInitializable with type '{0}'", 
                                                 initializable.lateInitializable.GetType());
                }
            }
        }

        private sealed class InitializableInfo
        {
            public readonly IInitializable initializable;
            public readonly int priority;

            public InitializableInfo(IInitializable initializable, int priority)
            {
                this.initializable = initializable;
                this.priority = priority;
            }
        }
        
        private sealed class LateInitializableInfo
        {
            public readonly ILateInitializable lateInitializable;
            public readonly int priority;

            public LateInitializableInfo(ILateInitializable lateInitializable, int priority)
            {
                this.lateInitializable = lateInitializable;
                this.priority = priority;
            }
        }
    }
}
