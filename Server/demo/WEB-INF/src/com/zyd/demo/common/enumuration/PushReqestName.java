package com.zyd.demo.common.enumuration;

public enum PushReqestName {
    // 公平竞技场信息推送
    ServerBattleMesPush("ServerBattleMesPush"),
	// 玩家重连,公平竞技场有玩家的战斗
	PlayerBattleConnectPush("PlayerBattleConnectPush"), 
	// 玩家可以进行下一帧操作推送
    PlayNextPush("PlayNextPush"),
	// 匹配开始推送
    FairBattleLevelStartPush("FairBattleLevelStartPush"),
    // 玩家战斗结束推送
    FairBattleLevelEndPush("FairBattleLevelEndPush"),
    // 所有玩家准备完成推送
    FairBattleLevelReadyFinishedPush("FairBattleLevelReadyFinishedPush");
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