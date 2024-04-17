namespace ZGame.Editor.CodeGen
{
    interface ICodeGen
    {
        string name { get; }
        void WriterLine(string code);
        void BeginCodeScope(string code);
        void EndCodeScope(string code = "");
    }
}