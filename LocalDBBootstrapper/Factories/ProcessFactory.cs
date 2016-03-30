using LocalDBBootstrapper.Wrappers;

namespace LocalDBBootstrapper.Factories
{
    public interface IProcessFactory
    {
        IProcess GetProcess();
    }

    public class ProcessFactory : IProcessFactory
    {
        public IProcess GetProcess()
        {
            return new ProcessWrapper();
        }
    }
}
