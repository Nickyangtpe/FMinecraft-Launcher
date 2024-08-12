using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using File = System.IO.File;

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
                string uninstallPath = Path.GetFullPath("uninstall FMinecraft Launcher.exe");

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
                string uninstallRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + appName;
                string appPathsRegistryPath1 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + Path.GetFileName(appPath);
                string appPathsRegistryPath2 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + "FML";
                string appPathsRegistryPath3 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + "FMC";
                // 创建卸载信息注册表项
                using (RegistryKey uninstallKey = Registry.LocalMachine.CreateSubKey(uninstallRegistryPath))
                {
                    uninstallKey.SetValue("DisplayName", appName);
                    uninstallKey.SetValue("DisplayVersion", version);
                    uninstallKey.SetValue("Publisher", publisher);
                    uninstallKey.SetValue("InstallLocation", $"\"{installLocation}\"");
                    uninstallKey.SetValue("UninstallString", $"\"{uninstallPath}\"");
                    uninstallKey.SetValue("DisplayIcon", appPath);
                    uninstallKey.SetValue("NoModify", 1);
                    uninstallKey.SetValue("NoRepair", 1);
                    uninstallKey.SetValue("EstimatedSize", fileSize); // 单位为 KB
                }
                string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                // 创建应用程序路径注册表项
                using (RegistryKey appPathsKey = Registry.LocalMachine.CreateSubKey(appPathsRegistryPath1))
                {
                    appPathsKey.SetValue("", Path.Combine(Path.GetDirectoryName(appPath),"FML.exe"));
                }
                // 创建应用程序路径注册表项
                using (RegistryKey appPathsKey = Registry.LocalMachine.CreateSubKey(appPathsRegistryPath2))
                {
                    appPathsKey.SetValue("", Path.Combine(Path.GetDirectoryName(appPath), "FML.exe"));
                }
                // 创建应用程序路径注册表项
                using (RegistryKey appPathsKey = Registry.LocalMachine.CreateSubKey(appPathsRegistryPath3))
                {
                    appPathsKey.SetValue("", Path.Combine(Path.GetDirectoryName(appPath), "FML.exe"));
                }

                byte[] FML = Properties.Resources.FML;
                File.WriteAllBytes(Path.Combine(roamingFolder, "FMinecraft Launcher", "FML.exe"), FML);

                // 创建快捷方式
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
