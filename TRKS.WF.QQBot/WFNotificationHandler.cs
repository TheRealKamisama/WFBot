﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace TRKS.WF.QQBot
{
    [Configuration("WFConfig")]
    class Config : Configuration<Config>
    {
        public List<string> WFGroupList = new List<string>();

        public List<string> InvationRewardList = new List<string>();

        public string Code;

        public string QQ;
    }


    public class WFNotificationHandler
    {
        public WFNotificationHandler()
        {
            InitWFNotification();

        }

        public HashSet<string> SendedAlertsSet = new HashSet<string>();
        public HashSet<string> SendedInvSet = new HashSet<string>();
        private bool _inited;// 谁加了个readonly????????
        public Timer timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        private WFChineseAPI api = new WFChineseAPI();

        private void InitWFNotification()
        {
            if (_inited) return;
            var alerts = api.GetAlerts();
            var invs = api.GetInvasions();

            foreach (var alert in alerts)
            {
                SendedAlertsSet.Add(alert.Id);
            }

            foreach (var inv in invs)
            {
                SendedInvSet.Add(inv.id);
            }

            timer.Elapsed += (sender, eventArgs) =>
            {
                UpdateAlerts();
                UpdateInvasions();
            };
            timer.Start();
            _inited = true;
        }

        public void UpdateInvasions()
        {
            var invs = new List<WFInvasion>();
            try
            {
                invs = api.GetInvasions();
            }
            catch (WebException)
            {
                // 也挺正常 不慌不慌
            }
            catch (Exception e)
            {
                // 问题有点大 我慌一下
                Messenger.SendPrivate(Config.Instance.QQ, e.ToString());
            }
            foreach (var inv in invs)
            {
                if (inv.completed) continue; // 不发已经完成的入侵
                // 你学的好快啊
                if(SendedInvSet.Contains(inv.id)) continue;// 不发已经发过的入侵

                var list = GetAllInvasionsCountedItems(inv);
                foreach (var item in list)
                {
                    if (Config.Instance.InvationRewardList.Contains(item))
                    {
                        var notifyText = $"指挥官, 太阳系陷入了一片混乱, 查看你的星图\r\n" +
                                         $"{WFFormatter.ToString(inv)}";

                        foreach (var group in Config.Instance.WFGroupList)
                        {
                            Messenger.SendGroup(group, notifyText);
                        }

                        SendedInvSet.Add(inv.id);
                        break;
                    }
                }
            }
        }

        private static List<string> GetAllInvasionsCountedItems(WFInvasion inv)
        {
            var list = new List<string>();
            foreach (var reward in inv.attackerReward.countedItems)
            {
                list.Add(reward.type);
            }

            foreach (var reward in inv.defenderReward.countedItems)
            {
                list.Add(reward.type);
            }

            return list;
        }

        public void SendAllInvasions(string group)
        {
            var invasions = api.GetInvasions();
            // UpdateAlerts();

            var sb = new StringBuilder();
            sb.AppendLine("指挥官, 下面是太阳系内所有的入侵任务."); //TODO 这里的语言你改一下
            foreach (var invasion in invasions)
            {
                if (!invasion.completed)
                {
                    sb.AppendLine(WFFormatter.ToString(invasion));
                    sb.AppendLine();
                }
            }

            Messenger.SendGroup(group, sb.ToString().Trim()); // trim 去掉最后的空格
        }


        public void UpdateAlerts()
        {
            try
            {
                var alerts = api.GetAlerts();
                foreach (var alert in alerts)
                {
                    if (!SendedAlertsSet.Contains(alert.Id))
                    {
                        SendWFAlert(alert);
                    }
                }

            }
            catch (WebException)
            {
                // 问题不大 不用慌
            }
            catch (Exception e)
            {
                var qq = Config.Instance.QQ;
                Messenger.SendPrivate(qq, e.ToString());
            }

        }

        public void SendAllAlerts(string group)
        {
            var alerts = api.GetAlerts();
            // UpdateAlerts();

            var sb = new StringBuilder();
            sb.AppendLine("指挥官, 下面是太阳系内所有的警报任务, 供您挑选.");
            foreach (var alert in alerts)
            {
                sb.AppendLine(WFFormatter.ToString(alert));
                sb.AppendLine();
            }

            Messenger.SendGroup(group, sb.ToString().Trim()); // trim 去掉最后的空格
        }

        public void SendWFAlert(WFAlert alert)
        {
            var reward = alert.Mission.Reward;
            if (reward.Items.Length > 0 || reward.CountedItems.Length > 0)
            {
                var result = "指挥官, Ordis拦截到了一条警报, 您要开始另一项光荣的打砸抢任务了吗?\r\n" +
                    WFFormatter.ToString(alert);

                foreach (var group in Config.Instance.WFGroupList)
                {
                    Messenger.SendGroup(group, result);
                }
            }
            SendedAlertsSet.Add(alert.Id);
        }

        /* 以下是废弃的代码和注释
        // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
        // RenderAlert(result, path);
        // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");
        // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");// 图片渲染还是问题太多 文字好一点吧...
        // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
        // RenderAlert(result, path);

        public void RenderAlert(string content, string path)// 废弃了呀...
        {
            var strs = content.Split(Environment.NewLine.ToCharArray());
            var height = 60;
            var width = 60;
            var font = new Font("Microsoft YaHei", 16);// 雅黑还挺好看的
            var size = TextRenderer.MeasureText(strs[0], font);
            width += GetlongestWidth(strs, font);
            height += size.Height * strs.Length;
            height += 10 * (strs.Length - 1);
            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            var p = new Point(30, 30);
            graphics.Clear(Color.Gray);
            foreach (var str in strs)
            {
                TextRenderer.DrawText(graphics, str, font, p, Color.Lavender);
                p.Y += TextRenderer.MeasureText(str, font).Height + 10;
            }

            bitmap.Save(path);
        }

        public int GetlongestWidth(string[] strs, Font font)
        {
            var width = new List<int>();
            foreach (var str in strs)
            {
                var size = TextRenderer.MeasureText(str, font);
                width.Add(size.Width);
            }

            return width.Max();
        }

        */
    }
}
