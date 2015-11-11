using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;

namespace Box
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public IList<com_champ> _com_champ = new List<com_champ>(); //常用英雄 : name, count
        public IList<player_profile> _player_profile = new List<player_profile>(); //Player Profile : player_id, server, fighting
        public IList<normal_statistics> _normal_statistics = new List<normal_statistics>(); //normal statistics
        public IList<rank_statistics> _rank_statistics = new List<rank_statistics>(); //rank statistics
        public IList<played_champs_display> _played_champs_display = new List<played_champs_display>();
        public IList<match_list> _match_list = new List<match_list>(); //match_list
        public IList<playerMatchDetails> _playerMatchDetails = new List<playerMatchDetails>();
        public IList<matchDetails> _matchDetailsA = new List<matchDetails>();
        public IList<honour> _honourA = new List<honour>();
        public IList<items> _itemA = new List<items>();
        public IList<matchDetails> _matchDetailsB = new List<matchDetails>();
        public IList<honour> _honourB = new List<honour>();
        public IList<items> _itemB = new List<items>();
        string _playerId = "";
        string _server = "";
        string server = "";
        string playerId = "";
        string tier = "";
        string rank = "";
        string points = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _playerId = txtPlayerId.Text;
            _server = "比尔吉沃特";
            Application.Current.Resources["_server"] = _server;
            Application.Current.Resources["_playerId"] = _playerId;
            //server = System.Web.HttpUtility.UrlEncode(dpServer.SelectedValue, System.Text.Encoding.UTF8);
            //playerId = System.Web.HttpUtility.UrlEncode(txtPlayerId.Text, System.Text.Encoding.UTF8);
            server = _server;
            playerId = _playerId;
            Application.Current.Resources["server"] = server;
            Application.Current.Resources["playerId"] = playerId;

            string url = "http://lolbox.duowan.com/playerDetail.php?serverName=" + server + "&playerName=" + playerId;
            var getHtmlWeb = new HtmlWeb();
            var htmlDocument = getHtmlWeb.Load(url);
            HtmlNode bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");

            if (!bodyNode.InnerHtml.Contains("暂无数据"))
            {
                lblMsg.Content = "";
                sendRequest(server, playerId);
                parseHtml(server, playerId);
            }
            else
            {
                lblMsg.Content = "暂无数据";
                //gvPlayerProfile.ItemsSource = null;
                //gvPlayerProfile.DataSource = null;
                //gvPlayerProfile.DataBind();
                //gvRankStat.DataSource = null;
                //gvRankStat.DataBind();
                ////gvComChamp.DataSource = null;
                ////gvComChamp.DataBind();
                //gvMatchList.DataSource = null;
                //gvMatchList.DataBind();
                //gvNormalStat.DataSource = null;
                //gvNormalStat.DataBind();
                //gvPlayedChamps.DataSource = null;
                //gvPlayedChamps.DataBind();
            }
        }
        private void sendRequest(string server, string playerId)
        {
            WebRequest request = WebRequest.Create("http://lolbox.duowan.com/ajaxGetWarzone.php?playerName=" + playerId + "&serverName=" + server);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            tier = Regex.Match(Regex.Match(responseFromServer, @"tier"".*"",""rank").Value, @""".*""").Value.Replace(@"""", "").Replace(":", "").Replace(",", "");
            tier = Unicode2Chinese(tier);
            rank = tier + " " + Regex.Match(Regex.Match(responseFromServer, @"rank"".*"",").Value, @"""\w+""").Value.Replace(@"""", "");
            points = Regex.Match(Regex.Match(responseFromServer, @"league_points"".*,").Value, ":.*,").Value.Replace(":", "").Replace(",", "").Replace(@"""", "");
        }
        private string Unicode2Chinese(string strUnicode)
        {
            string[] splitString = new string[1];
            splitString[0] = "\\u";
            string[] unicodeArray = strUnicode.Split(splitString, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();

            foreach (string item in unicodeArray)
            {
                byte[] codes = new byte[2];
                int code1, code2;
                code1 = Convert.ToInt32(item.Substring(0, 2), 16);
                code2 = Convert.ToInt32(item.Substring(2), 16);
                codes[0] = (byte)code2;
                codes[1] = (byte)code1;
                sb.Append(Encoding.Unicode.GetString(codes));
            }

            return sb.ToString();
        }
        private void parseHtml(string server, string playerId)
        {
            string url = "http://lolbox.duowan.com/playerDetail.php?serverName=" + server + "&playerName=" + playerId;
            string fighting = "";
            string level = "";
            string playerIcon = "";
            string first_win = "";

            var getHtmlWeb = new HtmlWeb();
            var htmlDocument = getHtmlWeb.Load(url);
            HtmlNode bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");

            //战斗力 : update_datetime, fighting
            IEnumerable<HtmlNode> fightingNodes = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes.Contains("class")
                && x.Attributes["class"].Value.Split().Contains("fighting"));
            foreach (HtmlNode child in fightingNodes)
            {
                HtmlNode node = child.SelectSingleNode("//span");
                fighting = Regex.Match(child.InnerText, @"\d+").Value;
            }

            //get icon, level
            IEnumerable<HtmlNode> profileNodes = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes.Contains("class")
                && x.Attributes["class"].Value.Split().Contains("avatar"));
            foreach (HtmlNode child in profileNodes)
            {
                if (!child.InnerHtml.Contains("yy"))
                {
                    level = child.SelectSingleNode("//em").InnerText;
                    playerIcon = Regex.Match(child.InnerHtml, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                }
            }

            //get first win
            IEnumerable<HtmlNode> firstWinNodes = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes.Contains("class")
                && x.Attributes["class"].Value.Split().Contains("act"));
            foreach (HtmlNode child in firstWinNodes)
            {
                first_win = child.InnerText.Replace("&nbsp;", "");
            }
            //add profile
            _player_profile.Add(new player_profile
            {
                player_id = _playerId,
                server = _server,
                fighting = fighting,
                level = level,
                icon = playerIcon,
                first_win = first_win
            });

            //常用英雄 : name, count
            IEnumerable<HtmlNode> com_heroNodes = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes.Contains("class")
                && x.Attributes["class"].Value.Split().Contains("com-hero"));
            foreach (HtmlNode child in com_heroNodes)
            {
                if (child.NodeType != HtmlNodeType.Text)
                {
                    HtmlNodeCollection nodes = child.SelectNodes("//li");
                    foreach (HtmlNode node in nodes)
                    {
                        string _count = Regex.Match(Regex.Match(node.InnerHtml, @"\d次").Value, @"\d").Value;
                        if (node.Attributes.Contains("champion-name-ch"))
                        {
                            _com_champ.Add(new com_champ
                            {
                                champion_name_ch = node.Attributes["champion-name-ch"].Value,
                                champion_name = node.Attributes["champion-name"].Value,
                                icon = Regex.Match(node.InnerHtml, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value,
                                count = _count
                            });
                        }
                    }
                }
            }

            //get normal statistics
            IEnumerable<HtmlNode> tableNodes = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "table");
            foreach (HtmlNode table in tableNodes)
            {
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    HtmlNodeCollection cell = row.SelectNodes("td");
                    if (cell != null)
                    {
                        if (cell.Count == 6) //normal
                        {
                            _normal_statistics.Add(new normal_statistics
                            {
                                type = cell[0].InnerText,
                                total_matches = cell[1].InnerText,
                                win_rate = cell[2].InnerText,
                                matches_winned = cell[3].InnerText,
                                matches_lost = cell[4].InnerText,
                                update_datetime = cell[5].InnerText
                            });
                        }
                        if (cell.Count == 8) //current season
                        {
                            _rank_statistics.Add(new rank_statistics
                            {
                                type = cell[0].InnerText,
                                rank = rank,
                                point = points,
                                total_matches = cell[3].InnerText,
                                win_rate = cell[4].InnerText,
                                matches_winned = cell[5].InnerText,
                                matches_lost = cell[6].InnerText,
                                update_datetime = cell[7].InnerText
                            });
                        }
                        if (cell.Count == 7) //history
                        {
                            _rank_statistics.Add(new rank_statistics
                            {
                                type = cell[0].InnerText,
                                rank = "",
                                point = "",
                                total_matches = cell[3].InnerText,
                                win_rate = cell[4].InnerText,
                                matches_winned = cell[5].InnerText,
                                matches_lost = cell[6].InnerText,
                                update_datetime = ""
                            });
                        }
                    }
                }
            }

            //bind com champ
            //bindGvPlayedChamps(server, playerId);

            //bind match list
            //bindGvMatchList(server, playerId);
            imgPlayerIcon.Source = icon(playerIcon);
            //gvPlayerProfile.DataBind();
            //gvNormalStat.DataSource = _normal_statistics;
            //gvNormalStat.DataBind();
            //gvRankStat.DataSource = _rank_statistics;
            //gvRankStat.DataBind();
        }

        protected BitmapImage icon(string url)
        {
            var image = new BitmapImage();
            int BytesToRead = 100;

            WebRequest request = WebRequest.Create(new Uri(url, UriKind.Absolute));
            request.Timeout = -1;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            BinaryReader reader = new BinaryReader(responseStream);
            MemoryStream memoryStream = new MemoryStream();

            byte[] bytebuffer = new byte[BytesToRead];
            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

            while (bytesRead > 0)
            {
                memoryStream.Write(bytebuffer, 0, bytesRead);
                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
            }

            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);

            image.StreamSource = memoryStream;
            image.EndInit();
            return image;
        }
        //protected void bindGvPlayedChamps(string server, string playerId)
        //{
        //    //new comChamp
        //    string comHeroUrl = "http://lolbox.duowan.com/new/api/index.php?_do=personal/championslist&serverName=" + server + "&playerName=" + playerId;
        //    string json = "";
        //    using (WebClient wc = new WebClient())
        //    {
        //        json = wc.DownloadString(comHeroUrl);
        //    }
        //    if (!json.Contains("empty data"))
        //    {
        //        played_champs playedChamps = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<played_champs>(json);

        //        for (int i = 0; i < playedChamps.content.Count; i++)
        //        {
        //            string averageKDAString = string.Join("/", playedChamps.content[i].averageKDA);
        //            string averageDamageString = playedChamps.content[i].averageDamage.First();
        //            string averageEarnString = playedChamps.content[i].averageEarn.First();
        //            _played_champs_display.Add(new played_champs_display
        //            {
        //                icon = "http://img.lolbox.duowan.com/champions/" + playedChamps.content[i].championName + "_40x40.jpg",
        //                championName = playedChamps.content[i].championName,
        //                championNameCN = playedChamps.content[i].championNameCN,
        //                winRate = playedChamps.content[i].winRate + "%",
        //                matchStat = playedChamps.content[i].matchStat,
        //                averageKDA = averageKDAString,
        //                averageKDARating = playedChamps.content[i].averageKDARating,
        //                averageDamage = averageDamageString,
        //                averageEarn = averageEarnString,
        //                averageMinionsKilled = playedChamps.content[i].averageMinionsKilled,
        //                totalMVP = playedChamps.content[i].totalMVP,
        //                totalHope = playedChamps.content[i].totalHope
        //            });
        //        }
        //    }
        //    gvPlayedChamps.DataSource = _played_champs_display;
        //    gvPlayedChamps.DataBind();
        //}
        //private void bindGvMatchList(string server, string playerId)
        //{
        //    //get match list
        //    string matchListUrl = "http://lolbox.duowan.com/matchList.php?serverName=" + server + "&playerName=" + playerId;
        //    for (int i = 1; i <= 8; i++)
        //    {
        //        var getMatchHistoryWeb = new HtmlWeb();
        //        var doc = getMatchHistoryWeb.Load(matchListUrl + "&page=" + i.ToString());

        //        HtmlNode matchHistorybodyNode = doc.DocumentNode.SelectSingleNode("//body");

        //        //match_list
        //        IEnumerable<HtmlNode> historyNodes = doc.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes.Contains("class")
        //            && x.Attributes["class"].Value.Split().Contains("l-box"));
        //        foreach (HtmlNode child in historyNodes)
        //        {
        //            foreach (HtmlNode ul in child.SelectNodes("ul"))
        //            {
        //                foreach (HtmlNode li in ul.SelectNodes("li"))
        //                {
        //                    if (li.Attributes["id"] != null)
        //                    {
        //                        string id = li.Attributes["id"].Value.Substring(3);
        //                        HtmlNode championSpan = li.SelectSingleNode("span[@class='avatar']");
        //                        string icon = Regex.Match(championSpan.InnerHtml, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
        //                        string champion_name_ch = championSpan.SelectSingleNode("img").Attributes["title"].Value;
        //                        string status = li.SelectSingleNode("p").SelectSingleNode("em").InnerText;
        //                        HtmlNode modeNode = li.SelectSingleNode("p[@class='info']").SelectSingleNode("span[@class='game']");
        //                        string mode = modeNode.InnerText;
        //                        string date = Regex.Replace(li.SelectSingleNode("p[@class='info']").LastChild.InnerText, @"\s", "").Replace("&nbsp;", "");
        //                        _match_list.Add(new match_list
        //                        {
        //                            id = id,
        //                            playerId = _playerId,
        //                            icon = icon,
        //                            champion_name_ch = champion_name_ch,
        //                            status = status,
        //                            mode = mode,
        //                            date = date
        //                        });
        //                    }
        //                    else
        //                    {
        //                        lblMsg2.Text = "由于未知原因，此场比赛消失在遥远的二次元空间中";
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    gvMatchList.DataSource = _match_list;
        //    gvMatchList.DataBind();
        //}
        public class player_profile
        {
            public string player_id { get; set; }
            public string server { get; set; }
            public string icon { get; set; }
            public string level { get; set; }
            public string fighting { get; set; }
            public string first_win { get; set; }
        }
        public class normal_statistics
        {
            public string type { get; set; }
            public string total_matches { get; set; }
            public string win_rate { get; set; }
            public string matches_winned { get; set; }
            public string matches_lost { get; set; }
            public string update_datetime { get; set; }
        }
        public class rank_statistics
        {
            public string type { get; set; }
            public string rank { get; set; }
            public string point { get; set; }
            public string total_matches { get; set; }
            public string win_rate { get; set; }
            public string matches_winned { get; set; }
            public string matches_lost { get; set; }
            public string update_datetime { get; set; }
        }
        public class com_champ
        {
            public string champion_name_ch { get; set; }
            public string champion_name { get; set; }
            public string icon { get; set; }
            public string count { get; set; }
        }
        public class played_champs_display
        {
            public string icon { get; set; }
            public string championName { get; set; }
            public string championNameCN { get; set; }
            public string winRate { get; set; }
            public string matchStat { get; set; }
            public string averageKDA { get; set; }
            public string averageKDARating { get; set; }
            public string averageDamage { get; set; }
            public string averageEarn { get; set; }
            public string averageMinionsKilled { get; set; }
            public string totalMVP { get; set; }
            public string totalHope { get; set; }
        }
        public class played_champs
        {
            public string snFull { get; set; }
            public string championsNum { get; set; }
            public List<played_champs_content> content { get; set; }
        }
        public class played_champs_content
        {
            public string championName { get; set; }
            public string championNameCN { get; set; }
            public string winRate { get; set; }
            public string matchStat { get; set; }
            public string[] averageKDA { get; set; }
            public string averageKDARating { get; set; }
            public string[] averageDamage { get; set; }
            public string[] averageEarn { get; set; }
            public string averageMinionsKilled { get; set; }
            public string totalMVP { get; set; }
            public string totalHope { get; set; }
        }
        public class match_list
        {
            public string id { get; set; }
            public string playerId { get; set; }
            public string champion_name_ch { get; set; }
            public string icon { get; set; }
            public string status { get; set; }
            public string mode { get; set; }
            public string date { get; set; }
        }
        public class matchHeader
        {
            public string mode { get; set; }
            public string duration { get; set; }
            public string endTime { get; set; }
            public string kills { get; set; }
            public string gold { get; set; }
        }
        public class matchDetails
        {
            public string matchId { get; set; }
            public string playerId { get; set; }
            public string gold { get; set; }
            public string KDA { get; set; }
        }
        public class honour
        {
            public string playerId { get; set; }
            public string matchId { get; set; }
            public string honourDesc { get; set; }
        }
        public class items
        {
            public string playerId { get; set; }
            public string matchId { get; set; }
            public string itemIcon1 { get; set; }
            public string itemName1 { get; set; }
            public string itemDesc1 { get; set; }
            public string itemIcon2 { get; set; }
            public string itemName2 { get; set; }
            public string itemDesc2 { get; set; }
            public string itemIcon3 { get; set; }
            public string itemName3 { get; set; }
            public string itemDesc3 { get; set; }
            public string itemIcon4 { get; set; }
            public string itemName4 { get; set; }
            public string itemDesc4 { get; set; }
            public string itemIcon5 { get; set; }
            public string itemName5 { get; set; }
            public string itemDesc5 { get; set; }
            public string itemIcon6 { get; set; }
            public string itemName6 { get; set; }
            public string itemDesc6 { get; set; }
            public string itemIcon7 { get; set; }
            public string itemName7 { get; set; }
            public string itemDesc7 { get; set; }
        }
        public class playerMatchDetails
        {
            public string matchId { get; set; }
            public string playerId { get; set; }
            public string champion_name { get; set; }
            public string champion_name_ch { get; set; }
            public string champIcon { get; set; }
            public string firstSpellIcon { get; set; }
            public string secondSpellIcon { get; set; }
            public string warScore { get; set; }
            public string lastHits { get; set; }
            public string creeps { get; set; }
            public string towersDestroyed { get; set; }
            public string barracksDestroyed { get; set; }
            public string wards { get; set; }
            public string dewards { get; set; }
            public string maxContKills { get; set; }
            public string maxMultiKills { get; set; }
            public string maxCrit { get; set; }
            public string totalHeal { get; set; }
            public string totalDmg { get; set; }
            public string totalTank { get; set; }
            public string totalHeroDmg { get; set; }
            public string totalHeroPhyDmg { get; set; }
            public string totalHeroMagicDmg { get; set; }
            public string totalHeroTrueDmg { get; set; }
        }
    }
}
