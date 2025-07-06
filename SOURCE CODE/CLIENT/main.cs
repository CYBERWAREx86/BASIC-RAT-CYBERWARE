using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Teste_Cliente;

namespace RemoteClient
{
    class Program
    {
        static TcpClient telaClient;
        static TcpClient comandoClient;
        static NetworkStream comandoStream;

        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                string mensagem = null;
                using (var formInput = new IpPedid())
                {
                    if (formInput.ShowDialog() == DialogResult.OK)
                    {
                        mensagem = formInput.InputText;
                    }
                    else
                    {
                        MessageBox.Show("Entrada cancelada pelo usuário.");
                        return;
                    }
                }

                telaClient = new TcpClient(mensagem, 1234);
                comandoClient = new TcpClient(mensagem, 4321);
                comandoStream = comandoClient.GetStream();

                new Thread(EnviarTela).Start();
                new Thread(ReceberComandos).Start();

                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro no cliente: " + ex.Message);
            }
        }

        static void EnviarTela()
        {
            NetworkStream stream = telaClient.GetStream();
            while (true)
            {
                Bitmap screenshot = CapturarTela();
                byte[] imagemBytes = ImagemParaBytes(screenshot);

                byte[] tamanhoBytes = BitConverter.GetBytes(imagemBytes.Length);
                stream.Write(tamanhoBytes, 0, 4);
                stream.Write(imagemBytes, 0, imagemBytes.Length);

                Thread.Sleep(100);
            }
        }

        static void ReceberComandos()
        {
            StreamReader reader = new StreamReader(comandoStream);
            while (true)
            {
                try
                {
                    string linha = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(linha))
                        ExecutarComando(linha);
                }
                catch { break; }
            }
        }

        static Bitmap CapturarTela()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                CURSORINFO ci = new CURSORINFO();
                ci.cbSize = Marshal.SizeOf(ci);
                if (GetCursorInfo(out ci) && ci.flags == CURSOR_SHOWING)
                {
                    DrawIcon(g.GetHdc(), ci.ptScreenPos.x, ci.ptScreenPos.y, ci.hCursor);
                    g.ReleaseHdc();
                }
            }
            return bmp;
        }

        static byte[] ImagemParaBytes(Bitmap imagem)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imagem.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        static void ExecutarComando(string cmd)
        {
            try
            {
                string[] partes = cmd.Split('|');
                switch (partes[0])
                {
                    case "MOVE":
                        SetCursorPos(int.Parse(partes[1]), int.Parse(partes[2]));
                        break;
                    case "CLICK_LEFT":
                        mouse_event(0x02 | 0x04, 0, 0, 0, UIntPtr.Zero);
                        break;
                    case "CLICK_RIGHT":
                        mouse_event(0x08, 0, 0, 0, UIntPtr.Zero);
                        mouse_event(0x10, 0, 0, 0, UIntPtr.Zero);
                        break;
                    case "KEY":
                        byte vk = byte.Parse(partes[1]);
                        keybd_event(vk, 0, 0, 0);
                        keybd_event(vk, 0, 2, 0);
                        break;
                }
            }
            catch { }
        }

        [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")] static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")] static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport("user32.dll")] static extern bool GetCursorInfo(out CURSORINFO pci);
        [DllImport("user32.dll")] static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int x, y; }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        const int CURSOR_SHOWING = 0x00000001;
    }
}