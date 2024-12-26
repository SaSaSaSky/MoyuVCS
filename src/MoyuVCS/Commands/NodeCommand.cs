using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace MoyuVCS
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NodeCommand
    {
        public const int Update_CommandId = 256;
        public const int Commit_CommandId = 257;
        public const int ShowLog_CommandId = 258;
        public const int Revert_CommandId = 259;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6664729f-049d-4c50-a236-aa626e4306a2");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private NodeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID1 = new CommandID(CommandSet, Update_CommandId);
            var menuItem1 = new MenuCommand(this.Update_Execute, menuCommandID1);
            commandService.AddCommand(menuItem1);

            var menuCommandID2 = new CommandID(CommandSet, Commit_CommandId);
            var menuItem2 = new MenuCommand(this.Commit_Execute, menuCommandID2);
            commandService.AddCommand(menuItem2);

            var menuCommandID3 = new CommandID(CommandSet, ShowLog_CommandId);
            var menuItem3 = new MenuCommand(this.ShowLog_Execute, menuCommandID3);
            commandService.AddCommand(menuItem3);

            var menuCommandID4 = new CommandID(CommandSet, Revert_CommandId);
            var menuItem4 = new MenuCommand(this.Revert_Execute, menuCommandID4);
            commandService.AddCommand(menuItem4);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NodeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in NodeCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new NodeCommand(package, commandService);
        }


        private void Update_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); // 获取选中文件路径
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:update /path:\"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private void Commit_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); // 获取选中文件路径
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:commit /path:\"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private void ShowLog_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); // 获取选中文件路径
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:log /path:\"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private void Revert_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); // 获取选中文件路径
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "TortoiseProc.exe",
                Arguments = $"/command:revert /path:\"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private string GetSelectedFilePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var path = string.Empty;
            path = GetSelectedFileNodePath();
            if (!string.IsNullOrEmpty(path))
                return path;
            throw new Exception("未找到当前选择文件路径！");
        }

        /// <summary>
        /// 资源管理器（文件、文件夹）
        /// </summary>
        /// <returns></returns>
        private string GetSelectedFileNodePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // 获取 DTE 对象
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte == null) return null;

            // 获取当前选中项
            Array selectedItems = dte.ToolWindows.SolutionExplorer.SelectedItems as Array;
            if (selectedItems == null || selectedItems.Length == 0) return null;

            // 提取选中项路径
            foreach (UIHierarchyItem item in selectedItems)
            {
                if (item.Object is Project project && project.FullName != null)
                {
                    return Path.GetDirectoryName(project.FullName); // 返回项目路径 (.csproj)
                }
                if (item.Object is Solution solution && solution.FullName != null)
                {
                    return Path.GetDirectoryName(solution.FullName); // 返回解决方案路径 (.csproj)
                }
                else
                {
                    var projectItem = item.Object as ProjectItem;
                    if (projectItem?.Properties?.Item("FullPath")?.Value is string fullPath)
                    {
                        return fullPath; // 返回完整路径
                    }
                }
            }

            return null;
        }
    }
}
