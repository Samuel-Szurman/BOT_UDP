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
using System.Net.NetworkInformation;

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
        IPAddress[] localIPs;

        UdpClient udpClient;
        public MainWindow()
        {
            InitializeComponent();
            localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            Random random = new();
            randomInt = random.Next();
            randomLabel.Content = Convert.ToString(randomInt);
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(UpdateRandomValue);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

        private void MyMethod()
        {
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IP, port));


            try
            {
                IPEndPoint RemoteIpEndPoint = new(IP, 0);

                string message1 = string.Empty;
                do
                {
                    byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    message1 = Encoding.ASCII.GetString(receiveBytes);
                    //messageText = "This is the message you received: " + message1;
                    string timeStamp = DateTime.Now.ToString();
                    IPAddress senderAddress = RemoteIpEndPoint.Address;
                    messageText += "Date: " + timeStamp + "   IP: " + senderAddress.ToString() + "   Message: " + message1 + "\n";

                    int senderPort = RemoteIpEndPoint.Port;


                    UdpClient udpClient2 = new UdpClient();
                    udpClient2.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient2.Connect(senderAddress, senderPort);
                    string message2 = "ACK! " + Convert.ToString(randomInt);
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(message2);
                    udpClient2.Send(sendBytes, sendBytes.Length);
                    udpClient2.Close();


                }
                while (isStarted);
                udpClient.Close();

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
            messageTextBox.Text = messageText;
        }


        private void start_stop_button_Click(object sender, RoutedEventArgs e)
        {
            if (isStarted)
            {
                isStarted = !isStarted;
                start_stop_button.Content = "START";
                port_textBox.IsEnabled = true;
                IP_textBox.IsEnabled = true;
            }
            else
            {
                bool isNumeric = int.TryParse(port_textBox.Text, out port);
                if (isNumeric)
                {
                    if (port is >= 5000 and <= 60000)
                    {
                        //StartListener(port, messageTextBox);
                        if (IP_textBox.Text == string.Empty)
                        {
                            IP = IPAddress.Any;
                            isStarted = !isStarted;
                            start_stop_button.Content = "STOP";
                            port_textBox.IsEnabled = false;
                            IP_textBox.IsEnabled = false;
                            new Thread(() => MyMethod()).Start();
                        }
                        else
                        {
                            try
                            {
                                IP = IPAddress.Parse(IP_textBox.Text);
                                if (localIPs.Contains(IP))
                                {
                                    isStarted = !isStarted;
                                    start_stop_button.Content = "STOP";
                                    port_textBox.IsEnabled = false;
                                    IP_textBox.IsEnabled = false;
                                    new Thread(() => MyMethod()).Start();
                                }
                                else
                                {
                                    IP_textBox.Text = "Podany adres IP nie jest lokalny";
                                }
                            }
                            catch (Exception)
                            {
                                IP_textBox.Text = "Niepoprawny adres IP";
                            }
                        }
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
    }
}
