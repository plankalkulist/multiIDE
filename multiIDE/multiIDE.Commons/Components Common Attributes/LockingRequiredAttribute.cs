using System;

namespace multiIDE
{
    /// <summary>
    /// Indicates that the method/field/property requires a locking preparation before using.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class LockingRequiredAttribute : Attribute
    {
        public readonly bool IsLockingRequired;

        public LockingRequiredAttribute(bool isThreadUnsafe = true)
        {
            IsLockingRequired = isThreadUnsafe;
        }
    }
}
