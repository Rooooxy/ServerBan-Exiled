using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MEC;
using System.Globalization;

namespace ServerBan_Exiled
{
    public class BannedDataPlugin : Plugin<Config>
    {
        public override string Author { get; } = "Roxy";
        public override string Prefix { get; } = "全服封禁";
        private const string BannedDataFolderPath = "BannedData";
        private string BannedDataPath;
        public override void OnEnabled()
        {
            string bannedDataDirectory = Path.Combine(Paths.Configs, BannedDataFolderPath);
            BannedDataPath = Path.Combine(bannedDataDirectory, "Data.txt");
            if (!Directory.Exists(bannedDataDirectory))
            {
                Log.Info("BannedData 目录不存在,正在创建...");
                Directory.CreateDirectory(bannedDataDirectory);
            }
            if (!File.Exists(BannedDataPath))
            {
                Log.Info("Data.txt 文件不存在,正在创建...");
                File.Create(BannedDataPath).Close();
            }
            BannedDataPlugin.PluginConfig = base.Config;
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            if (BannedDataPlugin.PluginConfig.IsAddBannedData)
            {
                Exiled.Events.Handlers.Player.Banning += OnPlayerBanning;
            }
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.Banning -= OnPlayerBanning;
        }

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            CheckBan(ev.Player);
        }

        private void OnPlayerBanning(BanningEventArgs ev)
        {
            TimeSpan duration = TimeSpan.FromSeconds(ev.Duration);
            Player bannedPlayer = ev.Target;
            Player executor = ev.Player;
            DateTime banExpires = DateTime.Now.Add(duration);
            string banReason = ev.Reason;
            BanPlayer(bannedPlayer, executor, banExpires, banReason);
            Timing.CallDelayed(0.4f, delegate
            {
                if (ev.Player == null)
                {
                    Map.Broadcast(new Exiled.API.Features.Broadcast($"[{ev.Target.Nickname}]已被全服封禁\n原因:[{ev.Reason}]", 10, true, 0), true);
                }
                else
                {
                    Map.Broadcast(new Exiled.API.Features.Broadcast($"[{ev.Target.Nickname}]已被[{ev.Player.Nickname}]全服封禁\n原因:[{ev.Reason}]", 10, true, 0), true);
                }
            });
        }

        public void BanPlayer(Player bannedPlayer, Player executor, DateTime banExpires, string banReason)
        {
            string executorName = executor?.Nickname ?? "Dedicated Server";
            string executorUserId = executor?.UserId ?? "ID_Dedicated";
            string banExpiresString = banExpires.ToString("yyyy/MM/dd HH:mm:ss");
            string bannedNickname = bannedPlayer.Nickname;
            if (bannedPlayer.Nickname.Contains("-"))
            {
                bannedNickname = bannedPlayer.Nickname.Replace("-", "");
            }
            string banData = $"封禁UserId-{bannedPlayer.UserId};封禁NickName-{bannedNickname};封禁IP-{bannedPlayer.IPAddress};封禁到期时间-{banExpiresString};执行人-{executorName};执行人UserId-{executorUserId};封禁原因-{banReason};";

            try
            {
                Log.Info($"尝试将封禁数据写入: {BannedDataPath}");
                File.AppendAllText(BannedDataPath, banData + Environment.NewLine);
                Log.Info("封禁数据成功写入！");
            }
            catch (Exception ex)
            {
                Log.Error($"封禁数据写入失败: {ex.Message}");
            }
        }

        private void CheckBan(Player player)
        {
            List<string> lines = File.ReadAllLines(BannedDataPath).ToList();
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length < 8)
                    continue;
                string bannedUserId = parts[0].Split('-')[1].Trim();
                string bannedIp = parts[2].Split('-')[1].Trim();
                string bannedNickName = parts[1].Split('-')[1].Trim();
                string banExpiresString = parts[3].Split('-')[1].Trim();
                string executorNickName = parts[4].Split('-')[1].Trim();
                string executorUserId = parts[5].Split('-')[1].Trim();
                string banReason = parts[6].Split('-')[1].Trim();
                DateTimeOffset banExpires;
                if (DateTimeOffset.TryParse(banExpiresString, null, DateTimeStyles.None, out banExpires))
                {
                    bool UserIdCheck = BannedDataPlugin.PluginConfig.IsUserIdCheck;
                    bool IPCheck = BannedDataPlugin.PluginConfig.IsIPCheck;
                    bool NicknameCheck = BannedDataPlugin.PluginConfig.IsNicknameCheck;
                    bool flag = false;
                    if (UserIdCheck)
                    {
                        flag = player.UserId == bannedUserId;
                        if (IPCheck)
                        {
                            flag = flag || IsIpBanned(player.IPAddress, bannedIp);
                        }
                        if (NicknameCheck)
                        {
                            flag = flag || player.Nickname == bannedNickName;
                        }
                    }
                    else if (IPCheck)
                    {
                        flag = IsIpBanned(player.IPAddress, bannedIp);
                        if (NicknameCheck)
                        {
                            flag = flag || player.Nickname == bannedNickName;
                        }
                    }
                    else if (NicknameCheck)
                    {
                        flag = player.Nickname == bannedNickName;
                    }
                    if (flag)
                    {
                        if (DateTimeOffset.Now < banExpires)
                        {
                            string unbanTime = banExpires.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            string disconnectMessage = $"你已被全服封禁，如有疑问去群内联系管理员申诉\nSteamID:[{player.UserId}]  IP:[{player.IPAddress}]\n封禁原因: {banReason}\n解封时间:[{unbanTime}]\n申诉QQ群:{BannedDataPlugin.PluginConfig.qqGroup}";
                            if (executorNickName == "Dedicated Server" && executorUserId == "ID_Dedicated")
                            {
                                disconnectMessage += "\n执行人: Unknown";
                            }
                            else
                            {
                                disconnectMessage += $"\n执行人: {executorNickName} (SteamID: {executorUserId})";
                            }
                            player.Disconnect(disconnectMessage);
                            return;
                        }
                        else
                        {
                            lines.Remove(line);
                            File.WriteAllLines(BannedDataPath, lines.ToArray());
                        }
                    }
                }
                else
                {
                    Log.Error($"解析DateTime字符串时出错: {banExpiresString}");
                }
            }
        }

        private bool IsIpBanned(string playerIp, string bannedIp)
        {
            string[] playerIpParts = playerIp.Split('.');
            string[] bannedIpParts = bannedIp.Split('.');

            for (int i = 0; i < 3; i++)
            {
                if (bannedIpParts[i] != "*" && !playerIpParts[i].Equals(bannedIpParts[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static Config PluginConfig;
    }
}