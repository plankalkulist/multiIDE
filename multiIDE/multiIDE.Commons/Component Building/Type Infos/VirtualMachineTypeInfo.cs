using System.Collections.Generic;

namespace multiIDE
{
    public sealed class VirtualMachineTypeInfo : ComponentTypeInfo
    {
        public string TargetLanguage { get; private set; }
        public List<string> SupportedLanguages { get; private set; }
        public string BaseLanguage { get; private set; }
        public string ProgramFileFilter { get; private set; }

        public VirtualMachineTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
                            , string defaultName, string version, string author, string description
                            , string sourceFileName, object tag
                            , string targetLanguage, List<string> supportedLanguages, string baseLanguage, string programFileFilter)
                : base(assemblyQualifiedTypeName, typeName, typeFullName
                            , defaultName, version, author, description
                            , sourceFileName, tag)
        {
            TargetLanguage = targetLanguage;
            SupportedLanguages = supportedLanguages;
            BaseLanguage = baseLanguage;
            ProgramFileFilter = programFileFilter;
        }

        public VirtualMachineTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
            , string sourceFileName, object tag
            , IVirtualMachine componentExample)
            : base(assemblyQualifiedTypeName, typeName, typeFullName
                , componentExample.DefaultName, componentExample.Version, componentExample.Author, componentExample.Description
                , sourceFileName, tag)
        {
            TargetLanguage = componentExample.TargetLanguage;
            SupportedLanguages = new List<string>(componentExample.SupportedLanguages.Split(';', ','));
            BaseLanguage = componentExample.BaseLanguage;
            ProgramFileFilter = componentExample.ProgramFileFilter;
        }

        public override object Clone()
        {
            var virtualMachineTypeInfo = new VirtualMachineTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag
                , TargetLanguage, SupportedLanguages, BaseLanguage, ProgramFileFilter);
            virtualMachineTypeInfo.IsHidden = this.IsHidden;
            return virtualMachineTypeInfo;
        }
    }
}
