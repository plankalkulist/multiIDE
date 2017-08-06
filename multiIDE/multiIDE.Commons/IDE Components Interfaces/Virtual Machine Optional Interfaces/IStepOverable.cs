using System.Threading.Tasks;

namespace multiIDE.Machines
{
    public interface IStepOverable : IPausable
    {
        Task<VirtualMachineActionPosition> StepOverAsync();
    }
}
