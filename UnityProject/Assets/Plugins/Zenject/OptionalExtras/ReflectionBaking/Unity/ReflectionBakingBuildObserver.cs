using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ModestTree;
using UnityEditor;
using UnityEditor.Compilation;
using Zenject.ReflectionBaking.Mono.Cecil;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;

namespace Zenject.ReflectionBaking
{
    public static class ReflectionBakingBuildObserver
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
        }

        static void OnAssemblyCompiled(string assemblyAssetPath, CompilerMessage[] messages)
        {
            #if !UNITY_2018_1_OR_NEWER
            if (Application.isEditor && !BuildPipeline.isBuildingPlayer)
                return;
            #endif

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WSAPlayer)
                TryWeaveAssembly(assemblyAssetPath);
        }

        private static void TryWeaveAssembly(string assemblyAssetPath)
        {
            ZenjectReflectionBakingSettings settings = ReflectionBakingInternalUtil.TryGetEnabledSettingsInstance();

            if (settings == null)
                return;
            if (settings.AllGeneratedAssemblies && settings.ExcludeAssemblies.Contains(assemblyAssetPath))
                return;
            if (!settings.AllGeneratedAssemblies && !settings.IncludeAssemblies.Contains(assemblyAssetPath))
                return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            string assemblyFullPath = ReflectionBakingInternalUtil.ConvertAssetPathToSystemPath(assemblyAssetPath);

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = new UnityAssemblyResolver(),
            };

            ModuleDefinition module = ModuleDefinition.ReadModule(assemblyFullPath, readerParameters);
            List<string> assemblyRefNames = module.AssemblyReferences.Select(x => x.Name.ToLower()).ToList();

            if (!assemblyRefNames.Contains("zenject-usage"))
            {
                // Zenject-usage is used by the generated methods
                // Important that we do this check otherwise we can corrupt some dlls that don't have access to it
                return;
            }

            string assemblyName = Path.GetFileNameWithoutExtension(assemblyAssetPath);
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                                         .Where(x => x.GetName().Name == assemblyName)
                                         .OnlyOrDefault();

            Assert.IsNotNull(assembly, "Could not find unique assembly '{0}' in currently loaded list of assemblies", assemblyName);

            int numTypesChanged = ReflectionBakingModuleEditor.WeaveAssembly(
                module, assembly, settings.NamespacePatterns);

            if (numTypesChanged > 0)
            {
                var writerParams = new WriterParameters();
                module.Write(assemblyFullPath, writerParams);

                Debug.Log("Added reflection baking to '{0}' types in assembly '{1}', took {2:0.00} seconds"
                    .Fmt(numTypesChanged, Path.GetFileName(assemblyAssetPath), stopwatch.Elapsed.TotalSeconds));
            }
        }
    }
}
