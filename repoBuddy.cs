//!CompilerOption:AddRef:Plugins\repoBuddy\SharpSvn.dll
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ff14bot.AClasses;
using ff14bot.Helpers;
using SharpSvn;

namespace repoBuddy
{

	public class repoBuddy : BotPlugin
	{
		public override string Name => "repoBuddy";
		public override string Author => "Zimble";
		public override Version Version => new Version(0, 0, 0, 1);
		public override string Description => "Automatically update rb accessories from repositories";
		public override bool WantButton => true;
		public override string ButtonText => "Settings";
		public static DataSet repoDataSet = new DataSet();
		public static string repoXML = @"Plugins\repoBuddy\repoBuddyRepos.xml";
		
        private static Color LogColor = Colors.Wheat;
		public override void OnButtonPress()
		{
			CreateSettingsForm();
		}
		public override void OnEnabled()
		{
			Logging.Write(LogColor, $"[{Name}] checking for updates");
			GetrepoData();
			repoStart();
		}
		public void CreateSettingsForm()
		{
			Form1 settingsForm = new Form1();
			settingsForm.ShowDialog();
		}
		public void GetrepoData()
		{
			repoDataSet.Clear();
			repoDataSet.ReadXml(repoXML);
		}
		static repoBuddy()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) //force load sharpsvn
		{
			AssemblyName asmName = new AssemblyName(args.Name);
			if (asmName.Name != "SharpSvn")
				return null;
			return Assembly.LoadFrom(@"Plugins\repoBuddy\SharpSvn.dll");
		}
		#region repo logic
		public void repoStart()
		{
			SvnRevertArgs revertArgs = new SvnRevertArgs()
			{
				Depth = SvnDepth.Infinity
			};

			Stopwatch stopwatch = Stopwatch.StartNew();

			//List<DataRow> test = repoDataSet.Tables["Repo"].Rows.Cast<DataRow>();
			//foreach (DataRow row in repoDataSet.Tables["Repo"].Rows)
			Parallel.ForEach(repoDataSet.Tables["Repo"].Rows.Cast<DataRow>(), row =>
			{
				string repoName = row[0].ToString();
				string repoType = row[1].ToString() + "s";
				string repoUrl = row[2].ToString();
				string repoPath = $@"{repoType}\{repoName}";

				long currentLap;
				long totalLap;
				currentLap = stopwatch.ElapsedMilliseconds;
				
				using (SvnClient client = new SvnClient())
				{
					
					if (System.IO.Directory.Exists($@"{repoPath}\.svn"))
					{
						SvnInfoEventArgs remoteRev;
						client.GetInfo(repoUrl, out remoteRev);

						SvnInfoEventArgs localRev;
						client.GetInfo(repoPath, out localRev);
						
						if (localRev.Revision < remoteRev.Revision) 
						{
							client.Revert(repoPath, revertArgs);
							client.Update(repoPath);
							totalLap = stopwatch.ElapsedMilliseconds - currentLap;
							Logging.Write(LogColor, $"[{Name}] updated [{repoType}] {repoName} from {localRev.Revision} to {remoteRev.Revision} in {totalLap} ms.");
						}

					}
					else
					{
						client.CheckOut(new Uri(repoUrl), repoPath);
						totalLap = stopwatch.ElapsedMilliseconds - currentLap;
						Logging.Write(LogColor, $"[{Name}] {repoName} checkout complete in {totalLap} ms.");
					}
				}

			});
			stopwatch.Stop();
			Logging.Write(LogColor, $"[{Name}] processes complete in {stopwatch.ElapsedMilliseconds} ms.");
		}
		#endregion
	}
	#region GUI stuff
	public class Form1 : Form
	{

		private FlowLayoutPanel controlPanel = new FlowLayoutPanel();
		private DataGridView repoDataGridView = new DataGridView();
		private TextBox nameBox = new TextBox();
		private ComboBox typeBox = new ComboBox();
		private TextBox urlBox = new TextBox();
		private Button addNewRowButton = new Button();
		private Button deleteRowButton = new Button();
		private DataSet repoDataSet = repoBuddy.repoDataSet;
		private string repoXML = repoBuddy.repoXML;
		private bool styleState = Application.RenderWithVisualStyles;
		private void SetupLayout()
		{
			Text = "repoBuddy Settings";
			Size = new System.Drawing.Size(517, 400);
			MaximizeBox = false;
			MinimizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			
			nameBox.Text = "Repo Name";
			nameBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);
			nameBox.GotFocus += new EventHandler(NameBox_GotFocus);
			nameBox.LostFocus += new EventHandler(NameBox_LostFocus);

			typeBox.Items.AddRange(new string[] { "BotBase", "Plugin", "Profile", "Routine" });
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

			controlPanel.Controls.Add(nameBox);
			controlPanel.Controls.Add(typeBox);
			controlPanel.Controls.Add(urlBox);
			controlPanel.Controls.Add(addNewRowButton);
			controlPanel.Controls.Add(deleteRowButton);
			controlPanel.AutoSize = true;
			controlPanel.Dock = DockStyle.Bottom;


			Controls.Add(repoDataGridView); //controls have their Dock setting evaluated from the bottom up, so this is first to prevent overlap with control panel
			Controls.Add(controlPanel);

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
			repoDataGridView.DataMember = "repo";
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
	}
	#endregion
}