using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class UnexpectedProgramStatementException : MachineDesignRuntimeException
    {
        public UnexpectedProgramStatementException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public UnexpectedProgramStatementException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public UnexpectedProgramStatementException(string message, Exception inner) : base(message, inner) { }
        protected UnexpectedProgramStatementException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
