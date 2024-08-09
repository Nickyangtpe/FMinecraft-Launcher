using System;
using System.Windows.Forms;

namespace FMinecraft_Launcher_Fixer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Hide();
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\FileSystem", true))
                {
                    if (key != null)
                    {
                        key.SetValue("LongPathsEnabled", 1, Microsoft.Win32.RegistryValueKind.DWord);
                        var result = MessageBox.Show("Long path support has been enabled. Please restart your computer for the changes to take effect. Do you want to restart now?",
                                        "FMinecraft Launcher",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Information);
                        if (result == DialogResult.Yes)
                        {
                            RestartComputer();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to modify registry. Long path support could not be enabled.",
                                        "FMinecraft Launcher",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to enable long path support: {ex.Message}",
                                "FMinecraft Launcher",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            Application.Exit();
        }

        private void RestartComputer()
        {
            var psi = new System.Diagnostics.ProcessStartInfo("shutdown", "/r /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            System.Diagnostics.Process.Start(psi);
        }
    }
}
