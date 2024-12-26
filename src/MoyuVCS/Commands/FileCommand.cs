using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MoyuVCS.Common;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace MoyuVCS
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FileCommand
    {
        public const int Update_CommandId = 0x0100;
        public const int Commit_CommandId = 0x0101;
        public const int ShowLog_CommandId = 0x0102;
        public const int Revert_CommandId = 0x0103;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("4224b7c8-c371-40fe-8106-2437237815f1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private readonly MoyuVCSProvider _moyuVCSProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private FileCommand(AsyncPackage package, OleMenuCommandService commandService)
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

            MoyuVCSPackage moyuPackage = this.package as MoyuVCSPackage;
            _moyuVCSProvider = new MoyuVCSProvider(moyuPackage.MoyuTopVCS);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FileCommand Instance
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
            // Switch to the main thread - the call to AddCommand in FileCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FileCommand(package, commandService);
        }


        private void Update_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); 
            _moyuVCSProvider.Update(filePath);
        }

        private void Commit_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); 
            _moyuVCSProvider.Commit(filePath);
        }

        private void ShowLog_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); 
            _moyuVCSProvider.ShowLog(filePath);
        }

        private void Revert_Execute(object sender, EventArgs e)
        {
            var filePath = GetSelectedFilePath(); 
            _moyuVCSProvider.Revert(filePath);
        }

        private string GetSelectedFilePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var path = string.Empty;
            path = GetSelectedFileActivePath();
            if (!string.IsNullOrEmpty(path))
                return path;
            throw new Exception("未找到当前选择文件路径！");
        }

        /// <summary>
        /// 当前文件
        /// </summary>
        /// <returns></returns>
        private string GetSelectedFileActivePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // 获取 DTE 服务
            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            var activeDocument = dte?.ActiveDocument;
            if (activeDocument != null)
            {
                return activeDocument.FullName;
            }
            return null;
        }
    }
}
