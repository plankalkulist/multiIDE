namespace multiIDE
{
    public sealed class InputDeviceTypeInfo : ComponentTypeInfo
    {
        public InputDeviceTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string defaultName, string version, string author, string description
            , string sourceFileName, object tag = null)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , defaultName, version, author, description
                , sourceFileName, tag)
        { }

        public InputDeviceTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string sourceFileName, object tag
            , IInputDevice componentExample)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , componentExample.DefaultName, componentExample.Version, componentExample.Author, componentExample.Description
                , sourceFileName, tag)
        { }

        public override object Clone()
        {
            var inputDeviceTypeInfo = new InputDeviceTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag);
            inputDeviceTypeInfo.IsHidden = this.IsHidden;
            return inputDeviceTypeInfo;
        }
    }
}
