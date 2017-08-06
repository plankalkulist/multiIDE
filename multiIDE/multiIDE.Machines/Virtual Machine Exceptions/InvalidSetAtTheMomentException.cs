using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class InvalidSetAtTheMomentException : MachineSettingException
    {
        public InvalidSetAtTheMomentException() { }
        public InvalidSetAtTheMomentException(string message) : base(message) { }
        public InvalidSetAtTheMomentException(string message, Exception inner) : base(message, inner) { }
        protected InvalidSetAtTheMomentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
