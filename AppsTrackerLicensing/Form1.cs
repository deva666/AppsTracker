using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using AppsTracker;

namespace AppLoggerLicenseKeys
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();            
        }


        private byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private  byte[] GetHashSHA2(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private string GetHashSHA2String(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHashSHA2(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int version = 1;
            Int32.TryParse(tbVersion.Text, out version);
            string username = string.Format("{0} {1} {2}", tbUsername.Text, Constants.APP_NAME, version.ToString() );
            tbLicense.Text = GetHashSHA2String(username);
        } 
    }
}
