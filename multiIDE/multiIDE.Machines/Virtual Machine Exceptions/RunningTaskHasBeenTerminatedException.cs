using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class RunningTaskHasBeenTerminatedException : MachineRunningException
    {
        public RunningTaskHasBeenTerminatedException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public RunningTaskHasBeenTerminatedException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public RunningTaskHasBeenTerminatedException(string message, Exception inner) : base(message, inner) { }
        protected RunningTaskHasBeenTerminatedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
