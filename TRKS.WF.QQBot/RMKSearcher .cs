using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Settings;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Channels;
using System.Security.Permissions;
using System.Timers;

namespace TRKS.WF.QQBot
{
    public class RMKSearcher
    {
        private static WFChineseAPI api => WFResource.WFChineseApi;
        private static WFTranslator translator => WFResource.WFTranslator;
        private string platform => Config.Instance.Platform.ToString();
        public static string rivenMarketUrlCreate(int limit, string onlinefirst, string weapon, string stats, string neg, int page, string platform, string recency, string reroll)
        {
            return "https://riven.market/_modules/riven/showrivens.php?baseurl=Lw==" +
                "&platform=" + platform +
                "&limit=" + limit +
                "&recency=" + recency +
                "&veiled=false" +
                "&onlinefirst=" + onlinefirst +
                "&polarity=all" +
                "&rank=all" +
                "&mastery=16" +
                "&weapon=" + weapon +
                "&stats=" + stats +
                "&neg=" + neg +
                "&price=99999" +
                "&rerolls=" + reroll +
                "&sort=price" +
                "&direction=ASC" +
                "&page=" + page +
                "&time=" + Convert.ToInt64((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000).ToString();
        }
        public static string getHtml(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";// POST OR GET， 如果是GET, 则没有第二步传参，直接第三步，获取服务端返回的数据
            req.AllowAutoRedirect = false;//服务端重定向。一般设置false
            req.ContentType = "text/html; charset=UTF-8";//数据一般设置这个值，除非是文件上传
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            string html = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            return html;
        }
        public static string[,] GetRivens(string urltextdata, int limit)
        {
            string[,] data_temp = new string[limit, 18];
            List<RivenInfoRm> Rmrivens = new List<RivenInfoRm>();
            Match norivens = Regex.Match(urltextdata, @"norivens");
            if (norivens.Length != 0)
            {
                data_temp = new string[0, 0];
                return data_temp;
            }
            else
            {
                MatchCollection data_Weapon = Regex.Matches(urltextdata, @"data-weapon=""(.+)""");
                MatchCollection data_Name = Regex.Matches(urltextdata, @"data-name=""(.+)""");
                MatchCollection data_Price = Regex.Matches(urltextdata, @"data-price=""(.+)""");
                MatchCollection data_Rank = Regex.Matches(urltextdata, @"data-rank=""(.+)""");
                MatchCollection data_Mr = Regex.Matches(urltextdata, @"data-mr=""(.+)""");
                MatchCollection data_Age = Regex.Matches(urltextdata, @"data-age=""(.+)""");
                MatchCollection data_Polarity = Regex.Matches(urltextdata, @"data-polarity=""(.+)""");
                MatchCollection data_Rerolls = Regex.Matches(urltextdata, @"data-rerolls=""(.+)""");
                MatchCollection data_Stat1 = Regex.Matches(urltextdata, @"data-stat1=""(.+)""");
                MatchCollection data_Stat1val = Regex.Matches(urltextdata, @"data-stat1val=""(.+)""");
                MatchCollection data_Stat2 = Regex.Matches(urltextdata, @"data-stat2=""(.+)""");
                MatchCollection data_Stat2val = Regex.Matches(urltextdata, @"data-stat2val=""(.+)""");
                MatchCollection data_Stat3 = Regex.Matches(urltextdata, @"data-stat3=""(.+)""");
                MatchCollection data_Stat3val = Regex.Matches(urltextdata, @"data-stat3val=""(.+)""");
                MatchCollection data_Stat4 = Regex.Matches(urltextdata, @"data-stat4=""(.+)""");
                MatchCollection data_Stat4val = Regex.Matches(urltextdata, @"data-stat4val=""(.+)""");
                MatchCollection data_Seller = Regex.Matches(urltextdata, @"seller "">\r\n +(.+)");
                MatchCollection data_Online = Regex.Matches(urltextdata, @"attribute (online.+)"" ");
                int i = 0;
                foreach (Match match in data_Weapon)
                {
                    data_temp[i, 0] = "\n[" + match.Value.Substring(13).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Name)
                {
                    data_temp[i, 1] = match.Value.Substring(11).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Price)
                {
                    data_temp[i, 2] = "- " + match.Value.Substring(12).Replace("\"", "") + " 白金]\n";
                    i++;
                }
                i = 0;
                foreach (Match match in data_Rank)
                {
                    data_temp[i, 3] = "等级:" + match.Value.Substring(11).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Mr)
                {
                    data_temp[i, 4] = "段位:" + match.Value.Substring(9).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Age)
                {
                    data_temp[i, 5] = match.Value.Substring(10).Replace("\"", "").Replace("new", "[新上架]").Replace("&gt; 1 day", "[上架>1天]").Replace("&gt; 1 week", "[上架>1周]").Replace("&gt; 1 month", "[上架>1月]").Replace("&gt; 1 year", "[上架>1年]");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Polarity)
                {
                    data_temp[i, 6] = match.Value.Substring(15).Replace("\"", "").Replace("madurai", "r槽").Replace("naramon", "-槽").Replace("vazarin", "D槽");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Rerolls)
                {
                    data_temp[i, 7] = "洗卡:" + match.Value.Substring(14).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat1)
                {
                    data_temp[i, 8] = match.Value.Substring(12).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat1val)
                {
                    data_temp[i, 9] = match.Value.Substring(15).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat2)
                {
                    data_temp[i, 10] = match.Value.Substring(12).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat2val)
                {
                    data_temp[i, 11] = match.Value.Substring(15).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat3)
                {
                    data_temp[i, 12] = match.Value.Substring(12).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat3val)
                {
                    data_temp[i, 13] = match.Value.Substring(15).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat4)
                {
                    data_temp[i, 14] = match.Value.Substring(12).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Stat4val)
                {
                    data_temp[i, 15] = match.Value.Substring(15).Replace("\"", "");
                    i++;
                }
                i = 0;
                foreach (Match match in data_Seller)
                {
                    data_temp[i, 16] = "\n卖家:[" + match.Value.Substring(11).Replace("</div>", "").Trim() + "]";
                    i++;
                }
                i = 0;
                foreach (Match match in data_Online)
                {
                    data_temp[i, 17] = "- " + match.Value.Substring(10).Replace("\"", "").Trim().Replace("online ingame", "游戏中").Replace("online offline", "离线").Replace("online", "网页在线");
                    i++;
                }
                return data_temp;
            }
            
        }
        public void SendRivenInfos(GroupNumber group, bool quickReply, string weapon, int limit, string onlinefirst, string stats, string neg, int page, string platform, string recency, string reroll, bool definename)
        {
            var sb = new StringBuilder();
            try
            {
                if (translator.ContainsWeapon(weapon) || weapon == "任意" || definename == true)
                {
                    //数据处理
                    Messenger.SendGroup(group, "俗话说:一卡在手天下我有,这紫卡的生意可不能含糊,咱要花时间好好筛选.");
                    if (platform != "PC")
                    {
                        platform = platform.Replace("平台", "").Replace("-", "");
                        if (platform == "NS1" || platform == "ns1")
                        {
                            platform = "NS1";
                        }
                        else if (platform == "PS4" || platform == "ps4")
                        {
                            platform = "PS4";
                        }
                        else if (platform == "NSW" || platform == "nsw")
                        {
                            platform = "NSW";
                        }
                        else if(platform == "PC" || platform == "pc")
                        {
                            platform = "PC";
                        }
                        else
                        {
                            platform = "PC";
                            sb.AppendLine("平台只有：PC、PS4、NS1、NSW,给你改回PC了.");
                        }
                    }
                    if (recency != "-1")
                    {
                        recency = recency.Replace("小于", "").Replace("-", "");
                        if (recency == "1天" || recency == "1")
                        {
                            recency = "1";
                        }
                        else if (recency == "1周" || recency == "7")
                        {
                            recency = "7";
                        }
                        else if (recency == "1月" || recency == "30")
                        {
                            recency = "30";
                        }
                        else
                        {
                            recency = "-1";
                            sb.AppendLine("上架时间只有小于：1天、1周、1月,给你改回不限了.");
                        }
                    }
                    string weaponen = "Any";
                    if (definename == true)
                    {
                        weaponen = weapon;
                    }
                    else if (weapon != "任意")
                    {
                        weaponen = translator.TranslateWeapon(weapon).Replace("&", "%26");
                    }
                    if (stats != "Any")
                    {
                        string[] rstats = stats.Replace("正面", "").Replace("-", "").Split('+');
                        string wtemp = "";
                        foreach (string i in rstats)
                        {
                            string trans = translator.TranslateAbrPCntoEn(i);
                            if (trans != "Any")
                            {
                                wtemp = wtemp + trans + ",";
                            }
                            else
                            {
                                sb.AppendLine("词条" + i + "输错了吧,本帝删了.");
                            }
                        }
                        if (wtemp.Length == 0)
                        {
                            stats = "Any";
                        }
                        else
                        {
                            stats = wtemp.Substring(0, wtemp.Length - 1);
                        }
                    }                   
                    if (neg != "all")
                    {
                        neg = neg.Replace("负面", "").Replace("-", "");
                        string trans = translator.TranslateAbrPCntoEn(neg);
                        if (trans.Length == 0)
                        {
                            neg = "Any";
                            sb.AppendLine("负面" + neg + "输错了吧,本帝帮你改成有负面了.");
                        }
                        else
                        {
                            neg = trans;
                        }
                    }

                    //消息队列
                    string htmlurl = rivenMarketUrlCreate(limit, onlinefirst, weaponen, stats, neg, page, platform, recency, reroll);
                    Trace.WriteLine($"查紫卡链接：" + htmlurl, "rmriven");
                    string urltext = getHtml(htmlurl);
                    string[,] Rivensinfo = GetRivens(urltext,limit);
                    if (Rivensinfo.Length == 0)
                    {
                        sb.AppendLine("没有找到这种紫卡诶，是参数输错了呢?还是没人在卖呢..");
                        if (definename == true)
                        {
                            sb.AppendLine("注意：自定义名字要英文，需要把空格换成“_”来代替，如果查不出，先检查英文名是不是对，试试首字母大写，如果有“_”，那后面首字母也大写，再查不出就是因为紫卡太新网站没有更新。");
                        }
                    }
                    else
                    {
                        string msgstr = "";
                        for (int i = 0; i < limit; i++)
                        {
                            if (Rivensinfo[i, 0] == null)
                            {
                                msgstr = msgstr + "\n没有更多紫卡了诶,或许可以换个参数..";
                                break;
                            }
                            string[] temp = { "", "", "", ""};
                            for (int j = 0; j < 18; j++)
                            {
                                if (j == 8)
                                {
                                    temp = translator.TranslateAbrP(Rivensinfo[i, j]);
                                    Rivensinfo[i, j] = "\n正面:" + temp[0] + temp[1];
                                }
                                if (j == 9)
                                {
                                    Rivensinfo[i, j] = Rivensinfo[i, j] + temp[2];
                                }
                                if (j == 10)
                                {
                                    temp = translator.TranslateAbrP(Rivensinfo[i, j]);
                                    Rivensinfo[i, j] = "| " + temp[0] + temp[1];
                                }
                                if (j == 11)
                                {
                                    Rivensinfo[i, j] = Rivensinfo[i, j] + temp[2];
                                }
                                if (j == 12 )
                                {
                                    if (Rivensinfo[i, j] != null && Rivensinfo[i, 13] != "0.0")
                                    {
                                        temp = translator.TranslateAbrP(Rivensinfo[i, j]);
                                        Rivensinfo[i, j] = "| " + temp[0] + temp[1];
                                    }
                                    else
                                    {
                                        j = 14;
                                    }
                                }
                                if (j == 13)
                                {
                                    Rivensinfo[i, j] = Rivensinfo[i, j] + temp[2];
                                }
                                if (j == 14)
                                {
                                    if (Rivensinfo[i, j] != null && Rivensinfo[i, 15] != "0.0")
                                    {
                                        temp = translator.TranslateAbrP(Rivensinfo[i, j]);
                                        Rivensinfo[i, j] = "\n负面:" + temp[3] + temp[1];
                                    }
                                    else
                                    {
                                        j = 16;
                                    }
                                }
                                if (j == 15)
                                {
                                    Rivensinfo[i, j] = Rivensinfo[i, j] + temp[2];
                                }
                                msgstr = msgstr + Rivensinfo[i, j] + " ";
                                if (j == 17 && quickReply == true && Rivensinfo[i, 16].Any() && Rivensinfo[i, 0].Any() && Rivensinfo[i, 1].Any() && Rivensinfo[i, 2].Any())
                                {
                                    msgstr = msgstr + "\n快捷回复：/w " + Rivensinfo[i, 16].Replace("\n卖家:[", "").Replace("]", "").Trim() + " Hey! I'd like to buy the " + Rivensinfo[i, 0].Replace("\n[","") + " " + Rivensinfo[i, 1] + " Riven that you sell on Riven.market for " + Rivensinfo[i, 2].Replace(" 白金]\n", "").Replace("- ", "").Trim() + " Platinum!";
                                }
                            }
                        }
                        sb.AppendLine(msgstr);
                    }
                    sb.AppendLine("这些紫卡是从Riven.Market找来哒！");
                }
                else
                {
                    sb.AppendLine($"咱可不认识这什么 {weapon} .");
                    var similarlist = translator.GetSimilarItem(weapon, "rm");
                    if (similarlist.Any())
                    {
                        sb.AppendLine("你要找的是不是这些?");
                        foreach (var item in similarlist)
                        {
                            sb.AppendLine($"    {item}");
                        }
                    }
                    sb.AppendLine("你可以用“带回复”、“无在线优先”、“零洗/低洗”“正面-属性1+属性2+属性3-”、“负面-属性4-”、“页码-数字-”、“平台-PC/PS4/NS1/NSW-”、“小于-1天/1周/1月-”来筛选紫卡噢！");
                    sb.AppendLine("如：紫卡 绝路 带回复 无在线优先 零洗 正面-基伤+多重- 负面-有- 页码-2- 平台-PS4- 小于-1天-");
                }
            }
            catch (WebException)
            {
                sb.AppendLine("防火墙结界太强了!咱无法访问紫卡市场,要不重新试一次?");
            }
            Messenger.SendGroup(group, sb.ToString().Trim());
        }
    }
}
