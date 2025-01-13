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
