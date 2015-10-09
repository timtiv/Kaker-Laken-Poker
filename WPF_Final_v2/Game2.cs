using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;

namespace Gamecode2
{
    public class PlayerRegister {
        public Dictionary<string, Player> dictionary = new Dictionary<string, Player>();
        public void Add(string key, Player val)
        {
            this.dictionary.Add(key, val);
        }
        public Player Lookup(string key)
        {
            return this.dictionary[key];
        }
        public string getRef(Player p)
        {
            var key = this.dictionary.FirstOrDefault(x => x.Value == p);
            return key.Key;
        }
        public int Count() 
        {
            return this.dictionary.Count;
        }
    }
    public class Player
    {
        //public void Send(string cardname, Player to)
        //{
        //    to.onpass = take_handcard(cardname);
        //    this.handcard.Remove(to.onpass);
        //    this.passcardstatus = true;
        //}

        //public void Claim(string cardname, Player to) 
        //{
        //    to.claim = cardname;
        //    to.last = this;
        //}

        //public bool Pass(Player to, Game g) {
        //    int passed = 0;
        //    for (int i = 0; i < g.playerName.Length; i++)
        //    {
        //        if (g.reg.Lookup(g.playerName[i]).passcardstatus)
        //            passed++;
        //    }
        //    if (passed == g.playerName.Length - 1)
        //        return false;
        //    to.onpass = this.onpass;
        //    this.onpass = null;
        //    this.passcardstatus = true;
        //    return true;
        //}

        //public bool Guess(bool honest, Game g)
        //{
        //    // Reset all player to next round
        //    for (int i = 0; i < g.playerName.Length; i++)
        //    {
        //        g.reg.Lookup(g.playerName[i]).passcardstatus = false;
        //    }
        //    if (this.claim.Equals(this.onpass.type))
        //    {
        //        return (honest) ? true : false;
        //    }
        //    else
        //    {
        //        return (!honest) ? true : false;
        //    }
        //}
        public string last_str = null;
        public Player last;
        public Card take_handcard(string cardname)
        {
            foreach (Card c in this.handcard) 
            {
                if (c.type.Equals(cardname))
                {
                    return c;
                }
            }
            // only for debugging, remove it later.
            return new Card("no_card");
        }
        // send/claim/pass/guess
        private string _claim;
        public string claim
        {
            get
            {
                return this._claim;
            }
            set
            {
                this._claim = value;
            }
        }
        private Card _onpass;
        public Card onpass 
        {
            get
            {
                return this._onpass;
            }
            set
            {
                this._onpass = value;
            }
        }
        private ArrayList _handcard = new ArrayList();
        public ArrayList handcard
        {
            get 
            {
                return this._handcard;
            }
            set 
            {
                this._handcard = value;
            }
        }
        private ArrayList _showedcardlist = new ArrayList();
        public ArrayList showedcardlist
        {
            get 
            {
                return this._showedcardlist;
            }
            set 
            {
                this._showedcardlist = value;
            }
        }
        private bool _passcardstatus = false;
        public bool passcardstatus 
        {
            get 
            {
                return this._passcardstatus;
            }
            set
            {
                this._passcardstatus = value;
            }
        }
    }
    //public class Host
    //{
    //    public Random rnd = new Random();
    //    public void Deal(PlayerRegister reg)
    //    {
    //        string[] animal_type = { "bat", "fly", "cocktail", "toad", "rat", "scorpion", "spider", "bug" };
    //        ArrayList TotalCardList = new ArrayList();
    //        for (int i = 0; i < 8; i++)
    //        {
    //            for (int t = 0; t < 8; t++)
    //            {
    //                TotalCardList.Add(new Card(animal_type[i]));
    //            }
    //        }
    //        int mod = TotalCardList.Count % reg.Count();
    //        int cards_each = (TotalCardList.Count - mod) / reg.Count();
    //        foreach (KeyValuePair<string, Player> pair in reg.dictionary)
    //        {

    //            for (int i = 0; i < cards_each; i++)
    //            {
    //                int index = rnd.Next(0, TotalCardList.Count);
    //                pair.Value.handcard.Add(TotalCardList[index]);
    //                TotalCardList.RemoveAt(index);
    //            }
    //        }
    //        if (mod != 0)
    //        {
    //            foreach (KeyValuePair<string, Player> pair in reg.dictionary)
    //            {
    //                if (TotalCardList.Count == 0)
    //                    break;
    //                int index = rnd.Next(0, TotalCardList.Count);
    //                pair.Value.handcard.Add(TotalCardList[index]);
    //                TotalCardList.RemoveAt(index);
    //            }
    //        }
    //    }
    //    public string chooseStarter(PlayerRegister reg)
    //    {
    //        ArrayList PlayerList = new ArrayList();
    //        foreach (KeyValuePair<string, Player> pair in reg.dictionary)
    //        {
    //            PlayerList.Add(pair.Key.ToString());
    //        }
    //        return (string)PlayerList[rnd.Next(0, PlayerList.Count)];
    //    }

    //}
    public class Card
    {
        // Type: bat, fly, cocktail, toad, rat, scorpion, spider, bug
        public Card(string type)
        {
            this.type = type;
        }

        private string _type;
        public string type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
    }
    public class Sentence 
    {
        public Sentence(string sentence, PlayerRegister reg)
        {
            this.ToObject(sentence, reg);
        }
        public Player from;
        public string from_str;
        public Player to;
        public string to_str;
        public ArrayList action = new ArrayList();
        public void ToObject(string sentence, PlayerRegister reg)
        {
            string[] stage1 = sentence.Split(';');
            string[] stage11 = stage1[0].Split(',');
            this.from_str =stage11[0];
            this.to_str = stage11[1];
            string[] stage12 = stage1[1].Split(',');
            for (int i = 0; i < stage12.Length; i++)
            {
                this.action.Add(stage12[i].Split(':'));
            }
        }
        public string ToSentence() 
        {
            string sentence = this.from_str + "," + this.to_str + ";";
            foreach (string[] a in this.action)
            {
                sentence += String.Join(":", a)+",";
            }
            return sentence.Substring(0,sentence.Length-1);
        }
    }
    public class Game 
    {
        public string starter;
        public PlayerRegister reg = new PlayerRegister();
        public string[] playerName;
        public Game(){
   
        }
        public bool gameGoing = true;
        //public string Input(string c) 
        //{
        //    string result = "";
        //    Sentence n = new Sentence(c, reg);
        //    if (this.isGameOver(n))
        //    {
        //        this.gameGoing = false;
        //        return result;
        //    }
        //    foreach (string[] action in n.action) {
        //        if (action[0].Equals("send"))
        //        {
        //            n.from.Send(action[1], n.to);
        //            result = n.to_str;
        //        }
        //        else if (action[0].Equals("claim"))
        //        {
        //            n.from.Claim(action[1], n.to);
        //            result = n.to_str;
        //        }
        //        else if (action[0].Equals("pass"))
        //        {
        //            if (n.from.Pass(n.to, this))
        //            {
        //                result = n.to_str;
        //            }
        //            else
        //            {
        //                result = "!" + n.from_str;
        //                break;
        //            }
        //        }
        //        else if (action[0].Equals("guess"))
        //        {
        //            bool guess = (action[1] == "1") ? true : false;
        //            if (n.from.Guess(guess, this))
        //            {
        //                n.from.last.showedcardlist.Add(n.from.onpass);
        //                result = reg.getRef(n.from.last);
        //            }
        //            else
        //            {
        //                n.from.showedcardlist.Add(n.from.onpass);
        //                result = n.from_str;
        //            }
        //        }
        //        else
        //        {
        //            result = "DIE";
        //        }
        //    }
        //    return result;
        //}

        //public bool isGameOver(Sentence n)
        //{
        //    bool gameover = false;
        //    for (int i = 0; i < playerName.Length; i++)
        //    {
        //        ArrayList showcard = this.reg.Lookup(playerName[i]).showedcardlist;
        //        Card[] showlist = (Card[])showcard.ToArray(typeof(Card));
        //        if (showlist.Distinct().Count() == 8)
        //        {
        //            gameover = true;
        //            return gameover;
        //        }
        //    }
        //    for (int i = 0; i < playerName.Length; i++)
        //    {
        //        ArrayList showcard = this.reg.Lookup(playerName[i]).showedcardlist;
        //        Card[] showlist = (Card[])showcard.ToArray(typeof(Card));
        //        Dictionary<string, int> dic = new Dictionary<string, int>();
        //        string[] animal_type = { "bat", "fly", "cocktail", "toad", "rat", "scorpion", "spider", "bug" };
        //        for (int j = 0; j < animal_type.Length; j++)
        //        {
        //            dic.Add(animal_type[j], 0);
        //        }
        //        for (int k = 0; k < showlist.Length; k++)
        //        {
        //            dic[showlist[k].type]++;
        //        }
        //        foreach (KeyValuePair<string, int> pair in dic)
        //        {
        //            if (pair.Value == 4)
        //            {
        //                gameover = true;
        //                return gameover;
        //            }
        //        }
        //    }
        //    if (n.from.handcard.Count == 0 && n.action[0].Equals("send"))
        //    {
        //        gameover = true;
        //        return gameover;
        //    }
        //    return gameover;
        //    /* 三種結束遊戲情況:
        //    * 1.showedcard有八種動物(done)
        //    * 2.showedcard有四張一樣的動物
        //    * 3.輪到自己出牌時手牌 = 0
        //    *
        //    */
        //}
    }
}
