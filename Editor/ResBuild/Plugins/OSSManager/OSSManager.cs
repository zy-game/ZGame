using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class OSSManager
    {
        private OSSApi api;
        public OSSObject root { get; private set; }

        public OSSManager()
        {
        }

        public void Refresh(OSSOptions options)
        {
            api = options.type switch
            {
                OSSType.Aliyun => new AliyunApi(options),
                OSSType.Tencent => new TencentApi(options),
                _ => null
            };
            root?.Dispose();
            root = new OSSObject(options.bucket);
            List<OSSObject> files = api.GetFileList();
            foreach (var VARIABLE in files)
            {
                AddFile(VARIABLE);
            }

            if (BuilderConfig.instance.output.IsNullOrEmpty())
            {
                return;
            }

            //todo 读取本地文件
            string ex = "*" + BuilderConfig.instance.fileExtension ?? ".*";
            string[] localList = Directory.GetFiles(BuilderConfig.instance.output, ex, SearchOption.AllDirectories);
            foreach (var VARIABLE in localList)
            {
                string temp = VARIABLE.Substring(BuilderConfig.instance.output.Length + 1);
                AddFile(new OSSObject(temp));
            }
        }

        public void Upload()
        {
        }

        public void Download()
        {
        }

        public void Dispose()
        {
            root.Dispose();
        }

        public void AddFile(OSSObject file)
        {
            OSSObject folder = GetFolder(file.path);
            folder.Add(file);
        }

        public OSSObject GetFolder(string path)
        {
            string[] splits = path.Replace('\\', '/').Split('/');
            OSSObject folder = root;
            foreach (var VARIABLE in splits)
            {
                OSSObject temp = folder.childs.Find(x => x.name == VARIABLE && x.isFolder);
                if (temp == null)
                {
                    folder.Add(temp = new OSSObject(VARIABLE, true));
                }

                folder = temp;
            }

            return folder;
        }
    }
}