//!CompilerOption:AddRef:Plugins\repoBuddy\SharpSvn.dll
//!CompilerOption:AddRef:System.IO.Compression.dll
//!CompilerOption:AddRef:System.IO.Compression.FileSystem.dll
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Media;
using Newtonsoft.Json;
using ff14bot.AClasses;
using ff14bot.Forms.ugh;
using ff14bot.Helpers;
using ff14bot.Managers;
using SharpSvn;

using System.IO.Compression;
using System.Net.Http;


namespace repoBuddy
{
	public class repoBuddy : BotPlugin
	{
		#if RB_CN
		public override string Name => "RB 资源更新器";
		#else
		public override string Name => "repoBuddy";
		#endif		
		public override string Author => "Zimble";
		public override Version Version => new Version(1,11);
		public override string Description => "Automatically update rb accessories from repositories";
		public override bool WantButton => true;
		public override string ButtonText => "Settings";
		public static DataSet repoDataSet = new DataSet();
		public static string repoXML = @"Plugins\repoBuddy\repoBuddyRepos.xml";
		private static Color LogColor = Colors.Wheat;
		public bool restartNeeded = false;
		public static List<String> repoLog = new List<String>();
		public static Dictionary<String, List<String>> ddlDict = new Dictionary<String, List<String>>();

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

			GetrepoData();
			GetddlData();
		}
		public void MigrateLlamaLibrary()
		{
			try
			{
				for (int i = 0; i < repoDataSet.Tables["Repo"].Rows.Count; i++)
				{
					if (repoDataSet.Tables["Repo"].Rows[i]["Name"].ToString()=="FCBuffPlugin")
					{
						repoDataSet.Tables["Repo"].Rows.RemoveAt(i);
					}
					if (repoDataSet.Tables["Repo"].Rows[i]["Name"].ToString()=="LisbethVentures")
					{
						repoDataSet.Tables["Repo"].Rows.RemoveAt(i);
					}
				}
				if (System.IO.Directory.Exists($@"Plugins\FCBuffPlugin"))
				{					
					ZipFile.CreateFromDirectory($@"Plugins\FCBuffPlugin", $@"Plugins\FCBuffPlugin_{DateTime.Now.Ticks}.zip");
					Directory.Delete($@"Plugins\FCBuffPlugin", true);
					//restartNeeded = true;
				}
				if (System.IO.Directory.Exists($@"Plugins\LisbethVentures"))
				{					
					ZipFile.CreateFromDirectory($@"Plugins\LisbethVentures", $@"Plugins\LisbethVentures_{DateTime.Now.Ticks}.zip");
					Directory.Delete($@"Plugins\LisbethVentures", true);
					//restartNeeded = true;
				}
				repoDataSet.WriteXml(repoXML);		
			}
			catch (Exception e)
			{
				Logging.Write(LogColor, $"[{Name}-v{Version}] Cleaning up migrated Llamalibrary misc. failed, delete LisbethVentures and FCBuffPlugin manually. {e}");
			}
			try
			{
				if (System.IO.Directory.Exists($@"BotBases\LlamaLibrary"))
				{
					for (int i = 0; i < repoDataSet.Tables["Repo"].Rows.Count; i++)
					{
						if (repoDataSet.Tables["Repo"].Rows[i]["Name"].ToString()=="LlamaLibrary")
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
					ZipFile.CreateFromDirectory($@"BotBases\LlamaLibrary", $@"BotBases\LlamaLibrary_{DateTime.Now.Ticks}.zip");
					Directory.Delete($@"BotBases\LlamaLibrary", true);
				}
				
			}
			catch (Exception e)
			{
				Logging.Write(LogColor, $"[{Name}-v{Version}] Archiving Llamalibrary failed, please backup and delete manually. {e}");
			}
		}
		public void CreateSettingsForm()
		{
			Form1 settingsForm = new Form1();
			settingsForm.ShowDialog();
		}
		public void GetrepoData()
		{
			if (!File.Exists(@"Plugins\repoBuddy\repoBuddyRepos.xml"))
			{
				File.Copy(@"Plugins\repoBuddy\Default.repoBuddyRepos.xml", @"Plugins\repoBuddy\repoBuddyRepos.xml");
			}
			
			repoDataSet.Clear();
			repoDataSet.ReadXml(repoXML);
		}
		public void GetddlData()
		{
			using (StreamReader file = File.OpenText(@"Plugins\repoBuddy\ddls.json"))
			{
				JsonSerializer serializer = new JsonSerializer();
				ddlDict = (Dictionary<String, List<String>>)serializer.Deserialize(file, typeof(Dictionary<string, List<string>>));
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
				return null;
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
		public void WriteLog(List<String> array, String msg)
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
			void RestartTimer (ReadOnlyCollection<Logging.LogMessage> message)
			{
				logwatch.Stop();
				logwatch.Start();
			}
			void OnTimedEvent (object o, System.Timers.ElapsedEventArgs e)
			{
				
				Logging.Write(LogColor, $"[{Name}-v{Version}] RB fully loaded!");
				Logging.OnLogMessage -= RestartTimer;
				logwatch.Elapsed -= OnTimedEvent;
				logwatch.Stop();
				logwatch.Dispose();

				using (StreamReader file = File.OpenText(@"Plugins\repoBuddy\repoLog.json"))
				{
					JsonSerializer serializer = new JsonSerializer();
					repoLog = (List<String>)serializer.Deserialize(file, typeof(List<String>));
					
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
						if (System.IO.Directory.Exists($@"{repoPath}\.svn"))
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
									String logString = logentry.LogMessage.Replace(System.Environment.NewLine, " ");
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
				catch (SharpSvn.SvnAuthenticationException e)
				{
					WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} No more credentials or we tried too many times. {e}");
				}
				catch (System.AccessViolationException e)
				{
					WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} Access Violation, something is locking the folder. {e}");
				}
				catch (SharpSvn.SvnFileSystemException e)
				{
					WriteLog(repoLog, $"[{Name}-v{Version}] {repoName} FileSystemException, repo has probably been moved/deleted. {e}");
				}
				catch (SharpSvn.SvnException e)
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
			using (var stream = new MemoryStream(files))
			{
				var Zip = new ZipArchive(stream);
				ZipFileExtensions.ExtractToDirectory(Zip, directory);
			}
			
			return true;
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
	#region GUI stuff
	public class Form1 : Form
	{

		private FlowLayoutPanel controlPanel = new FlowLayoutPanel();
		private FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
		private DataGridView repoDataGridView = new DataGridView();
		private TextBox nameBox = new TextBox();
		private ComboBox typeBox = new ComboBox();
		private TextBox urlBox = new TextBox();
		private Button addNewRowButton = new Button();
		private Button deleteRowButton = new Button();
		private Button restartRBButton = new Button();
		private TabPage repoTab = new TabPage();
		private	TabPage ddlTab = new TabPage();
		private TabControl tabControls = new TabControl();
		private DataSet repoDataSet = repoBuddy.repoDataSet;
		private string repoXML = repoBuddy.repoXML;
		private Dictionary<String, List<String>> ddlDict = repoBuddy.ddlDict;

		private void SetupLayout()
		{
			Text = "repoBuddy Settings";
			Size = new System.Drawing.Size(525, 400);
			MaximizeBox = false;
			MinimizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;

			repoTab.Text = "Repositories";
			repoTab.TabIndex = 0;
			ddlTab.Text = "One-time Downloads";
			ddlTab.TabIndex = 1;
			
			nameBox.Text = "Repo Name";
			nameBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);
			nameBox.GotFocus += new EventHandler(NameBox_GotFocus);
			nameBox.LostFocus += new EventHandler(NameBox_LostFocus);

			typeBox.Items.AddRange(new string[] { "BotBase", "Plugin", "Profile", "Routine", "Quest Behavior" });
			typeBox.DropDownStyle = ComboBoxStyle.DropDownList;
			typeBox.SelectedIndex = 0;

			urlBox.Text = "Repo URL";
			urlBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);
			urlBox.GotFocus += new EventHandler(urlBox_GotFocus);
			urlBox.LostFocus += new EventHandler(urlBox_LostFocus);

			addNewRowButton.Text = "Add Row";
			addNewRowButton.Click += new EventHandler(AddNewRowButton_Click);

			deleteRowButton.Text = "Delete Row";
			deleteRowButton.Click += new EventHandler(DeleteRowButton_Click);
			
			restartRBButton.Text = "Restart RebornBuddy";
			restartRBButton.Click += new EventHandler(restartRBButton_Click);
			restartRBButton.AutoSize = true;
			restartRBButton.Width = 495;
			restartRBButton.FlatStyle = FlatStyle.Flat;

			buttonPanel.Controls.Add(restartRBButton);

			foreach (KeyValuePair<string, List<string>> pair in ddlDict)
			{
				string ddlname = pair.Key;
				string ddlmask = pair.Value[0];
				string ddluri = pair.Value[1];
				string ddldesc = pair.Value[2];
				
				Button button = new Button();				
				button.Click += new EventHandler(ddlButton_Click);
				button.Name = ddlname;
				button.Text = ddlname + " - " +ddldesc;
				button.AutoSize = true;
				button.Width = 495;
				buttonPanel.Controls.Add(button);

				string resolvedMask;

				if (ddlmask.Contains(ddlname.Replace("-CN", "")))
				{
					resolvedMask = ddlmask;
				}
				else
				{
					resolvedMask = ddlmask + @"\" + ddlname.Replace("-CN", "");
				}

				if(Directory.Exists(resolvedMask))
				{
					button.Text = $"[INSTALLED] {button.Text}";
					button.Enabled = false;
				}
				button.FlatStyle = FlatStyle.Flat;
			}

			controlPanel.Controls.Add(nameBox);
			controlPanel.Controls.Add(typeBox);
			controlPanel.Controls.Add(urlBox);
			controlPanel.Controls.Add(addNewRowButton);
			controlPanel.Controls.Add(deleteRowButton);
			controlPanel.AutoSize = true;
			controlPanel.Dock = DockStyle.Bottom;

			buttonPanel.AutoScroll = true;
			buttonPanel.AutoSize = true;
			buttonPanel.Dock = DockStyle.Fill;
			tabControls.Dock = DockStyle.Fill;
			tabControls.Controls.Add(repoTab);
			repoTab.Controls.Add(repoDataGridView); //controls have their Dock setting evaluated from the bottom up, so this is first to prevent overlap with control panel
			repoTab.Controls.Add(controlPanel);
			
			tabControls.Controls.Add(ddlTab);
			ddlTab.Controls.Add(buttonPanel);
			
			Controls.Add(tabControls);
		}
		private void SetupDataGridView()
		{
			DataGridViewTextBoxColumn repoNameColumn = new DataGridViewTextBoxColumn()
			{
				HeaderText = "Name",
				DataPropertyName = "Name",
				Width = 75
			};

			DataGridViewTextBoxColumn repoTypeColumn = new DataGridViewTextBoxColumn()
			{
				HeaderText = "Type",
				DataPropertyName = "Type",
				Width = 75
			};

			DataGridViewTextBoxColumn repoURLColumn = new DataGridViewTextBoxColumn()
			{
				HeaderText = "URL",
				DataPropertyName = "URL",
				AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
				SortMode = DataGridViewColumnSortMode.NotSortable
			};

			repoDataGridView.Columns.Add(repoNameColumn);
			repoDataGridView.Columns.Add(repoTypeColumn);
			repoDataGridView.Columns.Add(repoURLColumn);

			repoDataGridView.AllowUserToAddRows = false;
			repoDataGridView.AllowUserToDeleteRows = false;
			repoDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			repoDataGridView.ReadOnly = true;

			repoDataGridView.AutoGenerateColumns = false;
			repoDataGridView.AllowUserToResizeColumns = false;
			repoDataGridView.AllowUserToResizeRows = false;
			repoDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			repoDataGridView.Dock = DockStyle.Fill;
			repoDataGridView.RowHeadersVisible = false;
		}
		private void PopulateDataGridView()
		{
			repoDataGridView.DataSource = repoDataSet;
			repoDataGridView.DataMember = "Repo";
		}
		public Form1()
		{
			Load += new EventHandler(Form1_Load);
			FormClosing += new FormClosingEventHandler(Form1_Unload);
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			SetupLayout();
			SetupDataGridView();
			PopulateDataGridView();
			repoDataGridView.Select(); //focus repoDataGridView
		}
		private void Form1_Unload(object sender, FormClosingEventArgs e)
		{
			repoDataSet.WriteXml(repoXML);
		}
		private void textBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == Convert.ToChar(1)) //unicode char (decimal) for ctrl+a
			{
				((TextBox)sender).SelectAll();
				e.Handled = true;
			}
		}
		private void NameBox_GotFocus(object sender, EventArgs e)
		{
			if (nameBox.Text == "Repo Name")
			{
				nameBox.Text = "";
			}
		}
		private void NameBox_LostFocus(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(nameBox.Text))
			{
				nameBox.Text = "Repo Name";
			}
		}
		private void urlBox_GotFocus(object sender, EventArgs e)
		{
			if (urlBox.Text == "Repo URL" || urlBox.Text == "Invalid URL")
			{
				urlBox.Text = "";
			}
		}
		private void urlBox_LostFocus(object sender, EventArgs e)
		{

			if (string.IsNullOrWhiteSpace(urlBox.Text) || !ValidateURL(urlBox.Text))
			{
				urlBox.Text = "Invalid URL";
			}
			
		}
		private bool ValidateURL(string url)
		{
			bool result;

			try
			{
				Uri validateUri = new Uri(url);
				result = true;
			}
			catch (UriFormatException)
			{
				result = false;
			}

			return result;
		}
		private void AddNewRow(DataSet dataSet)
		{
			DataTable table;
			table = dataSet.Tables["Repo"];
			DataRow newRow = table.NewRow();

			newRow["Name"] = nameBox.Text;
			newRow["Type"] = typeBox.Text;
			newRow["URL"] = urlBox.Text;

			if (ValidateURL(urlBox.Text))
			{
				table.Rows.Add(newRow);

			}
		}
		private void AddNewRowButton_Click(object sender, EventArgs e)
		{
			AddNewRow(repoDataSet);
		}
		private void DeleteRowButton_Click(object sender, EventArgs e)
		{
			if (repoDataGridView.SelectedRows.Count > 0)
			{
				repoDataGridView.Rows.RemoveAt(repoDataGridView.SelectedRows[0].Index);
			}
		}
		private void ddlButton_Click(object sender, EventArgs e)
		{
			Button button = sender as Button;
			string path = $@"{ddlDict[button.Name][0]}";
			string zipUrl = ddlDict[button.Name][1];
			repoBuddy.DirectDownload(path, zipUrl);
			button.Text = $"[INSTALLED] {button.Text}";
			button.Enabled = false;
		}
		private void restartRBButton_Click(object sender, EventArgs e)
		{
			this.Close();
			Task.Delay(100).ContinueWith(t=> repoBuddy.RestartRebornBuddy());
		}

	}
	#endregion
}
