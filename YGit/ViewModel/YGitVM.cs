using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.VisualStudio.Shell.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace YGit.ViewModel
{
    internal class YGitVM: ObservableObject
    {
        private IVsOutputWindow output;

        public YGitVM() 
        {
            this.CloneCmd = new AsyncRelayCommand(CloneAsync);
            GlobalSettings.NativeLibraryPath = @"C:\Users\jiede\source\repos\YGit\YGit\bin\Debug\lib\win32\x64\git2-182d0d1.dll"; 
        }

        public ICommand CloneCmd { get; set; }
        public ICommand PullCmd { get; set; }
        public ICommand MergeCmd { get; set; }

        public async Task CloneAsync()
        {
            var progressHandler = new ProgressHandler(serverProgressOutput =>
            {
                YGitPackage.VSLogger.WriteLine(serverProgressOutput);
                return true;
            });

            var cloneOptions = new CloneOptions
            {
                BranchName ="main",
                //OnProgress = progressHandler,
                CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials
                    {
                        Username = "jasondevstudio",
                        Password = "yaojie05120108"
                    }
            };
             
            // Clone the repository
            var msg = Repository.Clone("https://github.com/JasonDevStudio/YGit.git", @"C:\Users\jiede\Downloads\git", cloneOptions); 
        }
    }
}
