using Inworld.Grpc;
using UnityEngine;

namespace Inworld
{
    public class InworldConfig
    {
        public string workspace;
        public string password;
        public string key;
        public string secret;
        public string scene;

        public const string RuntimeServer = "api-engine.inworld.ai:443";
        public const string StudioServer = "api-studio.inworld.ai:443";
        private const string DEFAULT_PASSWORD = "aHVON0tmYTR0TXlFSHpsMUE0cHAya3JPNUNBdmRmMWU6T2UwVG9FN3cwU3BVZzNVam9DRno4YmxDSjgydHZVb3laMHBRMUdkTHpqenRkekdjY3BmenBvdmplNmY2NE1UTQ==";
        private const string DEFAULT_KEY = "huN7Kfa4tMyEHzl1A4pp2krO5CAvdf1e";
        private const string DEFAULT_SECRET = "Oe0ToE7w0SpUg3UjoCFz8blCJ82tvUoyZ0pQ1GdLzjztdzGccpfzpovje6f64MTM";

        public InworldConfig(string workspace) : this(workspace, "workspaces/mogaverseaichat/scenes/metavevrsaichat")
        {
        }

        public InworldConfig(string workspace, string scene) : this(workspace, scene, DEFAULT_PASSWORD, DEFAULT_KEY, DEFAULT_SECRET)
        {
        }

        public InworldConfig(string workspace, string scene, string password, string key, string secret)
        {
            this.workspace = workspace;
            this.password = password;
            this.secret = secret;
            this.scene = scene;
            this.key = key;
        }
    }
}