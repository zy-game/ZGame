using System;
using System.Collections;
using System.Collections.Generic;

namespace ZGame.Editor.Package
{
    class GitNetClient
    {
        public static IEnumerator GetPackageList(string url, Action<List<ProjectPackageData>> callback)
        {
            //todo 如果ur为空则表示获取unity的包列表，否则就是取git的版本列表
            yield break;
        }

        public static IEnumerator GetPackage(string name, Action<ProjectPackageData> callback)
        {
            yield break;
        }

        public static IEnumerator AddPackage(string name, string version, Action<ProjectPackageData> callback)
        {
            yield break;
        }

        public static IEnumerator RemovePackage(string name, Action<bool> callback)
        {
            yield break;
        }
    }
}