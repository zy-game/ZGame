using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            root = new OSSObject(options.bucket, true);
            List<OSSObject> files = api.GetObjectList();
            foreach (var VARIABLE in files)
            {
                AddFile(VARIABLE);
            }

            if (Application.dataPath.IsNullOrEmpty())
            {
                return;
            }

            //todo 读取本地文件
            string[] localList = Directory.GetFiles(BuilderConfig.output, "*.*", SearchOption.AllDirectories);
            foreach (var VARIABLE in localList)
            {
                string temp = VARIABLE.Substring(BuilderConfig.output.Length);
                AddFile(new OSSObject(temp));
            }
        }

        public void Upload(Action completion)
        {
            IEnumerator Start()
            {
                yield return this.root.OnUpload(api);
                completion?.Invoke();
            }

            WindowDocker.StartCoroutine(Start());
        }

        public void Download(Action completion)
        {
            IEnumerator Start()
            {
                yield return root.OnDownload(api);
                completion?.Invoke();
            }

            WindowDocker.StartCoroutine(Start());
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