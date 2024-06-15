namespace ZGame.Editor
{
    public abstract class EditorSceneWindow
    {
        public abstract string name { get; }

        public virtual void Awake()
        {
        }

        public virtual void OnGUI()
        {
        }

        public virtual void Release()
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnSave()
        {
        }

        public virtual void OnDrawToolbar()
        {
        }

        public virtual void OnScriptCompiled()
        {
        }

        public virtual void SetTitleName(string name)
        {
        }

    }
}