//!CompilerOption:AddRef:SharpSvn.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace repoBuddy;

public class SettingsForm : Form
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
    private TabPage ddlTab = new TabPage();
    private TabControl tabControls = new TabControl();
    private DataSet repoDataSet = repoBuddy.repoDataSet;
    private string repoXML = repoBuddy.repoXML;
    private Dictionary<string, List<string>> ddlDict = repoBuddy.ddlDict;

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
            button.Text = ddlname + " - " + ddldesc;
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

            if (Directory.Exists(resolvedMask))
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

    public SettingsForm()
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
        Task.Delay(100).ContinueWith(t => repoBuddy.RestartRebornBuddy());
    }
}
