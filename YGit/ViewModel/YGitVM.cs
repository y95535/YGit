using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.VisualStudio.Shell.Interop;
using YGit.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace YGit.ViewModel
{
    internal class YGitVM : ObservableObject
    {
        private YGitConf gitConf;
        private ProgressHandler progressHandler = new ProgressHandler(msg => YGitPackage.VSLogger.WriteLine(msg));
        private string cmodule;
        private string cmsg;
        private string sourceMergeBranch;

        /// <summary>
        /// Gets or sets the git conf.
        /// </summary>
        /// <value>
        /// The git conf.
        /// </value>
        public YGitConf GitConf { get => this.gitConf; set => this.SetProperty(ref this.gitConf, value); }

        /// <summary>
        /// Gets or sets the c module.
        /// </summary>
        /// <value>
        /// The c module.
        /// </value>
        public string CModule { get => this.cmodule; set => this.SetProperty(ref this.cmodule, value); }

        /// <summary>
        /// Gets or sets the c MSG.
        /// </summary>
        /// <value>
        /// The c MSG.
        /// </value>
        public string CMsg { get => this.cmsg; set => this.SetProperty(ref this.cmsg, value); }

        /// <summary>
        /// Gets or sets the source merge branch.
        /// </summary>
        /// <value>
        /// The source merge branch.
        /// </value>
        public string SourceMergeBranch { get => this.sourceMergeBranch; set => this.SetProperty(ref this.sourceMergeBranch, value); }

        /// <summary>
        /// Initializes a new instance of the <see cref="YGitVM"/> class.
        /// </summary>
        public YGitVM()
        {
            this.LoadConf();
        }

        /// <summary>
        /// Gets or sets the clone command.
        /// </summary>
        /// <value>
        /// The clone command.
        /// </value>
        public ICommand CloneCmd => new AsyncRelayCommand(CloneAsync);

        /// <summary>
        /// Gets or sets the pull command.
        /// </summary>
        /// <value>
        /// The pull command.
        /// </value>
        public ICommand PullCmd => new AsyncRelayCommand(PullAsync);

        /// <summary>
        /// Gets or sets the merge command.
        /// </summary>
        /// <value>
        /// The merge command.
        /// </value>
        public ICommand MergeCmd { get; set; }

        /// <summary>
        /// Gets or sets the push command.
        /// </summary>
        /// <value>
        /// The push command.
        /// </value>
        public ICommand PushCmd { get; set; }

        /// <summary>
        /// Gets or sets the commit command.
        /// </summary>
        /// <value>
        /// The commit command.
        /// </value>
        public ICommand CommitCmd => new AsyncRelayCommand(CommitAsync);

        /// <summary>
        /// Gets or sets the save conf command.
        /// </summary>
        /// <value>
        /// The save conf command.
        /// </value>
        public ICommand SaveConfCmd => new RelayCommand(SaveConf);

        /// <summary>
        /// Clones the asynchronous.
        /// </summary>
        private async Task CloneAsync()
        {
            await Task.Run(() =>
            {
                if (this.GitConf.OneConf != null)
                    this.CloneModule(this.GitConf.OneConf);

                if (this.GitConf.TwoConf != null)
                    this.CloneModule(this.GitConf.TwoConf);

                if (this.GitConf.ThirdConf != null)
                    this.CloneModule(this.GitConf.ThirdConf);
            });
        }

        /// <summary>
        /// Pulls the asynchronous.
        /// </summary>
        private async Task PullAsync()
        {
            await Task.Run(() =>
            {
                if (this.GitConf.OneConf != null)
                    this.PullModule(this.GitConf.OneConf);

                if (this.GitConf.TwoConf != null)
                    this.PullModule(this.GitConf.TwoConf);

                if (this.GitConf.ThirdConf != null)
                    this.PullModule(this.GitConf.ThirdConf);
            });
        }

        /// <summary>
        /// Commits the asynchronous.
        /// </summary>
        private async Task CommitAsync()
        {
            if (string.IsNullOrWhiteSpace(this.CModule))
                throw new ArgumentNullException(nameof(this.CModule));

            if (string.IsNullOrWhiteSpace(this.CMsg))
                throw new ArgumentNullException(nameof(this.CMsg));

            var message = $"[{this.CModule}] {this.CMsg}";

            await Task.Factory.StartNew(obj =>
            {
                var msg = $"{obj}";
                if (this.GitConf.OneConf != null)
                    this.CommitModule(this.GitConf.OneConf, msg);

                if (this.GitConf.TwoConf != null)
                    this.CommitModule(this.GitConf.TwoConf, msg);

                if (this.GitConf.ThirdConf != null)
                    this.CommitModule(this.GitConf.ThirdConf, msg);
            }, message);
        }

        /// <summary>
        /// Pulls the module.
        /// </summary>
        /// <param name="conf">The conf.</param>
        private void PullModule(YGitRepoConf conf)
        {
            this.Initialize(conf);
            var signature = new Signature(this.GitConf.UserName, this.GitConf.Email, DateTimeOffset.Now);
            var mergeOpts = new MergeOptions() { };
            var fetchOpts = new FetchOptions { Prune = true, OnProgress = progressHandler };
            var pullOpts = new PushOptions { CredentialsProvider = (url, username, password) => new UsernamePasswordCredentials { Username = this.GitConf.UserName, Password = this.GitConf.Password } };

            #region 私仓

            var origin = conf.Repository.Network.Remotes[conf.RemoteName];
            var orefSpecs = origin.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(conf.Repository, conf.RemoteName, orefSpecs, null, null);
            Commands.Pull(conf.Repository, signature, new PullOptions { FetchOptions = fetchOpts });

            #endregion

            #region 团仓

            if (string.IsNullOrWhiteSpace(conf.TeamRemoteName) || string.IsNullOrWhiteSpace(conf.TeamRemoteUrl))
                return;

            var teamOrigin = conf.Repository.Network.Remotes[conf.TeamRemoteName];
            var teamBranch = $"{conf.TeamRemoteName}/{this.GitConf.BranchName}";
            var localBranch = conf.Repository.Branches[this.GitConf.BranchName];
            Commands.Fetch(conf.Repository, conf.TeamRemoteName, orefSpecs, null, null);
            var mergeResult = conf.Repository.Merge(teamBranch, signature, mergeOpts);

            if (mergeResult.Status == MergeStatus.Conflicts)
            {
                // 提示冲突 
                YGitPackage.VSLogger.WriteLine($"Merge branch '{conf.TeamRemoteName}/{this.GitConf.BranchName}' into {this.GitConf.BranchName} conflict.");
            }
            else
            {
                conf.Repository.Commit($"Merge branch '{conf.TeamRemoteName}/{this.GitConf.BranchName}' into {this.GitConf.BranchName} .", signature, signature);
                conf.Repository.Network.Push(localBranch, pullOpts);
            }

            #endregion
        }

        /// <summary>
        /// Clones the module.
        /// </summary>
        /// <param name="conf">The conf.</param>
        private void CloneModule(YGitRepoConf conf)
        {
            var cloneOptions = new CloneOptions
            {
                BranchName = this.GitConf.BranchName,
                OnProgress = progressHandler,
                CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials
                    {
                        Username = this.GitConf.UserName,
                        Password = this.GitConf.Password
                    }
            };

            // Clone the repository
            var path = Repository.Clone(conf.RemoteUrl, conf.LocalPath, cloneOptions);

            if (Directory.Exists(path))
            {
                this.AddRemote(conf);
                this.SetPushUrl(conf);
                this.Fetch(conf);
            }
        }

        /// <summary>
        /// Commits the module.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="msg">The MSG.</param>
        private void CommitModule(YGitRepoConf conf, string msg)
        {
            var signature = new Signature(this.GitConf.UserName, this.GitConf.Email, DateTimeOffset.Now);
            this.Initialize(conf).Commit(msg, signature, signature);
        }

        /// <summary>
        /// Adds the remote.
        /// </summary>
        /// <param name="conf">The conf.</param>
        private void AddRemote(YGitRepoConf conf)
        {
            if (string.IsNullOrWhiteSpace(conf.TeamRemoteName))
                return;

            if (string.IsNullOrWhiteSpace(conf.TeamRemoteUrl))
                return;

            this.Initialize(conf)?.Network.Remotes.Add(conf.TeamRemoteName, conf.RemoteUrl);
        }

        /// <summary>
        /// Sets the push URL.
        /// </summary>
        /// <param name="conf">The conf.</param>
        private void SetPushUrl(YGitRepoConf conf)
        {
            if (string.IsNullOrWhiteSpace(conf.RemoteUrl))
                return;

            this.Initialize(conf)?.Network.Remotes.Update(conf.TeamRemoteName, r => r.PushUrl = conf.RemoteUrl);
        }

        /// <summary>
        /// Fetches the specified conf.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <exception cref="LibGit2Sharp.NotFoundException">
        /// Remote '{conf.RemoteName}' not found
        /// or
        /// Remote '{conf.TeamRemoteName}' not found
        /// </exception>
        private void Fetch(YGitRepoConf conf)
        {
            this.Initialize(conf);

            #region 私仓

            if (string.IsNullOrWhiteSpace(conf.RemoteName))
                return;

            var remote = conf.Repository.Network.Remotes[conf.RemoteName];

            if (remote == null)
                throw new NotFoundException($"Remote '{conf.RemoteName}' not found");

            var remoteRefSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(conf.Repository, remote.Name, remoteRefSpecs, null, null);

            #endregion

            #region 团仓

            if (string.IsNullOrWhiteSpace(conf.TeamRemoteName))
                return;

            var teamRemote = conf.Repository.Network.Remotes[conf.TeamRemoteName];

            if (teamRemote == null)
                throw new NotFoundException($"Remote '{conf.TeamRemoteName}' not found");

            var teamRemoteRefSpecs = teamRemote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(conf.Repository, teamRemote.Name, teamRemoteRefSpecs, null, null);

            #endregion
        }

        /// <summary>
        /// Initializes the specified conf.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <exception cref="System.ArgumentNullException">LocalPath</exception>
        /// <exception cref="LibGit2Sharp.NotFoundException"></exception>
        private Repository Initialize(YGitRepoConf conf)
        {
            if (string.IsNullOrWhiteSpace(conf.LocalPath))
                throw new ArgumentNullException(nameof(conf.LocalPath));

            if (!Directory.Exists(conf.LocalPath))
                throw new DirectoryNotFoundException($"{conf.LocalPath} not found.");

            if (conf.Repository == null)
                conf.Repository = new Repository(conf.LocalPath);

            return conf.Repository;
        }

        /// <summary>
        /// Loads the conf.
        /// </summary>
        private void LoadConf()
        {
            var confDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YGit");
            var confPath = Path.Combine(confDir, "YGit.json");

            if (File.Exists(confPath))
            {
                var json = File.ReadAllText(confPath);
                this.GitConf = Newtonsoft.Json.JsonConvert.DeserializeObject<YGitConf>(json);
            }
        }

        /// <summary>
        /// Saves the conf.
        /// </summary>
        private void SaveConf()
        {
            var confDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YGit");
            var confPath = Path.Combine(confDir, "YGit.json");

            if (File.Exists(confPath))
                File.Delete(confPath);

            if (this.GitConf.OneConf != null)
                this.GitConf.OneConf.LocalPath = Path.Combine(this.GitConf.RootPath, this.GitConf.OneConf.RepoName);

            if (this.GitConf.TwoConf != null)
                this.GitConf.TwoConf.LocalPath = Path.Combine(this.GitConf.RootPath, this.GitConf.TwoConf.RepoName);

            if (this.GitConf.ThirdConf != null)
                this.GitConf.ThirdConf.LocalPath = Path.Combine(this.GitConf.RootPath, this.GitConf.ThirdConf.RepoName);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this.GitConf);
            File.WriteAllText(confPath, json);
        }
    }
}
