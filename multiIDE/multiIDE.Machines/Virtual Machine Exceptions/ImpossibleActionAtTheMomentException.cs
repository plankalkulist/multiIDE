using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class ImpossibleActionAtTheMomentException : MachineRunningException
    {
        public ImpossibleActionAtTheMomentException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public ImpossibleActionAtTheMomentException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public ImpossibleActionAtTheMomentException(string message, Exception inner) : base(message, inner) { }
        protected ImpossibleActionAtTheMomentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
