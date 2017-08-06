namespace multiIDE
{
    [System.Serializable]
    public class MachineSettingException : System.Exception
    {
        public MachineSettingException() { }
        public MachineSettingException(string message) : base(message) { }
        public MachineSettingException(string message, System.Exception inner) : base(message, inner) { }
        protected MachineSettingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
