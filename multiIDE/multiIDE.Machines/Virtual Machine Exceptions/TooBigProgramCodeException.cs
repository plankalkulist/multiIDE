using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class TooBigProgramCodeException : MachineSettingException
    {
        public TooBigProgramCodeException() { }
        public TooBigProgramCodeException(string message) : base(message) { }
        public TooBigProgramCodeException(string message, Exception inner) : base(message, inner) { }
        protected TooBigProgramCodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
