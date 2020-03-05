using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GammaLibrary.Extensions;
using Settings;

namespace TRKS.WF.QQBot
{
    public class RMSearcher
    {
        private Timer timer = new Timer(TimeSpan.FromHours(2).TotalMilliseconds);
        private WFTranslator translator => WFResource.WFTranslator;

        private bool isWFA = !Config.Instance.ClientId.IsNullOrWhiteSpace() &&
                             !Config.Instance.ClientSecret.IsNullOrWhiteSpace();

        private string platfrom = Config.Instance.Platform.ToString();

        public RMSearcher()
        {
            UpdateAccessToken();
            timer.Elapsed += (s, e) => UpdateAccessToken();
            timer.Start();
        }

        public string GetAccessToken()
        {
            var body = $"client_id={Config.Instance.ClientId}&client_secret={Config.Instance.ClientSecret}&grant_type=client_credentials";
            var header = new WebHeaderCollection
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var accessToken = WebHelper.UploadJson<AccessToken>("https://api.richasy.cn/connect/token", body, header).access_token;

            Config.Instance.Last_update = DateTime.Now;
            Config.Save();

            return accessToken;
        }

        public void UpdateAccessToken()
        {
            if (isWFA && DateTime.Now - Config.Instance.Last_update > TimeSpan.FromDays(7))
            {
                Config.Instance.AcessToken = GetAccessToken();
                Config.Save();
            }
        }

        public List<RivenInfo> GetRivenInfos(string weapon)
        {
            var header = new WebHeaderCollection();
            var count = Config.Instance.WFASearchCount;
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            header.Add("Platform", platform);
            header.Add("Weapon", weapon.ToBase64());
            return WebHelper.DownloadJson<List<RivenInfo>>($"https://api.richasy.cn/wfa/rm/riven", header).Where(info => info.isSell == 1).Take(count).ToList(); // 操 云之幻好蠢 为什么不能在请求里限制是买还是卖
        }

        public void SendRivenInfos(GroupNumber group, string weapon)
        {
            var sb = new StringBuilder();
            try
            {
                if (isWFA)
                {
                    if (translator.ContainsWeapon(weapon))
                    {
                        Messenger.SendGroup(group, "国内的紫卡呀,我给你找找看.");
                        var info = GetRivenInfos(weapon);
                        var msg = info.Any() ? WFFormatter.ToString(info) : $"哇, WFA紫卡市场果然没有出售: {weapon} 紫卡的用户.".AddRemainCallCount(group);

                        sb.AppendLine(msg.AddPlatformInfo());
                    }
                    else
                    {
                        sb.AppendLine($"咱可不认识什么 {weapon} .");
                        var similarlist = translator.GetSimilarItem(weapon, "rm");
                        if (similarlist.Any())
                        {
                            sb.AppendLine("你要找的是不是这个?");
                            foreach (var item in similarlist)
                            {
                                sb.AppendLine($"    {item}");
                            }
                        }

                    }
                }
                else
                {
                    sb.AppendLine("本兔还没有 WFA 授权, 暂时没法使用.");
                }
            }
            catch (WebException)
            {
                sb.AppendLine("经过兔子多次尝试, 还是无法访问紫卡市场. 如果你不能谅解, 有本事顺着网线来打我呀.");
            }
            Messenger.SendGroup(group, sb.ToString().Trim());
        }
    }
}
