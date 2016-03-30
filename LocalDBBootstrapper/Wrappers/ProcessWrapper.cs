using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IProcess : IDisposable
    {
        IProcessStartInfo StartInfo { get; set; }
        string ProcessName { get; }
        StreamReader StandardOutput { get; }
        StreamReader StandardError { get; }
        int ExitCode { get; }
        bool Start();
        void Start(ProcessStartInfo startInfo);
        void Kill();
        void WaitForExit();
        bool WaitForExit(int milliseconds);
        int Id { get; }
        IProcess GetCurrentProcess();
        IProcess GetProcessById(int processId);
        IProcess[] GetProcesses();
    }

    public partial class ProcessWrapper : IProcess
    {
        private Process _process;

        public IProcessStartInfo StartInfo
        {
            get
            {
                return new ProcessStartInfoWrapper(_process.StartInfo);
            }
            set
            {
                //_process.StartInfo.UseShellExecute = value.UseShellExecute;
                //_process.StartInfo.ErrorDialog = value.ErrorDialog;
                //_process.StartInfo.RedirectStandardOutput = value.RedirectStandardOutput;
                //_process.StartInfo.CreateNoWindow = value.CreateNoWindow;
                _process.StartInfo.FileName = value.FileName;
                _process.StartInfo.Arguments = value.Arguments;
            }
        }

        //
        // Summary:
        //     Gets the name of the process.
        //
        // Returns:
        //     The name that the system uses to identify the process to the user.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The process does not have an identifier, or no process is associated with
        //     the System.Diagnostics.Process.-or- The associated process has exited.
        //
        //   System.PlatformNotSupportedException:
        //     The platform is Windows 98 or Windows Millennium Edition (Windows Me); set
        //     System.Diagnostics.ProcessStartInfo.UseShellExecute to false to access this
        //     property on Windows 98 and Windows Me.
        //
        //   System.NotSupportedException:
        //     The process is not on this computer.
        public string ProcessName
        {
            get
            {
                return _process.ProcessName;
            }
        }

        public ProcessWrapper()
        {
            _process = new Process();
        }

        public ProcessWrapper(Process process)
        {
            _process = process;
        }

        //
        // Summary:
        //     Gets a stream used to read the output of the application.
        //
        // Returns:
        //     A System.IO.StreamReader that can be used to read the standard output stream
        //     of the application.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The System.Diagnostics.Process.StandardOutput stream has not been defined
        //     for redirection; ensure System.Diagnostics.ProcessStartInfo.RedirectStandardOutput
        //     is set to true and System.Diagnostics.ProcessStartInfo.UseShellExecute is
        //     set to false.- or - The System.Diagnostics.Process.StandardOutput stream
        //     has been opened for asynchronous read operations with System.Diagnostics.Process.BeginOutputReadLine().
        public StreamReader StandardOutput
        {
            get
            {
                return _process.StandardOutput;
            }
        }

        //
        // Summary:
        //     Gets a stream used to read the error output of the application.
        //
        // Returns:
        //     A System.IO.StreamReader that can be used to read the standard error stream
        //     of the application.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The System.Diagnostics.Process.StandardError stream has not been defined
        //     for redirection; ensure System.Diagnostics.ProcessStartInfo.RedirectStandardError
        //     is set to true and System.Diagnostics.ProcessStartInfo.UseShellExecute is
        //     set to false.- or - The System.Diagnostics.Process.StandardError stream has
        //     been opened for asynchronous read operations with System.Diagnostics.Process.BeginErrorReadLine().
        public StreamReader StandardError
        {
            get
            {
                return _process.StandardError;
            }
        }

        public int ExitCode
        {
            get
            {
                return _process.ExitCode;
            }
        }

        //
        // Summary:
        //     Starts (or reuses) the process resource that is specified by the System.Diagnostics.Process.StartInfo
        //     property of this System.Diagnostics.Process component and associates it with
        //     the component.
        //
        // Returns:
        //     true if a process resource is started; false if no new process resource is
        //     started (for example, if an existing process is reused).
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     No file name was specified in the System.Diagnostics.Process component's
        //     System.Diagnostics.Process.StartInfo.-or- The System.Diagnostics.ProcessStartInfo.UseShellExecute
        //     member of the System.Diagnostics.Process.StartInfo property is true while
        //     System.Diagnostics.ProcessStartInfo.RedirectStandardInput, System.Diagnostics.ProcessStartInfo.RedirectStandardOutput,
        //     or System.Diagnostics.ProcessStartInfo.RedirectStandardError is true.
        //
        //   System.ComponentModel.Win32Exception:
        //     There was an error in opening the associated file.
        //
        //   System.ObjectDisposedException:
        //     The process object has already been disposed.
        public bool Start()
        {
            return _process.Start();
        }

        //
        // Summary:
        //     Starts the process resource that is specified by the parameter containing
        //     process start information (for example, the file name of the process to start)
        //     and associates the resource with a new System.Diagnostics.Process component.
        //
        // Parameters:
        //   startInfo:
        //     The System.Diagnostics.ProcessStartInfo that contains the information that
        //     is used to start the process, including the file name and any command-line
        //     arguments.
        //
        // Returns:
        //     A new System.Diagnostics.Process component that is associated with the process
        //     resource, or null if no process resource is started (for example, if an existing
        //     process is reused).
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     No file name was specified in the startInfo parameter's System.Diagnostics.ProcessStartInfo.FileName
        //     property.-or- The System.Diagnostics.ProcessStartInfo.UseShellExecute property
        //     of the startInfo parameter is true and the System.Diagnostics.ProcessStartInfo.RedirectStandardInput,
        //     System.Diagnostics.ProcessStartInfo.RedirectStandardOutput, or System.Diagnostics.ProcessStartInfo.RedirectStandardError
        //     property is also true.-or-The System.Diagnostics.ProcessStartInfo.UseShellExecute
        //     property of the startInfo parameter is true and the System.Diagnostics.ProcessStartInfo.UserName
        //     property is not null or empty or the System.Diagnostics.ProcessStartInfo.Password
        //     property is not null.
        //
        //   System.ArgumentNullException:
        //     The startInfo parameter is null.
        //
        //   System.ObjectDisposedException:
        //     The process object has already been disposed.
        //
        //   System.IO.FileNotFoundException:
        //     The file specified in the startInfo parameter's System.Diagnostics.ProcessStartInfo.FileName
        //     property could not be found.
        //
        //   System.ComponentModel.Win32Exception:
        //     An error occurred when opening the associated file. -or-The sum of the length
        //     of the arguments and the length of the full path to the process exceeds 2080.
        //     The error message associated with this exception can be one of the following:
        //     "The data area passed to a system call is too small." or "Access is denied."
        public void Start(ProcessStartInfo startInfo)
        {
            _process = Process.Start(startInfo);
        }

        //
        // Summary:
        //     Immediately stops the associated process.
        //
        // Exceptions:
        //   System.ComponentModel.Win32Exception:
        //     The associated process could not be terminated. -or-The process is terminating.-or-
        //     The associated process is a Win16 executable.
        //
        //   System.NotSupportedException:
        //     You are attempting to call System.Diagnostics.Process.Kill() for a process
        //     that is running on a remote computer. The method is available only for processes
        //     running on the local computer.
        //
        //   System.InvalidOperationException:
        //     The process has already exited. -or-There is no process associated with this
        //     System.Diagnostics.Process object.
        public void Kill()
        {
            _process.Kill();
        }

        //
        // Summary:
        //     Instructs the System.Diagnostics.Process component to wait indefinitely for
        //     the associated process to exit.
        //
        // Exceptions:
        //   System.ComponentModel.Win32Exception:
        //     The wait setting could not be accessed.
        //
        //   System.SystemException:
        //     No process System.Diagnostics.Process.Id has been set, and a System.Diagnostics.Process.Handle
        //     from which the System.Diagnostics.Process.Id property can be determined does
        //     not exist.-or- There is no process associated with this System.Diagnostics.Process
        //     object.-or- You are attempting to call System.Diagnostics.Process.WaitForExit()
        //     for a process that is running on a remote computer. This method is available
        //     only for processes that are running on the local computer.
        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        //
        // Summary:
        //     Instructs the System.Diagnostics.Process component to wait the specified
        //     number of milliseconds for the associated process to exit.
        //
        // Parameters:
        //   milliseconds:
        //     The amount of time, in milliseconds, to wait for the associated process to
        //     exit. The maximum is the largest possible value of a 32-bit integer, which
        //     represents infinity to the operating system.
        //
        // Returns:
        //     true if the associated process has exited; otherwise, false.
        //
        // Exceptions:
        //   System.ComponentModel.Win32Exception:
        //     The wait setting could not be accessed.
        //
        //   System.SystemException:
        //     No process System.Diagnostics.Process.Id has been set, and a System.Diagnostics.Process.Handle
        //     from which the System.Diagnostics.Process.Id property can be determined does
        //     not exist.-or- There is no process associated with this System.Diagnostics.Process
        //     object.-or- You are attempting to call System.Diagnostics.Process.WaitForExit(System.Int32)
        //     for a process that is running on a remote computer. This method is available
        //     only for processes that are running on the local computer.
        public bool WaitForExit(int milliseconds)
        {
            return _process.WaitForExit(milliseconds);
        }

        //
        // Summary:
        //     Gets the unique identifier for the associated process.
        //
        // Returns:
        //     The system-generated unique identifier of the process that is referenced
        //     by this System.Diagnostics.Process instance.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     The process's System.Diagnostics.Process.Id property has not been set.-or-
        //     There is no process associated with this System.Diagnostics.Process object.
        //
        //   System.PlatformNotSupportedException:
        //     The platform is Windows 98 or Windows Millennium Edition (Windows Me); set
        //     the System.Diagnostics.ProcessStartInfo.UseShellExecute property to false
        //     to access this property on Windows 98 and Windows Me.
        public int Id
        {
            get
            {
                return _process.Id;
            }
        }

        //
        // Summary:
        //     Gets a new System.Diagnostics.Process component and associates it with the
        //     currently active process.
        //
        // Returns:
        //     A new System.Diagnostics.Process component associated with the process resource
        //     that is running the calling application.
        public IProcess GetCurrentProcess()
        {
            return new ProcessWrapper(Process.GetCurrentProcess());
        }

        //
        // Summary:
        //     Returns a new System.Diagnostics.Process component, given the identifier
        //     of a process on the local computer.
        //
        // Parameters:
        //   processId:
        //     The system-unique identifier of a process resource.
        //
        // Returns:
        //     A System.Diagnostics.Process component that is associated with the local
        //     process resource identified by the processId parameter.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The process specified by the processId parameter is not running. The identifier
        //     might be expired.
        //
        //   System.InvalidOperationException:
        //     The process was not started by this object.
        public IProcess GetProcessById(int processId)
        {
            return new ProcessWrapper(Process.GetProcessById(processId));
        }

        //
        // Summary:
        //     Creates a new System.Diagnostics.Process component for each process resource
        //     on the local computer.
        //
        // Returns:
        //     An array of type System.Diagnostics.Process that represents all the process
        //     resources running on the local computer.
        public IProcess[] GetProcesses()
        {
            return Process.GetProcesses().Select(x => new ProcessWrapper(x)).ToArray();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_process != null)
                {
                    _process.Dispose();
                    _process = null;
                }
            }
        }
    }
}
