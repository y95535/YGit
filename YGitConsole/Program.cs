// See https://aka.ms/new-console-template for more information

using LibGit2Sharp.Handlers;
using LibGit2Sharp;

//GlobalSettings.NativeLibraryPath = @"C:\Users\jiede\source\repos\YGit\YGitConsole\bin\Debug\net7.0\git2-182d0d1.dll";

var progressHandler = new ProgressHandler(serverProgressOutput =>
{
    Console.WriteLine(serverProgressOutput);
    return true;
});

var checkoutProgressHandler = new CheckoutProgressHandler((title, completedSteps, totalSteps) =>
{
    Console.WriteLine($"Progress update: {title} ({completedSteps} of {totalSteps} completed)."); 
});

var cloneOptions = new CloneOptions
{
    OnProgress = progressHandler,
    CredentialsProvider = (url, usernameFromUrl, types) =>
        new UsernamePasswordCredentials
        {
            Username = "jasondevstudio",
            Password = "yaojie05120108"
        }
};

// Clone the repository
var path = Repository.Clone("https://github.com/JasonDevStudio/YGit.git", @"C:\Users\jiede\Downloads\git", cloneOptions);
var repo = new Repository(path);

// Get a reference to the branch you want to switch to 
var remoteName = "origin";
var remoteBranchName = "develop";
var localBranchName = "develop"; 

foreach (var item in repo.Branches)
{
    Console.WriteLine($"{item.RemoteName}, {item.FriendlyName}, {item.CanonicalName}, {item.TrackedBranch}");
}

var remoteBranch = repo.Branches[$"{remoteName}/{remoteBranchName}"];
if (remoteBranch == null)
{
    throw new Exception($"Remote branch '{remoteName}/{remoteBranchName}' not found");
}

var localBranch = repo.Branches[localBranchName];
if (localBranch != null)
{
    throw new Exception($"Local branch '{localBranchName}' already exists");
}

localBranch = repo.Branches.Add(localBranchName, remoteBranch.Tip);
 
// Switch to the branch
Commands.Checkout(repo, localBranch, new CheckoutOptions()
{
    CheckoutModifiers = CheckoutModifiers.Force,
    OnCheckoutProgress = checkoutProgressHandler
});
