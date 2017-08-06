using System;

namespace multiIDE
{
    public interface IComponentTypeInfo : ICloneable
    {
        string AssemblyQualifiedTypeName { get; }
        string TypeName { get; }
        string TypeFullName { get; }
        string DefaultName { get; }
        string Version { get; }
        string Author { get; }
        string Description { get; }
        string SourceFileName { get; }
        //
        object Tag { get; set; }
        bool IsHidden { get; set; }
    }
}
