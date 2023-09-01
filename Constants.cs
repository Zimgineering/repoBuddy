using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace repoBuddy;
internal static class Constants
{

    public static string RepoBuddyDirectory = GetSourceDirectory().FullName;

    public static string ReposXmlPath = Path.Combine(RepoBuddyDirectory, "repoBuddyRepos.xml");
    public static string DefaultReposXmlPath = Path.Combine(RepoBuddyDirectory, "Default.repoBuddyRepos.xml");
    public static string DdlsJsonPath = Path.Combine(RepoBuddyDirectory, "ddls.json");
    public static string WatchdogBatPath = Path.Combine(RepoBuddyDirectory, "watchdog.bat");
    public static string RepoLogJsonPath = Path.Combine(RepoBuddyDirectory, "repoLog.json");
    public static string SharpSvnDllPath = Path.Combine(RepoBuddyDirectory, "SharpSvn.dll");

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static DirectoryInfo GetSourceDirectory()
    {
        var frame = new StackFrame(0, true);
        var file = frame.GetFileName();

        if (!string.IsNullOrEmpty(file) && File.Exists(file))
        {
            return new DirectoryInfo(Path.GetDirectoryName(file));
        }

        return null;
    }
}
