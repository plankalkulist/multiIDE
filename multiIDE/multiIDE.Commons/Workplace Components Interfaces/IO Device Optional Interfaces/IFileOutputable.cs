namespace multiIDE.IODevices
{
    public interface IFileOutputable : IOutputDevice
    {
        string FileName { get; set; }
    }
}
