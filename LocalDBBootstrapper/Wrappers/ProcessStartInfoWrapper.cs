using System.Diagnostics;

namespace LocalDBBootstrapper.Wrappers
{
    public interface IProcessStartInfo
    {
        bool UseShellExecute { get; set; }
        bool ErrorDialog { get; set; }
        bool RedirectStandardOutput { get; set; }
        bool CreateNoWindow { get; set; }
        string FileName { get; set; }
        string Arguments { get; set; }
    }

    public class ProcessStartInfoWrapper : IProcessStartInfo
    {
        private readonly ProcessStartInfo _processStartInfo;

        public bool UseShellExecute
        {
            get
            {
                return _processStartInfo.UseShellExecute;
            }
            set
            {
                _processStartInfo.UseShellExecute = value;
            }
        }

        public bool ErrorDialog
        {
            get
            {
                return _processStartInfo.ErrorDialog;
            }
            set
            {
                _processStartInfo.ErrorDialog = value;
            }
        }

        public bool RedirectStandardOutput
        {
            get
            {
                return _processStartInfo.RedirectStandardOutput;
            }
            set
            {
                _processStartInfo.RedirectStandardOutput = value;
            }
        }

        public bool CreateNoWindow
        {
            get
            {
                return _processStartInfo.CreateNoWindow;
            }
            set
            {
                _processStartInfo.CreateNoWindow = value;
            }
        }

        public string FileName
        {
            get
            {
                return _processStartInfo.FileName;
            }
            set
            {
                _processStartInfo.FileName = value;
            }
        }

        public string Arguments
        {
            get
            {
                return _processStartInfo.Arguments;
            }
            set
            {
                _processStartInfo.Arguments = value;
            }
        }

        public ProcessStartInfoWrapper()
        {
            _processStartInfo = new ProcessStartInfo();
        }

        public ProcessStartInfoWrapper(ProcessStartInfo processStartInfo)
        {
            _processStartInfo = processStartInfo;
        }
    }
}
