using UnityEngine;

namespace ZGame.Editor.ResBuild
{
    public class ResBuilder : PageScene
    {
        public override string name { get; } = "资源";


        public ResBuilder(Docker window) : base(window)
        {
            SubScenes.Add(new ResRuleSeting(window));
            SubScenes.Add(new ResBuildSeting(window));
            SubScenes.Add(new ResUploader(window));
        }
        
        

        public override void OnGUI(Rect rect)
        {
        }
    }
}