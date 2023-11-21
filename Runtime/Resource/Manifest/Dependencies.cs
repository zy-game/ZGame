using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Resource
{
    [Serializable]
    public class Dependencies
    {
        public string name;
        public uint version;
        public string owner;
    }
}