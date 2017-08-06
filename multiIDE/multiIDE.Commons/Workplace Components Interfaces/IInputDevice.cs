using System;
using System.Text;

namespace multiIDE
{
    public interface IInputDevice : IWorkplaceComponent, IDisposable
    {
        #region Essential properties
        bool IsInitialized { get; }
        object InitializedBy { get; }
        bool IsReadyToInput { get; }
        Encoding CharSet { get; set; }
        #endregion

        #region Technicals
        object Locking { get; }
        #endregion

        string ToString(); // it is recommended to override Object.ToString() method basing on Profile and Environment properties, look at ConsoleDevice for example.
        int GetHashCode(); // it is recommended to override Object.GetHashCode() method basing on Profile and Environment properties, look at ConsoleDevice for example.
        byte GetStatus(object sender);
        void Initialize(object sender);
        void Input(object sender, out byte[] inData, int length);
    }
}
