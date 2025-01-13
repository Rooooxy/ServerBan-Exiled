# ServerBan-Exiled
## 一、简介
ServerBan_Exiled 是一个基于 Exiled 框架开发的服务器插件，旨在提供全服封禁功能，并可灵活地管理封禁信息。该插件允许服务器管理员在 Config 文件中自由开关封禁信息的录入功能，并且在启用插件时，会检查本地文件 C:/BannedData/Data.txt 中的封禁记录，确保已被录入封禁记录的玩家无法进入服务器游玩。同时，它支持对玩家名称、IP 地址和 UserId 进行检查，管理员可以自由开启或关闭这些检查项，从而对进入服务器的玩家进行相应信息的对比检查，以确定玩家是否处于封禁状态。
## 二、功能特性
### （一）封禁信息录入
可以在 Config 文件中通过配置 IsAddBannedData 选项来自由开关是否在封禁玩家时将封禁信息录入。当开启该功能时，每次封禁玩家的信息将被记录在本地文件 C:/BannedData/Data.txt 中
### （二）封禁检查
当玩家尝试进入服务器（在 OnPlayerVerified 事件中触发）时，会调用 CheckBan 方法进行封禁检查。该方法会读取 C:/BannedData/Data.txt 中的封禁记录，并根据以下逻辑进行检查：
  - 首先，将文件中的每一行数据按 ; 分割，提取封禁玩家的 UserId、IP、Nickname、封禁过期时间、执行人信息和封禁原因等。
  - 然后，根据配置中的 IsUserIdCheck、IsIPCheck 和 IsNicknameCheck 选项，对玩家的 UserId、IP 和 Nickname 进行检查。这些检查项可单独开启或关闭。
  - 对于 IP 检查，使用 IsIpBanned 方法进行比较，该方法会比较玩家的 IP 地址和封禁记录中的 IP 地址，支持通配符 *，前三位相同则认为匹配。
### （三）封禁操作
在 OnPlayerBanning 事件中，当玩家被封禁时，会记录封禁的详细信息，包括：
  - 封禁玩家的 UserId、Nickname、IPAddress。
  - 封禁的到期时间，根据封禁时长计算得到，并以 yyyy/MM/dd HH:mm:ss 的格式存储。
  - 执行人的 Nickname 和 UserId，如果是服务器执行封禁，将显示为 Dedicated Server 和 ID_Dedicated。
  - 封禁的原因。
  - 所有这些信息会以特定格式存储在 Data.txt 文件中，格式如下：
  ```txt
  封禁UserId-{bannedPlayer.UserId};封禁NickName-{bannedNickname};封禁IP-{bannedPlayer.IPAddress};封禁到期时间-{banExpiresString};执行人-{executorName};执行人UserId-{executorUserId};封禁原因-{banReason};
  ```
### （四）玩家封禁通知
当玩家被封禁时，会根据不同情况进行广播通知：
  - 如果是服务器执行的封禁，会广播消息 [{ev.Target.Nickname}]已被全服封禁\n原因:[{ev.Reason}]。
  - 若由其他玩家执行封禁，会广播消息 [{ev.Target.Nickname}]已被[{ev.Player.Nickname}]全服封禁\n原因:[{ev.Reason}]。
### （五）玩家封禁处理
对于被封禁且封禁尚未过期的玩家，将收到一个包含详细信息的断开连接消息，包含以下内容：
  - 被封禁的信息，包括 SteamID 和 IP。
  - 封禁的原因。
  - 解封时间。
  - 申诉 QQ 群号（从 PluginConfig.qqGroup 中获取）。
  - 执行人信息。
  - 格式如下：
  ```txt
  你已被全服封禁，如有疑问去群内联系管理员申诉
  SteamID:[{player.UserId}]  IP:[{player.IPAddress}]
  封禁原因: {banReason}
  解封时间:[{unbanTime}]
  申诉QQ群:{BannedDataPlugin.PluginConfig.qqGroup}
  执行人: {executorNickName} (SteamID: {executorUserId})
  ```
### （六）过期封禁处理
对于已经过期的封禁记录，会在玩家登录检查时自动从 Data.txt 文件中移除，确保不再对玩家进行误判。
## 三、使用方法
### （一）配置文件
确保在 Config 文件中正确配置以下选项：
  - IsAddBannedData：控制是否在封禁玩家时将封禁信息录入文件。
  - IsUserIdCheck：控制是否对玩家的 UserId 进行封禁检查。
  - IsIPCheck：控制是否对玩家的 IP 进行封禁检查。
  - IsNicknameCheck：控制是否对玩家的 Nickname 进行封禁检查。
  - qqGroup：设置申诉的 QQ 群号，方便玩家申诉封禁问题。
### （二）安装步骤
1.将插件文件添加到C:\Users\User\AppData\Roaming\EXILED\Plugins插件目录。
2.启动服务器，插件会自动读取 Config 文件进行相应的配置初始化。
3.确保 C:/BannedData 目录存在，如果不存在，插件会自动创建。
4.确保 C:/BannedData/Data.txt 文件存在，如果不存在，插件会自动创建。
