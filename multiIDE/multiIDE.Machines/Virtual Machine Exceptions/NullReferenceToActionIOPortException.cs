using System;

namespace multiIDE.Machines
{
    [Serializable]
    public class NullReferenceToActionIOPortException : MachineDesignRuntimeException
    {
        public NullReferenceToActionIOPortException(int nextSymbol, int actionCell) : base(nextSymbol, actionCell) { }
        public NullReferenceToActionIOPortException(string message, int nextSymbol, int actionCell) : base(message, nextSymbol, actionCell) { }

        public NullReferenceToActionIOPortException(string message, Exception inner) : base(message, inner) { }
        protected NullReferenceToActionIOPortException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
