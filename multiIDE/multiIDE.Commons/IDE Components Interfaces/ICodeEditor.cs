using System;

namespace multiIDE
{
    public interface ICodeEditor : IIdeComponent
    {
        #region Essential properties
        string CurrentCode { get; set; }
        #endregion

        #region Events
        event EventHandler CodeChanged;
        #endregion

        #region Environment subs
        void Activate();
        #endregion

        string ToString(); // it is recommended to override Object.ToString() method basing on Profile and Environment properties, look at StdCodeEditor for example.
        int GetHashCode(); // it is recommended to override Object.GetHashCode() method basing on Profile and Environment properties, look at StdCodeEditor for example.
        string GetProgramCode();
        void ClearCode();
    }
}
