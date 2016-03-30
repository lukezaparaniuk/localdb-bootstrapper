using System.Collections.Generic;
using System.IO;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IDirectory
    {
        bool Exists(string path);
        DirectoryInfo CreateDirectory(string path);
        IEnumerable<string> EnumerateFiles(string path);
        void Delete(string path, bool recursive);
    }

    public class DirectoryWrapper : IDirectory
    {
        //
        // Summary:
        //     Determines whether the given path refers to an existing directory on disk.
        //
        // Parameters:
        //   path:
        //     The path to test.
        //
        // Returns:
        //     true if path refers to an existing directory; otherwise, false.
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        // Summary:
        //     Creates all directories and subdirectories in the specified path.
        //
        // Parameters:
        //   path:
        //     The directory path to create.
        //
        // Returns:
        //     A System.IO.DirectoryInfo as specified by path.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     The directory specified by path is a file .-or-The network name is not known.
        //
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.
        //
        //   System.ArgumentException:
        //     path is a zero-length string, contains only white space, or contains one
        //     or more invalid characters as defined by System.IO.Path.InvalidPathChars.-or-path
        //     is prefixed with, or contains only a colon character (:).
        //
        //   System.ArgumentNullException:
        //     path is null.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid (for example, it is on an unmapped drive).
        //
        //   System.NotSupportedException:
        //     path contains a colon character (:) that is not part of a drive label ("C:\").
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        //
        // Summary:
        //     Returns an enumerable collection of file names in a specified path.
        //
        // Parameters:
        //   path:
        //     The directory to search.
        //
        // Returns:
        //     An enumerable collection of the full names (including paths) for the files
        //     in the directory specified by path.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     path is a zero-length string, contains only white space, or contains invalid
        //     characters as defined by System.IO.Path.GetInvalidPathChars().
        //
        //   System.ArgumentNullException:
        //     path is null.
        //
        //   System.IO.DirectoryNotFoundException:
        //     path is invalid, such as referring to an unmapped drive.
        //
        //   System.IO.IOException:
        //     path is a file name.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or combined exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters and file names must be less than 260 characters.
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        //
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.
        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public void Delete(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }
    }
}
