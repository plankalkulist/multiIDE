namespace multiIDE
{
    public sealed class InputPortTypeInfo : ComponentTypeInfo
    {
        public InputPortTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string defaultName, string version, string author, string description
            , string sourceFileName, object tag = null)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , defaultName, version, author, description
                , sourceFileName, tag)
        { }

        public InputPortTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string sourceFileName, object tag
            , IInputPort componentExample)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , componentExample.DefaultName, componentExample.Version, componentExample.Author, componentExample.Description
                , sourceFileName, tag)
        { }

        public override object Clone()
        {
            var inputPortTypeInfo = new InputPortTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag);
            inputPortTypeInfo.IsHidden = this.IsHidden;
            return inputPortTypeInfo;
        }
    }
}
