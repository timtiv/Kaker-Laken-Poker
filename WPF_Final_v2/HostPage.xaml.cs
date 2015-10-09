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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using WPF_Final_v2;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using Gamecode;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Newtonsoft.Json;

namespace WPF_Final_v2
{
    public partial class HostPage : Window
    {
        public HostPage()
        {
            InitializeComponent();
        }
        public String DisplayIPAddresses()
        {
            StringBuilder sb = new StringBuilder();
            String result;
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface network in networkInterfaces)
            {
                IPInterfaceProperties properties = network.GetIPProperties();
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (IPAddress.IsLoopback(address.Address))
                        continue;
                    sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }
            result = sb.ToString();
            return result;
        }
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        public static Hashtable clientsList = new Hashtable();
        public Game g;
        public int playerNum = 3;
        public static Dictionary<string, bool> playerReadyDic = new Dictionary<string, bool>();
        private void Listening_Click(object sender, RoutedEventArgs e)
        {
            // Start Server
            string IP = HostIP.Text;
            playerNum = int.Parse(gameNumBox.Text);
            Thread thread = new Thread(delegate()
            {
                doServer(IP, this.playerNum);
            });
            thread.Start();

            // Start Client
            ClientJoin cJoin = new ClientJoin();
            cJoin.Show();
            cJoin.textIP.Text = IP;
            cJoin.textName.Text = this.gameID.Text;
            ButtonAutomationPeer peer = new ButtonAutomationPeer(cJoin.btnConnect);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
            this.Close();
        }
        private void doServer(string IP, int gameNum)
        {
            AllocConsole();
            IPAddress localaddr = IPAddress.Parse(IP);
            Int32 port = 8888;
            TcpListener serverSocket = new TcpListener(localaddr, port);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;
            serverSocket.Start();
            Console.WriteLine("Server Started...at: " + IP);
            counter = 0;
            while ((true))
            {
                counter += 1;

                clientSocket = serverSocket.AcceptTcpClient();
                byte[] bytesFrom = new byte[10025];
                String dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                clientsList.Add(dataFromClient, clientSocket);

                string msg = dataFromClient + " Joined , Wait For "+ (gameNum - counter) + " Player(s) to Join.";
                broadcast(msg, dataFromClient, false);
                string[] ready = dataFromClient.Split(',');
                playerReadyDic.Add(ready[0], false);
                Console.WriteLine(msg);
                if (gameNum == counter)
                {
                    string[] pArray = playerReadyDic.Keys.ToArray();
                    this.g = new Game(pArray);
                    Console.WriteLine("Game Instance Created.");
                }
                handleClient client = new handleClient();
                client.startClient(clientSocket, dataFromClient, clientsList, playerReadyDic, this);
            }
        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                try
                {
                    TcpClient broadcastSocket;
                    broadcastSocket = (TcpClient)Item.Value;
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;
                    if (flag == true)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(uName + " says: " + msg);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }
                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IPList.Text = DisplayIPAddresses();
        }

    }
    public class handleClient
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientList;
        Game g = null;
        HostPage h;
        Dictionary<string, bool> playerReady;

        public void startClient(TcpClient inClientSocket, string clientNo, Hashtable cList, Dictionary<string,bool> playerReadyDic, HostPage h)
        {
            this.h = h;
            this.clientSocket = inClientSocket;
            this.clNo = clientNo;
            this.clientList = cList;
            this.playerReady = playerReadyDic;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        public void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("From Client - " + clNo + " : " + dataFromClient);
                    rCount = Convert.ToString(requestCount);
                    
                    string response = null;
                    string player = "";
                    string ready = "";
                    if(!allReady()) 
                    {
                        string[] readys = dataFromClient.Split(',');
                        player = readys[0];
                        ready = readys[1];
                        if (ready.Equals("ready"))
                            playerReady[player] = true;
                        response = dataFromClient;
                    }
                    Console.WriteLine(JsonConvert.SerializeObject(this.playerReady, Formatting.Indented).ToString());
                    if (allReady())
                    {
                        if (this.g == null)
                        {
                            this.g = this.h.g;
                            if (ready.Equals("ready"))
                            {
                                response = "game.started";
                            }
                            else if (dataFromClient == "get.game.instance")
                            {
                                response = this.g.starter;
                                response += "%%" + GameMonitor.See(this.g);
                            }
                        }
                        else
                        {
                            if (dataFromClient == "get.game.instance")
                            {
                                response = this.g.starter;
                                response += "%%" + GameMonitor.See(this.g);
                            }
                            else
                            {
                                response = this.g.Input(dataFromClient);
                                response += "%" + dataFromClient+ "%"+GameMonitor.See(this.g);
                            }
                        }
                    }
                    
                    WPF_Final_v2.HostPage.broadcast(response, clNo, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public bool allReady() {
            bool res = true;
            if (h.playerNum != this.playerReady.Count)
                return false;
            foreach(KeyValuePair<string,bool> pair in this.playerReady){
                if (!pair.Value)
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
    }

}
