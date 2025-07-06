using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Teste_Servidor
{
    public class main
    {
        public static void Main()
        {
            Application.Run(new Form1());

            Thread.Sleep(-1);
        }
    }
}
