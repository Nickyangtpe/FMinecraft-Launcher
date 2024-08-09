using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace FMinecraft_App_Registrar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 获取当前应用程序的路径
                string appPath = Path.GetFullPath("FMinecraft Launcher.exe");

                string unainstallPath = Path.GetFullPath("uninstall FMinecraft Launcher.exe");

                // 应用程序名称
                string appName = "FMinecraft Launcher";

                // 获取应用程序的版本号
                string version = FileVersionInfo.GetVersionInfo(appPath).ProductVersion;

                // 获取应用程序的发布者名称
                string publisher = FileVersionInfo.GetVersionInfo(appPath).CompanyName;

                // 获取应用程序的安装目录
                string installLocation = Path.GetDirectoryName(appPath);

                // 估算应用程序的大小 (以 KB 为单位)
                long fileSize = new FileInfo(appPath).Length / 1024;

                // 注册表路径
                string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + appName;

                // 创建注册表项
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryPath))
                {
                    key.SetValue("DisplayName", $"{appName}");
                    key.SetValue("DisplayVersion", $"{version}");
                    key.SetValue("Publisher", $"{publisher}");
                    key.SetValue("InstallLocation", $"\"{installLocation}\"");
                    key.SetValue("UninstallString", $"\"{unainstallPath}\"");
                    key.SetValue("DisplayIcon", $"{appPath}");
                    key.SetValue("NoModify", 1);
                    key.SetValue("NoRepair", 1);
                    key.SetValue("EstimatedSize", fileSize); // 单位为 KB
                }

                // 创建快捷方式路径
                CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "FMinecraft Launcher.lnk"));
                CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "FMinecraft Launcher.lnk"));

            }
            catch
            {
                Application.Exit();
            }
            finally
            {
                Application.Exit();
            }
        }

        private void CreateShortcut(string shortcutPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = Path.GetFullPath("FMinecraft Launcher.exe");
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortcut.TargetPath);
            shortcut.IconLocation = shortcut.TargetPath + ",0";
            shortcut.Save();
        }
    }
}
