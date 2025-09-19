using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace TCPFileClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            tbFilename.Text = openFileDialog.FileName;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            {
                try
                {
                    // Buka file dan alokasikan memori
                    Stream fileStream = File.OpenRead(tbFilename.Text);
                    byte[] fileBuffer = new byte[fileStream.Length];
                    fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
                    fileStream.Close();

                    // Buka koneksi TCP/IP dan kirim data
                    TcpClient clientSocket = new TcpClient(tbServer.Text, 8080);
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Write(fileBuffer, 0, fileBuffer.GetLength(0));

                    // Tutup koneksi
                    networkStream.Close();
                    clientSocket.Close();

                    MessageBox.Show("File sent successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}
