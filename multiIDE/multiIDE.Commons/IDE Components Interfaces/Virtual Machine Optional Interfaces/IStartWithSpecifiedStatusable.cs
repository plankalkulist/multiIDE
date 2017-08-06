using System.Threading.Tasks;

namespace multiIDE.Machines
{
    public interface IStartWithSpecifiedStatusable : IVirtualMachine, IPausable
    {
        Task<VirtualMachineRunResult> StartAsync(VirtualMachineRunningStatus withSpecifiedStatus);
    }
}
