using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace EncEnc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int len;
            byte[] buffer = new byte[4096];

            //Output file path.
            string OutFilePath = Path.Combine(Path.GetDirectoryName(this.textBox1.Text), Path.GetFileNameWithoutExtension(this.textBox1.Text)) + ".hogehoge";

            using (var outfs = new FileStream(OutFilePath, FileMode.Create, FileAccess.Write))
            {
                using (var aes = new AesManaged())
                {
                    aes.BlockSize = 128;
                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes("", 16);
                    byte[] salt = new byte[16];

                    salt = deriveBytes.Salt;

                    // 生成した擬似乱数から16バイト切り出したデータをパスワードにする
                    byte[] bufferKey = deriveBytes.GetBytes(16);
                    aes.Key = bufferKey;
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(salt, 0, 16);
                        outfs.Write(aes.IV, 0, 16);
                        using (var ds = new DeflateStream(cse, CompressionMode.Compress))
                        {
                            using (FileStream fs = new FileStream(this.textBox1.Text, FileMode.Open, FileAccess.Read))
                            {
                                while ((len = fs.Read(buffer, 0, 4096)) > 0)
                                {
                                    ds.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int len;
            byte[] buffer = new byte[4096];

            //Output file path.
            string OutFilePath = Path.Combine(Path.GetDirectoryName(this.textBox1.Text), Path.GetFileNameWithoutExtension(this.textBox1.Text)) + ".txt";

            using (FileStream outfs = new FileStream(OutFilePath, FileMode.Create, FileAccess.Write))
            {
                using (FileStream fs = new FileStream(this.textBox1.Text, FileMode.Open, FileAccess.Read))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes("", salt);
                        byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                        aes.Key = bufferKey;

                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
