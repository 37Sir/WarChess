package com.zyd.demo.round.service;

public class BattleConfig {
	/** 玩家一步操作的时间*/
	public static int playTime = 60 * 1000;
	/** 玩家操作响应的处理时间 */
	public static int playReadTime = 5 * 1000;
	/** 玩家结束响应处理时间 */
	public static int finishReadyTime = 5 * 1000;
	/** 等待玩家发送准备完成的时间*/
	public static int startReadyTime = 20 * 1000;
}