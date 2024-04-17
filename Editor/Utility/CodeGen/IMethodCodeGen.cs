namespace ZGame.Editor.CodeGen
{
    interface IMethodCodeGen : ICodeGen
    {
        IClassCodeGen owner { get; }
    }
}