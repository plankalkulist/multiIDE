namespace multiIDE
{
    [System.Serializable]
    public class NoFreeIOPortSlotsException : MachineSettingException
    {
        public NoFreeIOPortSlotsException() { }
        public NoFreeIOPortSlotsException(string message) : base(message) { }
        public NoFreeIOPortSlotsException(string message, System.Exception inner) : base(message, inner) { }
        protected NoFreeIOPortSlotsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
