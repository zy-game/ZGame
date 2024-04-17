namespace ZGame.Editor.CodeGen
{
    interface IClassCodeGen : ICodeGen
    {
        IClassCodeGen parent { get; }
        void SetInherit(params string[] args);
        IClassCodeGen AddClass(string className, string desc, ACL acl);
        IMethodCodeGen AddConstructor(ParamsList args);
        IClassCodeGen SwitchClass(string className);
        IMethodCodeGen SwitchMethod(string methodName);
        void AddProperty(string propertyName, string propertyType, string desc, ACL acl = ACL.Public);
        IMethodCodeGen AddMethod(string methodName, bool isStatic, bool isOverride, string desc, string returnType = "", ACL acl = ACL.Public, ParamsList paramsList = null);
    }
}