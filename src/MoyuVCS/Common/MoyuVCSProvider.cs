using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using MoyuVCS.Enums;
using MoyuVCS.IService.Service;
using MoyuVCS.Service;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace MoyuVCS.Common
{
    public class MoyuVCSProvider
    {
        protected IMoyuVCSService _moyuVCSService;


        private Dictionary<MoyuVCSEnum, string> _vcsFolderMapping = new Dictionary<MoyuVCSEnum, string>
            {
                { MoyuVCSEnum.SVN, ".svn" },
                { MoyuVCSEnum.Git, ".git" },
            };

        public MoyuVCSProvider(MoyuVCSEnum topVCS)
        {
            InitService(topVCS);
        }

        private void InitService(MoyuVCSEnum topVCS)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte == null || string.IsNullOrEmpty(dte.Solution.FullName))
                throw new System.Exception("未获取到项目根目录");

            var baseFolder = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
            SwitchService(GetVcsService(baseFolder, topVCS));
        }

        public void Update(string path)
        {
            _moyuVCSService.Update(path);
        }

        public void Commit(string path)
        {
            _moyuVCSService.Commit(path);
        }

        public void ShowLog(string path)
        {
            _moyuVCSService.ShowLog(path);
        }

        public void Revert(string path)
        {
            _moyuVCSService.Revert(path);
        }


        private void SwitchService(MoyuVCSEnum moyuVCS)
        {
            switch (moyuVCS)
            {
                case MoyuVCSEnum.SVN:
                    _moyuVCSService = new MoyuSVNVCSService();
                    return;
                case MoyuVCSEnum.Git:
                    _moyuVCSService = new MoyuGitVCSService();
                    return;
                default:
                    throw new System.Exception("未获取到VCS版本信息");
            }
        }

        private MoyuVCSEnum GetVcsService(string folderPath, MoyuVCSEnum topVCS)
        {
            if (string.IsNullOrEmpty(folderPath)) throw new System.Exception("未获取到路劲信息");

            var vcsTypes = _vcsFolderMapping.Keys
                   .OrderByDescending(vcs => vcs == topVCS)
                   .ToList();
            DirectoryInfo directory = new DirectoryInfo(folderPath);
            while (directory != null)
            {
                foreach (var vcsType in vcsTypes)
                {
                    string vcsFolder = _vcsFolderMapping[vcsType];
                    if (Directory.Exists(Path.Combine(directory.FullName, vcsFolder)))
                    {
                        return vcsType;
                    }
                }

                directory = directory.Parent;
            }

            throw new System.Exception("未获取到VCS信息");
        }
    }
}
