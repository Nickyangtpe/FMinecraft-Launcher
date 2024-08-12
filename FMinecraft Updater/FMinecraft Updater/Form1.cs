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
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Checking version";
            string url = "https://github.com/Nickyangtpe/FMinecraft-Launcher/raw/main/FMinecraft%20Launcher/last-version.json";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonString = await response.Content.ReadAsStringAsync();

                    VersionInfo newVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(jsonString);
                    Console.WriteLine($"New Version: {newVersionInfo.Version}");
                    Console.WriteLine($"URL: {newVersionInfo.Url}");

                    string filePath = Path.GetFullPath(@"FMinecraft Launcher.exe");
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    Console.WriteLine("File Version: " + fileVersionInfo.FileVersion);

                    Version currentVersion = new Version(fileVersionInfo.FileVersion);
                    Version latestVersion = new Version(newVersionInfo.Version);

                    int comparison = currentVersion.CompareTo(latestVersion);

                    if (comparison < 0)
                    {
                        label1.Text = "Downloading the latest version";
                        string downloadUrl = newVersionInfo.Url;
                        string downloadPath = Path.Combine(Path.GetTempPath(), "update.zip");

                        // 下載檔案並更新進度條
                        await DownloadFileAsync(downloadUrl, downloadPath);

                        label1.Text = "Decompressing version archive";

                        // 解壓縮檔案並更新進度條
                        string extractPath = Path.Combine(Path.GetTempPath(), "update");
                        Directory.CreateDirectory(extractPath);
                        await ExtractZipFileAsync(downloadPath, extractPath);

                        label1.Text = "Closing process";

                        // 關閉所有 FMinecraft Launcher.exe 進程
                        CloseRunningProcesses("FMinecraft Launcher.exe");

                        await Task.Delay(200);

                        label1.Text = "Replace obsolete files";
                        // 替換原本檔案
                        ReplaceOldFiles(filePath, extractPath);

                        File.Delete(downloadPath);
                        Directory.Delete(extractPath, true);

                        await Task.Delay(500);

                        Hide();

                        Process.Start("FMinecraft App Registrar.exe").WaitForExit();

                        isFinish = true;
                        Application.Exit();
                    }
                    else
                    {
                        isFinish = true;
                        Application.Exit();
                    }
                }
                catch (Exception ex)
                {
                    Process.Start(Path.GetFullPath("FMinecraft Launcher.exe"));
                    isFinish = true;
                    Clipboard.SetText(ex.Message);
                    Application.Exit();
                }

                await Task.Delay(500);

                Process.Start(Path.GetFullPath("FMinecraft Launcher.exe"));

            }
        }

        bool isFinish = false;

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
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
                                int progressPercentage = (int)((totalBytesRead * 100) / totalBytes);
                                UpdateProgressBar(progressPercentage);
                            }
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
                        // 如果文件名是 "Newtonsoft.Json.dll"，则跳过
                        if (entry.FullName.Equals("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

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
                        int progressPercentage = (int)((processedEntries * 100) / totalEntries);
                        UpdateProgressBar(progressPercentage);
                    }
                }
            });
        }

        private void ReplaceOldFiles(string originalFilePath, string newFilesPath)
        {
            // 將解壓縮的檔案替換原本的檔案
            foreach (var file in Directory.GetFiles(newFilesPath))
            {
                string fileName = Path.GetFileName(file);
                string destinationPath = Path.Combine(Path.GetDirectoryName(originalFilePath), fileName);

                // 如果目標檔案已存在，則刪除
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                // 移動新檔案到原本的路徑
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
