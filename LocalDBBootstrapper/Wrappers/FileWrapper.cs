using System.IO;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IFile
    {
        bool Exists(string path);
        void WriteAllText(string path, string contents);
        string ReadAllText(string path);
        void Delete(string path);
        void Copy(string sourceFileName, string destFileName);
    }

    public class FileWrapper : IFile
    {
        //
        // Summary:
        //     Determines whether the specified file exists.
        //
        // Parameters:
        //   path:
        //     The file to check.
        //
        // Returns:
        //     true if the caller has the required permissions and path contains the name
        //     of an existing file; otherwise, false. This method also returns false if
        //     path is null, an invalid path, or a zero-length string. If the caller does
        //     not have sufficient permissions to read the specified file, no exception
        //     is thrown and the method returns false regardless of the existence of path.
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        //
        // Summary:
        //     Creates a new file, writes the specified string to the file, and then closes
        //     the file. If the target file already exists, it is overwritten.
        //
        // Parameters:
        //   path:
        //     The file to write to.
        //
        //   contents:
        //     The string to write to the file.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     path is a zero-length string, contains only white space, or contains one
        //     or more invalid characters as defined by System.IO.Path.InvalidPathChars.
        //
        //   System.ArgumentNullException:
        //     path is null or contents is empty.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid (for example, it is on an unmapped drive).
        //
        //   System.IO.IOException:
        //     An I/O error occurred while opening the file.
        //
        //   System.UnauthorizedAccessException:
        //     path specified a file that is read-only.-or- This operation is not supported
        //     on the current platform.-or- path specified a directory.-or- The caller does
        //     not have the required permission.
        //
        //   System.NotSupportedException:
        //     path is in an invalid format.
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        //
        // Summary:
        //     Opens a text file, reads all lines of the file, and then closes the file.
        //
        // Parameters:
        //   path:
        //     The file to open for reading.
        //
        // Returns:
        //     A string containing all lines of the file.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     path is a zero-length string, contains only white space, or contains one
        //     or more invalid characters as defined by System.IO.Path.InvalidPathChars.
        //
        //   System.ArgumentNullException:
        //     path is null.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid (for example, it is on an unmapped drive).
        //
        //   System.IO.IOException:
        //     An I/O error occurred while opening the file.
        //
        //   System.UnauthorizedAccessException:
        //     path specified a file that is read-only.-or- This operation is not supported
        //     on the current platform.-or- path specified a directory.-or- The caller does
        //     not have the required permission.
        //
        //   System.IO.FileNotFoundException:
        //     The file specified in path was not found.
        //
        //   System.NotSupportedException:
        //     path is in an invalid format.
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        //
        // Summary:
        //     Deletes the specified file.
        //
        // Parameters:
        //   path:
        //     The name of the file to be deleted. Wildcard characters are not supported.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     path is a zero-length string, contains only white space, or contains one
        //     or more invalid characters as defined by System.IO.Path.InvalidPathChars.
        //
        //   System.ArgumentNullException:
        //     path is null.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid (for example, it is on an unmapped drive).
        //
        //   System.IO.IOException:
        //     The specified file is in use. -or-There is an open handle on the file, and
        //     the operating system is Windows XP or earlier. This open handle can result
        //     from enumerating directories and files. For more information, see How to:
        //     Enumerate Directories and Files.
        //
        //   System.NotSupportedException:
        //     path is in an invalid format.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.-or- path is a directory.-or-
        //     path specified a read-only file.
        public void Delete(string path)
        {
            File.Delete(path);
        }

        //
        // Summary:
        //     Copies an existing file to a new file. Overwriting a file of the same name
        //     is not allowed.
        //
        // Parameters:
        //   sourceFileName:
        //     The file to copy.
        //
        //   destFileName:
        //     The name of the destination file. This cannot be a directory or an existing
        //     file.
        //
        // Exceptions:
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.
        //
        //   System.ArgumentException:
        //     sourceFileName or destFileName is a zero-length string, contains only white
        //     space, or contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.-or-
        //     sourceFileName or destFileName specifies a directory.
        //
        //   System.ArgumentNullException:
        //     sourceFileName or destFileName is null.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The path specified in sourceFileName or destFileName is invalid (for example,
        //     it is on an unmapped drive).
        //
        //   System.IO.FileNotFoundException:
        //     sourceFileName was not found.
        //
        //   System.IO.IOException:
        //     destFileName exists.-or- An I/O error has occurred.
        //
        //   System.NotSupportedException:
        //     sourceFileName or destFileName is in an invalid format.
        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }
    }
}
