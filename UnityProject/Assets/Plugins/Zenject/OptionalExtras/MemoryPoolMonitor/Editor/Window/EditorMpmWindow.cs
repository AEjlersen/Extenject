using UnityEditor;
using UnityEngine;
using Zenject.EditorWindowTemplate;

namespace Zenject.MemoryPoolMonitor
{
    public sealed class EditorMpmWindow : EditorTemplateWindow<EditorMpmView, IMemoryPool>
    {
        [MenuItem("Window/Zenject/Pool Monitor")]
        public static EditorMpmWindow GetOrCreateWindow()
        {
            var window = GetWindow<EditorMpmWindow>();
            window.titleContent = new GUIContent("Pool Monitor");
            return window;
        }
    }
}