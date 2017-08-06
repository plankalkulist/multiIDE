using System.Collections.Generic;

namespace multiIDE.Machines
{
    public interface IStateShottable : IVirtualMachine, IPausable, ISupportCheckable
    {
        int GetDataArrangementHashCode();
        VirtualMachineStateShotResult GetState(out List<object> outState);
        VirtualMachineStateShotResult SetState(List<object> sourceState);
    }
}
