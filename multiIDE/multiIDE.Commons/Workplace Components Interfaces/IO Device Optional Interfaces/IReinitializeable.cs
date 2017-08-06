namespace multiIDE.IODevices
{
    public interface IReinitializeable : IComponent
    {
        void Reinitialize(object sender, bool saveData);
    }
}
