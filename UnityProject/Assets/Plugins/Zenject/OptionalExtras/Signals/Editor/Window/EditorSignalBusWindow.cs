using UnityEditor;
using UnityEngine;
using Zenject.EditorWindowTemplate;

namespace Zenject.MemoryPoolMonitor
{
    public sealed class EditorSignalBusWindow : EditorTemplateWindow<EditorSignalBusView, SignalBus>
    {
        [MenuItem("Window/Zenject/Signal Bus Monitor")]
        public static EditorSignalBusWindow GetOrCreateWindow()
        {
            var window = GetWindow<EditorSignalBusWindow>();
            window.titleContent = new GUIContent("Signal Bus Monitor");
            return window;
        }
    }
}