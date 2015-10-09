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
using Gamecode;

namespace WPF_Final_v2
{
    /// <summary>
    /// ClientPage.xaml 的互動邏輯
    /// </summary>
    public partial class ClientPage : Window
    {
        public Button[] PlayerButtonArray;
        public ClientJoin c;
        public string pass;
        public string last_player;
        public Boolean pass_check = false;
        public bool feedback;

        public ClientPage(ClientJoin c1)
        {
            this.c = c1;
            InitializeComponent();
        }


        public void ini()
        {
            PlayerButtonArray = new Button[8]{ this.p1, this.p2, this.p3, this.p4, this.p5, this.p6, this.p7, this.p8 };
            var result = MessageBox.Show("開始遊戲?", "確認準備", MessageBoxButton.OK);
            if (result == MessageBoxResult.OK)
            {
                this.c.SendMsgViaSOCKET(this.c.textName.Text + ",ready");
                Player.Content = "我是:\n" + this.c.textName.Text;
            }
        }

       
        public string claim = null;
        public string assign = null;
        public string Claim_for_user = null;
        
        private void claim_card(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            claim = btn.Name.ToString();
            Claim_for_user = btn.Content.ToString();

        }
        
        private void assign_player(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            assign = btn.Content.ToString();
        }

        

        public string pick = null;
        public string pick_for_user = null;
       
        private void pick_handcard(object sender, RoutedEventArgs e)
        {
            Button picker = (Button)sender;
            switch (picker.Name.ToString())
            {
                case "send_rat":
                    pick = "rat";
                    pick_for_user = "老鼠";
                    break;
                case "send_bat":
                    pick = "bat";
                    pick_for_user = "蝙蝠";
                    break;
                case "send_fly":
                    pick = "fly";
                    pick_for_user = "蒼蠅";
                    break;
                case "send_toad":
                    pick = "toad";
                    pick_for_user = "蟾蜍";
                    break;
                case "send_bug":
                    pick = "bug";
                    pick_for_user = "臭蟲";
                    break;
                case "send_spider":
                    pick = "spider";
                    pick_for_user = "蜘蛛";
                    break;
                case "send_scorpion":
                    pick = "scorpion";
                    pick_for_user = "蠍子";
                    break;
                case "send_cockroach":
                    pick = "cocktail";
                    pick_for_user = "蟑螂";
                    break;
           }      
        }

        private void Guess_1_Click(object sender, RoutedEventArgs e)
        {
            string MyName;
            string GuessCommand;
            MyName = this.c.textName.Text;
            GuessCommand = MyName + "," + last_player + ";" + "guess:1";

            var result = MessageBox.Show("你確定要相信玩者 "+last_player+" 的嘴巴說說?", "確認送出資訊", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.c.SendMsgViaSOCKET(GuessCommand);
            }
            
        }

        private void Guess_2_Click(object sender, RoutedEventArgs e)
        {
            string MyName;
            string GuessCommand;
            MyName = this.c.textName.Text;
            GuessCommand = MyName + "," + last_player + ";" + "guess:0";

            var result = MessageBox.Show("你確定玩者 "+last_player+" 在唬爛而要抓包他?", "確認送出資訊", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.c.SendMsgViaSOCKET(GuessCommand);
            }
        }

        
        public string DataToServer_Send_Pass = null;

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string MyName;
            MyName = this.c.textName.Text;

            if (pass_check == true)
            {
                DataToServer_Send_Pass = MyName + "," + assign + ";" + "send:" + pick + ",claim:" + claim;
            }
            else
            {
                DataToServer_Send_Pass = MyName + "," + assign + ";" + "pass:" + pass + ",claim:" + claim;
            }

            //MessageBox.Show(DataToServer_Send_Pass,"",MessageBoxButton.OKCancel,MessageBoxImage.Asterisk);

            var result = MessageBox.Show("你將要對玩者 " + assign + " 送出 " + pick_for_user + "並宣稱他為 " + Claim_for_user + "?", "確認送出資訊", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.c.SendMsgViaSOCKET(DataToServer_Send_Pass);
            }   
        }   
    }
}

