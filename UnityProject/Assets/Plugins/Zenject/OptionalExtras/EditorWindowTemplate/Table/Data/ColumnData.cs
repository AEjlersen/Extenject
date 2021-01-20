using System;

namespace Zenject.EditorWindowTemplate
{
    public sealed class ColumnData<T>
    {
        public delegate IComparable OnGetValue(T t);
        
        public readonly string title;
        public readonly OnGetValue action;

        public ColumnData(string title, OnGetValue action)
        {
            this.title = title;
            this.action = action;
        }
    }
}