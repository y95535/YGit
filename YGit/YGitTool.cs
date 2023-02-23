using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace YGit
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("e96141b9-6b60-4af7-9cf1-b9e44a6b45f2")]
    public class YGitTool : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YGitTool"/> class.
        /// </summary>
        public YGitTool() : base(null)
        {
            this.Caption = "YGitTool";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new YGitToolControl();
        }
    }
}
