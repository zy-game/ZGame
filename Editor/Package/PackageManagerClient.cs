using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Editor.Package
{
    public class Commit
    {
        /// <summary>
        /// 
        /// </summary>
        public string sha { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string url { get; set; }
    }

    public class GitPackage
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zipball_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string tarball_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Commit commit { get; set; }
    }

    class PackageManagerClient
    {



        public static void GetPackageVersionList(string owner, string repo, Action<List<string>> callback)
        {
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