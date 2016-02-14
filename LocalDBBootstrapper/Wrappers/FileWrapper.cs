using System.IO;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IFile
    {
        bool Exists(string path);
    }

    public class FileWrapper : IFile
    {
        public static IFile Instance
        {
            get
            {
                return new FileWrapper();
            }
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}
