using System;

namespace multiIDE
{
    /// <summary>
    /// Indicates that the instance should be initialized right after been create.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InitializeAfterCreateAttribute : Attribute
    {
        public readonly bool InitializeAfterCreate;

        public InitializeAfterCreateAttribute(bool initializeAfterCreate = true)
        {
            InitializeAfterCreate = initializeAfterCreate;
        }
    }
}
