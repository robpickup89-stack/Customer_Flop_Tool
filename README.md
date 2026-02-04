# Multi-Site Encrypted Zip Temp Runner

A Windows Forms application (.NET 8) that allows engineers to manage encrypted ZIP packages across multiple site configurations.

## Features

- **Multi-Site Support**: Manage up to 6 independent site configurations simultaneously
- **Encrypted ZIP Handling**: Create and load AES-256 encrypted ZIP archives
- **Temp Folder Management**: Build, populate, and clear temporary working folders per site
- **Drag & Drop Support**: Drop files, folders, or ZIP archives directly onto site panels
- **Executable Runner**: Run all `.exe` files in a site's temp folder with one click
- **Activity Logging**: Per-site timestamped activity logs

## Requirements

- Windows 10/11 (x64)
- .NET 8.0 Runtime

## Building

```bash
cd MultiSiteTempRunner
dotnet build
```

## Running

```bash
dotnet run --project MultiSiteTempRunner
```

Or after building, run the executable directly from the `bin` folder.

## Usage

1. Enter your encryption password in the global password field (default: `peek`)
2. Select a site tab (Site 1-6)
3. Use **Build Temp** to create/rebuild the site's temp folder
4. Drag & drop files/folders or use **Load Encrypted Zip** to populate the temp folder
5. Use **Run (.exe)** to execute all executables in the temp folder
6. Use **Create Encrypted Zip** to package configuration files into an encrypted archive

## File Structure

```
MultiSiteTempRunner/
├── MultiSiteTempRunner.csproj  # Project file
├── Program.cs                   # Entry point
├── MainForm.cs                  # Main application form
├── SitePanel.cs                 # Per-site UI control
├── EncryptionHelper.cs          # AES-256 encryption utilities
├── FileHelper.cs                # File operation utilities
├── swarco.ico                   # Application icon
└── default_Flop_Files.zip       # Default files extracted to new temp folders
```

## Temp Folder Location

Site temp folders are created at:
```
%LOCALAPPDATA%\MultiSiteTempRunner\{SiteName}\Temp
```

## Encrypted ZIP Format

- Encryption: AES-256 with PBKDF2 key derivation
- Extensions: `.zip.enc` or `.enczip`
- Output naming: `{SiteName}_Config_{yyyyMMdd_HHmm}.zip.enc`
