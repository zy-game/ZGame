using System;

namespace ZGame.Resource
{
    class VFSData
    {
        public string vfs;
        public string file;
        public uint version;
        public int offset;
        public int lenght;
        public bool use;
        public int index;

        public void Unuse()
        {
            file = String.Empty;
            version = 0;
            lenght = 0;
            use = false;
            index = 0;
        }

        public void Use(string name, uint version, int lenght, int index)
        {
            this.file = name;
            this.version = version;
            this.lenght = lenght;
            this.use = true;
            this.index = index;
        }

        public override string ToString()
        {
            return $"VFS:{vfs} FILE:{file} SORT:{index} LENGTH:{lenght}";
        }

        public static VFSData Create(string vfs, int offset)
        {
            return new VFSData()
            {
                vfs = vfs,
                offset = offset,
                use = false,
                lenght = 0,
                file = String.Empty,
                index = 0,
                version = 0
            };
        }
    }
}