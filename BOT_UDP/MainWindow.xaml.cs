using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace BOT_UDP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int randomInt;
        private bool isStarted;
        private UdpClient listener;
        private IPAddress IP;
        IPEndPoint groupEP;
        private int port;
        private string messageText;

        UdpClient udpClient;
        public MainWindow()
        {
            InitializeComponent();
            Random random = new Random();
            randomInt = random.Next();
            randomLabel.Content = Convert.ToString(randomInt);
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(UpdateRandomValue);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void MyMethod()
        {
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            UdpClient udpClient2 = new UdpClient();
            udpClient2.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient2.Connect("localhost", port);
            try
            {
                string message = "ACK!" + Convert.ToString(randomInt);
                Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                udpClient2.Send(sendBytes, sendBytes.Length);


                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                string message2 = String.Empty;
                do
                {
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    message2 = Encoding.ASCII.GetString(receiveBytes);
                    messageText = "This is the message you received: " + message2;
                }
                while (isStarted);
                udpClient.Close();
                udpClient2.Close();
            }
            catch (Exception ex)
            {
                messageText = ex.ToString();
            }
        }

        private void UpdateRandomValue(object sender, EventArgs e)
        {
            Random random = new Random();
            randomInt = random.Next();
            randomLabel.Content = Convert.ToString(randomInt);
            messageTextBlock.Text = messageText;
        }


        private void start_stop_button_Click(object sender, RoutedEventArgs e)
        {
            if (isStarted)
            {
                isStarted = !isStarted;
                start_stop_button.Content = "START";
                port_textBox.IsEnabled = true;
            }
            else
            {
                bool isNumeric = int.TryParse(port_textBox.Text, out port);
                if (isNumeric)
                {
                    if(port >= 5000 && port <= 60000)
                    {
                        //StartListener(port, messageTextBlock);

                        isStarted = !isStarted;
                        start_stop_button.Content = "STOP";
                        port_textBox.IsEnabled = false;
                        Thread myNewThread = new Thread(() => MyMethod());
                        myNewThread.Start();
                    }
                    else
                    {
                        port_textBox.Text = "Numer portu musi zawierać się w przedziale 5000-60000";
                    }
                }
                else
                {
                    port_textBox.Text = "Błąd";
                }
            }
        }

        private static void StartListener(int port, TextBlock textBlock)
        {
            UdpClient listener = new UdpClient(port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string text = $"Received broadcast from {groupEP} :";
                    text += $" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}";
                    textBlock.Text = text;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }
    }
}
