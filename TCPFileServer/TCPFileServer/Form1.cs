using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace TCPFileServer
{
    public partial class Form1 : Form
    {
        // Deklarasikan ArrayList global di sini
        private ArrayList alSockets;

        // Kosongkan konstruktor Form1. Hanya inisialisasi komponen di sini.
        public Form1()
        {
            InitializeComponent();
        }

        // Jalankan semua logika startup di event Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
#pragma warning restore CS0618 // Type or member is obsolete
            lblStatus.Text = "My IP address is: " + IPHost.AddressList[0].ToString();

            alSockets = new ArrayList();

            // Mulai listenerThread di thread terpisah dari sini
            Thread thdListener = new Thread(new ThreadStart(listenerThread));
            thdListener.IsBackground = true;
            thdListener.Start();
        }

        // Ini adalah satu-satunya metode listenerThread yang benar
        public void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080);
            tcpListener.Start();

            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                if (handlerSocket.Connected)
                {
                    // Gunakan Invoke untuk memperbarui UI
                    this.Invoke((MethodInvoker)delegate
                    {
                        lbConnections.Items.Add(handlerSocket.RemoteEndPoint.ToString() + " connected.");
                    });

                    lock (alSockets)
                    {
                        alSockets.Add(handlerSocket);
                    }

                    Thread thdHandler = new Thread(new ThreadStart(handlerThread));
                    thdHandler.IsBackground = true;
                    thdHandler.Start();
                }
            }
        }

        private void handlerThread()
        {
            Socket handlerSocket = null;
            lock (alSockets)
            {
                if (alSockets.Count > 0)
                {
                    handlerSocket = (Socket)alSockets[alSockets.Count - 1];
                    alSockets.RemoveAt(alSockets.Count - 1);
                }
                else
                {
                    return;
                }
            }

            try
            {
                NetworkStream networkStream = new NetworkStream(handlerSocket);
                int blockSize = 1024;
                byte[] dataByte = new byte[blockSize];
                int thisRead;

                this.Invoke((MethodInvoker)delegate
                {
                    try
                    {
                        lock (this)
                        {
                            // Direktori penyimpanan file telah diubah ke lokasi yang Anda tentukan
                            string directoryPath = @"D:\Yoga\Kuliah\Semester 5\Jaringan\Sent Recieve File";
                            string filePath = Path.Combine(directoryPath, "upload.txt");

                            using (FileStream fileStream = File.OpenWrite(filePath))
                            {
                                while ((thisRead = networkStream.Read(dataByte, 0, blockSize)) > 0)
                                {
                                    fileStream.Write(dataByte, 0, thisRead);
                                }
                            }
                        }
                        lbConnections.Items.Add("File Written");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error writing file: " + ex.Message);
                    }
                    finally
                    {
                        networkStream.Close();
                        handlerSocket.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in handlerThread: " + ex.Message);
            }
        }
    }
}