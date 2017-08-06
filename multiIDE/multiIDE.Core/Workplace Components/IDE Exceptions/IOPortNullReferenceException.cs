namespace multiIDE
{
    [System.Serializable]
    public class IOPortNullReferenceException : MachineSettingException
    {
        public IOPortNullReferenceException() { }
        public IOPortNullReferenceException(string message) : base(message) { }
        public IOPortNullReferenceException(string message, System.Exception inner) : base(message, inner) { }
        protected IOPortNullReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
