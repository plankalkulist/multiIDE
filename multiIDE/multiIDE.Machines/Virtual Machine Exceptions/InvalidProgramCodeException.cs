using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class InvalidProgramCodeException : MachineSettingException
    {
        public InvalidProgramCodeException() { }
        public InvalidProgramCodeException(string message) : base(message) { }
        public InvalidProgramCodeException(string message, Exception inner) : base(message, inner) { }
        protected InvalidProgramCodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
