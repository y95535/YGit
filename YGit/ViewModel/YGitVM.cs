using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YGit.ViewModel
{
    internal class YGitVM: ObservableObject
    {
        public ICommand CloneCmd { get; set; }


    }
}
