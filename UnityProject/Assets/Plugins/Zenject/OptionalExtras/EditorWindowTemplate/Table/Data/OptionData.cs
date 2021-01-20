using System;

namespace Zenject.EditorWindowTemplate
{
    public sealed class OptionData<T>
    {
        public readonly string title;
        public readonly Action<T> action;

        public OptionData(string title, Action<T> action)
        {
            this.title = title;
            this.action = action;
        }
    }
}