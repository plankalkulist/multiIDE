using System.Collections.Generic;

namespace multiIDE
{
    public interface IComponentBuilder<TComponent, TTypeInfo>
                    where TComponent : IComponent
                    where TTypeInfo : ComponentTypeInfo
    {
        List<TTypeInfo> RegisteredTypes { get; }
        List<TTypeInfo> VisibleTypes { get; }
        string ComponentDefaultNamespace { get; }
        string InternalSourceKeyword { get; }

        List<TTypeInfo> GetTypesInfosFrom(string sourceFileName, bool fromComponentDefaultNamespaceOnly);
        int RegisterTypesFrom(string sourceFileName, List<string> typeFullNames);
        int RegisterTypesFrom(string sourceFileName, List<TTypeInfo> typeInfos);
        int RegisterTypesFrom(string sourceFileName);

        TComponent GetNew(TTypeInfo componentTypeInfo);

        bool HasTypeInfo(TTypeInfo componentTypeInfo);
        TTypeInfo GetTypeInfo(string typeFullName);
        void HideType(string typeFullName);
        void ShowType(string typeFullName);
        void RemoveType(string typeFullName);
    }
}
