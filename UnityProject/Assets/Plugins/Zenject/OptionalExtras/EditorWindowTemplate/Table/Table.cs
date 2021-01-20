namespace Zenject.EditorWindowTemplate
{
    public sealed class Table<T>
    {
        public readonly ColumnData<T>[] columns;
        public readonly OptionData<T>[] options;

        public Table(ColumnData<T>[] columns, OptionData<T>[] options)
        {
            this.columns = columns;
            this.options = options;
        }
    }
}