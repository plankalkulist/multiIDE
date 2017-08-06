namespace multiIDE.Machines
{
    public interface IMultiIOable : IVirtualMachine
    {
        IInputPort[] InputPorts { get; set; }
        IOutputPort[] OutputPorts { get; set; }
        int ActionInputPortIndex { get; set; }
        int ActionOutputPortIndex { get; set; }
    }
}
