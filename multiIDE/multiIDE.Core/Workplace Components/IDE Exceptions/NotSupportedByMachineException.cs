namespace multiIDE
{
    [System.Serializable]
    public class NotSupportedByMachineException : MachineSettingException
    {
        public NotSupportedByMachineException() { }
        public NotSupportedByMachineException(string message) : base(message) { }
        public NotSupportedByMachineException(string message, System.Exception inner) : base(message, inner) { }
        protected NotSupportedByMachineException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
