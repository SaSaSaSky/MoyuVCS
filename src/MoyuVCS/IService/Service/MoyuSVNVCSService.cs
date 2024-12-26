using MoyuVCS.Service;

namespace MoyuVCS.IService.Service
{
    public class MoyuSVNVCSService : IMoyuVCSService
    {
        public void Update(string path)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:update /path:\"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        public void Commit(string path)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:commit /path:\"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        public void ShowLog(string path)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:log /path:\"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        public void Revert(string path)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:revert /path:\"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }
    }
}
