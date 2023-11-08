using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class ResRuleSeting : SubPageScene
    {
        public override string name { get; } = "规则管理";
        private ResourceBuildRulerConfig _config;

        public ResRuleSeting(Docker window) : base(window)
        {
            window.SetupOpenAssetCallback(typeof(ResourceBuildRulerConfig), OnOpenAssetCallback);
        }

        private void OnOpenAssetCallback(Object target)
        {
            docker.SwitchPageScene(this);
            _config = (ResourceBuildRulerConfig)target;
        }
    }
}