using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ZGame
{
    public static partial class Extension
    {

        public static void SetParent(this GameObject gameObject, Transform parent)
        {
            gameObject.transform.SetParent(parent);
        }
        
        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position,Vector3 rotation,Vector3 scale)
        {
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
        }

        public static string ToBase64String(this AudioClip clip)
        {
            int position = clip.samples;
            float[] soundata = new float[position * clip.channels];
            clip.GetData(soundata, 0);
            int rescaleFactor = 32767;
            byte[] outData = new byte[soundata.Length * 2];
            for (int i = 0; i < soundata.Length; i++)
            {
                short temshort = (short)(soundata[i] * rescaleFactor);
                byte[] temdata = BitConverter.GetBytes(temshort);
                outData[i * 2] = temdata[0];
                outData[i * 2 + 1] = temdata[1];
            }

            return Convert.ToBase64String(outData);
        }

        /// <summary>
        /// 获取真正大小的录音
        /// </summary>
        /// <param name="recordedClip"></param>
        /// <returns></returns>
        public static byte[] GetRealAudio(this AudioClip recordedClip)
        {
            int position = recordedClip.samples;
            float[] soundata = new float[position * recordedClip.channels];
            recordedClip.GetData(soundata, 0);
            recordedClip = AudioClip.Create(recordedClip.name, position, recordedClip.channels, recordedClip.frequency, false);
            recordedClip.SetData(soundata, 0);
            int rescaleFactor = 32767;
            byte[] outData = new byte[soundata.Length * 2];
            for (int i = 0; i < soundata.Length; i++)
            {
                short temshort = (short)(soundata[i] * rescaleFactor);
                byte[] temdata = BitConverter.GetBytes(temshort);
                outData[i * 2] = temdata[0];
                outData[i * 2 + 1] = temdata[1];
            }

            //Debug.Log("position=" + position + "  outData.leng=" + outData.Length);
            return outData;
        }

        /// <summary>
        /// 写文件头
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="clip"></param>
        public static void WriteHeader(MemoryStream stream, AudioClip clip)
        {
            int hz = clip.frequency;

            int channels = clip.channels;
            int samples = clip.samples;

            stream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            stream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(stream.Length - 8);
            stream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            stream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            stream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            stream.Write(subChunk1, 0, 4);

            UInt16 one = 1;
            UInt16 two = 2;

            Byte[] audioFormat = BitConverter.GetBytes(one);
            stream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(channels);
            stream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(hz);
            stream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
            stream.Write(byteRate, 0, 4);

            UInt16 blockAlign = (ushort)(channels * 2);
            stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            stream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            stream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            stream.Write(subChunk2, 0, 4);
        }

        public static Texture2D Screenshot(this Camera camera, int width, int height, GameObject gameObject)
        {
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            Vector3 scale = camera.transform.localScale;
            float view = camera.fieldOfView;

            camera.SetupCameraFocusAndMaximizeTargetObject(gameObject);
            RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
            camera.targetTexture = renderTexture;
            RenderTexture.active = camera.targetTexture;
            camera.Render();
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.name = gameObject.name.Replace("(Clone)", "");
            texture.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;
            camera.transform.position = position;
            camera.transform.rotation = rotation;
            camera.transform.localScale = scale;
            camera.fieldOfView = view;
            return texture;
        }

        public static void SetupCameraFocusAndMaximizeTargetObject(this Camera camera, GameObject gameObject)
        {
            if (camera == null || gameObject == null)
            {
                return;
            }

            Bounds meshBound = GetGameObjectBoundSize(gameObject); //renderer.sharedMesh.bounds;
            float max = Mathf.Max(meshBound.extents.x, meshBound.extents.y, meshBound.extents.z);
            camera.transform.position = new Vector3(0, meshBound.center.y, camera.transform.position.z);
            float distance = Vector3.Distance(camera.transform.position, gameObject.transform.position);
            camera.fieldOfView = 2.0f * Mathf.Atan(max / distance) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// 获取物体包围盒
        /// </summary>
        /// <param name="obj">父物体</param>
        /// <returns>物体包围盒</returns>
        public static Bounds GetGameObjectBoundSize(this GameObject obj)
        {
            var bounds = new Bounds();
            if (obj == null)
            {
                return bounds;
            }

            var renders = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            var filter = obj.GetComponentsInChildren<MeshFilter>();
            var boundscenter = Vector3.zero;
            int count = 0;
            if (filter is not null && filter.Length > 0)
            {
                foreach (var VARIABLE in filter)
                {
                    // VARIABLE.sharedMesh.RecalculateBounds();
                    boundscenter += VARIABLE.sharedMesh.bounds.center;
                    Debug.Log(VARIABLE.sharedMesh.bounds);
                    count++;
                }
            }

            if (renders is not null && renders.Length > 0)
            {
                foreach (var item in renders)
                {
                    item.sharedMesh.RecalculateBounds();
                    boundscenter += item.bounds.center;
                    count++;
                }
            }

            if (count > 0)
            {
                boundscenter /= count;
            }

            bounds = new Bounds(boundscenter, Vector3.zero);
            foreach (var item in renders)
            {
                bounds.Encapsulate(item.bounds);
            }

            foreach (var item in filter)
            {
                bounds.Encapsulate(item.sharedMesh.bounds);
            }

            return bounds;
        }

        public static Color ToColor(this string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
        }

        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static T GetData<T>(this UnityWebRequest request)
        {
            if (request.result is not UnityWebRequest.Result.Success)
            {
                return default;
            }

            object _data = default;
            if (typeof(T) == typeof(string))
            {
                _data = request.downloadHandler.text;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                _data = request.downloadHandler.data;
            }
            else
            {
                _data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }

            return (T)_data;
        }

        public static void OnDestroyEventCallback(this GameObject gameObject, UnityAction callback)
        {
            BevaviourScriptable listener = gameObject.GetComponent<BevaviourScriptable>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<BevaviourScriptable>();
            }

            listener.onDestroy.AddListener(callback);
        }

        public static string Replace(this string value, params string[] t)
        {
            foreach (var VARIABLE in t)
            {
                value = value.Replace(VARIABLE, String.Empty);
            }

            return value;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static Type GetType(this AppDomain domain, string name)
        {
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    if (VARIABLE2.Name == name || VARIABLE2.FullName == name)
                    {
                        return VARIABLE2;
                    }
                }
            }

            return default;
        }

        public static List<Type> GetAllSubClasses<T>(this AppDomain domain)
        {
            return domain.GetAllSubClasses(typeof(T));
        }


        public static List<Type> GetAllSubClasses(this AppDomain domain, Type parent)
        {
            List<Type> result = new List<Type>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    if (parent.IsAssignableFrom(VARIABLE2) && VARIABLE2.IsInterface is false && VARIABLE2.IsAbstract is false)
                    {
                        result.Add(VARIABLE2);
                    }
                }
            }

            return result;
        }

        public static List<Type> GetAllSubClasses<T>(this Assembly assembly)
        {
            List<Type> result = new List<Type>();
            Type baseType = typeof(T);
            foreach (var VARIABLE in assembly.GetTypes())
            {
                if (baseType.IsAssignableFrom(VARIABLE) && VARIABLE.IsInterface is false && VARIABLE.IsAbstract is false)
                {
                    result.Add(VARIABLE);
                }
            }

            return result;
        }

        public static Dictionary<Type, T> GetCustomAttributeMap<T>(this AppDomain domain) where T : Attribute
        {
            Dictionary<Type, T> map = new Dictionary<Type, T>();
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    T attribute = type.GetCustomAttribute<T>();
                    if (attribute is null)
                    {
                        continue;
                    }

                    map.Add(type, attribute);
                }
            }

            return map;
        }

        public static List<T> GetCustomAttributes<T>(this AppDomain domain) where T : Attribute
        {
            List<T> result = new List<T>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    T[] attribute = VARIABLE2.GetCustomAttributes<T>().ToArray();

                    if (attribute != null && attribute.Length > 0)
                    {
                        result.AddRange(attribute);
                    }
                }
            }

            return result;
        }

        public static List<Type> GetCustomAttributesWithoutType<T>(this AppDomain domain) where T : Attribute
        {
            List<Type> result = new List<Type>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    T[] attribute = VARIABLE2.GetCustomAttributes<T>().ToArray();

                    if (attribute != null && attribute.Length > 0)
                    {
                        result.Add(VARIABLE2);
                    }
                }
            }

            return result;
        }
    }
}