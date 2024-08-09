using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FMinecraft_Installer
{
    public partial class Form2 : Form
    {
        private string FMinecraft_Launcher_FilePath; // Path to FMinecraft Launcher
        private bool downloading = true; // Download status
        private bool _createdestop;

        public bool Createdestop
        {
            get { return _createdestop; }
            set { _createdestop = value; }
        }

        public Form2()
        {
            InitializeComponent();
        }

        private async void Form2_Load(object sender, EventArgs e)
        {
            // Get the application data folder path
            string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            FMinecraft_Launcher_FilePath = Path.Combine(roamingFolder, "FMinecraft Launcher", "FMinecraft Launcher.exe");

            // Check for internet connection
            if (!CheckInternetConnection())
            {
                MessageBox.Show("網路連接失敗，請檢查您的網絡連接。", "網絡錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("Internet connection failed.");
                downloading = false;
                Application.Exit();
                return;
            }

            // Check disk space (minimum 500MB required)
            if (!CheckDiskSpace(roamingFolder, 500 * 1024 * 1024))
            {
                MessageBox.Show("警告: 磁碟空間不足，安裝完後下載資源檔案可能會失敗。", "磁碟空間不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("Insufficient disk space.");
            }

            try
            {
                label3.Text = "獲取最新版本資訊";
                Update();
                // URL of the latest download link on GitHub
                string gitHubUrl = "https://raw.githubusercontent.com/Nickyangtpe/FMinecraft-Launcher/main/latest";
                string downloadUrl;

                // Get the latest download link using HttpClient
                using (HttpClient client = new HttpClient())
                {
                    downloadUrl = await client.GetStringAsync(gitHubUrl);
                }

                // Set the target folder for the download file
                string customFolder = Path.Combine(roamingFolder, "FMinecraft Launcher");

                // Get all files and directories in the target folder
                string[] files = Directory.GetFiles(customFolder);
                string[] directories = Directory.GetDirectories(customFolder);

                // Set the progress bar maximum value
                progressBar1.Maximum = files.Length + directories.Length;
                progressBar1.Value = 0;


                // 關閉所有 FMinecraft Launcher.exe 進程
                CloseRunningProcesses("FMinecraft Launcher.exe");

                await Task.Delay(200);


                label3.Text = "移除過時檔案";
                Update();

                // Remove all files and folders asynchronously, except for those in the .minecraft folder
                await Task.Run(() =>
                {
                    foreach (string file in files)
                    {
                        if (!file.Contains(".minecraft"))
                        {
                            File.Delete(file);
                            Invoke(new Action(() => {
                                progressBar1.Value++;
                                label2.Text = $"{(int)((progressBar1.Value * 1.0 / progressBar1.Maximum) * 100)}%";
                                Update();
                            }));
                        }
                    }

                    foreach (string directory in directories)
                    {
                        if (!directory.Contains(".minecraft"))
                        {
                            DeleteDirectory(directory);
                            Invoke(new Action(() => {
                                progressBar1.Value++;
                                label2.Text = $"{(int)((progressBar1.Value * 1.0 / progressBar1.Maximum) * 100)}%";
                                Update();
                            }));
                        }
                    }

                });

                // Create the target folder if it doesn't exist
                if (!Directory.Exists(customFolder))
                {
                    Directory.CreateDirectory(customFolder);
                }

                label3.Text = "下載最新版本";
                Update();

                // Get the file name and full path for the download file
                string fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
                string filePath = Path.Combine(customFolder, fileName);

                // Download the file and show progress
                await DownloadWithRetry(downloadUrl, filePath, 3);

                label3.Text = "解壓檔案";
                Update();

                // Extract the downloaded file and delete the zip file
                string[] dirFiles = Directory.GetFiles(customFolder);
                if (dirFiles.Length == 1)
                {
                    ZipFile.ExtractToDirectory(dirFiles[0], customFolder);
                    File.Delete(dirFiles[0]);
                }

                label3.Text = "創建快捷";
                Update();

                byte[] APP = Properties.Resources.FMinecraft_App_Registrar;
                File.WriteAllBytes(Path.Combine(roamingFolder, "FMinecraft Launcher", "FMinecraft App Registrar.exe"), APP);

                byte[] uninstall = Properties.Resources.Uninstall_FMinecraft_Launcher;
                File.WriteAllBytes(Path.Combine(roamingFolder, "FMinecraft Launcher", "uninstall FMinecraft Launcher.exe"), uninstall);

                string workingDirectory = Path.Combine(roamingFolder, "FMinecraft Launcher");

                // 創建 ProcessStartInfo 物件
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(roamingFolder, "FMinecraft Launcher", "FMinecraft App Registrar.exe"),
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true // 設定為 true 使用 Windows Shell 來啟動進程
                };

                Process.Start(startInfo);
                

                // Create a desktop shortcut for the application
                CreateShortcut("FMinecraft Launcher", FMinecraft_Launcher_FilePath, customFolder);

                downloading = false;
                button1.Enabled = true;
            }
            catch (Exception ex)
            {
                Log("An error occurred during installation: " + ex.Message);
                if (MessageBox.Show("安裝過程中發生錯誤，是否打開日誌檔案？", "安裝錯誤", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    Process.Start("notepad.exe", GetLogPath());
                }
                downloading = false;
                Application.Exit();
                button1.Enabled = true;
            }

            label3.Text = "安裝完成";
            Activate();
            SystemSounds.Exclamation.Play();
            BringToFront();
            TopMost = true;
            TopMost = false;
        }



        private void CloseRunningProcesses(string processName)
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"無法關閉進程 {process.ProcessName}: {ex.Message}");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            Thread.Sleep(500);
            Process.Start(new ProcessStartInfo
            {
                FileName = FMinecraft_Launcher_FilePath,
                WorkingDirectory = Path.GetDirectoryName(FMinecraft_Launcher_FilePath)
            });
            Application.Exit();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (downloading)
            {
                e.Cancel = true;
                MessageBox.Show("在下載期間無法關閉", "操作取消", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Application.Exit();
            }
        }

        // Check internet connection
        private bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        // Check disk space
        private bool CheckDiskSpace(string path, long requiredSpace)
        {
            DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));
            return drive.AvailableFreeSpace > requiredSpace;
        }

        // Log installation events
        private void Log(string message)
        {
            string logPath = GetLogPath();
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
        }

        private string GetLogPath()
        {
            return Path.Combine(Path.GetTempPath(), "FMinecraft_Installer", "install.log");
        }

        // Download with retry mechanism
        private async Task DownloadWithRetry(string url, string filePath, int maxRetries)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromHours(0.5) })
                    {
                        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var buffer = new byte[8192];
                            long totalRead = 0L;
                            int read;

                            progressBar1.Maximum = 100;
                            progressBar1.Value = 0;

                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (totalBytes != -1)
                                {
                                    int progressPercentage = (int)((totalRead * 1.0 / totalBytes) * 100);
                                    progressBar1.Value = progressPercentage;
                                    label2.Text = $"{progressPercentage:0}%";
                                    Update();
                                }
                            }
                        }
                    }
                    break;
                }
                catch
                {
                    attempt++;
                    if (attempt == maxRetries)
                    {
                        throw;
                    }
                }
            }
        }

        // Recursively delete a directory and its contents
        private void DeleteDirectory(string directoryPath)
        {
            foreach (string file in Directory.GetFiles(directoryPath))
            {
                File.Delete(file);
            }

            foreach (string directory in Directory.GetDirectories(directoryPath))
            {
                DeleteDirectory(directory);
            }

            Directory.Delete(directoryPath);
        }

        // Create a shortcut for the application
        private void CreateShortcut(string shortcutName, string targetPath, string customFolder)
        {
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows\Start Menu\Programs", $"{shortcutName}.lnk");

            Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath));
            byte[] resourceData = Properties.Resources.FMinecraft_Launcher_exe;
            File.WriteAllBytes(shortcutPath, resourceData);

            if (Createdestop)
            {
                string desktopShortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{shortcutName}.lnk");
                Directory.CreateDirectory(Path.GetDirectoryName(desktopShortcutPath));
                File.WriteAllBytes(desktopShortcutPath, resourceData);
            }
        }

    }
}
