// See https://aka.ms/new-console-template for more information

using LibGit2Sharp.Handlers;
using LibGit2Sharp;

//GlobalSettings.NativeLibraryPath = @"C:\Users\jiede\source\repos\YGit\YGitConsole\bin\Debug\net7.0\git2-182d0d1.dll";

var progressHandler = new ProgressHandler(serverProgressOutput =>
{
    Console.WriteLine(serverProgressOutput);
    return true;
});

var cloneOptions = new CloneOptions
{
    BranchName = "main",
    OnProgress = progressHandler,
    CredentialsProvider = (url, usernameFromUrl, types) =>
        new UsernamePasswordCredentials
        {
            Username = "jasondevstudio",
            Password = "yaojie05120108"
        }
};

// Clone the repository
var msg = Repository.Clone("https://github.com/JasonDevStudio/YGit.git", @"C:\Users\jiede\Downloads\git", cloneOptions);

Console.WriteLine("Hello, World!");
