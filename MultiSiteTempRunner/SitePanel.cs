using System.Diagnostics;
using System.Security.Cryptography;

namespace MultiSiteTempRunner;

/// <summary>
/// User control representing a single site's interface.
/// </summary>
public class SitePanel : UserControl
{
    private readonly string _siteName;
    private readonly Func<string> _getPassword;

    private TextBox _tempPathTextBox = null!;
    private Button _buildTempButton = null!;
    private Button _loadZipButton = null!;
    private Button _createZipButton = null!;
    private Button _runExeButton = null!;
    private Button _clearTempButton = null!;
    private CheckBox _includeSubfoldersCheckBox = null!;
    private Panel _dropZone = null!;
    private Label _dropZoneLabel = null!;
    private ListBox _logListBox = null!;

    public SitePanel(string siteName, Func<string> getPassword)
    {
        _siteName = siteName;
        _getPassword = getPassword;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Panel settings
        Dock = DockStyle.Fill;
        Padding = new Padding(10);
        AllowDrop = true;
        DragEnter += SitePanel_DragEnter;
        DragDrop += SitePanel_DragDrop;

        // Main layout using TableLayoutPanel
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),     // Temp folder path
                new RowStyle(SizeType.AutoSize),     // Buttons row
                new RowStyle(SizeType.Percent, 30),  // Drop zone
                new RowStyle(SizeType.Percent, 70)   // Log area
            }
        };

        // Row 1: Temp folder path
        var tempPathPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 5)
        };

        var tempPathLabel = new Label
        {
            Text = "Temp Folder:",
            AutoSize = true,
            Margin = new Padding(0, 5, 5, 0)
        };

        _tempPathTextBox = new TextBox
        {
            Width = 600,
            ReadOnly = true,
            Text = FileHelper.GetSiteTempPath(_siteName)
        };

        tempPathPanel.Controls.Add(tempPathLabel);
        tempPathPanel.Controls.Add(_tempPathTextBox);

        // Row 2: Buttons
        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 10)
        };

        _buildTempButton = new Button { Text = "Build Temp", Width = 100 };
        _buildTempButton.Click += BuildTemp_Click;

        _loadZipButton = new Button { Text = "Load Encrypted Zip", Width = 130 };
        _loadZipButton.Click += LoadZip_Click;

        _createZipButton = new Button { Text = "Create Encrypted Zip", Width = 140 };
        _createZipButton.Click += CreateZip_Click;

        _runExeButton = new Button { Text = "Run (.exe)", Width = 90 };
        _runExeButton.Click += RunExe_Click;

        _clearTempButton = new Button { Text = "Clear Temp", Width = 90 };
        _clearTempButton.Click += ClearTemp_Click;

        _includeSubfoldersCheckBox = new CheckBox
        {
            Text = "Include Subfolders",
            AutoSize = true,
            Margin = new Padding(10, 5, 0, 0)
        };

        buttonsPanel.Controls.Add(_buildTempButton);
        buttonsPanel.Controls.Add(_loadZipButton);
        buttonsPanel.Controls.Add(_createZipButton);
        buttonsPanel.Controls.Add(_runExeButton);
        buttonsPanel.Controls.Add(_clearTempButton);
        buttonsPanel.Controls.Add(_includeSubfoldersCheckBox);

        // Row 3: Drop zone
        _dropZone = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(240, 248, 255),
            AllowDrop = true,
            Margin = new Padding(0, 5, 0, 5)
        };
        _dropZone.DragEnter += SitePanel_DragEnter;
        _dropZone.DragDrop += SitePanel_DragDrop;

        _dropZoneLabel = new Label
        {
            Text = "Drag & Drop Files, Folders, or Zip Archives Here",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 130, 180),
            AllowDrop = true
        };
        _dropZoneLabel.DragEnter += SitePanel_DragEnter;
        _dropZoneLabel.DragDrop += SitePanel_DragDrop;
        _dropZone.Controls.Add(_dropZoneLabel);

        // Row 4: Log area
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
        mainLayout.Controls.Add(tempPathPanel, 0, 0);
        mainLayout.Controls.Add(buttonsPanel, 0, 1);
        mainLayout.Controls.Add(_dropZone, 0, 2);
        mainLayout.Controls.Add(logPanel, 0, 3);

        Controls.Add(mainLayout);

        Log($"Site '{_siteName}' initialized");
    }

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logListBox.Items.Add($"[{timestamp}] {message}");
        _logListBox.TopIndex = _logListBox.Items.Count - 1;
    }

    private void EnsureTempExists()
    {
        string tempPath = FileHelper.GetSiteTempPath(_siteName);
        if (!Directory.Exists(tempPath))
        {
            FileHelper.BuildSiteTemp(_siteName, Log);
        }
    }

    #region Button Event Handlers

    private void BuildTemp_Click(object? sender, EventArgs e)
    {
        try
        {
            Log("Building temp folder...");
            FileHelper.BuildSiteTemp(_siteName, Log);
            Log("Temp folder built successfully");
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
        }
    }

    private void LoadZip_Click(object? sender, EventArgs e)
    {
        string password = _getPassword();
        if (string.IsNullOrEmpty(password))
        {
            Log("ERROR: Password is required for decryption");
            MessageBox.Show("Please enter a password in the global password field.", "Password Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var openDialog = new OpenFileDialog
        {
            Title = "Select Encrypted Zip File",
            Filter = "Encrypted Zip Files (*.zip.enc;*.enczip)|*.zip.enc;*.enczip|All Files (*.*)|*.*",
            FilterIndex = 1
        };

        if (openDialog.ShowDialog() == DialogResult.OK)
        {
            LoadEncryptedZip(openDialog.FileName, password);
        }
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
                string outputFileName = $"{_siteName}_Config_{timestamp}.zip.enc";

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

    private void RunExe_Click(object? sender, EventArgs e)
    {
        RunExecutables();
    }

    private void ClearTemp_Click(object? sender, EventArgs e)
    {
        try
        {
            FileHelper.ClearSiteTemp(_siteName, Log);
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
        }
    }

    #endregion

    #region Drag & Drop

    private void SitePanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void SitePanel_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] droppedItems)
            return;

        EnsureTempExists();
        string tempPath = FileHelper.GetSiteTempPath(_siteName);

        foreach (var item in droppedItems)
        {
            try
            {
                ProcessDroppedItem(item, tempPath);
            }
            catch (Exception ex)
            {
                Log($"ERROR processing '{Path.GetFileName(item)}': {ex.Message}");
            }
        }
    }

    private void ProcessDroppedItem(string itemPath, string tempPath)
    {
        if (File.Exists(itemPath))
        {
            ProcessDroppedFile(itemPath, tempPath);
        }
        else if (Directory.Exists(itemPath))
        {
            Log($"Processing folder: {Path.GetFileName(itemPath)}");
            FileHelper.CopyMatchingFiles(itemPath, tempPath, Log);
        }
    }

    private void ProcessDroppedFile(string filePath, string tempPath)
    {
        string fileName = Path.GetFileName(filePath);
        string ext = Path.GetExtension(filePath).ToLowerInvariant();

        // Check for encrypted zip
        if (EncryptionHelper.IsEncryptedZip(filePath))
        {
            string password = _getPassword();
            if (string.IsNullOrEmpty(password))
            {
                Log($"ERROR: Password required to decrypt '{fileName}'");
                MessageBox.Show("Please enter a password in the global password field.", "Password Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadEncryptedZip(filePath, password);
        }
        // Check for plain zip
        else if (ext == ".zip")
        {
            Log($"Extracting zip: {fileName}");
            FileHelper.ExtractZipFileToFolder(filePath, tempPath, Log);
        }
        // Regular file - copy to temp
        else
        {
            FileHelper.CopyFileToTemp(filePath, tempPath, Log);
        }
    }

    private void LoadEncryptedZip(string filePath, string password)
    {
        try
        {
            Log($"Decrypting: {Path.GetFileName(filePath)}");

            EnsureTempExists();
            string tempPath = FileHelper.GetSiteTempPath(_siteName);

            byte[] zipData = EncryptionHelper.DecryptZipFromFile(filePath, password);
            FileHelper.ExtractZipToFolder(zipData, tempPath, Log);

            Log("Encrypted zip loaded successfully");
        }
        catch (CryptographicException)
        {
            Log("ERROR: Decryption failed - incorrect password or corrupted file");
            MessageBox.Show("Failed to decrypt the file.\nPlease check the password and try again.", "Decryption Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show($"Failed to load encrypted zip:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Runs all executables in the site's temp folder.
    /// </summary>
    public void RunExecutables()
    {
        string tempPath = FileHelper.GetSiteTempPath(_siteName);

        if (!Directory.Exists(tempPath))
        {
            Log("Temp folder does not exist - nothing to run");
            return;
        }

        var executables = FileHelper.FindExecutables(tempPath, _includeSubfoldersCheckBox.Checked);

        if (executables.Count == 0)
        {
            Log("No executables found in temp folder");
            return;
        }

        Log($"Found {executables.Count} executable(s)");

        foreach (var exePath in executables)
        {
            try
            {
                string workingDir = Path.GetDirectoryName(exePath) ?? tempPath;
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = workingDir,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Log($"Launched: {Path.GetFileName(exePath)}");
            }
            catch (Exception ex)
            {
                Log($"ERROR launching '{Path.GetFileName(exePath)}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears this site's temp folder.
    /// </summary>
    public void ClearTemp()
    {
        try
        {
            FileHelper.ClearSiteTemp(_siteName, Log);
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
        }
    }

    #endregion
}
