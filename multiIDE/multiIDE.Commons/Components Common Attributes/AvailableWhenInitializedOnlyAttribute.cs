using System;

namespace multiIDE
{
    /// <summary>
    /// Indicates that the method/field/property is available only when associated parent object is initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class AvailableWhenInitializedOnlyAttribute : Attribute
    {
        public readonly bool Checking;

        public AvailableWhenInitializedOnlyAttribute(bool checking)
        {
            Checking = checking;
        }
    }
}
