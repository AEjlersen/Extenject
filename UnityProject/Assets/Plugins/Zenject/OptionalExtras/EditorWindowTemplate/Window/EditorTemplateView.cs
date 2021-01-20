using System.Collections.Generic;
using UnityEditor;

namespace Zenject.EditorWindowTemplate
{
    public abstract class EditorTemplateView<T> : IGuiRenderable, ITickable, IInitializable
    {
        private readonly EditorWindow window;

        private Table<T> table;
        protected TableDrawer<T> drawer;
        
        public EditorTemplateView(EditorWindow window)
        {
            this.window = window;
        }

        public void Initialize()
        {
            table = ConstructTable();
            drawer = new TableDrawer<T>(window, this, table);
            PostInitialize();
        }

        protected abstract Table<T> ConstructTable();
        protected abstract void PostInitialize();
        public abstract void PopulateList(List<T> list);
        
        public void Tick()
        {
            drawer.Tick();
        }

        public void GuiRender()
        {
            drawer.Draw();
        }
    }
}
