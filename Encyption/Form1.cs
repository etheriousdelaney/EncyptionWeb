using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;


//驗證比對網頁：http://www.seacha.com/tools/aes.html

namespace Encyption
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
        }



        //解密資料
        public static string DecryptStringAES(string cipherText, string key)
        {
            string key16;
            if (key.Length >= 16)
                key16 = key.Substring(0, 16);
            else
            {
                key16 = key;
                while (key16.Length < 16)
                {
                    key16 = key16 + "8";
                }
            }
            var keybytes = Encoding.UTF8.GetBytes(key16);
            var iv = Encoding.UTF8.GetBytes("8080808080808080");
            try
            {
                var encrypted = Convert.FromBase64String(cipherText);
                var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
                return string.Format(decriptedFromJavascript);
            }
            catch
            {
                MessageBox.Show("KEY or FILE ERROR!!");
                return "";
            }
        }
        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            string plaintext = null;
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.ECB;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                try
                {
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }
            return plaintext;
        }
        //加密資料
        public static string EncryptStringAES(string cipherText, string key)
        {
            string key16;
            if (key.Length >= 16)
                key16 = key.Substring(0, 16);
            else
            {
                key16 = key;
                while (key16.Length < 16)
                {
                    key16 = key16 + "8";
                }
            }
            var keybytes = Encoding.UTF8.GetBytes(key16);
            var iv = Encoding.UTF8.GetBytes("8080808080808080");         //自行設定
            var EncryptString = EncryptStringToBytes(cipherText, keybytes, iv);

            var str = BitConverter.ToString(EncryptString).Replace("-", string.Empty).ToLower();
            Console.WriteLine("Hex=" + str + "\n");

            return Convert.ToBase64String(EncryptString);
        }
        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            byte[] encrypted;
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.ECB;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }
        static void Pause()
        {
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
        public static String RandomString(int count)
        {
            Random R = new Random();//亂數種子
            String StrBuf = "";
            String StrArray = "QAZXSWEDCVFRTGBNHYUJMKIOLP";
            for (int i = 0; i < count; i++)
            {

                int j = R.Next(0, 25);//0~25

                StrBuf += StrArray.Substring(j, 1);

            }
            return StrBuf;
        }


        private void button1_Click(object sender, EventArgs e)
        {

            String EncryptText = EncryptStringAES(richTextBox1.Text, textBox1.Text);

            String NewText = "";
            int keylength = textBox1.Text.Length;
            int i;
            for (i = 0; i < keylength; i++)
            {
                NewText = NewText + textBox1.Text[i] + EncryptText[i];
            }
            NewText = NewText + EncryptText.Substring(i);

            saveFileDialog1.Title = "儲存加密檔";
            saveFileDialog1.FileName = "EncryptFile";
            //saveFileDialog1.Filter = "EncryptFile (*.raw)|*.raw";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            String file_name = saveFileDialog1.FileName;
            try
            {
                Bitmap bmp_src = (Bitmap)Image.FromFile(file_name, true);
                int width = bmp_src.Width;
                int high = bmp_src.Height;
                int r, g, b;
                int pxl_cnt = 0;

                bmp_src.SetPixel(0, 0, Color.FromArgb(NewText.Length));


                for (int y = 1; y < high; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        r = (bmp_src.GetPixel(x, y).R & 0xf0) | (NewText[pxl_cnt] & 0x0f);
                        g = bmp_src.GetPixel(x, y).G;
                        b = (bmp_src.GetPixel(x, y).B & 0xf0) | ((NewText[pxl_cnt] >> 4) & 0x0f);
                        bmp_src.SetPixel(x, y, Color.FromArgb(r, g, b));
                        pxl_cnt++;

                        if (pxl_cnt == NewText.Length) break;
                    }
                    if (pxl_cnt == NewText.Length) break;
                }
                bmp_src.Save(file_name.Substring(0, file_name.Length - 4) + "_E.bmp");
                bmp_src.Dispose();

            }
            catch
            {
                MessageBox.Show("Operation Fail!!");
                return;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "開啟加密檔";
            openFileDialog1.FileName = "EncryptFile";
            //openFileDialog1.Filter = "EncryptFile (*.raw)|*.raw";

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            String EncryptPath = openFileDialog1.FileName;
            String EncryptText = "";

            try
            {
                Bitmap bmp_src = (Bitmap)Image.FromFile(EncryptPath, true);
                int width = bmp_src.Width;
                int high = bmp_src.Height;
                int pxl_cnt = 0;
                int Encrypt_Leng = bmp_src.GetPixel(0, 0).ToArgb() & 0xffffff;
                int Encrypt_Subval = 0;

                for (int y = 1; y < high; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Encrypt_Subval = (bmp_src.GetPixel(x, y).R & 0x0f) | ((bmp_src.GetPixel(x, y).B << 4) & 0xf0);
                        EncryptText = EncryptText + ((char)Encrypt_Subval).ToString();
                        pxl_cnt++;

                        if (pxl_cnt == Encrypt_Leng) break;
                    }
                    if (pxl_cnt == Encrypt_Leng) break;
                }
                bmp_src.Dispose();


                String NewText = "";
                int keylength = textBox1.Text.Length;
                int i;
                for (i = 0; i < keylength; i++)
                {
                    NewText = NewText + EncryptText[i * 2 + 1];
                }
                NewText = NewText + EncryptText.Substring(i * 2);
                String DecryptText = DecryptStringAES(NewText, textBox1.Text);

                richTextBox1.Clear();
                richTextBox1.AppendText(DecryptText + "\n");

            }
            catch
            {
                MessageBox.Show("Operation Fail!!");
                return;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "儲存原始檔";
            saveFileDialog1.FileName = "SourceFile";
            //saveFileDialog1.Filter = "SourcetFile (*.txt)|*.txt";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            String DecryptPath = saveFileDialog1.FileName;
            String DecryptText = richTextBox1.Text;
            File.WriteAllText(DecryptPath, DecryptText);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "SourceFile";
            //openFileDialog1.Filter = "SourcetFile (*.txt)|*.txt";
            openFileDialog1.Title = "開啟原始檔";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            String DecryptPath = openFileDialog1.FileName;
            String DecryptText = System.IO.File.ReadAllText(DecryptPath);

            String EncryptText = EncryptStringAES(DecryptText, textBox1.Text);
            richTextBox1.Clear();
            richTextBox1.AppendText(EncryptText + "\n");

            string NewText = "";
            int keylength = textBox1.Text.Length;
            int i;
            for (i = 0; i < keylength; i++)
            {
                NewText = NewText + textBox1.Text[i] + EncryptText[i];
            }
            NewText = NewText + EncryptText.Substring(i);


            openFileDialog1.FileName = "CombineFile";
            //openFileDialog1.Filter = "SourcetFile (*.txt)|*.txt";
            openFileDialog1.Title = "開啟結合檔";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            String file_name = openFileDialog1.FileName;
            try
            {
                Bitmap bmp_src = (Bitmap)Image.FromFile(file_name, true);
                int width = bmp_src.Width;
                int high = bmp_src.Height;
                int r,g,b;
                int pxl_cnt = 0;

                bmp_src.SetPixel(0, 0, Color.FromArgb(NewText.Length));


                for (int y = 1; y < high; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        r = (bmp_src.GetPixel(x, y).R & 0xf0) | (NewText[pxl_cnt] & 0x0f);
                        g = bmp_src.GetPixel(x, y).G;
                        b = (bmp_src.GetPixel(x, y).B & 0xf0) | ((NewText[pxl_cnt] >> 4) & 0x0f);
                        bmp_src.SetPixel(x,y,Color.FromArgb(r, g, b));
                        pxl_cnt++;

                        if (pxl_cnt == NewText.Length) break;
                    }
                    if (pxl_cnt == NewText.Length) break;
                }
                bmp_src.Save(file_name.Substring(0, file_name.Length - 4) + "_E.bmp");
                bmp_src.Dispose();

            }
            catch
            {
                MessageBox.Show("Operation Fail!!");
                return;
            }
        }

    }
}
