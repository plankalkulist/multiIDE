using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class UnhandledMachineProgramRuntimeErrorException : MachineRunningException
    {
        public UnhandledMachineProgramRuntimeErrorException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public UnhandledMachineProgramRuntimeErrorException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public UnhandledMachineProgramRuntimeErrorException(string message, Exception inner) : base(message, inner) { }
        protected UnhandledMachineProgramRuntimeErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
