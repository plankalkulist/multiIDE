using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using multiIDE.ComponentBuilding;
using XSpace;
using System.Xml.Serialization;

namespace multiIDE
{
    [Serializable]
    public struct ComponentTypesAssemblySourceInfo
    {
        public string RelativeAssemblyPath { get; set; }
        public string FullAssemblyPath { get; set; }
        //
        public ComponentTypeSourceInfo[] TypesToRegister { get; set; }
    }

    [Serializable]
    public struct ComponentTypeSourceInfo
    {
        public string TypeFullName { get; set; }
        //
        public string TextTag { get; set; }
        public bool ShouldBeHidden { get; set; }

        public ComponentTypeSourceInfo(string typeFullName, string textTag, bool shouldBeHidden)
        {
            TypeFullName = typeFullName;
            TextTag = textTag;
            ShouldBeHidden = shouldBeHidden;
        }
    }

    public static class ComponentBuildersService
    {
        #region Default Builders
        public static readonly ComponentBuilder<IVirtualMachine, VirtualMachineTypeInfo> DefaultVirtualMachineBuilder
                = new ComponentBuilder<IVirtualMachine, VirtualMachineTypeInfo>("multiIDE.Machines");
        public static readonly ComponentBuilder<ICodeEditor, CodeEditorTypeInfo> DefaultCodeEditorBuilder
                = new ComponentBuilder<ICodeEditor, CodeEditorTypeInfo>("multiIDE.CodeEditors");
        public static readonly ComponentBuilder<IInputPort, InputPortTypeInfo> DefaultInputPortBuilder
                = new ComponentBuilder<IInputPort, InputPortTypeInfo>("multiIDE.IOPorts");
        public static readonly ComponentBuilder<IOutputPort, OutputPortTypeInfo> DefaultOutputPortBuilder
                = new ComponentBuilder<IOutputPort, OutputPortTypeInfo>("multiIDE.IOPorts");
        public static readonly ComponentBuilder<IExtraIdeComponent, ComponentTypeInfo> DefaultIdeExtraBuilder
                = new ComponentBuilder<IExtraIdeComponent, ComponentTypeInfo>("multiIDE.Extras");
        //
        public static readonly ComponentBuilder<IInputDevice, InputDeviceTypeInfo> DefaultInputDeviceBuilder
                = new ComponentBuilder<IInputDevice, InputDeviceTypeInfo>("multiIDE.IODevices");
        public static readonly ComponentBuilder<IOutputDevice, OutputDeviceTypeInfo> DefaultOutputDeviceBuilder
                = new ComponentBuilder<IOutputDevice, OutputDeviceTypeInfo>("multiIDE.IODevices");
        public static readonly ComponentBuilder<IExtraWorkplaceComponent, ComponentTypeInfo> DefaultWorkplaceExtraBuilder
                = new ComponentBuilder<IExtraWorkplaceComponent, ComponentTypeInfo>("multiIDE.Extras");
        //
        public static readonly ComponentBuilder<IComponent, ComponentTypeInfo> CommonBuilder
                = new ComponentBuilder<IComponent, ComponentTypeInfo>("multiIDE.Commons");
        #endregion

        public static IEnumerable<ComponentTypesAssemblySourceInfo> DefaultComponentTypesAssemblySourceInfos
            => DefaultVirtualMachineBuilder.RegisteredTypes.Cast<ComponentTypeInfo>()
                .Concat(DefaultCodeEditorBuilder.RegisteredTypes)
                .Concat(DefaultInputPortBuilder.RegisteredTypes)
                .Concat(DefaultOutputPortBuilder.RegisteredTypes)
                .Concat(DefaultIdeExtraBuilder.RegisteredTypes)
                .Concat(DefaultInputDeviceBuilder.RegisteredTypes)
                .Concat(DefaultOutputDeviceBuilder.RegisteredTypes)
                .Concat(DefaultWorkplaceExtraBuilder.RegisteredTypes)
                .Concat(CommonBuilder.RegisteredTypes)
                .GetSourceInfos();

        static ComponentBuildersService()
        {
            IComponentBuilder<IComponent, ComponentTypeInfo>[] generalizedBuilders = new[]
            {
                DefaultVirtualMachineBuilder.ToGeneralBuilderType(),
                DefaultCodeEditorBuilder.ToGeneralBuilderType(),
                DefaultInputPortBuilder.ToGeneralBuilderType(),
                DefaultOutputPortBuilder.ToGeneralBuilderType(),
                DefaultIdeExtraBuilder.ToGeneralBuilderType(),
                //
                DefaultInputDeviceBuilder.ToGeneralBuilderType(),
                DefaultOutputDeviceBuilder.ToGeneralBuilderType(),
                DefaultWorkplaceExtraBuilder.ToGeneralBuilderType(),
                //
                CommonBuilder.ToGeneralBuilderType()
            };

            // seeking for & registering components' types from internal assembly
            foreach (var builder in generalizedBuilders)
            {
                int num;
                builder.RegisterTypesFrom
                    (builder.InternalSourceKeyword
                    , builder.GetTypesInfosFrom(builder.InternalSourceKeyword, true)
                        .Where(i => i.Version.IsNotNullOrEmpty()
                            && !((i.Version.Length > 4 && i.Version.EndsWith("alpha"))
                                || (i.Version.Length > 1 && i.Version.EndsWith("a") && int.TryParse(i.Version.Substring(i.Version.Length - 2)[0] + "", out num))))
                        .Select(j => j.TypeFullName)
                        .ToList()
                    );
            }

            // seeking for & registering components' types from default assemblies
            ComponentTypesAssemblySourceInfo[] defaultComponentTypesAssemblySourceInfosFromFile = new ComponentTypesAssemblySourceInfo[0];
            try
            {
                using (var fs = new StreamReader(Application.StartupPath + "\\" + Properties.Settings.Default.DefaultComponentsTypesSourcesFileName
                        , Encoding.UTF8))
                {
                    defaultComponentTypesAssemblySourceInfosFromFile =
                        (ComponentTypesAssemblySourceInfo[])(new XmlSerializer(
                            typeof(ComponentTypesAssemblySourceInfo[])).Deserialize(fs));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + "You can add default Virtual Machine types and default Code Editor types manually via Component Manager for all Workplaces or for every Workplace after loading it.", "Component Builders Service Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string sourceInfoPath;
            List<ComponentTypeInfo> defaultTypeFullNamesFromDefaultNamespace;
            foreach (var sourceInfo in defaultComponentTypesAssemblySourceInfosFromFile)
                try
                {
                    foreach (var builder in generalizedBuilders)
                    {
                        sourceInfoPath = File.Exists(sourceInfo.RelativeAssemblyPath) ? sourceInfo.RelativeAssemblyPath
                                : sourceInfo.FullAssemblyPath;
                        defaultTypeFullNamesFromDefaultNamespace = builder.GetTypesInfosFrom(sourceInfoPath, true);
                        if ((defaultTypeFullNamesFromDefaultNamespace?.Count ?? 0) > 0)
                            builder.RegisterTypesFrom
                                    (sourceInfoPath, sourceInfo.TypesToRegister
                                            .Where(t => defaultTypeFullNamesFromDefaultNamespace.Any(d => d.TypeFullName == t.TypeFullName))
                                            .Select(ct => new ComponentTypeInfo("", "", ct.TypeFullName, "", "", "", "", "") { IsHidden = ct.ShouldBeHidden }).ToList()
                            );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Component Builders Service Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        public static void SaveDefaultComponentsTypesSources()
        {
            try
            {
                using (var fs = new StreamWriter(Application.StartupPath + "\\" + Properties.Settings.Default.DefaultComponentsTypesSourcesFileName
                        , false, Encoding.UTF8))
                {
                    new XmlSerializer(typeof(ComponentTypesAssemblySourceInfo[]))
                            .Serialize(fs, DefaultComponentTypesAssemblySourceInfos.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Component Builders Service Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static IEnumerable<ComponentTypesAssemblySourceInfo> GetSourceInfos(this IEnumerable<ComponentTypeInfo> componentTypeInfos)
        {
            var sourceInfos = new List<ComponentTypesAssemblySourceInfo>();

            var sourcesInfosQuery = from ctInfo in componentTypeInfos
                                    group ctInfo by ctInfo.SourceFileName into sourceCTInfos
                                    select sourceCTInfos;

            foreach (var oneSourceInfos in sourcesInfosQuery)
            {
                sourceInfos.Add(new ComponentTypesAssemblySourceInfo()
                {
                    FullAssemblyPath = oneSourceInfos.Key,
                    RelativeAssemblyPath = oneSourceInfos.Key.StartsWith(Application.StartupPath)
                            ? oneSourceInfos.Key.Substring(Application.StartupPath.Length) : "",
                    TypesToRegister = oneSourceInfos.Select(i => new ComponentTypeSourceInfo
                            (i.TypeFullName, i.Tag?.ToString() ?? "", i.IsHidden)).ToArray()
                });
            }

            return sourceInfos;
        }

        /// <summary>
        /// Provides access to specified component bulder by way of IComponentBuilder<IComponent, AComponentTypeInfo> interface.
        /// </summary>
        /// <param name="builderInstance">Casting component builder. Should implement IComponentBuilder<TComponent, TTypeInfo> interface.</param>
        /// <returns>BuilderGeneralizer instance.</returns>
        public static IComponentBuilder<IComponent, ComponentTypeInfo> ToGeneralBuilderType<TComponent, TTypeInfo>(this IComponentBuilder<TComponent, TTypeInfo> builderInstance)
                    where TComponent : class, IComponent
                    where TTypeInfo : ComponentTypeInfo
        {
            return new BuilderGeneralizer(builderInstance);
        }

        public static dynamic GetCastedBuilder(this IComponentBuilder<IComponent, ComponentTypeInfo> generalizedBuilder)
        {
            return (generalizedBuilder as BuilderGeneralizer)?.CastedBuilder;
        }

        public static bool MostlyEquals<TTypeInfo1, TTypeInfo2>(this TTypeInfo1 componentTypeInfo1, TTypeInfo2 componentTypeInfo2)
                        where TTypeInfo1 : ComponentTypeInfo
                        where TTypeInfo2 : ComponentTypeInfo
        {
            return componentTypeInfo1.TypeName == componentTypeInfo2.TypeName
                && componentTypeInfo1.TypeFullName == componentTypeInfo2.TypeFullName
                && componentTypeInfo1.DefaultName == componentTypeInfo2.DefaultName
                && componentTypeInfo1.Version == componentTypeInfo2.Version
                && componentTypeInfo1.Author == componentTypeInfo2.Author
                && componentTypeInfo1.Description == componentTypeInfo2.Description
                && componentTypeInfo1.SourceFileName == componentTypeInfo2.SourceFileName;
        }

        private class BuilderGeneralizer : IComponentBuilder<IComponent, ComponentTypeInfo>
        {
            public readonly dynamic CastedBuilder;
            public List<ComponentTypeInfo> RegisteredTypes
            {
                get
                {
                    return new List<ComponentTypeInfo>((CastedBuilder.RegisteredTypes as IEnumerable).Cast<ComponentTypeInfo>());
                }
            }
            public List<ComponentTypeInfo> VisibleTypes
            {
                get
                {
                    return new List<ComponentTypeInfo>((CastedBuilder.VisibleTypes as IEnumerable).Cast<ComponentTypeInfo>());
                }
            }
            public string ComponentDefaultNamespace => CastedBuilder.ComponentDefaultNamespace;
            public string InternalSourceKeyword => CastedBuilder.InternalSourceKeyword;

            public BuilderGeneralizer(dynamic componentBuilder)
            {
                CastedBuilder = componentBuilder;
            }

            /// <summary>
            /// Gets Component classes' info list from specified source (internal assembly or DLL-file).
            /// </summary>
            /// <param name="filename">Full name of the DLL-file.</param>
            /// <param name="fromSameNamespaceOnly">Determines to seek classes only from the same namespace as built-ins are in (ComponentBuilding.Component.FromNamespace)</param>
            public List<ComponentTypeInfo> GetTypesInfosFrom(string sourceFileName, bool fromComponentDefaultNamespaceOnly)
            {
                var typeInfos = (CastedBuilder.GetTypesInfosFrom(sourceFileName, fromComponentDefaultNamespaceOnly) as IEnumerable).Cast<ComponentTypeInfo>();
                var generilizedTypeInfos = new List<ComponentTypeInfo>(typeInfos);

                return generilizedTypeInfos;
            }

            /// <summary>
            /// Registers Component classes' of specified types' full names (including namespace) list from specified source (internal assembly or DLL-file).
            /// </summary>
            /// <param name="filename">Full name of the DLL-file.</param>
            /// <param name="typeNames">Types' full names (including namespace) list to register.</param>
            public int RegisterTypesFrom(string sourceFileName, List<string> typeFullNames)
            {
                return (int)CastedBuilder.RegisterTypesFrom(sourceFileName, typeFullNames);
            }

            /// <summary>
            /// Registers Component classes' of specified types' info list from specified source (internal assembly or DLL-file).
            /// </summary>
            /// <param name="filename">Full name of the DLL-file.</param>
            /// <param name="typeInfos">Types' info list to register.</param>
            public int RegisterTypesFrom(string sourceFileName, List<ComponentTypeInfo> typeInfos)
            {
                int proccessed = 0;
                foreach (var typeInfo in typeInfos)
                    if ((int)CastedBuilder.RegisterTypesFrom(sourceFileName
                            , new List<string>() { typeInfo.TypeFullName }) > 0)
                    {
                        proccessed += 1;
                        if (typeInfo.IsHidden) CastedBuilder.HideType(typeInfo.TypeFullName);
                    }

                return proccessed;
            }

            /// <summary>
            /// Registers Component classes' of specified types' info list from specified source (internal assembly or DLL-file).
            /// </summary>
            /// <param name="filename">Full name of the DLL-file.</param>
            /// <param name="typeInfos">Types' info list to register.</param>
            public int RegisterTypesFrom(string sourceFileName)
            {
                return (int)CastedBuilder.RegisterTypesFrom(sourceFileName);
            }

            public IComponent GetNew(ComponentTypeInfo componentTypeInfo)
            {
                return (IComponent)CastedBuilder.GetNew(componentTypeInfo);
            }

            public bool HasTypeInfo(ComponentTypeInfo componentTypeInfo)
            {
                return RegisteredTypes.FindAll(i => i.MostlyEquals(componentTypeInfo))?.Count > 0;
            }

            public ComponentTypeInfo GetTypeInfo(string typeFullName)
            {
                return CastedBuilder.GetTypeInfo(typeFullName) as ComponentTypeInfo;
            }

            public void HideType(string typeFullName)
            {
                CastedBuilder.HideType(typeFullName);
            }

            public void ShowType(string typeFullName)
            {
                CastedBuilder.ShowType(typeFullName);
            }

            public void RemoveType(string typeFullName)
            {
                CastedBuilder.RemoveType(typeFullName);
            }
        }
    }
}
