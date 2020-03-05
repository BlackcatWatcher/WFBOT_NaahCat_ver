﻿using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Web;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using Settings;
using TextCommandCore;
using Number = System.Numerics.BigInteger;
using static TRKS.WF.QQBot.Messenger;
using System.Text.RegularExpressions;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMessageReceivedMahuaEvent1
        : IGroupMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        public static readonly WFNotificationHandler _WfNotificationHandler = new WFNotificationHandler();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            if (HotUpdateInfo.PreviousVersion) return;
            if (GroupCallDic.ContainsKey(context.FromGroup))
            {
                if (GroupCallDic[context.FromGroup] > Config.Instance.CallperMinute && Config.Instance.CallperMinute != 0) return;
            }
            else
            {
                GroupCallDic[context.FromGroup] = 0;
            }

            Task.Factory.StartNew(() =>
            {
                var message = HttpUtility.HtmlDecode(context.Message)?.ToLower();
                if (!message.StartsWith("/") && Config.Instance.IsSlashRequired) return;

                message = message.StartsWith("/") ? message.Substring(1) : message;

                var handler = new GroupMessageHandler(context.FromQq.ToHumanQQNumber(), context.FromGroup.ToGroupNumber(), message);
                var (matched, result) = handler.ProcessCommandInput();
                if (matched)
                {
                    IncreaseCallCounts(context.FromGroup);
                }

            }, TaskCreationOptions.LongRunning);
        }
    }

    public partial class GroupMessageHandler
    {
        [Matchers("金星赏金", "金星平原赏金", "福尔图娜赏金", "奥布山谷赏金")]
        void FortunaMissions(int index = 0)
        {
            _WFStatus.SendFortunaMissions(Group, index);
        }

        [Matchers("地球赏金", "地球平原赏金", "希图斯赏金")]
        void CetusMissions(int index = 0)
        {
            _WFStatus.SendCetusMissions(Group, index);
        }

        [Matchers("查询", "wm")]
        [CombineParams]
        void WM(string word)
        {
            var QR = false;
            var B = false;
            int rank = 0;
            int inventory = 0;
            string platform = "pc";
            bool definename = false;
            if (word.Contains("带回复"))
            {
                QR = true;
                word = word.Replace("带回复", "");
            }
            if (word.Contains("自定义名字"))
            {
                definename = true;
                word = word.Replace("自定义名字", "");
            }
            if (word.Contains("买家"))
            {
                B = true;
                word = word.Replace("买家", "");
            }
            if (word.Contains("平台"))
            {
                Match matchTemp = Regex.Match(word, @"平台-(.+?)-");
                platform = matchTemp.Value;
                word = word.Replace(platform, "");
            }
            if (word.Contains("等级"))
            {
                Match matchTemp = Regex.Match(word, @"等级-(.+?)-");
                Match matchTempNum = Regex.Match(word, @"等级-([0-9]+?)-");
                if (matchTempNum.Value.Length != 0)
                {
                    string rstats = matchTempNum.Value.Replace("等级", "").Replace("-", "");
                    rank = Convert.ToInt32(rstats);
                }
                else
                {
                    rank = 0;
                }
                word = word.Replace(matchTemp.Value, "");
            }
            if (word.Contains("数量"))
            {
                Match matchTemp = Regex.Match(word, @"数量-(.+?)-");
                Match matchTempNum = Regex.Match(word, @"数量-([0-9]+?)-");
                if (matchTempNum.Value.Length != 0)
                {
                    string rstats = matchTempNum.Value.Replace("数量", "").Replace("-", "");
                    inventory = Convert.ToInt32(rstats);
                }
                else
                {
                    inventory = 0;
                }
                word = word.Replace(matchTemp.Value, "");
            }
            // 小屎山
            _wmSearcher.SendWMInfo(word.Format(), Group, QR, B, rank, inventory, platform, definename);
        }

        [Matchers("紫卡", "rm")]
        [CombineParams]
        void RivenMarket(string word)
        {
            int limit = Config.Instance.RMSearchCount;
            string onlinefirst = "true";
            string weapon = "Any";
            string stats = "Any";
            string neg = "all";
            string platform = "PC";
            string recency = "-1";
            int page = 1;
            string reroll = "-1";
            var QR = false;
            bool definename = false;
            if (word.Contains("带回复"))
            {
                QR = true;
                word = word.Replace("带回复", "");
            }
            if (word.Contains("无在线优先"))
            {
                onlinefirst = "false";
                word = word.Replace("无在线优先", "");
            }
            if (word.Contains("零洗"))
            {
                reroll = "0";
                word = word.Replace("零洗", "");
            }
            if (word.Contains("低洗"))
            {
                reroll = "8";
                word = word.Replace("低洗", "");
            }
            if (word.Contains("自定义名字"))
            {
                definename = true;
                word = word.Replace("自定义名字", "");
            }
            if (word.Contains("平台"))
            {
                Match matchTemp = Regex.Match(word, @"平台-(.+?)-");
                platform = matchTemp.Value;
                word = word.Replace(platform, "");
            }
            if (word.Contains("小于"))
            {
                Match matchTemp = Regex.Match(word, @"小于-(.+?)-");
                recency = matchTemp.Value;
                word = word.Replace(recency, "");
            }
            if (word.Contains("正面"))
            {
                Match matchTemp = Regex.Match(word, @"正面-(.+?)-");
                stats = matchTemp.Value;
                word = word.Replace(stats, "");
            }
            if (word.Contains("负面"))
            {
                Match matchTemp = Regex.Match(word, @"负面-(.+?)-");
                neg = matchTemp.Value;
                word = word.Replace(neg, "");
            }
            if (word.Contains("页码"))
            {
                Match matchTemp = Regex.Match(word, @"页码-(.+?)-");
                Match matchTempNum = Regex.Match(word, @"页码-([0-9]+?)-");
                if (matchTempNum.Value.Length != 0)
                {
                    string[] rstats = matchTempNum.Value.Replace("页码", "").Replace("-", "").Split('+');
                    foreach (string i in rstats)
                    {
                        page = Convert.ToInt32(i);
                    }
                }
                else
                {
                    page = 1;
                }
                word = word.Replace(matchTemp.Value, "");
            }
            word = word.Format();
            weapon = word;
            _rmkSearcher.SendRivenInfos(Group, QR, weapon, limit, onlinefirst, stats, neg, page, platform, recency, reroll, definename);
        }

        [Matchers("国内紫卡", "wfa")]
        [CombineParams]
        void Riven(string word)
        {
            word = word.Format();
            _rmSearcher.SendRivenInfos(Group, word);
        }

        [Matchers("翻译")]
        [CombineParams]
        void Translate(string word)
        {
            word = word.Format();
            _WFStatus.SendTranslateResult(Group, word);
        }

        [Matchers("遗物")]
        [CombineParams]
        void RelicInfo(string word)
        {
            word = word.Format();
            _WFStatus.SendRelicInfo(Group, word);
        }

        [Matchers("警报")]
        void Alerts()
        {
            WfNotificationHandler.SendAllAlerts(Group);
        }

        [Matchers("平野", "夜灵平野", "平原", "夜灵平原", "金星平原", "奥布山谷", "金星平原温度", "平原温度", "平原时间")]
        void Cycles()
        {
            _WFStatus.SendCycles(Group);
        }

        [Matchers("入侵")]
        void Invasions()
        {
            WfNotificationHandler.SendAllInvasions(Group);
        }

        [Matchers("突击")]
        void Sortie()
        {
            _WFStatus.SendSortie(Group);
        }

        [Matchers("奸商", "虚空商人", "商人")]
        void VoidTrader()
        {
            _WFStatus.SendVoidTrader(Group);
        }

        [Matchers("活动", "事件")]
        void Events()
        {
            _WFStatus.SendEvent(Group);
        }


        [Matchers("裂隙", "裂缝")]
        void Fissures()
        {
            _WFStatus.SendFissures(Group);
        }

        [Matchers("小小黑", "追随者")]
        void AllPersistentEnemies()
        {
            WfNotificationHandler.SendAllPersistentEnemies(Group);
        }

        [Matchers("help", "帮助", "功能", "救命")]
        void HelpDoc()
        {
            SendHelpdoc(Group);
        }

        [DoNotMeasureTime]
        [Matchers("status", "状态", "机器人状态", "机器人信息", "我需要机器人")]
        void Status()
        {
            SendBotStatus(Group);
        }

        [Matchers("午夜电波", "电波", "每日任务", "每周任务", "每日任务", "每周挑战")]
        void NightWave()
        {
            _WFStatus.SendNightWave(Group);
        }

        [Matchers("wiki")]
        [CombineParams]
        string Wiki(string word = "wiki")
        {
            return _wikiSearcher.SendSearch(word).Replace("'", "%27");
            // 这简直就是官方吞mod最形象的解释
        }

        [Matchers("仲裁", "仲裁警报", "精英警报")]
        void Arbitration()
        {
            _WFStatus.SendArbitrationMission(Group);
        }

        [Matchers("赤毒", "赤毒虹吸器", "赤毒洪潮", "赤毒任务")]
        void Kuva()
        {
            _WFStatus.SendKuvaMissions(Group);
        }
    }

    public partial class GroupMessageHandler : ICommandHandler<GroupMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; }
        public Action<Message> ErrorMessageSender { get; }

        public HumanQQNumber Sender { get; }
        public string Message { get; }
        public GroupNumber Group { get; }

        internal static WFNotificationHandler WfNotificationHandler =>
            GroupMessageReceivedMahuaEvent1._WfNotificationHandler;

        string ICommandHandler<GroupMessageHandler>.Sender => Group.QQ;

        private static readonly WFStatus _WFStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();
        private static readonly RMKSearcher _rmkSearcher = new RMKSearcher();
        private static readonly WikiSearcher _wikiSearcher = new WikiSearcher();

        public GroupMessageHandler(HumanQQNumber sender, GroupNumber group, string message)
        {
            _ = InitEvent1.localVersion;
            Sender = sender;
            MessageSender = (id, msg) =>
            {
                SendGroup(id.ID.ToGroupNumber(), msg);
                Trace.WriteLine($"Message Processed: Group [{Group}], QQ [{Sender}], Message Content [{message}], Result [{msg.Content}].", "Message");

            };
            Group = group;
            Message = message;

            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
