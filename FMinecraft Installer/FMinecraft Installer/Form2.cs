using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Media;
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

            try
            {
                label3.Text = "獲取最新版本資訊";
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

                label3.Text = "移除過時檔案";

                // Remove all files and folders asynchronously
                await Task.Run(() =>
                {
                    foreach (string file in files)
                    {
                        File.Delete(file);
                        Invoke(new Action(() => progressBar1.Value++));
                    }

                    foreach (string directory in directories)
                    {
                        DeleteDirectory(directory);
                        Invoke(new Action(() => progressBar1.Value++));
                    }
                });

                // Create the target folder if it doesn't exist
                if (!Directory.Exists(customFolder))
                {
                    Directory.CreateDirectory(customFolder);
                }

                label3.Text = "下載最新版本";

                // Get the file name and full path for the download file
                string fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
                string filePath = Path.Combine(customFolder, fileName);

                // Download the file and show progress
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromHours(0.5) })
                {
                    var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
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
                            }
                        }
                    }
                }

                label3.Text = "解壓檔案";

                // Extract the downloaded file and delete the zip file
                string[] dirFiles = Directory.GetFiles(customFolder);
                if (dirFiles.Length == 1)
                {
                    ZipFile.ExtractToDirectory(dirFiles[0], customFolder);
                    File.Delete(dirFiles[0]);
                }

                label3.Text = "創建快捷";

                // Create a desktop shortcut for the application
                CreateShortcut("FMinecraft Launcher", FMinecraft_Launcher_FilePath, customFolder);

                downloading = false;
                button1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during installation:\n" + ex.Message);
                Application.Exit();
            }

            label3.Text = "安裝完成";
            Activate();
            SystemSounds.Exclamation.Play();
            BringToFront();
            TopMost = true;
            TopMost = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
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

            if (_createdestop)
            {
                shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{shortcutName}.lnk");
                Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath));
                File.WriteAllBytes(shortcutPath, resourceData);
            }
        }
    }
}
