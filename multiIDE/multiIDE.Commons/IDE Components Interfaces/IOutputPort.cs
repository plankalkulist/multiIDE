namespace multiIDE
{
    public interface IOutputPort : IIdeComponent
    {
        // optional but recommended still:
        /*#region Enums 
        public enum OutputConvertings : int 
        { . . . }
        #endregion*/

        #region Essential properties
        IOutputDevice OutputDevice { get; set; }
        byte Status { get; }
        object OutputConverting { get; set; } // it is recommended to have enum type (look at StdIOport for example), otherwise will be treated as int
        #endregion

        string ToString(); // it is recommended to override Object.ToString() method basing on Profile and Environment properties, look at StdIOport for example.
        int GetHashCode(); // it is recommended to override Object.GetHashCode() method basing on Profile and Environment properties, look at StdIOport for example.
        void Output(byte outByte);
        void Output(byte[] outData);
        void Close();
    }
}
