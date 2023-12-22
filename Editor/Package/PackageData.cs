using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.Package
{
    [Serializable]
    public class PackageData
    {
        public string title;
        public string name;
        public string version;
        public string url;
        public string recommended;

        [NonSerialized] public DependencyInfo[] dependenceis;
        [NonSerialized] public List<string> versions;
        [NonSerialized] public bool isWaiting = false;
        [NonSerialized] private int end = 11;
        [NonSerialized] private int cur = 0;
        [NonSerialized] public string icon = "";

        public static PackageData OnCreate(PackageInfo info)
        {
            PackageData packageData = new PackageData();
            packageData.name = info.name;
            packageData.version = info.version;
            packageData.title = info.displayName;
            packageData.recommended = info.versions.latestCompatible;
            packageData.dependenceis = info.dependencies;
            string[] split = info.packageId.Split("@");
            packageData.url = split[1].StartsWith("https") ? split[1] : split[0];
            return packageData;
        }

        public static PackageData OnCreate(string url)
        {
            return new PackageData()
            {
                title = url,
            };
        }

        public void ShowWaiting()
        {
            cur = 0;
            isWaiting = true;
            EditorManager.StartCoroutine(OnShow());
        }

        public void CloseWaiting()
        {
            isWaiting = false;
        }

        IEnumerator OnShow()
        {
            while (isWaiting)
            {
                string temp = "WaitSpin";
                if (cur < 10)
                {
                    temp += "0" + cur;
                }
                else
                {
                    temp += cur;
                }

                icon = temp;
                EditorManager.Refresh();
                yield return new EditorWaitForSeconds(0.1f);
                cur++;
                cur %= end;
            }
        }
    }
}