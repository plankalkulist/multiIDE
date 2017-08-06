namespace multiIDE
{
    public sealed class OutputDeviceTypeInfo : ComponentTypeInfo
    {
        public OutputDeviceTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string defaultName, string version, string author, string description
            , string sourceFileName, object tag = null)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , defaultName, version, author, description
                , sourceFileName, tag)
        { }

        public OutputDeviceTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string sourceFileName, object tag
            , IOutputDevice componentExample)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , componentExample.DefaultName, componentExample.Version, componentExample.Author, componentExample.Description
                , sourceFileName, tag)
        { }

        public override object Clone()
        {
            var outputDeviceTypeInfo = new OutputDeviceTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag);
            outputDeviceTypeInfo.IsHidden = this.IsHidden;
            return outputDeviceTypeInfo;
        }
    }
}
