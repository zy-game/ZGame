namespace ZGame.Editor.CodeGen
{
    interface IPropertyCodeGen : ICodeGen
    {
        IClassCodeGen owner { get; }
    }
}