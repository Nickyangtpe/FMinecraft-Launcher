using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FMinecraft_Installer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Event handler for the "Next" button click
        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            Thread.Sleep(100); // Sleep for a brief moment
            Form2 form = new Form2
            {
                Createdestop = checkBox2.Checked // Set the 'createdestop' property based on checkbox state
            };
            form.Show();
        }

        // Event handler for the "Exit" button click
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Event handler for form load
        private void Form1_Load(object sender, EventArgs e)
        {
            Hide();
            string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Set the target folder for downloaded files
            string customFolder = Path.Combine(roamingFolder, "FMinecraft Launcher");
            if (!Directory.Exists(customFolder))
            {
                Directory.CreateDirectory(customFolder); // Create the folder if it does not exist
            }

            string[] files = Directory.GetFiles(customFolder);
            if (files.Length > 0)
            {
                Form form3 = new Form3();
                form3.ShowDialog();
            }

            // Initialize the rich text box with the license agreement
            richTextBox.Font = new Font("Arial", 10, FontStyle.Regular);
            richTextBox.AppendText("FreeMinecraft Launcher 授權條款\n\n");

            AppendFormattedText("授權協議\n", 12, FontStyle.Bold);
            AppendFormattedText("\n本軟體授權協議（以下簡稱“本協議”）適用於 FreeMinecraft Launcher（以下簡稱“本軟體”）。通過下載、安裝、或使用本軟體，您（個人或單一實體）同意受本協議條款的約束。如果您不同意本協議的條款，請勿下載、安裝或使用本軟體。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("授權許可\n", 12, FontStyle.Bold);
            AppendFormattedText("\n根據本協議條款，本軟體的開發者（以下簡稱“開發者”）授予您以下權利：\n", 10, FontStyle.Regular);
            AppendFormattedText("1. 安裝和使用：您可以在您的個人計算機上免費安裝和使用本軟體，僅供個人非商業用途。\n", 10, FontStyle.Regular);
            AppendFormattedText("2. 備份副本：您可以製作一份本軟體的備份副本，但僅供備份和存檔用途。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("使用限制\n", 12, FontStyle.Bold);
            AppendFormattedText("\n除非法律允許或開發者書面同意，您不得：\n", 10, FontStyle.Regular);
            AppendFormattedText("1. 對本軟體進行反向工程、反編譯、反匯編或試圖以其他方式發現本軟體的源代碼。\n", 10, FontStyle.Regular);
            AppendFormattedText("2. 租賃、出租、出借、出售、再分發或再許可本軟體。\n", 10, FontStyle.Regular);
            AppendFormattedText("3. 移除、改變或隱藏本軟體中的任何版權聲明或其他所有權聲明。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("知識產權\n", 12, FontStyle.Bold);
            AppendFormattedText("\n本軟體及其所有副本的版權和其他知識產權歸開發者所有。本軟體受版權法和其他知識產權法保護。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("無擔保\n", 12, FontStyle.Bold);
            AppendFormattedText("\n本軟體按“現狀”提供，開發者不對本軟體的性能或結果作出任何明示或暗示的擔保，包括但不限於對適銷性、特定用途的適用性和不侵權的擔保。使用本軟體的風險由您自行承擔。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("責任限制\n", 12, FontStyle.Bold);
            AppendFormattedText("\n在任何情況下，開發者均不對因使用或無法使用本軟體而引起的任何損害承擔責任，包括但不限於直接、間接、附帶、特殊、懲罰性或後果性損害，即使開發者已被告知該等損害的可能性。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("終止\n", 12, FontStyle.Bold);
            AppendFormattedText("\n如您違反本協議的任何條款，開發者可隨時終止本協議並撤銷您對本軟體的使用權。終止後，您必須銷毀本軟體的所有副本及其任何部分。\n\n", 10, FontStyle.Regular);

            AppendFormattedText("一般條款\n", 12, FontStyle.Bold);
            AppendFormattedText("\n本協議受您所在國家或地區的法律管轄，與法律衝突原則無關。本協議構成您與開發者之間關於本軟體的完整協議，並取代之前的任何口頭或書面協議。如果本協議的任何條款被視為無效或不可執行，其餘條款將繼續完全有效。\n\n", 10, FontStyle.Regular);

            Show();
        }

        // Event handler for checkbox state change
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = checkBox1.Checked;
        }

        // Helper method to append formatted text to the rich text box
        private void AppendFormattedText(string text, float size, FontStyle style)
        {
            richTextBox.SelectionFont = new Font("Arial", size, style);
            richTextBox.AppendText(text);
        }
    }
}
