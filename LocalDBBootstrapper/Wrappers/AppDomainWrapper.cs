using System;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IAppDomain
    {
        string CurrentDomainBaseDirectory { get; }
    }

    public class AppDomainWrapper : IAppDomain
    {
        //
        // Summary:
        //     Gets the base directory that the assembly resolver uses to probe for assemblies.
        //
        // Returns:
        //     The base directory that the assembly resolver uses to probe for assemblies.
        //
        // Exceptions:
        //   T:System.AppDomainUnloadedException:
        //     The operation is attempted on an unloaded application domain.
        public string CurrentDomainBaseDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }
}
