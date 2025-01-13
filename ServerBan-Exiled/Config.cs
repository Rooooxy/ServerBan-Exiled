using Exiled.API.Interfaces;
using System.ComponentModel;

namespace ServerBan_Exiled
{
    public class Config : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }
        [Description("是否开启IP检查")]
        public bool IsIPCheck { get; set; } = true;
        [Description("是否开启名称检查")]
        public bool IsNicknameCheck { get; set; } = false;
        [Description("是否开启Steam64Id检查")]
        public bool IsUserIdCheck { get; set; } = true;
        [Description("填申诉qq群")]
        public string qqGroup { get; set; } = "xxxxxxxxx";
        [Description("是否在进行封禁行为时将封禁信息写入BannedData")]
        public bool IsAddBannedData { get; set; } = true;
    }
}
