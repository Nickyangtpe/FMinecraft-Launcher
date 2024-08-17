using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FMinecraft_Updater
{
    public partial class Form1 : Form
    {
        private bool isFinish = false;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Checking version";
            string url = "https://github.com/Nickyangtpe/FMinecraft-Launcher/raw/main/FMinecraft%20Launcher/last-version.json";

            try
            {
                VersionInfo newVersionInfo = await FetchVersionInfoAsync(url);
                Version currentVersion = GetCurrentVersion();
                Version latestVersion = new Version(newVersionInfo.Version);

                if (currentVersion.CompareTo(latestVersion) < 0)
                {
                    await UpdateApplicationAsync(newVersionInfo);
                }
                else
                {
                    ExitApplication();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task<VersionInfo> FetchVersionInfoAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VersionInfo>(jsonString);
            }
        }

        private Version GetCurrentVersion()
        {
            string filePath = Path.GetFullPath(@"FMinecraft Launcher.exe");
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return new Version(fileVersionInfo.FileVersion);
        }

        private async Task UpdateApplicationAsync(VersionInfo newVersionInfo)
        {
            label1.Text = "Downloading the latest version";
            string downloadUrl = newVersionInfo.Url;
            string downloadPath = Path.Combine(Path.GetTempPath(), "update.zip");

            await DownloadFileAsync(downloadUrl, downloadPath);

            label1.Text = "Decompressing version archive";
            string extractPath = Path.Combine(Path.GetTempPath(), "update");
            Directory.CreateDirectory(extractPath);
            await ExtractZipFileAsync(downloadPath, extractPath);

            label1.Text = "Closing process";
            CloseRunningProcesses("FMinecraft Launcher.exe");

            label1.Text = "Replacing obsolete files";
            string filePath = Path.GetFullPath(@"FMinecraft Launcher.exe");
            ReplaceOldFiles(filePath, extractPath);

            File.Delete(downloadPath);
            Directory.Delete(extractPath, true);

            Hide();
            Process.Start(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "FMinecraft App Registrar.exe")).WaitForExit();

            ExitApplication();
        }

        private void ExitApplication()
        {
            isFinish = true;
            Application.Exit();
        }

        private void HandleException(Exception ex)
        {
            Process.Start(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "FMinecraft Launcher.exe"));
            isFinish = true;
            Clipboard.SetText(ex.Message);
            Application.Exit();
        }

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? -1;
                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                    fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        if (totalBytes > 0)
                        {
                            UpdateProgressBar((int)((totalBytesRead * 100) / totalBytes));
                        }
                    }
                }
            }
        }

        private async Task ExtractZipFileAsync(string zipPath, string extractPath)
        {
            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    int totalEntries = archive.Entries.Count;
                    int processedEntries = 0;

                    foreach (var entry in archive.Entries)
                    {
                        string entryPath = Path.Combine(extractPath, entry.FullName);

                        if (entry.FullName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(entryPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
                            entry.ExtractToFile(entryPath, overwrite: true);
                        }

                        processedEntries++;
                        UpdateProgressBar((int)((processedEntries * 100) / totalEntries));
                    }
                }
            });
        }

        private void ReplaceOldFiles(string originalFilePath, string newFilesPath)
        {
            foreach (var file in Directory.GetFiles(newFilesPath))
            {
                string fileName = Path.GetFileName(file);

                if (fileName.Equals("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string destinationPath = Path.Combine(Path.GetDirectoryName(originalFilePath), fileName);

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                File.Move(file, destinationPath);
            }
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

        private void UpdateProgressBar(int percentage)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new Action(() => progressBar1.Value = Math.Min(percentage, 100)));
            }
            else
            {
                progressBar1.Value = Math.Min(percentage, 100);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isFinish)
            {
                return;
            }
            e.Cancel = true;
        }
    }

    public class VersionInfo
    {
        public string Version { get; set; }
        public string Info { get; set; }
        public string Url { get; set; }
    }
}
