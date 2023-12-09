using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class OSSObject : IDisposable
    {
        public bool foldout;
        public bool isOn;


        public string name;
        public string path;
        public bool isFolder = false;
        public OSSObject parent { get; private set; }
        public List<OSSObject> childs = new List<OSSObject>();

        public string fullPath => path + "/" + name;

        public string localPath => BuilderConfig.output + fullPath;

        public float progrss;

        public OSSObject(string path, bool isFolder = false)
        {
            this.path = Path.GetDirectoryName(path);
            this.name = Path.GetFileName(path);
            this.isFolder = isFolder;
        }


        public void Add(OSSObject file)
        {
            if (childs.Find(x => x.name == file.name) is not null)
            {
                return;
            }

            childs.Add(file);
        }

        public void OnSelection(bool state)
        {
            isOn = state;
            childs.ForEach(x => x.OnSelection(state));
        }

        public IEnumerator OnUpload(OSSApi api)
        {
            foreach (var VARIABLE in childs)
            {
                yield return VARIABLE.OnUpload(api);
            }

            if (this.isFolder)
            {
                yield break;
            }

            if (File.Exists(localPath) is false)
            {
                yield break;
            }

            bool state = false;
            api.Upload(this, args =>
            {
                progrss = args;
                state = args >= 1;
            });
            while (state is false)
            {
                yield return new WaitForSeconds(0.05f);
                EditorManager.Refresh();
            }

            progrss = 0;
        }


        public IEnumerator OnDownload(OSSApi api)
        {
            foreach (var VARIABLE in childs)
            {
                yield return VARIABLE.OnDownload(api);
            }

            if (isFolder)
            {
                yield break;
            }

            bool state = false;
            api.Download(this, args =>
            {
                progrss = args;
                state = args >= 1;
                EditorManager.Refresh();
            });
            while (state is false)
            {
                yield return new WaitForSeconds(0.05f);
                EditorManager.Refresh();
            }

            progrss = 0;
        }

        public void Dispose()
        {
            childs.ForEach(x => x.Dispose());
            childs.Clear();
        }
    }
}