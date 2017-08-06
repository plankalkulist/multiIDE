namespace multiIDE.IODevices
{
    public interface IFileInputable : IInputDevice
    {
        string FileName { get; set; }
    }
}
