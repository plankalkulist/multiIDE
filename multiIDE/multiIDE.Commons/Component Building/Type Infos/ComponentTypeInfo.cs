namespace multiIDE
{
    public class ComponentTypeInfo : IComponentTypeInfo
    {
        public string AssemblyQualifiedTypeName { get; protected set; }
        public string TypeName { get; protected set; }
        public string TypeFullName { get; protected set; }
        public string DefaultName { get; protected set; }
        public string Version { get; protected set; }
        public string Author { get; protected set; }
        public string Description { get; protected set; }
        public string SourceFileName { get; protected set; }
        //
        public object Tag { get; set; }
        public bool IsHidden { get; set; }

        public ComponentTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
                            , string defaultName, string version, string author, string description
                            , string sourceFileName, object tag = null)
        {
            AssemblyQualifiedTypeName = assemblyQualifiedTypeName;
            TypeName = typeName;
            TypeFullName = typeFullName;
            DefaultName = defaultName;
            Version = version;
            Author = author;
            Description = description;
            SourceFileName = sourceFileName;
            Tag = tag;
            //
            IsHidden = false;
        }

        public ComponentTypeInfo(string assemblyQualifiedTypeName, string typeName, string typeFullName
                , string sourceFileName, object tag
                , IComponent componentExample)
        {
            AssemblyQualifiedTypeName = assemblyQualifiedTypeName;
            TypeName = typeName;
            TypeFullName = typeFullName;
            DefaultName = componentExample.DefaultName;
            Version = componentExample.Version;
            Author = componentExample.Author;
            Description = componentExample.Description;
            SourceFileName = sourceFileName;
            Tag = tag;
            //
            IsHidden = false;
        }

        public virtual object Clone()
        {
            var componentTypeInfo = new ComponentTypeInfo(AssemblyQualifiedTypeName, TypeName, TypeFullName
                , DefaultName, Version, Author, Description
                , SourceFileName, Tag);
            componentTypeInfo.IsHidden = this.IsHidden;
            return componentTypeInfo;
        }

        /// <summary>
        /// Determines can specified object be threated as this instance in all ways ignoring visibility (IsHidden field) and tagging info (Tag field).
        /// </summary>
        /// <param name="componentTypeInfo">Object to check for value equality with this instance.</param>
        public override bool Equals(object componentTypeInfo)
        {
            if (componentTypeInfo == null || GetType() != componentTypeInfo.GetType())
            {
                return false;
            }

            return this.AssemblyQualifiedTypeName == (componentTypeInfo as ComponentTypeInfo).AssemblyQualifiedTypeName
                && this.TypeName == (componentTypeInfo as ComponentTypeInfo).TypeName
                && this.TypeFullName == (componentTypeInfo as ComponentTypeInfo).TypeFullName
                && this.DefaultName == (componentTypeInfo as ComponentTypeInfo).DefaultName
                && this.Version == (componentTypeInfo as ComponentTypeInfo).Version
                && this.Author == (componentTypeInfo as ComponentTypeInfo).Author
                && this.Description == (componentTypeInfo as ComponentTypeInfo).Description
                && this.SourceFileName == (componentTypeInfo as ComponentTypeInfo).SourceFileName;
        }

        public override int GetHashCode()
        {
            return this.AssemblyQualifiedTypeName.GetHashCode()
                 ^ this.TypeName.GetHashCode()
                 ^ this.TypeFullName.GetHashCode()
                 ^ this.DefaultName.GetHashCode()
                 ^ this.Version.GetHashCode()
                 ^ this.Author.GetHashCode()
                 ^ this.Description.GetHashCode()
                 ^ this.SourceFileName.GetHashCode();
        }

        public override string ToString()
        {
            return this.TypeName + " " + this.Version + " by " + this.Author;
        }
    }
}
