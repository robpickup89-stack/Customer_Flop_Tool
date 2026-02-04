using System.IO.Compression;
using System.Reflection;

namespace MultiSiteTempRunner;

/// <summary>
/// Provides file and folder operations for the application.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// List of required core files for creating encrypted zips.
    /// </summary>
    public static readonly string[] RequiredCoreFiles =
    [
        "VMFUNC.C",
        "iout.cfg",
        "Simulator.ini",
        "Trace.ini",
        "statecolors.ini",
        "BICS.DAT",
        "ED16.DAT",
        "IOT.dat",
        "MMI.DAT",
        "SADAT.DAT",
        "XP.DAT",
        "kop.def",
        "port.info",
        "SRM.LOG",
        "XLOG.LOG",
        "XPARCHANGEO.LOG",
        "report01.html",
        "configNotes.txt",
        "default_Flop_Files.zip"
    ];

    /// <summary>
    /// List of siteview files (1-10 for both .ini and .png).
    /// </summary>
    public static readonly string[] SiteViewFiles = GenerateSiteViewFiles();

    private static string[] GenerateSiteViewFiles()
    {
        var files = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            files.Add($"siteview{i}.ini");
            files.Add($"siteview{i}.png");
        }
        return files.ToArray();
    }

    /// <summary>
    /// Gets all required files (core + siteview).
    /// </summary>
    public static IEnumerable<string> AllRequiredFiles => RequiredCoreFiles.Concat(SiteViewFiles);

    /// <summary>
    /// Gets the base temp folder path for the application.
    /// </summary>
    public static string BaseTempPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiSiteTempRunner");

    /// <summary>
    /// Gets the temp folder path for a specific site.
    /// </summary>
    /// <param name="siteName">The name of the site.</param>
    /// <returns>The full path to the site's temp folder.</returns>
    public static string GetSiteTempPath(string siteName) =>
        Path.Combine(BaseTempPath, siteName, "Temp");

    /// <summary>
    /// Gets the path to the default flop files zip.
    /// </summary>
    public static string DefaultFlopFilesPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default_Flop_Files.zip");

    /// <summary>
    /// Checks if the default flop files zip exists.
    /// </summary>
    public static bool DefaultFlopFilesExist => File.Exists(DefaultFlopFilesPath);

    /// <summary>
    /// Extracts the embedded default_Flop_Files.zip to the application directory.
    /// </summary>
    public static void ExtractEmbeddedDefaultFlopFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("default_Flop_Files.zip");

        if (stream == null)
            return;

        using var fileStream = File.Create(DefaultFlopFilesPath);
        stream.CopyTo(fileStream);
    }

    /// <summary>
    /// Creates or rebuilds a site's temp folder.
    /// </summary>
    /// <param name="siteName">The site name.</param>
    /// <param name="log">Action to log messages.</param>
    /// <returns>The path to the created temp folder.</returns>
    public static string BuildSiteTemp(string siteName, Action<string> log)
    {
        string tempPath = GetSiteTempPath(siteName);

        // Delete existing temp folder if it exists
        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
            log($"Deleted existing temp folder: {tempPath}");
        }

        // Create fresh temp folder
        Directory.CreateDirectory(tempPath);
        log($"Created temp folder: {tempPath}");

        // Extract default_Flop_Files.zip if it exists
        if (DefaultFlopFilesExist)
        {
            try
            {
                ZipFile.ExtractToDirectory(DefaultFlopFilesPath, tempPath, true);
                log("Extracted default_Flop_Files.zip into temp folder");
            }
            catch (Exception ex)
            {
                log($"Warning: Failed to extract default_Flop_Files.zip: {ex.Message}");
            }
        }
        else
        {
            log("Warning: default_Flop_Files.zip not found");
        }

        return tempPath;
    }

    /// <summary>
    /// Clears a site's temp folder.
    /// </summary>
    /// <param name="siteName">The site name.</param>
    /// <param name="log">Action to log messages.</param>
    public static void ClearSiteTemp(string siteName, Action<string> log)
    {
        string tempPath = GetSiteTempPath(siteName);
        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
            log($"Cleared temp folder: {tempPath}");
        }
        else
        {
            log("Temp folder does not exist");
        }
    }

    /// <summary>
    /// Clears all site temp folders.
    /// </summary>
    /// <param name="log">Action to log messages.</param>
    public static void ClearAllTemps(Action<string> log)
    {
        if (Directory.Exists(BaseTempPath))
        {
            Directory.Delete(BaseTempPath, true);
            log("Cleared all temp folders");
        }
        else
        {
            log("No temp folders to clear");
        }
    }

    /// <summary>
    /// Extracts a zip file to a destination folder.
    /// </summary>
    /// <param name="zipData">The zip file data.</param>
    /// <param name="destinationPath">The destination folder path.</param>
    /// <param name="log">Action to log messages.</param>
    public static void ExtractZipToFolder(byte[] zipData, string destinationPath, Action<string> log)
    {
        using var stream = new MemoryStream(zipData);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            // Skip directories (entries ending with /)
            if (string.IsNullOrEmpty(entry.Name))
                continue;

            string destinationFile = Path.Combine(destinationPath, entry.FullName);
            string? destinationDir = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrEmpty(destinationDir))
                Directory.CreateDirectory(destinationDir);

            entry.ExtractToFile(destinationFile, true);
            log($"Extracted: {entry.FullName}");
        }
    }

    /// <summary>
    /// Extracts a zip file from disk to a destination folder.
    /// </summary>
    /// <param name="zipPath">The path to the zip file.</param>
    /// <param name="destinationPath">The destination folder path.</param>
    /// <param name="log">Action to log messages.</param>
    public static void ExtractZipFileToFolder(string zipPath, string destinationPath, Action<string> log)
    {
        ZipFile.ExtractToDirectory(zipPath, destinationPath, true);
        log($"Extracted zip: {Path.GetFileName(zipPath)}");
    }

    /// <summary>
    /// Copies matching required files from a source folder to a destination folder.
    /// </summary>
    /// <param name="sourceFolder">The source folder path.</param>
    /// <param name="destinationPath">The destination folder path.</param>
    /// <param name="log">Action to log messages.</param>
    public static void CopyMatchingFiles(string sourceFolder, string destinationPath, Action<string> log)
    {
        var sourceFiles = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
        int copiedCount = 0;

        foreach (var sourceFile in sourceFiles)
        {
            string fileName = Path.GetFileName(sourceFile);

            // Check if file matches any required file (case-insensitive)
            bool isRequired = AllRequiredFiles.Any(r =>
                string.Equals(r, fileName, StringComparison.OrdinalIgnoreCase));

            if (isRequired)
            {
                string destFile = Path.Combine(destinationPath, fileName);
                File.Copy(sourceFile, destFile, true);
                log($"Copied: {fileName}");
                copiedCount++;
            }
        }

        log($"Copied {copiedCount} matching file(s) from folder");
    }

    /// <summary>
    /// Copies a single file to the destination folder.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="destinationPath">The destination folder path.</param>
    /// <param name="log">Action to log messages.</param>
    public static void CopyFileToTemp(string filePath, string destinationPath, Action<string> log)
    {
        string fileName = Path.GetFileName(filePath);
        string destFile = Path.Combine(destinationPath, fileName);
        File.Copy(filePath, destFile, true);
        log($"Copied: {fileName}");
    }

    /// <summary>
    /// Creates an encrypted zip from a source folder containing required files.
    /// </summary>
    /// <param name="sourceFolder">The source folder path.</param>
    /// <param name="outputPath">The output encrypted zip path.</param>
    /// <param name="password">The encryption password.</param>
    /// <param name="log">Action to log messages.</param>
    /// <returns>List of missing files.</returns>
    public static List<string> CreateEncryptedZip(string sourceFolder, string outputPath, string password, Action<string> log)
    {
        var missingFiles = new List<string>();

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var requiredFile in AllRequiredFiles)
            {
                // Search for file case-insensitively
                var matchingFile = Directory.GetFiles(sourceFolder)
                    .FirstOrDefault(f => string.Equals(Path.GetFileName(f), requiredFile, StringComparison.OrdinalIgnoreCase));

                if (matchingFile != null)
                {
                    var entry = archive.CreateEntry(requiredFile);
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(matchingFile);
                    fileStream.CopyTo(entryStream);
                    log($"Added to zip: {requiredFile}");
                }
                else
                {
                    missingFiles.Add(requiredFile);
                }
            }
        }

        // Encrypt and save
        byte[] zipData = memoryStream.ToArray();
        EncryptionHelper.EncryptZipToFile(zipData, outputPath, password);
        log($"Created encrypted zip: {Path.GetFileName(outputPath)}");

        if (missingFiles.Count > 0)
        {
            log($"Warning: {missingFiles.Count} file(s) were missing");
        }

        return missingFiles;
    }

    /// <summary>
    /// Finds all executable files in a folder.
    /// </summary>
    /// <param name="folderPath">The folder to search.</param>
    /// <param name="includeSubfolders">Whether to search subfolders.</param>
    /// <returns>List of executable file paths.</returns>
    public static List<string> FindExecutables(string folderPath, bool includeSubfolders)
    {
        if (!Directory.Exists(folderPath))
            return new List<string>();

        var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.GetFiles(folderPath, "*.exe", searchOption).ToList();
    }
}
