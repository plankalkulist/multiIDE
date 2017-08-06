namespace multiIDE
{
    [System.Serializable]
    public class MachineRunningException : System.Exception
    {
        public readonly int NextSymbol;
        public readonly int ActionCell;

        public MachineRunningException(int nextSymbol, int actionCell)
                : this("", nextSymbol, actionCell) { }

        public MachineRunningException(string message, int nextSymbol, int actionCell)
                : base(message)
        {
            NextSymbol = nextSymbol;
            ActionCell = actionCell;
        }

        public MachineRunningException(string message, System.Exception inner, int nextSymbol, int actionCell)
                : base(message, inner)
        {
            NextSymbol = nextSymbol;
            ActionCell = actionCell;
        }

        public MachineRunningException(string message, System.Exception inner)
                : base(message, inner) { }

        protected MachineRunningException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
