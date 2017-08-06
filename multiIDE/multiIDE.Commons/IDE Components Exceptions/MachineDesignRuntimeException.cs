using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class MachineDesignRuntimeException : MachineRunningException
    {
        public MachineDesignRuntimeException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public MachineDesignRuntimeException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public MachineDesignRuntimeException(string message, Exception inner) : base(message, inner) { }
        protected MachineDesignRuntimeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
