namespace MultiSiteTempRunner;

/// <summary>
/// User control for the Tools tab containing Create Encrypted Zip functionality.
/// </summary>
public class ToolsPanel : UserControl
{
    private readonly Func<string> _getPassword;

    private Button _createZipButton = null!;
    private ListBox _logListBox = null!;

    public ToolsPanel(Func<string> getPassword)
    {
        _getPassword = getPassword;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Panel settings
        Dock = DockStyle.Fill;
        Padding = new Padding(10);

        // Main layout using TableLayoutPanel
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),     // Title
                new RowStyle(SizeType.AutoSize),     // Buttons row
                new RowStyle(SizeType.Percent, 100)  // Log area
            }
        };

        // Row 1: Title
        var titleLabel = new Label
        {
            Text = "Create Encrypted Zip",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 10)
        };

        // Row 2: Buttons
        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 10)
        };

        _createZipButton = new Button { Text = "Create Encrypted Zip", Width = 160, Height = 30 };
        _createZipButton.Click += CreateZip_Click;

        buttonsPanel.Controls.Add(_createZipButton);

        // Row 3: Log area
        var logPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 5, 0, 0)
        };

        var logLabel = new Label
        {
            Text = "Activity Log:",
            Dock = DockStyle.Top,
            AutoSize = true
        };

        _logListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9),
            IntegralHeight = false
        };

        logPanel.Controls.Add(_logListBox);
        logPanel.Controls.Add(logLabel);

        // Add all rows to main layout
        mainLayout.Controls.Add(titleLabel, 0, 0);
        mainLayout.Controls.Add(buttonsPanel, 0, 1);
        mainLayout.Controls.Add(logPanel, 0, 2);

        Controls.Add(mainLayout);

        Log("Tools panel initialized");
    }

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logListBox.Items.Add($"[{timestamp}] {message}");
        _logListBox.TopIndex = _logListBox.Items.Count - 1;
    }

    private void CreateZip_Click(object? sender, EventArgs e)
    {
        string password = _getPassword();
        if (string.IsNullOrEmpty(password))
        {
            Log("ERROR: Password is required for encryption");
            MessageBox.Show("Please enter a password in the global password field.", "Password Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select folder containing configuration files",
            UseDescriptionForTitle = true
        };

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                Log($"Creating encrypted zip from: {folderDialog.SelectedPath}");

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string outputFileName = $"Config_{timestamp}.zip.enc";

                using var saveDialog = new SaveFileDialog
                {
                    Title = "Save Encrypted Zip",
                    Filter = "Encrypted Zip (*.zip.enc)|*.zip.enc",
                    FileName = outputFileName
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var missingFiles = FileHelper.CreateEncryptedZip(
                        folderDialog.SelectedPath,
                        saveDialog.FileName,
                        password,
                        Log);

                    if (missingFiles.Count > 0)
                    {
                        Log($"Missing files: {string.Join(", ", missingFiles.Take(5))}{(missingFiles.Count > 5 ? "..." : "")}");
                    }

                    Log("Encrypted zip created successfully");
                    MessageBox.Show($"Encrypted zip saved to:\n{saveDialog.FileName}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"Failed to create encrypted zip:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
