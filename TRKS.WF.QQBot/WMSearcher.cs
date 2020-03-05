using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Settings;

namespace TRKS.WF.QQBot
{
    public class WMSearcher
    {
        private WFTranslator translator => WFResource.WFTranslator;
        private bool isWFA = !string.IsNullOrEmpty(Config.Instance.ClientId) &&
                             !string.IsNullOrEmpty(Config.Instance.ClientSecret);

        private string platform => Config.Instance.Platform.ToString();
        public WMInfo GetWMInfo(string searchword)
        {
            var header = new WebHeaderCollection();
            var platform = Config.Instance.Platform.GetSymbols().First();
            header.Add("platform", platform);
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            var info = WebHelper.DownloadJson<WMInfo>($"https://api.warframe.market/v1/items/{searchword}/orders?include=item", header);
            return info;
        }

        public WMInfoEx GetWMINfoEx(string searchword)
        {
            var header = new WebHeaderCollection();
            var platform = Config.Instance.Platform.GetSymbols().First();
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            var info = WebHelper.DownloadJson<WMInfoEx>($"https://api.richasy.cn/wfa/basic/{platform}/wm/{searchword}", header);
            return info;
        }

        public void OrderWMInfo(WMInfo info, bool isbuyer, int rank, int inventory, string platform)
        {
            info.payload.orders = (isbuyer ? info.payload.orders
            .Where(order => order.order_type == (isbuyer ? "buy" : "sell"))
            .Where(order => order.user.status == "online" || order.user.status == "ingame")
            .Where(order => order.mod_rank >= rank)
            .Where(order => order.platform == platform)
            .Where(order => order.quantity >= inventory)
            .OrderByDescending(order => order.platinum)
            : info.payload.orders
            .Where(order => order.order_type == (isbuyer ? "buy" : "sell"))
            .Where(order => order.user.status == "online" || order.user.status == "ingame")
            .Where(order => order.mod_rank >= rank)
            .Where(order => order.platform == platform)
            .Where(order => order.quantity >= inventory)
            .OrderBy(order => order.platinum))
            .Take(Config.Instance.WMSearchCount)
            .ToArray();

        }

        public void OrderWMInfoEx(WMInfoEx info, bool isbuyer)
        {
            info.orders = (isbuyer ? info.orders
                .Where(order => order.order_Type == (isbuyer ? "buy" : "sell"))
                .Where(order => order.status == "online" || order.status == "ingame") 
                .OrderByDescending(order => order.platinum)
                : info.orders
                .Where(order => order.order_Type == (isbuyer ? "buy" : "sell"))
                .Where(order => order.status == "online" || order.status == "ingame")
                .OrderBy(order => order.platinum))
                .Take(Config.Instance.WMSearchCount)
                .ToArray();


        }

        public void SendWMInfo(string item, GroupNumber group, bool quickReply, bool isbuyer, int rank, int inventory, string platform, bool definename)
        {
            if (!definename)
            {
                // 下面 你将要 看到的 是 本项目 最大的  粪山
                // Actually 这粪山挺好用的
                var words = new List<string> { "prime", "p", "甲" };
                var heads = new List<string> { "头部神经光", "头部神经", "头部神", "头部", "头" };
                foreach (var word in words)
                {
                    foreach (var head in heads)
                    {
                        if (!item.Contains("头部神经光元"))
                        {
                            if (item.Contains(word + head))
                            {
                                item = item.Replace(word + head, word + "头部神经光元");
                                break;
                            }
                        }
                    }
                }
                var searchword = translator.TranslateSearchWord(item);
                var formateditem = item;
                if (item == searchword)
                {
                    searchword = translator.TranslateSearchWord(item + "一套");
                    formateditem = item + "一套";
                    if (formateditem == searchword)
                    {
                        searchword = translator.TranslateSearchWord(item.Replace("p", "prime").Replace("总图", "蓝图"));
                        formateditem = item.Replace("p", "prime").Replace("总图", "蓝图");
                        if (formateditem == searchword)
                        {
                            searchword = translator.TranslateSearchWord(item.Replace("p", "prime") + "一套");
                            formateditem = item.Replace("p", "prime") + "一套";
                            if (formateditem == searchword)
                            {
                                var sb = new StringBuilder();
                                var similarlist = translator.GetSimilarItem(item.Format(), "wm");
                                sb.AppendLine($"你找的 {item} 是什么呀！");
                                if (similarlist.Any())
                                {
                                    sb.AppendLine($"你要找的是不是这些?确定了再告诉我.");
                                    foreach (var similarresult in similarlist)
                                    {
                                        sb.AppendLine($"    {similarresult}");
                                    }
                                }


                                sb.AppendLine("ps: 咱是查 WarframeMarket 上面的物品的, 不是其他什么东西.");
                                Messenger.SendGroup(group, sb.ToString().Trim().AddRemainCallCount(group));
                                return;
                            }
                        }
                    }
                }

                var msg = string.Empty;
                Messenger.SendGroup(group, "稍等稍等,本帝在找着呢.");

                var failed = false;
                if (Config.Instance.IsThirdPartyWM)
                {
                    try
                    {
                        if (isWFA)
                        {
                            var infoEx = GetWMINfoEx(searchword);
                            if (infoEx.orders.Any())
                            {
                                OrderWMInfoEx(infoEx, isbuyer);
                                translator.TranslateWMOrderEx(infoEx, searchword);
                                msg = WFFormatter.ToString(infoEx, quickReply, isbuyer);
                            }
                            else
                            {
                                msg = $"哇, WarframeMarket 上居然还没有卖 {item} 的仓鼠";
                            }
                        }
                        else
                        {
                            msg = "咱和 WFA 还没有授权关系, 查不了.";
                        }
                    }
                    catch (Exception)
                    {
                        Messenger.SendGroup(group, "在使用第三方 API 时遇到了网络问题. 正在为您转官方 API.");
                        failed = true;
                    }
                }

                if (!Config.Instance.IsThirdPartyWM || failed)
                {
                    //数据处理
                    switch (platform)
                    {
                        case "NS":
                            platform = "switch";
                            break;
                        case "PS4":
                            platform = "ps4";
                            break;
                        case "XBOX":
                            platform = "xbox";
                            break;
                        case "ns":
                            platform = "switch";
                            break;
                        case "ps4":
                            platform = "ps4";
                            break;
                        case "xbox":
                            platform = "xbox";
                            break;
                        default:
                            platform = "pc";
                            break;
                    }
                    if (rank > 10) { rank = 10; }
                    if (rank < 0) { rank = 0; }
                    if (inventory < 0) { inventory = 0; }
                    var info = GetWMInfo(searchword);
                    if (info.payload.orders.Any())
                    {
                        OrderWMInfo(info, isbuyer, rank, inventory, platform);
                        translator.TranslateWMOrder(info, searchword);
                        msg = WFFormatter.ToString(info, quickReply, isbuyer);
                    }
                    else
                    {
                        msg = $"哇, WarframeMarket 上居然还没有卖{inventory}件以上{rank}级 {item} 的仓鼠";
                    }

                }

                Messenger.SendGroup(group, msg.AddPlatformInfowm().AddRemainCallCount(group));
            }
            else
            {
                var msg = string.Empty;
                Messenger.SendGroup(group, "稍等稍等,本帝在找着呢.");
                var searchword = item;
                switch (platform)
                {
                    case "NS":
                        platform = "switch";
                        break;
                    case "PS4":
                        platform = "ps4";
                        break;
                    case "XBOX":
                        platform = "xbox";
                        break;
                    case "ns":
                        platform = "switch";
                        break;
                    case "ps4":
                        platform = "ps4";
                        break;
                    case "xbox":
                        platform = "xbox";
                        break;
                    default:
                        platform = "pc";
                        break;
                }
                if (rank > 10) { rank = 10; }
                if (rank < 0) { rank = 0; }
                if (inventory < 0) { inventory = 0; }
                var info = GetWMInfo(searchword);
                if (info.payload.orders.Any())
                {
                    OrderWMInfo(info, isbuyer, rank, inventory, platform);
                    //translator.TranslateWMOrder(info, searchword);
                    msg = WFFormatter.ToStringde(info, quickReply, isbuyer, item);
                }
                else
                {
                    msg = $"哇, WarframeMarket 上居然还没有卖{inventory}件以上{rank}级 {item} 的仓鼠";
                }
                Messenger.SendGroup(group, msg.AddPlatformInfowm().AddRemainCallCount(group));
        }
        }
           
    }
}
