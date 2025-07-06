using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Teste_Servidor
{
    public partial class Form1 : Form
    {
        TcpListener telaListener = new TcpListener(IPAddress.Any, 1234);
        TcpListener comandoListener = new TcpListener(IPAddress.Any, 4321);
        TcpClient clienteTela;
        TcpClient clienteComando;
        NetworkStream comandoStream;
        DateTime ultimoEnvioMouse = DateTime.MinValue;
        bool controleAtivo = false;
        public Form1()
        {
            InitializeComponent();
            new Thread(ListenTela).Start();
            new Thread(ListenComandos).Start();
        }

        void ListenTela()
        {
            telaListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            telaListener.Start(); 
            clienteTela = telaListener.AcceptTcpClient();
            var telaStream = clienteTela.GetStream();

            while (true)
            {
                try
                {
                    byte[] tamanhoBytes = new byte[4];
                    telaStream.Read(tamanhoBytes, 0, 4);
                    int tamanho = BitConverter.ToInt32(tamanhoBytes, 0);

                    byte[] buffer = new byte[tamanho];
                    int lido = 0;
                    while (lido < tamanho)
                        lido += telaStream.Read(buffer, lido, tamanho - lido);

                    using (var ms = new MemoryStream(buffer))
                    {
                        Image img = Image.FromStream(ms);
                        pictureBox1.Invoke(new MethodInvoker(() => pictureBox1.Image = img));
                    }
                }
                catch { break; }
            }
        }

        void ListenComandos()
        {
            comandoListener.Start();
            clienteComando = comandoListener.AcceptTcpClient();
            comandoStream = clienteComando.GetStream();
        }

        void EnviarComando(string cmd)
        {
            try
            {
                if (comandoStream != null && controleAtivo)
                {
                    byte[] data = Encoding.UTF8.GetBytes(cmd + "\n");
                    comandoStream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao enviar comando: " + ex.Message);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.K)
            {
                controleAtivo = !controleAtivo;
                MessageBox.Show("Controle remoto " + (controleAtivo ? "ATIVADO" : "DESATIVADO"));
                return true;
            }

            if (controleAtivo && comandoStream != null)
            {
                EnviarComando($"KEY|{(byte)keyData}");
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!controleAtivo || comandoStream == null) return;

            if (e.Button == MouseButtons.Left)
                EnviarComando("CLICK_LEFT");
            else if (e.Button == MouseButtons.Right)
                EnviarComando("CLICK_RIGHT");
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!controleAtivo || pictureBox1.Image == null || comandoStream == null) return;
            if ((DateTime.Now - ultimoEnvioMouse).TotalMilliseconds < 50) return;

            float escalaX = (float)Screen.PrimaryScreen.Bounds.Width / pictureBox1.Width;
            float escalaY = (float)Screen.PrimaryScreen.Bounds.Height / pictureBox1.Height;
            int realX = (int)(e.X * escalaX);
            int realY = (int)(e.Y * escalaY);

            EnviarComando($"MOVE|{realX}|{realY}");
            ultimoEnvioMouse = DateTime.Now;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                telaListener.Stop();
                comandoListener.Stop();
                clienteTela?.Close();
                clienteComando?.Close();
                comandoStream?.Close();
            }
            catch { }
        }
    }
}