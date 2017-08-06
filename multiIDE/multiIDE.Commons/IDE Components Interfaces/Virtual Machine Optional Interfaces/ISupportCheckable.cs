namespace multiIDE.Machines
{
    public interface ISupportCheckable : IVirtualMachine
    {
        bool DoesSupport(IVirtualMachine sourceMachine);
    }
}
