using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class MachineNotProgrammedYetException : MachineSettingException
    {
        public MachineNotProgrammedYetException() { }
        public MachineNotProgrammedYetException(string message) : base(message) { }
        public MachineNotProgrammedYetException(string message, Exception inner) : base(message, inner) { }
        protected MachineNotProgrammedYetException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
