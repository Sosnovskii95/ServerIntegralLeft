using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace ServerIntegralLeft
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string localIpServer;
        private int localPortServer;
        private Thread thread;
        private string[] result;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            localIpServer = ServerIp.Text;
            if (String.IsNullOrEmpty(localIpServer))
            {
                localIpServer = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
            }
            localPortServer = Convert.ToInt32(ServerPort.Text);
            TextServerPortStart.Content = localIpServer + ":" + localPortServer.ToString();

            thread = new Thread(new ThreadStart(Count));
            thread.IsBackground = true;
            thread.Start();
        }

        public void Count()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(localIpServer), localPortServer);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(iPEndPoint);
            socket.Listen(10);

            while (true)
            {
                Socket socket1 = socket.Accept();

                string builder = "";
                int bytes = 0; // количество полученных байтов
                byte[] data = new byte[1024]; // буфер для получаемых данных

                do
                {
                    bytes = socket1.Receive(data);
                    if (bytes != 0)
                    {
                        builder = Encoding.Unicode.GetString(data, 0, bytes);
                    }
                }
                while (socket1.Available > 0);

                if (bytes != 0 && !String.IsNullOrEmpty(builder))
                {

                    result = builder.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    double res = Solution(result[0], Convert.ToDouble(result[1]), Convert.ToDouble(result[2]), Convert.ToDouble(result[3]));

                    data = Encoding.Unicode.GetBytes(Convert.ToString(res));

                    socket1.Send(data);

                }

                socket1.Shutdown(SocketShutdown.Both);
                socket1.Close();
            }

        }

        private double Solution(string text_func, double temp_hx, double temp_hy, double temp_hz)
        {
            double result = 0;

            var calc = new Sprache.Calc.XtensibleCalculator();
            var expr = calc.ParseExpression(text_func, x => temp_hx, y => temp_hy, z => temp_hz);
            var func = expr.Compile();
            result = func();

            return result;
        }
    }
}
