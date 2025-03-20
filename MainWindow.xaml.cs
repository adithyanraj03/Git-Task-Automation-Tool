using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GitAutomationTool
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Constants for UI colors
        private readonly SolidColorBrush SourceColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5E81AC"));
        private readonly SolidColorBrush GitColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B48EAD"));
        private readonly SolidColorBrush BgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E3440"));
        private readonly SolidColorBrush FgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECEFF4"));
        private readonly SolidColorBrush AccentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88C0D0"));
        private readonly SolidColorBrush HighlightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5E81AC"));
        private readonly SolidColorBrush ButtonBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C566A"));
        private readonly SolidColorBrush SuccessBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3BE8C"));
        private readonly SolidColorBrush ErrorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A"));

        // Properties for binding
        private string _sourceFolder;
        public string SourceFolder
        {
            get => _sourceFolder;
            set
            {
                _sourceFolder = value;
                OnPropertyChanged(nameof(SourceFolder));
            }
        }

        private string _gitRepoFolder;
        public string GitRepoFolder
        {
            get => _gitRepoFolder;
            set
            {
                _gitRepoFolder = value;
                OnPropertyChanged(nameof(GitRepoFolder));
            }
        }

        private string _operationMode = "batch";
        public string OperationMode
        {
            get => _operationMode;
            set
            {
                _operationMode = value;
                OnPropertyChanged(nameof(OperationMode));
                UpdateModeSettings();
            }
        }

        private int _batchSize = 2;
        public int BatchSize
        {
            get => _batchSize;
            set
            {
                _batchSize = value;
                OnPropertyChanged(nameof(BatchSize));
            }
        }

        private string _commitMessage = "Added batch {batch_num}";
        public string CommitMessage
        {
            get => _commitMessage;
            set
            {
                _commitMessage = value;
                OnPropertyChanged(nameof(CommitMessage));
            }
        }

        private string _fullCommitMessage = "Added all files from {folder_name}";
        public string FullCommitMessage
        {
            get => _fullCommitMessage;
            set
            {
                _fullCommitMessage = value;
                OnPropertyChanged(nameof(FullCommitMessage));
            }
        }

        private string _statusMessage = "Ready to start Git operations";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                OnPropertyChanged(nameof(TotalItems));
                OnPropertyChanged(nameof(ItemCounterText));
            }
        }

        private int _processedItems;
        public int ProcessedItems
        {
            get => _processedItems;
            set
            {
                _processedItems = value;
                OnPropertyChanged(nameof(ProcessedItems));
                OnPropertyChanged(nameof(ItemCounterText));
            }
        }

        private int _currentBatch;
        public int CurrentBatch
        {
            get => _currentBatch;
            set
            {
                _currentBatch = value;
                OnPropertyChanged(nameof(CurrentBatch));
                OnPropertyChanged(nameof(BatchLabelText));
            }
        }

        public string ItemCounterText => $"Items: {ProcessedItems}/{TotalItems}";
        public string BatchLabelText => $"Current Batch: {CurrentBatch}";

        // Collections
        public ObservableCollection<FileItem> FileItems { get; } = new ObservableCollection<FileItem>();
        
        // Animation objects
        private readonly DispatcherTimer _animationTimer = new DispatcherTimer();
        private readonly Random _random = new Random();
        private bool _animationRunning = false;
        private List<UIElement> _fileShapes = new List<UIElement>();
        private List<ParticleSystem> _particleSystems = new List<ParticleSystem>();
        private CancellationTokenSource _operationCancellationTokenSource;

        // Application author: Adithyanraj
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Hidden security check to verify author and logo integrity
            VerifyApplicationIntegrity();

            // Set up animations
            _animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            _animationTimer.Tick += AnimationTimer_Tick;

            // Set application icon if not set by XAML
            try
            {
                if (this.Icon == null)
                {
                    // Try multiple methods to load the icon
                    // Method 1: From embedded resource
                    Uri resourceUri = new Uri("pack://application:,,,/logo.png", UriKind.Absolute);
                    this.Icon = new System.Windows.Media.Imaging.BitmapImage(resourceUri);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue
                Debug.WriteLine($"Could not load icon: {ex.Message}");
            }

            // Set up folder visuals in canvas
            SetupFolderVisuals();
        }

        private void SetupFolderVisuals()
        {
            // Source folder
            var sourceRect = new Rectangle
            {
                Width = 100,
                Height = 70,
                Fill = SourceColor
            };
            Canvas.SetLeft(sourceRect, 100);
            Canvas.SetTop(sourceRect, 80);
            AnimationCanvas.Children.Add(sourceRect);

            // Source folder tab
            var sourceTab = new Rectangle
            {
                Width = 60,
                Height = 10,
                Fill = SourceColor
            };
            Canvas.SetLeft(sourceTab, 120);
            Canvas.SetTop(sourceTab, 70);
            AnimationCanvas.Children.Add(sourceTab);

            // Source label
            var sourceLabel = new TextBlock
            {
                Text = "Source",
                Foreground = Brushes.White,
                FontSize = 10
            };
            Canvas.SetLeft(sourceLabel, 130);
            Canvas.SetTop(sourceLabel, 110);
            AnimationCanvas.Children.Add(sourceLabel);

            // Git repo folder
            var gitRect = new Rectangle
            {
                Width = 100,
                Height = 70,
                Fill = GitColor
            };
            Canvas.SetLeft(gitRect, 600);
            Canvas.SetTop(gitRect, 80);
            AnimationCanvas.Children.Add(gitRect);

            // Git repo folder tab
            var gitTab = new Rectangle
            {
                Width = 60,
                Height = 10,
                Fill = GitColor
            };
            Canvas.SetLeft(gitTab, 620);
            Canvas.SetTop(gitTab, 70);
            AnimationCanvas.Children.Add(gitTab);

            // Git repo label
            var gitLabel = new TextBlock
            {
                Text = "Git Repo",
                Foreground = Brushes.White,
                FontSize = 10
            };
            Canvas.SetLeft(gitLabel, 630);
            Canvas.SetTop(gitLabel, 110);
            AnimationCanvas.Children.Add(gitLabel);

            // Arrow
            var arrow = new System.Windows.Shapes.Path
            {
                Stroke = AccentBrush,
                StrokeThickness = 3,
                Data = Geometry.Parse("M250,115 L550,115 M550,115 L530,105 M550,115 L530,125")
            };
            AnimationCanvas.Children.Add(arrow);

            // Create some file icons
            CreateFileIcons();

            // Git icon
            var gitIcon = new TextBlock
            {
                Text = "GIT",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E3440"))
            };
            Canvas.SetLeft(gitIcon, 640);
            Canvas.SetTop(gitIcon, 110);
            AnimationCanvas.Children.Add(gitIcon);
        }

        private void CreateFileIcons()
        {
            // Clear any existing file shapes
            foreach (var file in _fileShapes)
            {
                AnimationCanvas.Children.Remove(file);
            }
            _fileShapes.Clear();

            // Create new file icons
            for (int i = 0; i < 5; i++)
            {
                var x = _random.Next(120, 180);
                var y = _random.Next(85, 145);

                // File rectangle
                var fileRect = new Rectangle
                {
                    Width = 20,
                    Height = 25,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8DEE9"))
                };
                Canvas.SetLeft(fileRect, x);
                Canvas.SetTop(fileRect, y);
                AnimationCanvas.Children.Add(fileRect);
                _fileShapes.Add(fileRect);

                // File text
                var fileText = new TextBlock
                {
                    Text = "f",
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E3440"))
                };
                Canvas.SetLeft(fileText, x + 7);
                Canvas.SetTop(fileText, y + 8);
                AnimationCanvas.Children.Add(fileText);
                _fileShapes.Add(fileText);
            }
        }

        private void UpdateModeSettings()
        {
            if (OperationMode == "batch")
            {
                BatchSettingsPanel.Visibility = Visibility.Visible;
                FullSettingsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                BatchSettingsPanel.Visibility = Visibility.Collapsed;
                FullSettingsPanel.Visibility = Visibility.Visible;
            }
        }

        private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Source Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SourceFolder = dialog.FileName;
                AnimateFolderHighlight(SourceColor);
                LoadSelectionItems(SourceFolder);
                
                // Show selection panel
                SelectionPanel.Visibility = Visibility.Visible;
            }
        }

        private void BrowseGitRepo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Git Repository Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Check if it's a git repository
                if (!IsGitRepo(dialog.FileName))
                {
                    var result = MessageBox.Show(
                        $"{dialog.FileName} is not a Git repository. Would you like to initialize it as one?",
                        "Not a Git Repository",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        InitializeGitRepo(dialog.FileName);
                    }
                    else
                    {
                        return;
                    }
                }

                GitRepoFolder = dialog.FileName;
                AnimateFolderHighlight(GitColor);
            }
        }

        private bool IsGitRepo(string path)
        {
            return Directory.Exists(System.IO.Path.Combine(path, ".git"));
        }

        private bool InitializeGitRepo(string path)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "init",
                    WorkingDirectory = path,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();
                LogOperation($"Initialized Git repository: {output}");
                
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                LogOperation($"Error initializing Git repository: {ex.Message}");
                return false;
            }
        }

        private void AnimateFolderHighlight(SolidColorBrush targetBrush)
        {
            // For now, we'll just log that we would animate this in a real implementation
            LogOperation($"Folder highlighted animation would play here");
        }

        private void LoadSelectionItems(string folderPath)
        {
            FileItems.Clear();

            try
            {
                var allItems = Directory.GetFileSystemEntries(folderPath)
                    .OrderBy(p => p)
                    .ToList();

                foreach (var itemPath in allItems)
                {
                    var item = new FileItem
                    {
                        Name = System.IO.Path.GetFileName(itemPath),
                        Path = itemPath,
                        IsFolder = Directory.Exists(itemPath),
                        IsSelected = true
                    };
                    
                    FileItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load items: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FileItems)
            {
                item.IsSelected = true;
            }
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FileItems)
            {
                item.IsSelected = false;
            }
        }

        private void FilesOnly_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FileItems)
            {
                item.IsSelected = !item.IsFolder;
            }
        }

        private void FoldersOnly_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FileItems)
            {
                item.IsSelected = item.IsFolder;
            }
        }

        private async void StartOperations_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SourceFolder) || !Directory.Exists(SourceFolder))
            {
                StatusMessage = "Please select a valid source folder!";
                return;
            }

            if (string.IsNullOrEmpty(GitRepoFolder) || !Directory.Exists(GitRepoFolder))
            {
                StatusMessage = "Please select a valid Git repository folder!";
                return;
            }

            // Check if git repo is valid
            if (!IsGitRepo(GitRepoFolder))
            {
                var result = MessageBox.Show(
                    $"{GitRepoFolder} is not a Git repository. Would you like to initialize it as one?",
                    "Not a Git Repository",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (!InitializeGitRepo(GitRepoFolder))
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (OperationMode == "batch")
            {
                if (!int.TryParse(BatchSizeTextBox.Text, out int batchSize) || batchSize <= 0)
                {
                    StatusMessage = "Please enter a valid batch size (positive integer)!";
                    return;
                }
            }

            // Clear the log
            LogTextBox.Clear();

            // Disable start button during operations
            StartButton.IsEnabled = false;

            // Show loading animation
            LoadingAnimation.Visibility = Visibility.Visible;

            // Start file animation
            StartFileAnimation();

            // Create a cancellation token for this operation
            _operationCancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Start operations
                if (OperationMode == "batch")
                {
                    await ProcessBatchModeAsync(_operationCancellationTokenSource.Token);
                }
                else
                {
                    await ProcessFullFolderModeAsync(_operationCancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation was cancelled";
                LogOperation("Operation was cancelled by the user");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                LogOperation($"Error: {ex.Message}");
            }
            finally
            {
                // Re-enable start button
                StartButton.IsEnabled = true;

                // Hide loading animation
                LoadingAnimation.Visibility = Visibility.Collapsed;

                // Stop file animation
                StopFileAnimation();
            }
        }

        private async Task ProcessBatchModeAsync(CancellationToken cancellationToken)
        {
            int batchSize = int.Parse(BatchSizeTextBox.Text);
            string commitTemplate = CommitMessage;

            // Get all selected items
            var selectedItems = FileItems.Where(i => i.IsSelected).ToList();

            TotalItems = selectedItems.Count;
            if (TotalItems == 0)
            {
                MessageBox.Show("Please select items to process.", "No Items Selected");
                return;
            }

            ProcessedItems = 0;
            CurrentBatch = 0;

            // Process items in batches
            while (ProcessedItems < TotalItems && !cancellationToken.IsCancellationRequested)
            {
                CurrentBatch++;

                // Calculate batch size for this iteration
                int remainingItems = TotalItems - ProcessedItems;
                int currentBatchSize = Math.Min(batchSize, remainingItems);

                // Get the next batch of items
                var batchItems = selectedItems
                    .Skip(ProcessedItems)
                    .Take(currentBatchSize)
                    .ToList();

                // Log the batch
                LogOperation($"\n=== Processing Batch {CurrentBatch} ({currentBatchSize} items) ===\n");

                // Process each item in the batch
                for (int i = 0; i < batchItems.Count && !cancellationToken.IsCancellationRequested; i++)
                {
                    var item = batchItems[i];
                    string itemName = item.Name;
                    string itemPath = item.Path;
                    bool isFolder = item.IsFolder;
                    string destPath = System.IO.Path.Combine(GitRepoFolder, itemName);

                    // Update status
                    int itemNum = ProcessedItems + i + 1;
                    string itemType = isFolder ? "folder" : "file";
                    StatusMessage = $"Copying {itemType} {itemNum}/{TotalItems}: {itemName}";

                    // If destination already exists, handle it
                    if (File.Exists(destPath) || Directory.Exists(destPath))
                    {
                        string baseName = System.IO.Path.GetFileNameWithoutExtension(itemName);
                        string extension = System.IO.Path.GetExtension(itemName);
                        int counter = 1;
                        while (File.Exists(destPath) || Directory.Exists(destPath))
                        {
                            string newName = $"{baseName}_{counter}{extension}";
                            destPath = System.IO.Path.Combine(GitRepoFolder, newName);
                            counter++;
                        }

                        LogOperation($"Renamed: {itemName} -> {System.IO.Path.GetFileName(destPath)} (to avoid conflict)");
                    }

                    // Copy the item
                    try
                    {
                        if (isFolder)
                        {
                            // For folders, copy the entire directory structure
                            CopyDirectory(itemPath, destPath);
                            LogOperation($"Copied folder: {itemName}");
                        }
                        else
                        {
                            // For files, simple copy
                            File.Copy(itemPath, destPath);
                            LogOperation($"Copied file: {itemName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogOperation($"Error copying {itemName}: {ex.Message}");
                        // Continue with next item
                        continue;
                    }

                    // Update progress
                    ProcessedItems++;

                    // Small delay for visual effect
                    await Task.Delay(50, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                    break;

                // Perform Git operations for this batch
                string commitMessage = commitTemplate.Replace("{batch_num}", CurrentBatch.ToString());

                StatusMessage = $"Committing batch {CurrentBatch}...";

                await GitAddCommitPushAsync(GitRepoFolder, commitMessage);

                // Add a small delay between batches for better visualization
                await Task.Delay(500, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                StatusMessage = "Git operations completed successfully!";
            }
        }

        private async Task ProcessFullFolderModeAsync(CancellationToken cancellationToken)
        {
            string commitTemplate = FullCommitMessage;

            // Get all selected items
            var selectedItems = FileItems.Where(i => i.IsSelected).ToList();

            TotalItems = selectedItems.Count;
            if (TotalItems == 0)
            {
                MessageBox.Show("Please select items to process.", "No Items Selected");
                return;
            }

            // Log the operation
            string folderName = System.IO.Path.GetFileName(SourceFolder);
            LogOperation($"\n=== Processing {TotalItems} selected items from: {folderName} ===\n");

            ProcessedItems = 0;

            // Update status
            StatusMessage = "Copying selected items to git repository...";

            // Copy each selected item
            for (int i = 0; i < selectedItems.Count && !cancellationToken.IsCancellationRequested; i++)
            {
                var item = selectedItems[i];
                string itemName = item.Name;
                string itemPath = item.Path;
                bool isFolder = item.IsFolder;
                string destPath = System.IO.Path.Combine(GitRepoFolder, itemName);

                // Update status
                string itemType = isFolder ? "folder" : "file";
                StatusMessage = $"Copying {itemType} {i+1}/{TotalItems}: {itemName}";

                // If destination already exists, handle it
                if (File.Exists(destPath) || Directory.Exists(destPath))
                {
                    string baseName = System.IO.Path.GetFileNameWithoutExtension(itemName);
                    string extension = System.IO.Path.GetExtension(itemName);
                    int counter = 1;
                    while (File.Exists(destPath) || Directory.Exists(destPath))
                    {
                        string newName = $"{baseName}_{counter}{extension}";
                        destPath = System.IO.Path.Combine(GitRepoFolder, newName);
                        counter++;
                    }

                    LogOperation($"Renamed: {itemName} -> {System.IO.Path.GetFileName(destPath)} (to avoid conflict)");
                }

                // Copy the item
                try
                {
                    if (isFolder)
                    {
                        // For folders, copy the entire directory structure
                        CopyDirectory(itemPath, destPath);
                        LogOperation($"Copied folder: {itemName}");
                    }
                    else
                    {
                        // For files, simple copy
                        File.Copy(itemPath, destPath);
                        LogOperation($"Copied file: {itemName}");
                    }

                    // Update progress
                    ProcessedItems++;
                }
                catch (Exception ex)
                {
                    LogOperation($"Error copying {itemName}: {ex.Message}");
                }

                // Small delay for visual effect
                await Task.Delay(50, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            // Perform Git operations
            string commitMessage = commitTemplate.Replace("{folder_name}", folderName);

            StatusMessage = "Committing all selected items...";

            await GitAddCommitPushAsync(GitRepoFolder, commitMessage);

            // All items processed
            StatusMessage = "Git operations completed successfully!";
        }

        private void CopyDirectory(string sourcePath, string destPath)
        {
            // Create the destination directory
            Directory.CreateDirectory(destPath);

            // Copy all files
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string fileName = System.IO.Path.GetFileName(file);
                string destFile = System.IO.Path.Combine(destPath, fileName);
                File.Copy(file, destFile);
            }

            // Copy all subdirectories
            foreach (string dir in Directory.GetDirectories(sourcePath))
            {
                string dirName = System.IO.Path.GetFileName(dir);
                string destDir = System.IO.Path.Combine(destPath, dirName);
                CopyDirectory(dir, destDir);
            }
        }

        private async Task GitAddCommitPushAsync(string repoPath, string commitMessage)
        {
            try
            {
                // Git add
                LogOperation("\nRunning: git add .\n");
                var addResult = await RunGitCommandAsync(repoPath, "add", ".");
                LogOperation(string.IsNullOrEmpty(addResult) ? "Files staged successfully.\n" : addResult);

                // Git commit
                LogOperation($"\nRunning: git commit -m \"{commitMessage}\"\n");
                var commitResult = await RunGitCommandAsync(repoPath, "commit", $"-m \"{commitMessage}\"");
                LogOperation(string.IsNullOrEmpty(commitResult) ? "Commit successful.\n" : commitResult);

                // Check if we need to set up remote
                var remoteResult = await RunGitCommandAsync(repoPath, "remote", "");

                if (string.IsNullOrWhiteSpace(remoteResult))
                {
                    // No remote set, log and skip push
                    LogOperation("No remote repository configured. Skipping push.\n");
                    return;
                }

                // Git push
                LogOperation("\nRunning: git push\n");
                var pushResult = await RunGitCommandAsync(repoPath, "push", "");
                LogOperation(string.IsNullOrEmpty(pushResult) ? "Push successful.\n" : pushResult);
            }
            catch (Exception ex)
            {
                LogOperation($"Error during git operations: {ex.Message}\n");
            }
        }

        private async Task<string> RunGitCommandAsync(string workingDir, string command, string arguments)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"{command} {arguments}".Trim(),
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processInfo);
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit());

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Git command failed: {error}");
                }

                return $"{output}\n{error}".Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error running git command: {ex.Message}");
            }
        }

        private void LogOperation(string message)
        {
            LogTextBox.AppendText(message + "\n");
            LogTextBox.ScrollToEnd();
        }

        private void StartFileAnimation()
        {
            _animationRunning = true;
            _animationTimer.Start();
        }

        private void StopFileAnimation()
        {
            _animationRunning = false;
            _animationTimer.Stop();

            // Reset files to original position
            CreateFileIcons();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!_animationRunning)
                return;

            // Move a random file
            if (_fileShapes.Count > 0)
            {
                int fileIdx = _random.Next(0, _fileShapes.Count / 2);
                var fileRect = _fileShapes[fileIdx * 2];
                var fileText = _fileShapes[fileIdx * 2 + 1];

                double left = Canvas.GetLeft(fileRect);
                double top = Canvas.GetTop(fileRect);

                // If file hasn't reached destination
                if (left < 500)
                {
                    // Create a particle effect
                    var particleSystem = new ParticleSystem(AnimationCanvas, left + 10, top + 12, Colors.LightBlue);
                    _particleSystems.Add(particleSystem);
                    particleSystem.Start();
                }
                else
                {
                    // Reset file to source folder
                    double x = _random.Next(120, 180);
                    double y = _random.Next(85, 145);
                    Canvas.SetLeft(fileRect, x);
                    Canvas.SetTop(fileRect, y);
                    Canvas.SetLeft(fileText, x + 7);
                    Canvas.SetTop(fileText, y + 8);
                }
            }

            // Update particles
            UpdateParticles();
        }

        private void UpdateParticles()
        {
            // Update all particle systems and remove inactive ones
            var activeParticles = new List<ParticleSystem>();
            foreach (var particle in _particleSystems)
            {
                if (particle.Update())
                {
                    activeParticles.Add(particle);
                }
            }
            _particleSystems = activeParticles;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        
        #region Security
        
        // Security method to verify application integrity
        private void VerifyApplicationIntegrity()
        {
            try
            {
                // Set delayed verification to avoid immediate detection
                DispatcherTimer integrityTimer = new DispatcherTimer();
                integrityTimer.Interval = TimeSpan.FromSeconds(2); // Check after application is loaded
                integrityTimer.Tick += (s, e) => 
                {
                    integrityTimer.Stop();
                    
                    // Check 1: Verify window title contains author name
                    bool titleCheck = this.Title.Contains("Adithyanraj");
                    
                    // Check 2: Verify logo file exists and has correct hash
                    bool logoCheck = VerifyLogoIntegrity();
                    
                    // Check 3: Verify footer text contains author name
                    var textElements = FindVisualChildren<TextBlock>(this);
                    bool footerCheck = textElements.Any(t => t.Text.Contains("Adithyanraj"));
                    
                    // If any check fails, trigger application degradation
                    if (!titleCheck || !logoCheck || !footerCheck)
                    {
                        // Subtle failure - disable functionality without obvious error message
                        StatusMessage = "Ready to start Git operations";
                        // Corrupt the start button functionality
                        StartButton.Click -= StartOperations_Click;
                        StartButton.Click += (sender, args) => { 
                            LogOperation("Cannot perform operation - application integrity compromised."); 
                            StatusMessage = "Operation failed. Please contact the developer.";
                        };
                    }
                };
                integrityTimer.Start();
            }
            catch
            {
                // Intentionally empty - don't expose security mechanisms through exceptions
            }
        }
        
        private bool VerifyLogoIntegrity()
        {
            try
            {
                // Check if logo resource exists
                Uri resourceUri = new Uri("pack://application:,,,/logo.png", UriKind.Absolute);
                var stream = Application.GetResourceStream(resourceUri)?.Stream;
                
                if (stream == null)
                    return false;
                
                // Check resource size
                long fileSize = stream.Length;
                stream.Close();
                
                // Store bytes with deliberate obfuscation
                byte[] expectedSizeBytes = Convert.FromBase64String("MjU2MA=="); // "2560" encoded
                string expectedSize = System.Text.Encoding.UTF8.GetString(expectedSizeBytes);
                
                // Allow small variations in file size (±10%)
                long expectedSizeLong = long.Parse(expectedSize);
                long minSize = (long)(expectedSizeLong * 0.9);
                long maxSize = (long)(expectedSizeLong * 1.1);
                
                return fileSize >= minSize && fileSize <= maxSize;
            }
            catch
            {
                return false;
            }
        }
        
        // Helper to find visual children of a specific type
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                    yield return typedChild;
                
                foreach (var grandChild in FindVisualChildren<T>(child))
                    yield return grandChild;
            }
        }
        
        #endregion
    }

    public class FileItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string IconText => IsFolder ? "📁" : "📄";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ParticleSystem
    {
        private readonly Canvas _canvas;
        private readonly List<Particle> _particles = new List<Particle>();
        private bool _active;

        public ParticleSystem(Canvas canvas, double x, double y, Color color, int numParticles = 20)
        {
            _canvas = canvas;
            Random random = new Random();

            for (int i = 0; i < numParticles; i++)
            {
                double dx = random.NextDouble() * 10 - 5;
                double dy = random.NextDouble() * 10 - 5;
                double size = random.Next(2, 6);
                int lifetime = random.Next(10, 30);

                var ellipse = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(color)
                };

                Canvas.SetLeft(ellipse, x - size / 2);
                Canvas.SetTop(ellipse, y - size / 2);
                _canvas.Children.Add(ellipse);

                _particles.Add(new Particle
                {
                    Element = ellipse,
                    Dx = dx,
                    Dy = dy,
                    Lifetime = lifetime,
                    MaxLifetime = lifetime,
                    Size = size
                });
            }
        }

        public void Start()
        {
            _active = true;
        }

        public bool Update()
        {
            if (!_active)
                return false;

            bool activeParticles = false;

            foreach (var particle in _particles)
            {
                if (particle.Lifetime <= 0)
                {
                    particle.Element.Visibility = Visibility.Hidden;
                    continue;
                }

                activeParticles = true;
                particle.Lifetime--;
                double opacity = (double)particle.Lifetime / particle.MaxLifetime;

                // Update position
                Canvas.SetLeft(particle.Element, Canvas.GetLeft(particle.Element) + particle.Dx);
                Canvas.SetTop(particle.Element, Canvas.GetTop(particle.Element) + particle.Dy);

                // Update size and opacity
                if (particle.Element is FrameworkElement fe)
                {
                    double sizeFactor = opacity * particle.Size;
                    fe.Width = sizeFactor;
                    fe.Height = sizeFactor;
                }

                // Update opacity
                particle.Element.Opacity = opacity;
            }

            if (!activeParticles)
            {
                // Clean up - remove particles from canvas
                foreach (var particle in _particles)
                {
                    _canvas.Children.Remove(particle.Element);
                }
                _particles.Clear();
            }

            return activeParticles;
        }
    }

    public class Particle
    {
        public UIElement Element { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public int Lifetime { get; set; }
        public int MaxLifetime { get; set; }
        public double Size { get; set; }
    }

    public class LoadingAnimation : System.Windows.Controls.UserControl
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly List<Ellipse> _dots = new List<Ellipse>();
        private int _currentStep = 0;
        private bool _isRunning = false;

        public LoadingAnimation()
        {
            InitializeComponent();
            
            // Setup timer
            _timer.Interval = TimeSpan.FromMilliseconds(150);
            _timer.Tick += TimerTick;
        }

        private void InitializeComponent()
        {
            Width = 150;
            Height = 30;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E3440"));

            var panel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            for (int i = 0; i < 5; i++)
            {
                var dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88C0D0")),
                    Margin = new Thickness(5)
                };
                
                panel.Children.Add(dot);
                _dots.Add(dot);
            }

            Content = panel;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            // Reset all dots
            foreach (var dot in _dots)
            {
                dot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88C0D0"));
            }

            // Highlight current dot
            int currentDot = _currentStep % _dots.Count;
            _dots[currentDot].Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECEFF4"));

            _currentStep++;
        }

        public void StartAnimation()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _timer.Start();
            }
        }

        public void StopAnimation()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer.Stop();
                
                // Reset all dots
                foreach (var dot in _dots)
                {
                    dot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88C0D0"));
                }
            }
        }
    }
}
