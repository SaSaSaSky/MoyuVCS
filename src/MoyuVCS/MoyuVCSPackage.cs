using Microsoft.VisualStudio.Shell;
using MoyuVCS.Enums;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace MoyuVCS
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(MoyuVCSPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid),
    "MoyuVCS Settings", "MoyuVCS Setting", 0, 0, true)]
    public sealed class MoyuVCSPackage : AsyncPackage
    {
        /// <summary>
        /// MoyuVCSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "417a7b28-ee71-419b-b146-3c12bf0920b8";

        public MoyuVCSEnum MoyuTopVCS
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.MoyuTopVCS;
            }
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await FileCommand.InitializeAsync(this);
            await NodeCommand.InitializeAsync(this);
        }

        #endregion
    }

    public class OptionPageGrid : DialogPage
    {
        private MoyuVCSEnum moyuTopVCS = MoyuVCSEnum.Git;

        [Category("MoyuVCS Setting")]
        [DisplayName("同时存在多个管理器时，优先选择：")]
        [Description("同时存在多个管理器时，优先选择：")]
        public MoyuVCSEnum MoyuTopVCS
        {
            get { return moyuTopVCS; }
            set { moyuTopVCS = value; }
        }
    }
}
