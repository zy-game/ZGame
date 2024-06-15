using System;

namespace ZGame.Editor
{
    public abstract class SubEditorSceneWindow : EditorSceneWindow
    {
        public abstract Type owner { get; }

     
    }
}