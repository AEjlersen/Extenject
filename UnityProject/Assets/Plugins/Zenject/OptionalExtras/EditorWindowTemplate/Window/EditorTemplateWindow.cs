namespace Zenject.EditorWindowTemplate
{
    public abstract class EditorTemplateWindow<T0, T1> : ZenjectEditorWindow where T0 : EditorTemplateView<T1>
    {
        public override void InstallBindings()
        {
            Container.BindInstance(this);
            Container.BindInterfacesTo<T0>()
                     .AsSingle();
        }
    }
}
