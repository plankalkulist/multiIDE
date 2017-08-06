using System.Threading.Tasks;

namespace multiIDE.Machines
{
    public interface IPausable : IVirtualMachine
    {
        Task<VirtualMachineActionPosition> PauseAsync();
    }
}
