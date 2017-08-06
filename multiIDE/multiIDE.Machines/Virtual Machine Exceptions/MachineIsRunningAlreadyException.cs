using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class MachineIsRunningAlreadyException : MachineRunningException
    {
        public MachineIsRunningAlreadyException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public MachineIsRunningAlreadyException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public MachineIsRunningAlreadyException(string message, Exception inner) : base(message, inner) { }
        protected MachineIsRunningAlreadyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
