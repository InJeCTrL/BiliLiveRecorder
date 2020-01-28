# BiliLiveRecorder

> B站直播录制机

下载地址：

### [BiliLiveRecorder](https://injectrl.github.io/BiliLiveRecorder/BiliLiveRecorder/bin/Release/BiliLiveRecorder.exe)

---

* 接口获取

	1. 直播间页面查看网络活动找到直播间信息接口

		https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id={直播间ID}

	2. 用户主页查看网络活动找到用户信息接口和直播间入口接口（旧）

		https://api.bilibili.com/x/space/acc/info?mid={UID}

		https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid={UID}

	3. 通过直播间页面加载的[player-loader-1.10.1.min.js](https://s1.hdslb.com/bfs/static/player/live/loader/player-loader-1.10.1.min.js)搜索(api.live.bilibili.com)可找到下列接口：

		https://api.live.bilibili.com/room/v1/Room/playUrl

		https://api.live.bilibili.com/room/v1/room/get_recommend_by_room

		https://api.live.bilibili.com/room/v1/Room/room_init

		https://api.live.bilibili.com/room/v1/Danmu/getConf
