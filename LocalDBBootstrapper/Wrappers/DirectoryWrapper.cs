using System.IO;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IDirectory
    {
        bool Exists(string path);
    }

    public class DirectoryWrapper : IDirectory
    {
        public static IDirectory Instance
        {
            get
            {
                return new DirectoryWrapper();
            }
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
