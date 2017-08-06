using System;

namespace multiIDE
{
    /// <summary>
    /// Indicates that the method/field/property might be displayed in GUI and be available to be invoked by user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class MenuBrowsableAttribute : Attribute
    {
        public readonly bool Browsable;

        public MenuBrowsableAttribute(bool browsable)
        {
            Browsable = browsable;
        }
    }
}
