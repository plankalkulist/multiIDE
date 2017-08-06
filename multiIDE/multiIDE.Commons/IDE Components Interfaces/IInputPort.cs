namespace multiIDE
{
    public interface IInputPort : IIdeComponent
    {
        // optional but recommended still:
        /*#region Enums
        public enum InputConvertings : int 
        { . . . }
        #endregion*/

        #region Essential properties
        IInputDevice InputDevice { get; set; }
        byte Status { get; }
        object InputConverting { get; set; } // it is recommended to have enum type (look at StdInputPort for example), otherwise will be treated as int
        #endregion

        string ToString(); // it is recommended to override Object.ToString() method basing on Profile and Environment properties, look at StdInputPort for example.
        int GetHashCode(); // it is recommended to override Object.GetHashCode() method basing on Profile and Environment properties, look at StdInputPort for example.
        byte Input();
        int Input(out byte[] inData, int length);
        void Close();
    }
}
