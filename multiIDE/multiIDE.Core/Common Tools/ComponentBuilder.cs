using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace multiIDE.ComponentBuilding
{
    public sealed class ComponentBuilder<TComponent, TTypeInfo> : IComponentBuilder<TComponent, TTypeInfo>
                        where TComponent : class, IComponent
                        where TTypeInfo : ComponentTypeInfo
    {
        public List<TTypeInfo> RegisteredTypes
        {
            get
            {
                var componentTypesCopy = new List<TTypeInfo>();
                foreach (TTypeInfo componentTypeInfo in _RegisteredTypes)
                    componentTypesCopy.Add((TTypeInfo)((componentTypeInfo as ICloneable).Clone()));
                return componentTypesCopy;
            }
            private set
            {
                _RegisteredTypes = value;
            }
        }
        public List<TTypeInfo> VisibleTypes
        {
            get
            {
                var componentTypesCopy = new List<TTypeInfo>();
                foreach (TTypeInfo componentTypeInfo in _RegisteredTypes)
                    if (!componentTypeInfo.IsHidden)
                        componentTypesCopy.Add((TTypeInfo)((componentTypeInfo as ICloneable).Clone()));
                return componentTypesCopy;
            }
        }
        public string ComponentDefaultNamespace { get; private set; } = "multiIDE";
        public string InternalSourceKeyword => "<Internal>";
        //
        private List<TTypeInfo> _RegisteredTypes = new List<TTypeInfo>();

        public ComponentBuilder()
        { }

        public ComponentBuilder(string componentDefaultNamespace)
        {
            ComponentDefaultNamespace = componentDefaultNamespace;
        }

        public ComponentBuilder(string componentDefaultNamespace, IEnumerable<TTypeInfo> withPreregisteredTypes)
                : this(componentDefaultNamespace)
        {
            _RegisteredTypes.AddRange(withPreregisteredTypes);
        }

        /// <summary>
        /// Gets Component classes' info list from specified source (internal assembly or DLL-file).
        /// </summary>
        /// <param name="sourceFileName">InternalSourceKeyword or full name of the DLL-file.</param>
        /// <param name="fromComponentDefaultNamespaceOnly">Determines to seek classes only from the same namespace as built-ins are in (ComponentBuilding.\<ComponentName\>.ComponentDefaultNamespace)</param>
        public List<TTypeInfo> GetTypesInfosFrom(string sourceFileName, bool fromComponentDefaultNamespaceOnly)
        {
            var typeInfos = new List<TTypeInfo>();
            Assembly assembly;
            bool assemblyIsLocal;

            if (sourceFileName == InternalSourceKeyword || string.IsNullOrEmpty(sourceFileName))
            {
                assembly = Assembly.GetExecutingAssembly();
                assemblyIsLocal = true;
                sourceFileName = InternalSourceKeyword;
            }
            else
            {
                assembly = Assembly.LoadFrom(sourceFileName);
                assemblyIsLocal = false;
            }

            IEnumerable<Type> types = assembly.GetTypes()
                .Where(t => (!fromComponentDefaultNamespaceOnly || t.Namespace == ComponentDefaultNamespace)
                    && typeof(TComponent).IsAssignableFrom(t) && t.GetConstructors().Any(c => c.GetParameters().Length == 0)
                    && t.IsPublic && t.IsClass && !t.IsAbstract && !t.IsInterface
                    && (assemblyIsLocal || t.IsVisible));

            string tcomponentname = typeof(TComponent).Name;
            Type[] typestypo = types.ToArray();

            TComponent componentExample;
            foreach (Type componentType in types)
                try
                {
                    componentExample = (TComponent)Activator.CreateInstance(componentType);

                    if (componentExample != null)
                        typeInfos.Add((TTypeInfo)Activator.CreateInstance(
                                typeof(TTypeInfo), componentExample.GetType().AssemblyQualifiedName
                                , componentExample.GetType().Name, componentExample.GetType().FullName
                                , componentExample.GetType().Assembly.Location, null, componentExample));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }

            return typeInfos;
        }

        /// <summary>
        /// Registers Input Component classes' of specified types' full names (including namespace) list from specified source (internal assembly or DLL-file).
        /// </summary>
        /// <param name="sourceFileName">InternalSourceKeyword or full name of the DLL-file.</param>
        /// <param name="typeNames">Types' full names (including namespace) list to register.</param>
        public int RegisterTypesFrom(string sourceFileName, List<string> typeFullNames)
        {
            int proccessedCount = 0;
            Assembly assembly;
            bool assemblyIsLocal;

            if (sourceFileName == InternalSourceKeyword || string.IsNullOrEmpty(sourceFileName))
            {
                assembly = Assembly.GetExecutingAssembly();
                assemblyIsLocal = true;
                sourceFileName = InternalSourceKeyword;
            }
            else
            {
                assembly = Assembly.LoadFile(sourceFileName);
                assemblyIsLocal = false;
            }

            IEnumerable<Type> types = assembly.GetTypes()
                .Where(t => (typeFullNames != null && typeFullNames.Count > 0 ? typeFullNames.Contains(t.FullName) : true)
                    && typeof(TComponent).IsAssignableFrom(t) && t.GetConstructors().Any(c => c.GetParameters().Length == 0)
                    && t.IsPublic && t.IsClass && !t.IsAbstract && !t.IsInterface
                    && (assemblyIsLocal || t.IsVisible));

            TComponent componentExample;
            foreach (Type componentType in types)
                try
                {
                    componentExample = (TComponent)assembly.CreateInstance(componentType.FullName);
                    if (componentExample != null)
                    {
                        TTypeInfo typeInfo = (TTypeInfo)Activator.CreateInstance(typeof(TTypeInfo),
                                componentExample.GetType().AssemblyQualifiedName
                                , componentExample.GetType().Name, componentExample.GetType().FullName
                                , componentExample.GetType().Assembly.Location, null, componentExample);
                        if (!_RegisteredTypes.Contains(typeInfo))
                            _RegisteredTypes.Add(typeInfo);
                        proccessedCount++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }

            return proccessedCount;
        }

        /// <summary>
        /// Registers Component classes' of specified types' info list from specified source (internal assembly or DLL-file).
        /// </summary>
        /// <param name="sourceFileName">InternalSourceKeyword or full name of the DLL-file.</param>
        /// <param name="typeInfos">Types' info list to register.</param>
        public int RegisterTypesFrom(string sourceFileName, List<TTypeInfo> typeInfos)
        {
            int proccessedCount = 0;
            Assembly assembly;
            bool assemblyIsLocal;

            if (sourceFileName == InternalSourceKeyword || string.IsNullOrEmpty(sourceFileName))
            {
                assembly = Assembly.GetExecutingAssembly();
                assemblyIsLocal = true;
                sourceFileName = InternalSourceKeyword;
            }
            else
            {
                assembly = Assembly.LoadFile(sourceFileName);
                assemblyIsLocal = false;
            }

            var types = assembly.GetTypes()
                .Where(t => (typeInfos != null && typeInfos.Count > 0 ? typeInfos.Any(i => i.TypeFullName == t.FullName) : true)
                            && typeof(TComponent).IsAssignableFrom(t) && t.GetConstructors().Any(c => c.GetParameters().Length == 0)
                            && t.IsPublic && t.IsClass && !t.IsAbstract && !t.IsInterface
                            && (assemblyIsLocal || t.IsVisible)).Select(t => new { Type = t, IsHidden = typeInfos.Find(i => i.TypeFullName == t.FullName).IsHidden });

            TComponent componentExample;
            TTypeInfo prevTypeInfo;
            foreach (var componentType in types)
                try
                {
                    componentExample = (TComponent)assembly.CreateInstance(componentType.Type.FullName);
                    if (componentExample != null)
                    {
                        TTypeInfo typeInfo = (TTypeInfo)Activator.CreateInstance(typeof(TTypeInfo),
                            componentExample.GetType().AssemblyQualifiedName
                            , componentExample.GetType().Name, componentExample.GetType().FullName
                            , componentExample.GetType().Assembly.Location, null, componentExample);
                        typeInfo.IsHidden = componentType.IsHidden;
                        prevTypeInfo = _RegisteredTypes.Find(t => t.AssemblyQualifiedTypeName == typeInfo.AssemblyQualifiedTypeName);
                        if (prevTypeInfo != null)
                            _RegisteredTypes.Remove(prevTypeInfo);
                        _RegisteredTypes.Add(typeInfo);
                        proccessedCount++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }

            return proccessedCount;
        }

        /// <summary>
        /// Registers all Component classes' from specified source (internal assembly or DLL-file).
        /// </summary>
        /// <param name="sourceFileName">InternalSourceKeyword or full name of the DLL-file.</param>
        public int RegisterTypesFrom(string sourceFileName)
        {
            return RegisterTypesFrom(sourceFileName, new List<string>());
        }

        public TComponent GetNew(TTypeInfo componentTypeInfo)
        {
            return (TComponent)Activator.CreateInstance
            (Type.GetType(componentTypeInfo.AssemblyQualifiedTypeName));
        }

        public bool HasTypeInfo(TTypeInfo componentTypeInfo)
        {
            if (typeof(ComponentTypeInfo).IsAssignableFrom(typeof(TTypeInfo)))
            {
                return _RegisteredTypes.Any(i => (i as ComponentTypeInfo).Equals(componentTypeInfo));
            }
            else
            {
                return _RegisteredTypes.Any(i => i.MostlyEquals(componentTypeInfo));
            }
        }

        public TTypeInfo GetTypeInfo(string typeFullName)
        {
            return _RegisteredTypes.Find(i => i.TypeFullName == typeFullName);
        }

        public void HideType(string typeFullName)
        {
            _RegisteredTypes.Find(i => i.TypeFullName == typeFullName)
                .IsHidden = true;
        }

        public void ShowType(string typeFullName)
        {
            _RegisteredTypes.Find(i => i.TypeFullName == typeFullName)
                .IsHidden = false;
        }

        public void RemoveType(string typeFullName)
        {
            _RegisteredTypes.RemoveAll(i => i.TypeFullName == typeFullName);
        }
    }
}
