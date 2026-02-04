namespace MultiSiteTempRunner;

/// <summary>
/// Main application form with TabControl for 6 sites.
/// </summary>
public class MainForm : Form
{
    private const int SiteCount = 6;
    private const string DefaultPassword = "peek";

    private TextBox _passwordTextBox = null!;
    private CheckBox _showPasswordCheckBox = null!;
    private Button _runAllButton = null!;
    private Button _clearAllButton = null!;
    private TabControl _tabControl = null!;
    private readonly List<SitePanel> _sitePanels = new();

    public MainForm()
    {
        InitializeComponent();
        SetIcon();
        EnsureDefaultFlopFilesExtracted();
        BuildAllTemps();
    }

    private void InitializeComponent()
    {
        // Form settings
        Text = "Multi-Site Encrypted Zip Temp Runner";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 600);

        // Main layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),    // Control bar
                new RowStyle(SizeType.Percent, 100) // Tab control
            },
            Padding = new Padding(10)
        };

        // Top control bar
        var controlBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 10)
        };

        var passwordLabel = new Label
        {
            Text = "🔐 Password:",
            AutoSize = true,
            Margin = new Padding(0, 7, 5, 0)
        };

        _passwordTextBox = new TextBox
        {
            Width = 200,
            UseSystemPasswordChar = true,
            Text = DefaultPassword,
            Margin = new Padding(0, 3, 5, 0)
        };

        _showPasswordCheckBox = new CheckBox
        {
            Text = "Show",
            AutoSize = true,
            Margin = new Padding(0, 5, 15, 0)
        };
        _showPasswordCheckBox.CheckedChanged += ShowPassword_CheckedChanged;

        _runAllButton = new Button
        {
            Text = "▶ Run ALL Sites",
            Width = 120,
            Height = 28
        };
        _runAllButton.Click += RunAllButton_Click;

        _clearAllButton = new Button
        {
            Text = "🧹 Clear ALL Temps",
            Width = 130,
            Height = 28
        };
        _clearAllButton.Click += ClearAllButton_Click;

        controlBar.Controls.Add(passwordLabel);
        controlBar.Controls.Add(_passwordTextBox);
        controlBar.Controls.Add(_showPasswordCheckBox);
        controlBar.Controls.Add(_runAllButton);
        controlBar.Controls.Add(_clearAllButton);

        // Tab control for sites
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Create 6 site tabs
        for (int i = 1; i <= SiteCount; i++)
        {
            string siteName = $"Site{i}";
            var tabPage = new TabPage($"Site {i}")
            {
                Name = siteName
            };

            var sitePanel = new SitePanel(siteName, GetPassword);
            sitePanel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(sitePanel);

            _tabControl.TabPages.Add(tabPage);
            _sitePanels.Add(sitePanel);
        }

        // Create Tools tab for Create Encrypted Zip
        var toolsTabPage = new TabPage("Tools")
        {
            Name = "Tools"
        };
        var toolsPanel = new ToolsPanel(GetPassword);
        toolsPanel.Dock = DockStyle.Fill;
        toolsTabPage.Controls.Add(toolsPanel);
        _tabControl.TabPages.Add(toolsTabPage);

        // Add controls to main layout
        mainLayout.Controls.Add(controlBar, 0, 0);
        mainLayout.Controls.Add(_tabControl, 0, 1);

        Controls.Add(mainLayout);
    }

    private void SetIcon()
    {
        try
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "swarco.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }
        }
        catch
        {
            // Ignore icon loading errors - not critical
        }
    }

    private void EnsureDefaultFlopFilesExtracted()
    {
        if (!FileHelper.DefaultFlopFilesExist)
        {
            FileHelper.ExtractEmbeddedDefaultFlopFiles();
        }
    }

    private void BuildAllTemps()
    {
        foreach (var sitePanel in _sitePanels)
        {
            sitePanel.BuildTemp();
        }
    }

    private void ShowPassword_CheckedChanged(object? sender, EventArgs e)
    {
        _passwordTextBox.UseSystemPasswordChar = !_showPasswordCheckBox.Checked;
    }

    private string GetPassword()
    {
        return _passwordTextBox.Text;
    }

    private void RunAllButton_Click(object? sender, EventArgs e)
    {
        foreach (var sitePanel in _sitePanels)
        {
            sitePanel.RunExecutables();
        }
    }

    private void ClearAllButton_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to clear all site temp folders?",
            "Confirm Clear All",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            foreach (var sitePanel in _sitePanels)
            {
                sitePanel.ClearTemp();
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        // Clean up all temp folders on exit
        FileHelper.ClearAllTemps(_ => { });
    }
}
