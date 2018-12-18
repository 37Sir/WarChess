package com.zyd.demo.common.enumuration;

public enum PushReqestName {
    // 信息推送
    ServerBattleMesPush("ServerBattleMesPush"),
	// 玩家重连,玩家的战斗
	PlayerBattleConnectPush("PlayerBattleConnectPush"), 
	// 玩家可以进行下一帧操作推送
    PlayNextPush("PlayNextPush"),
    //玩家悔棋结束
    PlayUndoNextPush("PlayUndoNextPush"),
	// 匹配开始推送
    PlayerStartPush("PlayerStartPush"),
    // 玩家战斗结束推送
    PlayerEndPush("PlayerEndPush"),
    //有玩家玩家未准备    
    PlayerNotReady("PlayerNotReady"),
    //某一玩家准备消息推送
    OnePlayerReady("OnePlayerReady"),
    // 所有玩家准备完成推送
    PlayerReadyFinishedPush("PlayerReadyFinishedPush"),
    //玩家同意请求推送
    PlayerAgreePush("PlayerAgreePush"),
    //玩家不同意请求推送
    PlayerNotAgreePush("PlayerNotAgreePush"),
    //玩家悔棋信息的推送
    PlayerUndoInfoPush("PlayerUndoInfoPush"),
    //玩家请求悔棋推送
    PlayerUndoPush("PlayerUndoPush");
  
	PushReqestName(String name) {
		this.requestName = name;
	}
	private String requestName;

	public String getRequestName() {
		return requestName;
	}

	public void setRequestName(String requestName) {
		this.requestName = requestName;
	}
	

}
