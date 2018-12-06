package com.zyd.demo.round.service;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.MessageLite;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.WarChess.BattleMes;
import com.zyd.common.proto.client.WarChess.PlayerEndPush;
import com.zyd.common.proto.client.WarChess.FairBattleLevelEndRequest;
import com.zyd.common.proto.client.WarChess.FairBattleLevelEndResponse;
import com.zyd.common.proto.client.WarChess.FairBattleLevelFattingFinishedRequest;
import com.zyd.common.proto.client.WarChess.FairBattleLevelFattingFinishedResponse;
import com.zyd.common.proto.client.WarChess.PlayerReadyFinishedPush;
import com.zyd.common.proto.client.WarChess.PlayerStartPush;
import com.zyd.common.proto.client.WarChess.OnePlayerReady;
import com.zyd.common.proto.client.WarChess.PlayNextPush;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesRequest;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesResponse;
import com.zyd.common.proto.client.WarChess.PlayerMes;
import com.zyd.common.proto.client.WarChess.PlayerNotReady;
import com.zyd.common.proto.client.WarChess.PlayerReadyRequest;
import com.zyd.common.proto.client.WarChess.PlayerRequireBattleMesAgainRequest;
import com.zyd.common.proto.client.WarChess.PlayerRequireBattleMesAgainResponse;
import com.zyd.common.proto.client.WarChess.ServerBattleMesPush;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.CommonService;
import com.zyd.demo.common.enumuration.PushReqestName;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.round.pojo.UserMatchInfo;


// 对战房间
public class BattleRoom {
	// 战斗房间的id
	public Long roomId;
	// 对战成员
	private List<UserMatchInfo> userMatchInfoList;
	// 当前动作玩家的索引
	private int actor;
	// 当前操作时间
	private long nextTime = 0;
	//
	private CommonService commonService;
	// 房间操作锁
	private Lock lock = new ReentrantLock();
	// 历史桢信息
//	public List<BattleMes> serverBattleMesList = new ArrayList<>();
	// 当前操作桢的索引
	private int currentPlayNum = 0;
	// 结束阶段发送的用户 Uid-FairBattleLevelEndRequest
	private Map<Integer, FairBattleLevelEndRequest> battleEndRequestMap = new HashMap<>();
	// 是否可以将房间移出
	public boolean canRemove = false;
	// 房间状态
	public BattleStatus battleStatus = BattleStatus.start;
	// 开始的玩家userId
	private long startUserId;
	// 完成操作的玩家
	private Map<Integer, Object> dealUserMap = new HashMap<>();
	// 房间管理器
	private BattleRoomManager battleRoomManager;
	// 是否是主动放弃结束
	private boolean isGiveUp = false;
	// 主动放弃的玩家
	private Integer giveUpUserId = null;


	private static final Logger logger = LoggerFactory.getLogger(BattleRoom.class);

	/** 战斗开始 */
	public void start() {
		nextTime = System.currentTimeMillis() + BattleConfig.startReadyTime;
		actor = 0;
		battleStatus = BattleStatus.start;
		startUserId = userMatchInfoList.get(actor).getUid();

		// 推送信息
		PlayerStartPush.Builder builder = PlayerStartPush.newBuilder();

		// 玩家信息
		for (UserMatchInfo userMatchInfo : userMatchInfoList) {
			PlayerMes.Builder playerBuilder = PlayerMes.newBuilder();
			playerBuilder.setUserId(userMatchInfo.getUid());
			playerBuilder.setUserName(userMatchInfo.getToken());
			builder.addPlayerMes(playerBuilder);
		}
		builder.setUserId(startUserId);
		builder.setRoomId(roomId);
		disrupAll(PushReqestName.PlayerStartPush, builder.build());
	}

	/** 处理玩家的开始请求 */
	public void doRequest(UserMatchInfo userMatchInfo,
	    PlayerReadyRequest battleMesRequest) {
		lock.lock();
		try {
			if (!battleStatus.equals(BattleStatus.start)) {
	            logger.warn("PLAYER_NOT_START userId:{}",userMatchInfo.getUid());
	            throw new BaseException(ErrorCode.PLAYER_NOT_START_VALUE);
	        }
			dealUserMap.put(userMatchInfo.getUid(), new Object());
			//将玩家点击准备的动作发给对战的客户端
			OnePlayerReady.Builder onePlayerReady = OnePlayerReady.newBuilder();
			onePlayerReady.setUserId(userMatchInfo.getUid());
			UserMatchInfo toUserMatchInfo = null; 
			for (UserMatchInfo u : userMatchInfoList) {
			    if (u.getUid() != userMatchInfo.getUid()) {
			        toUserMatchInfo = u;
			    }
			}
			disrupOne(PushReqestName.OnePlayerReady, toUserMatchInfo, onePlayerReady.build());
			
	        // 把所有人准备并且遍历所有玩家,如果所有玩家都准备发送准备完成推送
			if (dealUserMap.size() == userMatchInfoList.size()) {
				battleStatus = BattleStatus.fightWaiting;
				dealUserMap.clear();
				nextTime = System.currentTimeMillis() + BattleConfig.playTime;
				disrupAll(PushReqestName.PlayerReadyFinishedPush,
						PlayerReadyFinishedPush.newBuilder().build());
			}
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		return ;
	}

	/** 处理玩家发送的桢请求 */
	public PlayerBattleMesResponse doRequest(UserMatchInfo userMatchInfo,
			PlayerBattleMesRequest playerBattleMesRequest) {
		lock.lock();
		try {
			if (!battleStatus.equals(BattleStatus.fighting)) {
				PlayerBattleMesResponse.Builder battleMesResponse = PlayerBattleMesResponse.newBuilder();
				battleMesResponse.setRes(false);
				return battleMesResponse.build();
			}
			BattleMes battleMes = playerBattleMesRequest.getBattleMes();
			// 如果关键帧不是自增的
			if (battleMes.getPlayNum() != currentPlayNum + 1) {
				PlayerBattleMesResponse.Builder battleMesResponse = PlayerBattleMesResponse.newBuilder();
				battleMesResponse.setRes(false);
				return battleMesResponse.build();
			}

			if (!userMatchInfo.getUid().equals(userMatchInfoList.get(actor).getUid())) {
				PlayerBattleMesResponse.Builder battleMesResponse = PlayerBattleMesResponse.newBuilder();
				battleMesResponse.setRes(false);
				return battleMesResponse.build();
			}
			battleStatus = BattleStatus.fightWaiting;
			nextTime = System.currentTimeMillis() + BattleConfig.playReadTime;
			currentPlayNum = currentPlayNum + 1;
			// 保存桢
			saveBattleMes(battleMes);

			ServerBattleMesPush.Builder builder = ServerBattleMesPush.newBuilder();
			builder.setBattleMes(battleMes);
			builder.setCurrentTime(System.currentTimeMillis());
			builder.setNextTime(nextTime);

			// 操作信息推送
			disrupAll(PushReqestName.ServerBattleMesPush, builder.build());
			// 下一步操作赋值
			actor = nextActorIndex();

			PlayerBattleMesResponse.Builder battleMesResponse = PlayerBattleMesResponse.newBuilder();
			battleMesResponse.setRes(true);
			return battleMesResponse.build();
		} catch (Exception e) {
			e.printStackTrace();
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		PlayerBattleMesResponse.Builder battleMesResponse = PlayerBattleMesResponse.newBuilder();
		battleMesResponse.setRes(false);
		return battleMesResponse.build();
	}

	/** 处理玩家发送的桢处理完成请求 */
	public FairBattleLevelFattingFinishedResponse doRequest(UserMatchInfo userMatchInfo,
			FairBattleLevelFattingFinishedRequest requst) {
		lock.lock();
		try {
			if (battleStatus.equals(BattleStatus.fightWaiting)) {
				dealUserMap.put(userMatchInfo.getUid(), new Object());
				if (dealUserMap.size() == userMatchInfoList.size()) {
					dealUserMap.clear();
					nextTime = System.currentTimeMillis() + BattleConfig.playTime;
					battleStatus = BattleStatus.fighting;
					disrupAll(PushReqestName.PlayNextPush, PlayNextPush.newBuilder().build());
				}
			}
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		return FairBattleLevelFattingFinishedResponse.newBuilder().build();
	}

	/** 战斗结束请求 */
	public FairBattleLevelEndResponse doRequest(UserMatchInfo userMatchInfo,
			FairBattleLevelEndRequest fairBattleLevelEndRequest) {
		lock.lock();
		try {

			// 主动结束
			if (fairBattleLevelEndRequest.getIsGiveUp()) {
				this.isGiveUp = true;
				this.giveUpUserId = userMatchInfo.getUid();
				// 战斗结束
				battleFinished();
				return FairBattleLevelEndResponse.newBuilder().build();
			}

			battleStatus = BattleStatus.waitFinish;
			battleEndRequestMap.put(userMatchInfo.getUid(), fairBattleLevelEndRequest);
			if (battleEndRequestMap.size() >= 2) {
				// 战斗结束
				battleFinished();
			} else {
				// 设置结束倒计时
				nextTime = System.currentTimeMillis() + BattleConfig.finishReadyTime;
			}
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		return FairBattleLevelEndResponse.newBuilder().build();
	}

	public PlayerRequireBattleMesAgainResponse doRequest(PlayerRequireBattleMesAgainRequest request) {
		lock.lock();
		try {
			int startPlayNum = request.getStartPlayNum();
			PlayerRequireBattleMesAgainResponse.Builder responseBuilder = PlayerRequireBattleMesAgainResponse
					.newBuilder();

			PlayerStartPush.Builder startBuilder = PlayerStartPush.newBuilder();
			// 玩家信息
			for (UserMatchInfo userMatchInfo : userMatchInfoList) {
				PlayerMes.Builder playerBuilder = PlayerMes.newBuilder();
				playerBuilder.setUserId(userMatchInfo.getUid());

				startBuilder.addPlayerMes(playerBuilder);
			}
			startBuilder.setUserId(startUserId);
			responseBuilder.setPlayerSatrt(startBuilder);
			//
//			if (startPlayNum <= serverBattleMesList.size()) {
//				List<BattleMes> battleMes = serverBattleMesList.subList(startPlayNum - 1, serverBattleMesList.size());
//				responseBuilder.addAllBattleMes(battleMes);
//			}
			return responseBuilder.build();
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		return null;
	}

	/** 战斗结束 */
	public void battleFinished() {
		PlayerEndPush.Builder builder = PlayerEndPush.newBuilder();
//		// 所有的桢信息
//		for (BattleMes battleMes : serverBattleMesList) {
//			builder.addBattleMes(battleMes);
//		}
		if (!isGiveUp) {
			builder.setResult(0);
			// 推送结束信息
			disrupAll(PushReqestName.PlayerEndPush, builder.build());
		} else {
			for (UserMatchInfo userMatchInfo : userMatchInfoList) {
				if (userMatchInfo.getUid().equals(giveUpUserId)) {
					builder.setResult(1);
				} else {
					builder.setResult(2);
				}
				disrupOne(PushReqestName.PlayerEndPush, userMatchInfo, builder.build());
			}
		}

		battleStatus = BattleStatus.finished;
		canRemove = true;
		battleRoomManager.removeBattleRoom(this);
		return;
	}

	/** 更新方法,如果玩家操作超时 */
	public void onUpdate() {
		if (nextTime > System.currentTimeMillis()) {
			return;
		}
		lock.lock();
		try {
			if (System.currentTimeMillis() > nextTime) {
				// 如果所有的玩家都掉线,游戏结束
				/*
				 * boolean hasConnect = false; for (UserMatchInfo userMatchInfo
				 * : userMatchInfoList) { if (userMatchInfo.isConnect()) {
				 * hasConnect = true; break; } } if (!hasConnect) {
				 * battleFinished(); return; }
				 */
				// 根据游戏的状态进行超时处理
				if (battleStatus.equals(BattleStatus.start)) {
				    //有玩家长时间未准备，则退出此次匹配
				    havaPlayerNotReady();
				} else if (battleStatus.equals(BattleStatus.fighting)) {
					nextTime = System.currentTimeMillis() + BattleConfig.playReadTime;
					currentPlayNum = currentPlayNum + 1;

					BattleMes.Builder battleMesBuilder = BattleMes.newBuilder();
					battleMesBuilder.setPlayNum(currentPlayNum);
					battleStatus = BattleStatus.fightWaiting;

					// 保存桢
					saveBattleMes(battleMesBuilder.build());
					ServerBattleMesPush.Builder builder = ServerBattleMesPush.newBuilder();
					builder.setBattleMes(battleMesBuilder.build());
					builder.setCurrentTime(System.currentTimeMillis());
					builder.setNextTime(nextTime);

					// 操作信息推送
					disrupAll(PushReqestName.ServerBattleMesPush, builder.build());
					// 下一步操作赋值
					actor = nextActorIndex();
				} else if (battleStatus.equals(BattleStatus.fightWaiting)) {
					dealUserMap.clear();
					nextTime = System.currentTimeMillis() + BattleConfig.playTime;
					battleStatus = BattleStatus.fighting;
					disrupAll(PushReqestName.PlayNextPush, PlayNextPush.newBuilder().build());
				} else if (battleStatus.equals(BattleStatus.waitFinish)) {
					// 战斗结束
					battleFinished();
				}
			}
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
	}

	 /**玩家未准备退出房间和匹配,并将未准备玩家信息发送给客户端*/
	private void havaPlayerNotReady() {
	    PlayerNotReady.Builder playerNotReady = PlayerNotReady.newBuilder();
	    for ( UserMatchInfo userMatchInfo :userMatchInfoList) {
	        if (!dealUserMap.containsKey(userMatchInfo.getUid())) {
	            playerNotReady.addUserId(userMatchInfo.getUid());
	        }
	    }
	    //将信息推送给玩家
	    disrupAll(PushReqestName.PlayerNotReady, playerNotReady.build());
	    
	    battleStatus = BattleStatus.finished;
	    canRemove = true;
	    battleRoomManager.removeBattleRoom(this);
	    return;
    }

    /** 保存战斗桢 */
  	public void saveBattleMes(BattleMes battleMes) {
  	  //		serverBattleMesList.add(battleMes);
  	}

	public BattleRoom(BattleRoomManager battleRoomManager, CommonService commonService,
			UserMatchInfo... userMatchInfos) {
		this.battleRoomManager = battleRoomManager;
		this.commonService = commonService;
		userMatchInfoList = new ArrayList<>();
		for (UserMatchInfo userMatchInfo : userMatchInfos) {
			userMatchInfoList.add(userMatchInfo);
		}
	}

	public void disrupAll(PushReqestName pushRequestName, MessageLite messageLite) {
		List<String> userTokenList = new ArrayList<>();
		for (UserMatchInfo userMatchInfo : userMatchInfoList) {
			userTokenList.add(userMatchInfo.getToken());
		}
		commonService.addProxyPushData(pushRequestName, new Packet(messageLite), userTokenList, messageLite.toString());
	}

	public void disrupOne(PushReqestName pushRequestName, UserMatchInfo userMatchInfo, MessageLite messageLite) {
		List<String> userTokenList = new ArrayList<>();
		userTokenList.add(userMatchInfo.getToken());
		commonService.addProxyPushData(pushRequestName, new Packet(messageLite), userTokenList, messageLite.toString());
	}

	/** 返回下一个操作者的索引 */
	public int nextActorIndex() {
		int nextItem = actor + 1;
		if (userMatchInfoList.size() <= nextItem) {
			nextItem = 0;
		}
		return nextItem;
	}

	public List<UserMatchInfo> getUserMatchInfoList() {
		return userMatchInfoList;
	}

	public void setUserMatchInfoList(List<UserMatchInfo> userMatchInfoList) {
		this.userMatchInfoList = userMatchInfoList;
	}

	enum BattleStatus {
		start, fighting, fightWaiting, waitFinish, finished
	}

}
