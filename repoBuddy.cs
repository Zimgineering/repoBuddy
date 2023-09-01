//!CompilerOption:AddRef:SharpSvn.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot.AClasses;
using ff14bot.Helpers;
using ff14bot.Managers;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using SharpSvn;


namespace repoBuddy;

public class repoBuddy : BotPlugin
{
#if RB_CN
        public override string Name => "RB 资源更新器";
#else
    public override string Name => "repoBuddy";
#endif
    public override string Author => "Zimble";
    public override Version Version => new Version(1, 11);
    public override string Description => "Automatically update RB accessories from repositories";
    public override bool WantButton => true;
    public override string ButtonText => "Settings";
    public static DataSet repoDataSet = new DataSet();
    public static string repoXML => Path.Combine(SourceDirectory().FullName, "repoBuddyRepos.xml");
    private static Color LogColor = Colors.Wheat;
    public bool restartNeeded = false;
    public static List<string> repoLog = new List<string>();
    public static Dictionary<string, List<string>> ddlDict = new Dictionary<string, List<string>>();

    public override void OnButtonPress()
    {
        CreateSettingsForm();
    }

    public override void OnEnabled()
    {
        //Thread waitThread = new Thread(WaitForDone);
        //waitThread.Start();

        Logging.Write(LogColor, $"[{Name}-v{Version}] checking for updates");
        RoutineManager.RoutineChanged += new EventHandler(WaitForLog);
        repoStart();
    }

    public override void OnInitialize()
    {
        GetRepoData();
        GetDdlData();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static DirectoryInfo SourceDirectory()
    {
        var frame = new StackFrame(0, true);
        var file = frame.GetFileName();

        if (!string.IsNullOrEmpty(file) && File.Exists(file))
        {
            return new DirectoryInfo(Path.GetDirectoryName(file));
        }

        return null;
    }

    public void MigrateLlamaLibrary()
    {
        try
        {
            for (int i = 0; i < repoDataSet.Tables["Repo"].Rows.Count; i++)
            {
                if (repoDataSet.Tables["Repo"].Rows[i]["Name"].ToString() == "FCBuffPlugin")
                {
                    repoDataSet.Tables["Repo"].Rows.RemoveAt(i);
                }

                if (repoDataSet.Tables["Repo"].Rows[i]["Name"].ToString() == "LisbethVentures")
                {
                    repoDataSet.Tables["Repo"].Rows.RemoveAt(i);
                }
            }

            if (Directory.Exists($@"Plugins\FCBuffPlugin"))
            {
                ZipFolder($@"Plugins\FCBuffPlugin", $@"Plugins\FCBuffPlugin_{DateTime.Now.Ticks}.zip");
                Directory.Delete($@"Plugins\FCBuffPlugin", true);
                //restartNeeded = true;
            }

            if (Directory.Exists($@"Plugins\LisbethVentures"))
            {
                ZipFolder($@"Plugins\LisbethVentures", $@"Plugins\LisbethVentures_{DateTime.Now.Ticks}.zip");
                Directory.Delete($@"Plugins\LisbethVentures", true);
                //restartNeeded = true;
            }

            repoDataSet.WriteXml(repoXML);
        }
        catch (Exception e)
        {
            Logging.Write(LogColor, $"[{Name}-v{Version}] Cleaning up migrated LlamaLibrary misc. failed, delete LisbethVentures and FCBuffPlugin manually. {e}");
        }

        try
        {
            if (Directory.Exists($@"BotBases\LlamaLibrary"))
            {
                for (int i = 0; i < repoDataSet.Tables["Repo"].Rows.Count; i++)
                {
                    if (repoDataSet.Tables["Repo"].Rows[i]["Name"].ToString() == "LlamaLibrary")
                    {
                        repoDataSet.Tables["Repo"].Rows.RemoveAt(i);
                        repoDataSet.Tables["Repo"].Rows.Add("__LlamaLibrary", "Quest Behavior", "https://github.com/nt153133/__LlamaLibrary.git/trunk");
                        repoDataSet.Tables["Repo"].Rows.Add("LlamaUtilities", "Botbase", "https://github.com/nt153133/LlamaUtilities.git/trunk");
                        repoDataSet.Tables["Repo"].Rows.Add("ExtraBotbases", "Botbase", "https://github.com/nt153133/ExtraBotbases.git/trunk");
                        repoDataSet.Tables["Repo"].Rows.Add("ResplendentTools", "Botbase", "https://github.com/Sykel/ResplendentTools.git/trunk");
                        repoDataSet.Tables["Repo"].Rows.Add("LlamaPlugins", "Plugin", "https://github.com/nt153133/LlamaPlugins.git/trunk");
                    }
                }

                repoDataSet.WriteXml(repoXML);
                //restartNeeded = true;
                ZipFolder($@"BotBases\LlamaLibrary", $@"BotBases\LlamaLibrary_{DateTime.Now.Ticks}.zip");
                Directory.Delete($@"BotBases\LlamaLibrary", true);
            }
        }
        catch (Exception e)
        {
            Logging.Write(LogColor, $"[{Name}-v{Version}] Archiving LlamaLibrary failed, please backup and delete manually. {e}");
        }
    }

    public void CreateSettingsForm()
    {
        SettingsForm settingsForm = new SettingsForm();
        settingsForm.ShowDialog();
    }

    public void GetRepoData()
    {
        if (!File.Exists(@"Plugins\repoBuddy\repoBuddyRepos.xml"))
        {
            File.Copy(@"Plugins\repoBuddy\Default.repoBuddyRepos.xml", @"Plugins\repoBuddy\repoBuddyRepos.xml");
        }

        repoDataSet.Clear();
        repoDataSet.ReadXml(repoXML);
    }

    public void GetDdlData()
    {
        using (StreamReader file = File.OpenText(@"Plugins\repoBuddy\ddls.json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            ddlDict = (Dictionary<string, List<string>>)serializer.Deserialize(file, typeof(Dictionary<string, List<string>>));
        }
    }

    static repoBuddy()
    {
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) //force load sharpsvn
    {
        string path = @"Plugins\repoBuddy\SharpSvn.dll";

        try
        {
            Unblock(path);
        }
        catch (Exception)
        {
            // pass
        }

        AssemblyName asmName = new AssemblyName(args.Name);

        if (asmName.Name != "SharpSvn")
        {
            return null;
        }

        return Assembly.LoadFrom(path);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    public static extern bool DeleteFile(string name);

    public static bool Unblock(string fileName)
    {
        return DeleteFile(fileName + ":Zone.Identifier");
    }

    public static void RestartRebornBuddy()
    {
        //AppDomain.CurrentDomain.ProcessExit += new EventHandler(RebornBuddy_Exit);
        //
        //void RebornBuddy_Exit (object sender, EventArgs e)
        //{
        //	Process.Start("rebornbuddy", "-a"); //autologin using stored key
        //}

        Process RBprocess = Process.GetCurrentProcess();
        Process.Start(@"Plugins\repoBuddy\watchdog.bat", $"{RBprocess.Id} {ff14bot.Core.Memory.Process.Id}");
        RBprocess.CloseMainWindow();
    }

    public void WriteLog(List<string> array, string msg)
    {
        array.Add(msg);
        Logging.Write(LogColor, msg);
    }

    #region rebornbuddy init thread logic

    public void WaitForLog(object obj, EventArgs eve)
    {
        RoutineManager.RoutineChanged -= WaitForLog;
        Logging.Write(LogColor, $"[{Name}-v{Version}] waiting for Logs to end...");
        System.Timers.Timer logwatch = new System.Timers.Timer();
        logwatch.Interval = 3000;
        logwatch.AutoReset = true;
        logwatch.Enabled = true;
        logwatch.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        Logging.OnLogMessage += new Logging.LogMessageDelegate(RestartTimer);

        void RestartTimer(ReadOnlyCollection<Logging.LogMessage> message)
        {
            logwatch.Stop();
            logwatch.Start();
        }

        void OnTimedEvent(object o, System.Timers.ElapsedEventArgs e)
        {
            Logging.Write(LogColor, $"[{Name}-v{Version}] RB fully loaded!");
            Logging.OnLogMessage -= RestartTimer;
            logwatch.Elapsed -= OnTimedEvent;
            logwatch.Stop();
            logwatch.Dispose();

            using (StreamReader file = File.OpenText(@"Plugins\repoBuddy\repoLog.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                repoLog = (List<string>)serializer.Deserialize(file, typeof(List<string>));

                foreach (string change in repoLog)
                {
                    Logging.Write(LogColor, change);
                }
            }

            using (StreamWriter file = File.CreateText(@"Plugins\repoBuddy\repoLog.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                repoLog.Clear();
                serializer.Serialize(file, repoLog);
            }
        }
    }

    #endregion

    #region repo logic

    public void repoStart()
    {
        SvnRevertArgs revertArgs = new SvnRevertArgs()
        {
            Depth = SvnDepth.Infinity
        };

        Stopwatch stopwatch = Stopwatch.StartNew();

        Parallel.ForEach(repoDataSet.Tables["Repo"].Rows.Cast<DataRow>(), row =>
        {
            string repoName = row[0].ToString();
            string repoType = row[1].ToString() + "s";
            string repoUrl = row[2].ToString();
            string repoPath = $@"{repoType}\{repoName}";


            long currentLap;
            long totalLap;
            currentLap = stopwatch.ElapsedMilliseconds;

            try
            {
                using (SvnClient client = new SvnClient())
                {
                    if (Directory.Exists($@"{repoPath}\.svn"))
                    {
                        Collection<SvnLogEventArgs> logitems;

                        SvnInfoEventArgs remoteRev;
                        client.GetInfo(repoUrl, out remoteRev);

                        SvnInfoEventArgs localRev;
                        client.GetInfo(repoPath, out localRev);

                        SvnLogArgs logArgs = new SvnLogArgs()
                        {
                            Start = localRev.Revision + 1,
                            End = remoteRev.Revision
                        };

                        if (localRev.Revision < remoteRev.Revision)
                        {
                            client.Revert(repoPath, revertArgs);
                            client.Update(repoPath);
                            totalLap = stopwatch.ElapsedMilliseconds - currentLap;

                            client.GetLog(repoPath, logArgs, out logitems);

                            foreach (var logentry in logitems)
                            {
                                string logString = logentry.LogMessage.Replace(Environment.NewLine, " ");
                                WriteLog(repoLog, $@"[{Name}-v{Version}] {repoName} r{logentry.Revision}: {logString}");
                            }

                            WriteLog(repoLog, $"[{Name}-v{Version}] updated [{repoType}] {repoName} from {localRev.Revision} to {remoteRev.Revision} in {totalLap} ms.");
                            if (repoType != "Profiles")
                            {
                                restartNeeded = true;
                            }
                        }
                    }
                    else
                    {
                        client.CheckOut(new Uri(repoUrl), repoPath);
                        totalLap = stopwatch.ElapsedMilliseconds - currentLap;
                        WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} checkout complete in {totalLap} ms.");
                        if (repoType != "Profiles")
                        {
                            restartNeeded = true;
                        }
                    }
                }
            }
            catch (SvnAuthenticationException e)
            {
                WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} No more credentials or we tried too many times. {e}");
            }
            catch (AccessViolationException e)
            {
                WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} Access Violation, something is locking the folder. {e}");
            }
            catch (SvnFileSystemException e)
            {
                WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} FileSystemException, repo has probably been moved/deleted. {e}");
            }
            catch (SvnException e)
            {
                WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} Generic SvnException, do you have tortoiseSVN monitoring this folder? CN users may need a VPN to access GitHub. {e}");

                WriteLog(repoLog, $"[{Name}-v{Version}] **************************");
                WriteLog(repoLog, $"[{Name}-v{Version}] This will prevent further updates, delete the {repoName} .svn folder and make sure tortoiseSVN doesn't manage anything repoBuddy does.");
                WriteLog(repoLog, $"[{Name}-v{Version}] **************************");
                restartNeeded = false;
            }
        });
        stopwatch.Stop();
        Logging.Write(LogColor, $"[{Name}-v{Version}] processes complete in {stopwatch.ElapsedMilliseconds} ms.");

        MigrateLlamaLibrary();

        if (repoLog.Count > 0)
        {
            using (StreamWriter file = File.CreateText(@"Plugins\repoBuddy\repoLog.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, repoLog);
            }
        }

        if (restartNeeded)
        {
            Logging.Write(LogColor, $"[{Name}-v{Version}] Restarting to reload assemblies.");
            RestartRebornBuddy();
        }
    }

    #endregion

    #region ddl logic

    public static void DirectDownload(string path, string Url)
    {
        var bytes = DownloadLatestVersion(Url).Result;

        if (bytes == null || bytes.Length == 0)
        {
            Logging.Write(LogColor, $"[Error] Bad product data returned.");
            return;
        }

        if (!Extract(bytes, path))
        {
            Logging.Write(LogColor, $"[Error] Could not extract new files.");
            return;
        }
    }

    private static bool Extract(byte[] files, string directory)
    {
        try
        {
            using (var stream = new MemoryStream(files))
            {
                var zip = new FastZip();
                var previous = ZipConstants.DefaultCodePage;
                ZipConstants.DefaultCodePage = 437;
                zip.ExtractZip(stream, directory, FastZip.Overwrite.Always, null, null, null, false, true);
                ZipConstants.DefaultCodePage = previous;
            }

            return true;
        }
        catch (Exception ex)
        {
            Logging.Write(LogColor, $"[Error] Could not extract new files. {ex}");
            return false;
        }
    }

    public static void ZipFolder(string sourceFolder, string zipPath)
    {
        var zip = new FastZip();
        var previous = ZipConstants.DefaultCodePage;
        ZipConstants.DefaultCodePage = 437;
        zip.CreateZip(zipPath, sourceFolder, true, null);
        ZipConstants.DefaultCodePage = previous;
    }

    private static async Task<byte[]> DownloadLatestVersion(string Url)
    {
        using (var client = new HttpClient())
        {
            byte[] responseMessageBytes;
            try
            {
                responseMessageBytes = client.GetByteArrayAsync(Url).Result;
            }
            catch (Exception e)
            {
                Logging.Write(LogColor, e.Message);
                return null;
            }

            return responseMessageBytes;
        }
    }

    #endregion
}
