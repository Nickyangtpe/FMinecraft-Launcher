using System;
using System.Drawing;
using System.Windows.Forms;

namespace FMinecraft_Installer
{
    public partial class Form3 : Form
    {
        private bool isInstall = false;

        public Form3()
        {
            InitializeComponent();
        }

        // Event handler for form load
        private void Form3_Load(object sender, EventArgs e)
        {
            // Set font for the rich text box
            richTextBox.Font = new Font("Arial", 10, FontStyle.Regular);

            // Add title
            richTextBox.AppendText("重新安裝的注意事項\n\n");

            // Set font style for the content
            SetFont(10, FontStyle.Regular);

            // Add content
            richTextBox.AppendText(
                "1. 數據丟失警告\n" +
                "   重新安裝軟體將會清除所有現有的內容，包括但不限於用戶設定、儲存的數據、及任何自定義配置。請確保在進行重新安裝前，您已經備份了所有重要資料，以免數據遺失。\n\n" +
                "2. 重新下載必要性\n" +
                "   如果您之前已經下載過該軟體的任何版本，重新安裝後您將需要重新下載最新版本的安裝檔案。請前往我們的官方網站或授權下載源，獲取最新的安裝檔並進行安裝。\n\n" +
                "3. 安裝程序的完整性\n" +
                "   為了確保重新安裝過程順利進行，請在安裝前確保所有相關文件和依賴項目已經準備妥當。若遇到任何安裝錯誤或問題，請參考我們的安裝指南或聯繫技術支援團隊。\n\n" +
                "4. 版本兼容性\n" +
                "   在重新安裝後，請檢查所安裝的版本是否與您的系統配置和其他應用程序兼容。如有需要，請更新系統或相關應用以確保最佳的運行效果。\n\n" +
                "我們建議您仔細閱讀上述注意事項，並確保在重新安裝過程中遵循所有指示。如有任何疑問或需進一步協助，請隨時聯繫我們的客戶服務部門。\n");
        }

        // Helper method to set font style
        private void SetFont(float size, FontStyle style)
        {
            richTextBox.SelectionFont = new Font("Arial", size, style);
        }

        // Event handler for checkbox state change
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = checkBox1.Checked;
        }

        // Event handler for the "Exit" button click
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Event handler for the "Install" button click
        private void button1_Click(object sender, EventArgs e)
        {
            isInstall = true;
            Close();
        }

        // Event handler for form closing
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isInstall)
            {
                Application.Exit();
            }
        }
    }
}
