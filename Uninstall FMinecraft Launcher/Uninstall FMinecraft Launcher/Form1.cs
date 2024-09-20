using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Uninstall_FMinecraft_Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 關閉所有 FMinecraft Launcher.exe 進程
                CloseRunningProcesses("FMinecraft Launcher.exe");

                await Task.Delay(200);

                string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                // 获取当前应用程序的路径
                string appPath = Path.GetFullPath(Path.Combine(roamingFolder, "FMinecraft Launcher", "FMinecraft Launcher.exe"));
                string installLocation = Path.GetDirectoryName(appPath);
                string shortcutCommonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "FMinecraft Launcher.lnk");
                string shortcutStartMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "FMinecraft Launcher.lnk");

                // 删除快捷方式
                DeleteShortcut(shortcutCommonPath);
                DeleteShortcut(shortcutStartMenuPath);

                // 删除应用程序文件
                if (File.Exists(appPath))
                {
                    File.Delete(appPath);
                }

                // 删除注册表项
                string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FMinecraft Launcher";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKeyTree(""); // 删除子项及其所有值
                    }
                }

                // 删除其他指定的注册表项
                string appPathsRegistryPath1 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + Path.GetFileName(appPath);
                string appPathsRegistryPath2 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\FML.exe";
                string appPathsRegistryPath3 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\FMC.exe";

                DeleteRegistryKey(appPathsRegistryPath1);
                DeleteRegistryKey(appPathsRegistryPath2);
                DeleteRegistryKey(appPathsRegistryPath3);

                MessageBox.Show("Uninstalled successfully.", "FMinecraft Launcher",MessageBoxButtons.OK,MessageBoxIcon.Information);
                
                // 在完成所有操作后删除自身
                ScheduleSelfDeletion(installLocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during uninstallation: " + ex.Message);
                Application.Exit();
            }
        }

        private void DeleteShortcut(string shortcutPath)
        {
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }

        private void DeleteRegistryKey(string registryPath)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath, true))
                {
                    if (key != null)
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(registryPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to delete registry key {registryPath}: {ex.Message}");
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
                    Console.WriteLine($"Unable to close process {process.ProcessName}: {ex.Message}");
                }
            }
        }

        private void ScheduleSelfDeletion(string directoryToDelete)
        {
            // 延迟执行删除自身的操作
            string batchFilePath = Path.Combine(Path.GetTempPath(), "uninstall_self.bat");

            // 创建批处理文件内容
            string batchContent = $@"
@echo off
timeout /t 5 /nobreak >nul
rd /s /q ""{directoryToDelete}""
del ""{Application.ExecutablePath}""
";

            // 将批处理文件写入磁盘
            File.WriteAllText(batchFilePath, batchContent);

            // 启动批处理文件，并隐藏窗口
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchFilePath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(processInfo);

            // 退出应用程序
            Application.Exit();
        }
    }
}
