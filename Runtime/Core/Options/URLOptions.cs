using System;
using UnityEngine;
using UnityEngine.Serialization;
using ZEngine;

[Serializable]
public sealed class URLOptions
{
    [Header("是否启用")] public Switch state;
    [Header("别称")] public string name;
    [Header("地址")] public string address;
}