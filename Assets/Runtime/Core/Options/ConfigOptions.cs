using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigOptions : Attribute
{
    public enum Localtion
    {
        Internal,
        Project,
        Packaged,
    }

    internal string filePath;
    internal Localtion localtion;

    public ConfigOptions(string path, Localtion localtion)
    {
        this.filePath = path;
        this.localtion = localtion;
    }
}

[Serializable]
public sealed class URLOptions
{
    public bool isOn;
    public string name;
    public string address;
    public ushort port;
}