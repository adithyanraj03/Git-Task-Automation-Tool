# Git Task Automation Tool

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows-green.svg)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-purple.svg)

A professional WPF application for automating Git operations, designed to streamline batch processing of files and folders for Git repositories. Thereby streamling Git operations when dealing with large file sets.This tool provides an elegant solution to common Git batch processing challenges. 
💻 **Download Now**: [GitAutomationTool v1.0.0](https://github.com/adithyanraj/GitAutomationTool/releases/tag/v1.0.0)

<br>

![Screenshot 2025-03-21 041617](https://github.com/user-attachments/assets/bc0001f5-08ac-405b-9326-d6cfde2e7e40)

## 🚀 Key Features

- **Batch Processing** - Organize files into configurable batch sizes for smoother Git operations
- **Intelligent File Management** - Automatically handle file conflicts and naming
- **Visual Progress Tracking** - Real-time progress indicators and animations
- **Detailed Git Operation Logs** - Comprehensive logging of all Git operations
- **Dual Operation Modes**:
  - **Batch Mode** - Process files in controlled batches with separate commits
  - **Full Folder Mode** - Process all selected items in a single operation
- **Custom Commit Messages** - Templates for automatic batch numbering and folder naming
- **File/Folder Filtering** - Easily select specific file types or folders

## 🔧 Why You Need This Tool

### Solving Common Git Challenges

- **HTTP 413 Request Entity Too Large** - Avoid hitting size limits by automatically splitting large file sets into manageable batches
- **HTTP 408 Request Timeout** - Prevent timeouts when pushing many files by controlling batch sizes
- **Git Performance Issues** - Improve Git's performance when dealing with many files by using strategic batching
- **Merge Conflicts** - Reduce merge conflicts by organizing related files into logical batches
- **Repository Organization** - Keep your commit history clean and organized with templated commit messages

### For Teams & Projects

- **Consistent Commit Patterns** - Enforce consistent commit message formatting across team members
- **Code Release Management** - Easily organize files for releases or feature deployments
- **Large Asset Management** - Handle large numbers of assets (images, documents, etc.) efficiently
- **Automated Workflows** - Integrate into CI/CD pipelines with predictable commit patterns

## 📋 Requirements

- Windows 7 or later
- .NET 6.0 Runtime
- Git command-line tools installed and in PATH

## 📥 Installation

### Standard Installation: Download Release

1. Download the latest [release](https://github.com/adithyanraj03/Git-Task-Automation-Tool/releases)
2. Extract to your preferred location
3. Run `GitAutomationTool.exe`

![Screenshot 2025-03-21 041443](https://github.com/user-attachments/assets/ed5361d0-3d04-496a-94c9-57d26d54a0fb)


### For Developers : Build from Source

```bash
# Clone the repository
git clone https://github.com/adithyanraj03/Git-Task-Automation-Tool.git

# Navigate to project directory
cd Git-Task-Automation-Tool

# Build the application
dotnet build

# Run the application
dotnet run
```

## 📖 Usage Guide

### Quick Start

1. **Select Source Folder** - Choose the folder containing files you want to add to Git
2. **Select Git Repository** - Choose your target Git repository (or initialize a new one)

3. **Choose Mode** - Select either Batch Mode or Full Folder Mode
4. **Configure Settings**:
   - For Batch Mode: Set batch size and commit message template
   - For Full Folder Mode: Set the commit message template
5. **Select Files** - Choose which files to include in the operation
6. **Start Git Operations** - Click the button to begin the process

![Screenshot 2025-03-21 041839](https://github.com/user-attachments/assets/c92f0121-6fc6-4a1d-a20e-3009a58ded7e)
![Screenshot 2025-03-21 041849](https://github.com/user-attachments/assets/49f95f10-a6a6-4528-928c-a4f96ea6cd09)


### Advanced Usage

#### Customizing Commit Messages

Use these placeholders in your commit message templates:
- `{batch_num}` - Current batch number (Batch Mode)
- `{folder_name}` - Source folder name (Full Folder Mode)

Examples:
- `Added files from UI update - batch {batch_num}`
- `Initial import of {folder_name} assets`

#### Handling Large Repositories

For repositories with thousands of files:

1. Use smaller batch sizes (10-20 files per batch)
2. Enable selective file processing with the filter options
3. Use the log output to monitor for any Git errors

#### Integration with Workflows

The tool can be integrated into automation scripts:

```powershell
# Example PowerShell script to automate the Git Tool
Start-Process -FilePath "GitAutomationTool.exe" -ArgumentList "--source C:\project\assets --repo C:\project\repo --batch 15 --commit 'Asset batch {batch_num}' --autostart"
```

## 🛠️ Technical Details

### Architecture

The application is built using:
- C# with WPF for the UI framework
- MVVM pattern for clean separation of concerns
- Async/await for non-blocking operations
- Windows API Code Pack for modern folder dialogs

### Performance Considerations

- Memory-efficient file handling for large directories
- Asynchronous Git operations to keep UI responsive
- Batched file copying to prevent memory spikes

### Customization

Advanced users can modify:
- Default batch sizes in the app.config
- UI themes by editing XAML resources
- Git command templates in the GitOperations.cs file

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Nordic color scheme for the UI design
- Git community for command-line tools
- .NET WPF framework for robust UI capabilities


© 2025 Adithyanraj✨ Made for Git Community
