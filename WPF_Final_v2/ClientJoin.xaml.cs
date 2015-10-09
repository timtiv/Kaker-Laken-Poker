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
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Gamecode2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;



namespace WPF_Final_v2
{
    /// <summary>
    /// ClientJoin.xaml 的互動邏輯
    /// </summary>
    public partial class ClientJoin : Window
    {
        public ClientJoin()
        {
            InitializeComponent();
        }

        public string checkIfPassed;
        public bool GameStart = false;
        public TcpClient clientSocket = new TcpClient();
        public NetworkStream serverStream = default(NetworkStream);
        public ClientPage cPage;

        string DisplayData = null;

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clientSocket.Connect(textIP.Text, 8888);
                serverStream = clientSocket.GetStream();

                DisplayData = "Server Connected!";
                msg();

                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textName.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                Thread ctThread = new Thread(getMessage);
                ctThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Close();
            }

            cPage = new ClientPage(this);
            cPage.textDisplay.AppendText("Trying to Reach Server at " + this.textIP.Text + " ...");
            this.Content = cPage.Content;
            cPage.ini();
            cPage.Close();
        }

        private void getMessage()
        {
            while (true)
            {
                try
                {
                    serverStream = clientSocket.GetStream();
                    //int buffSize = 0;
                    byte[] inStream = new byte[204800];
                    //buffSize = clientSocket.ReceiveBufferSize;
                    serverStream.Read(inStream, 0, 204800);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream).TrimEnd(new char[] { (char)0 });
                    DisplayData = "" + returndata;
                    msg();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    this.Close();
                }
            }
        }
        // socket data received callback

        private void msg()
        {
            if (this.DisplayData.Equals("game.started")) {
                this.SendMsgViaSOCKET("get.game.instance");
                this.GameStart = true;
            }
            else if (this.GameStart)
            {
                string[] res = this.DisplayData.Split('%');
                string yourturn = res[0];
                string last_command = res[1];
                string json = res[2].Substring(0, res[2].LastIndexOf('}') + 1);

                // 寫一些前端的code在這裡
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    if (yourturn.Equals(this.textName.Text) || yourturn.Equals("!" + this.textName.Text))
                    {
                        if (yourturn.Equals("!" + this.textName.Text))
                        {
                            yourturn = this.textName.Text;
                        }

                        this.cPage.Send.IsEnabled = true;
                        this.cPage.Guess_1.IsEnabled = true;
                        this.cPage.Guess_2.IsEnabled = true;
                        this.cPage.You.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.cPage.Send.IsEnabled = false;
                        this.cPage.Guess_1.IsEnabled = false;
                        this.cPage.Guess_2.IsEnabled = false;
                        this.cPage.You.Visibility = Visibility.Hidden;
                    }
                   
                    this.cPage.textDisplay.Text = yourturn+"\n";
                    //this.cPage.textDisplay.Text += json;
                    Game obj = JsonConvert.DeserializeObject<Game>(json);
                    //this.cPage.me_bat.Content = obj.reg.Lookup(this.textName.Text).handcard.Count.ToString();
                    Player foo = obj.reg.Lookup(this.textName.Text);

                    if (!obj.gameGoing)
                    {
                        MessageBox.Show("Game over", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    if (!last_command.Equals(""))
                    {
                        Sentence sen = new Sentence(last_command, obj.reg);

                        string[] seperatePlayerAndCommand = last_command.Split(';');
                        string[] seperateSendAndClaim = seperatePlayerAndCommand[1].Split(',');
                        if (seperatePlayerAndCommand[1].Equals("guess:1") )
                        {
                            cPage.play_log.Text = "玩者 " + sen.from_str + " 相信玩者 " + sen.to_str + " 的為人!";
                        }
                        else if (seperatePlayerAndCommand[1].Equals("guess:0"))
                        {
                            cPage.play_log.Text = "玩者 " + sen.from_str + " 覺得玩者 " + sen.to_str + " 在唬爛!";
                        }
                        else
                        {
                            string[] seperateClaimAndAnimal = seperateSendAndClaim[1].Split(':');

                            //MessageBox.Show(seperateSendAndClaim[0].ToString());
                            string test = seperateClaimAndAnimal[1].ToString();
                            cPage.play_log.Text = "玩者 " + sen.from_str + " 給玩者 " + sen.to_str + "  一張宣稱是" + test + "的牌";

                        }
                        if (seperatePlayerAndCommand[1].Contains("guess") && yourturn == this.textName.Text)
                        {
                            string tempMsg = null;
                            if (sen.from_str == this.textName.Text)
                            {
                                tempMsg = "Oops! 猜錯了 QQ";
                            }
                            else {
                                tempMsg = "你被抓到了! Sucker!";
                            }
                            MessageBox.Show(tempMsg,"Oops",MessageBoxButton.OK,MessageBoxImage.Asterisk);
                        }
                    }

                    foreach (KeyValuePair<string, Player> pair in obj.reg.dictionary)
                    {
                        Player foo2 = pair.Value;
                        //String fooname = pair.Key;
                        if (foo2.onpass != null)
                        {
                            this.cPage.pass = foo2.onpass.type.ToString();
                        }
                    }

                    if (foo.last_str != null)
                    {
                        this.cPage.last_player = foo.last_str;  
                    }

                    /***
                    * 判斷send or pass
                    * ***/
                    bool allfalse = true;
                    foreach (KeyValuePair<string, Player> pair in obj.reg.dictionary)
                    {
                        if (pair.Value.passcardstatus == true)
                        {
                            allfalse = false;
                            
                            break;
                        }
                    }
                    if (allfalse && yourturn.Equals(this.textName.Text))
                    {
                        this.cPage.pass_check = true;
                    }
                    
                    /***
                     * HandCard Count
                     * ***/

                    Dictionary<string, int> handcard_dic = CalculateCard(foo.handcard);
                    Dictionary<string, Label> handcard_label = new Dictionary<string, Label>() { 
                        {"bat",this.cPage.me_bat},
                        {"fly",this.cPage.me_fly},
                        {"cocktail",this.cPage.me_cocktail},
                        {"toad",this.cPage.me_toad},
                        {"rat",this.cPage.me_rat},
                        {"scorpion",this.cPage.me_scorpion},
                        {"spider",this.cPage.me_spider},
                        {"bug",this.cPage.me_bug}
                    };
                    foreach (KeyValuePair<string, int> pair in handcard_dic)
                    {
                        handcard_label[pair.Key].Content = pair.Value;
                    }
                    /***
                    * ShowedCard Count
                    * ***/
                    Dictionary<string, int> showedcard_dic = CalculateCard(foo.showedcardlist);
                    Dictionary<string, Label> showedcard_label = new Dictionary<string, Label>() { 
                        {"bat",this.cPage.me_bat_blk},
                        {"fly",this.cPage.me_fly_blk},
                        {"cocktail",this.cPage.me_cocktail_blk},
                        {"toad",this.cPage.me_toad_blk},
                        {"rat",this.cPage.me_rat_blk},
                        {"scorpion",this.cPage.me_scorpion_blk},
                        {"spider",this.cPage.me_spider_blk},
                        {"bug",this.cPage.me_bug_blk}
                    };
                    foreach (KeyValuePair<string, int> pair in showedcard_dic)
                    {
                        showedcard_label[pair.Key].Content = pair.Value;              
                    }

                    /*看到別人*/
                    Player p1 = obj.reg.Lookup(yourturn);
                    for (int i = 0; i < obj.playerName.Length; i++)
                    {
                        this.cPage.PlayerButtonArray[i].Content = obj.playerName[i];
                    }

                    foreach(Button btn in this.cPage.PlayerButtonArray)
                    {
                        btn.IsEnabled = false;
                    }

                    for (int i = 0; i < obj.playerName.Length; i++)
                    {
                        this.cPage.PlayerButtonArray[i].IsEnabled = (obj.reg.Lookup(obj.playerName[i]).passcardstatus) ? false : true;
                        if (obj.reg.Lookup(obj.playerName[i]) == foo)
                        {
                            this.cPage.PlayerButtonArray[i].IsEnabled = false;
                        }
                    }

                    /**
                     *      牌數表格
                     * */
                    this.cPage.PlayerStatusGrid.Items.Clear();
                    foreach(KeyValuePair<string, Player> pair in obj.reg.dictionary){
                        Player p = pair.Value;
                        this.cPage.PlayerStatusGrid.Items.Add(new DataItem(pair.Key, p));
                    }
                }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    this.cPage.textDisplay.Text += Environment.NewLine + this.DisplayData;
                }));
            }
        }

        public static Dictionary<string, int> CalculateCard(ArrayList l) {
            Card[] handcard = new Card[l.Count];
            int handcard_count = 0;
            foreach (JObject j in l)
            {
                handcard[handcard_count] = j.ToObject<Card>();
                handcard_count++;
            }
            Dictionary<string, int> handcard_dic = new Dictionary<string, int>();
            string[] animal_type = { "bat", "fly", "cocktail", "toad", "rat", "scorpion", "spider", "bug" };
            for (int j = 0; j < animal_type.Length; j++)
            {
                handcard_dic.Add(animal_type[j], 0);
            }
            foreach (Card j in handcard)
            {
                handcard_dic[j.type]++;
            }
            return handcard_dic;
        }

        public void SendMsgViaSOCKET(string s)
        {
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(s + "$");
            this.serverStream.Write(outStream, 0, outStream.Length);
            this.serverStream.Flush();
        }
    }

    public class DataItem
    {
        public DataItem(string p_name, Player p) {
            Column1 = p_name;
            Column2 = p.handcard.Count.ToString();
            Dictionary<string, int> pDic = ClientJoin.CalculateCard(p.showedcardlist);
            Column3 = pDic["cocktail"].ToString();
            Column4 = pDic["rat"].ToString();
            Column5 = pDic["scorpion"].ToString();
            Column6 = pDic["fly"].ToString();
            Column7 = pDic["toad"].ToString();
            Column8 = pDic["spider"].ToString();
            Column9 = pDic["bat"].ToString();
            Column10 = pDic["bug"].ToString();
        }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
        public string Column6 { get; set; }
        public string Column7 { get; set; }
        public string Column8 { get; set; }
        public string Column9 { get; set; }
        public string Column10 { get; set; }
    }
}
