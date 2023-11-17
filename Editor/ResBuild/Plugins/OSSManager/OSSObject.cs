using System;
using System.Collections.Generic;
using System.IO;

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

        public void Dispose()
        {
            childs.ForEach(x => x.Dispose());
            childs.Clear();
        }
    }
}