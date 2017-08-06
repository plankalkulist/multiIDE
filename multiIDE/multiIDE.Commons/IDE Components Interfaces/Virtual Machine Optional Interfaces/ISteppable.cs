using System.Threading.Tasks;

namespace multiIDE.Machines
{
    public interface ISteppable : IPausable
    {
        Task<VirtualMachineActionPosition> StepAsync();
    }
}
