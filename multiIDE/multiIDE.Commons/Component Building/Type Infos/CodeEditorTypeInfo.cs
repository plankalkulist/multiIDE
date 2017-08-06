namespace multiIDE
{
    public sealed class CodeEditorTypeInfo : ComponentTypeInfo
    {
        public CodeEditorTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
                            , string defaultName, string version, string author, string description
                            , string sourceFileName, object tag = null)
                : base(assemblyQualifiedTypeName, typeName, typeFullName
                            , defaultName, version, author, description
                            , sourceFileName, tag)
        { }

        public CodeEditorTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string sourceFileName, object tag
            , ICodeEditor componentExample)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , componentExample.DefaultName, componentExample.Version, componentExample.Author, componentExample.Description
                , sourceFileName, tag)
        { }

        public override object Clone()
        {
            var codeEditorTypeInfo = new CodeEditorTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag);
            codeEditorTypeInfo.IsHidden = this.IsHidden;
            return codeEditorTypeInfo;
        }
    }
}
