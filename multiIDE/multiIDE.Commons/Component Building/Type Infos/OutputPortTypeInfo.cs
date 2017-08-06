namespace multiIDE
{
    public sealed class OutputPortTypeInfo : ComponentTypeInfo
    {
        public OutputPortTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string defaultName, string version, string author, string description
            , string sourceFileName, object tag = null)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , defaultName, version, author, description
                , sourceFileName, tag)
        { }

        public OutputPortTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string sourceFileName, object tag
            , IOutputPort componentExample)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , componentExample.DefaultName, componentExample.Version, componentExample.Author, componentExample.Description
                , sourceFileName, tag)
        { }

        public override object Clone()
        {
            var outputPortTypeInfo = new OutputPortTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag);
            outputPortTypeInfo.IsHidden = this.IsHidden;
            return outputPortTypeInfo;
        }
    }
}
