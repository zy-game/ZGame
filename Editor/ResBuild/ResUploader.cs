using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [Options(typeof(UploadSeting))]
    [BindScene("版本管理", typeof(ResBuilder))]
    public class ResUploader : PageScene
    {
    }
}