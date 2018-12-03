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
import com.zyd.common.proto.client.FireBattle.BattleMes;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelEndPush;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelEndRequest;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelEndResponse;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelFattingFinishedRequest;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelFattingFinishedResponse;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelReadyFinishedPush;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelReadyRequest;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelReadyResponse;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelStartPush;
import com.zyd.common.proto.client.FireBattle.PlayNextPush;
import com.zyd.common.proto.client.FireBattle.PlayerBattleMesRequest;
import com.zyd.common.proto.client.FireBattle.PlayerBattleMesResponse;
import com.zyd.common.proto.client.FireBattle.PlayerMes;
import com.zyd.common.proto.client.FireBattle.PlayerRequireBattleMesAgainRequest;
import com.zyd.common.proto.client.FireBattle.PlayerRequireBattleMesAgainResponse;
import com.zyd.common.proto.client.FireBattle.ServerBattleMesPush;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.CommonService;
import com.zyd.demo.common.enumuration.PushReqestName;
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
	private Map<Long, FairBattleLevelEndRequest> fairBattleLevelEndRequestMap = new HashMap<>();
	// 是否可以将房间移出
	public boolean canRemove = false;
	// 房间状态
	public BattleStatus battleStatus = BattleStatus.start;
	// 开始的玩家userId
	private long startUserId;
	// 完成操作的玩家
	private Map<Long, Object> dealUserMap = new HashMap<>();
	// 房间管理器
	private BattleRoomManager battleRoomManager;
	// 是否是主动放弃结束
	private boolean isGiveUp = false;
	// 主动放弃的玩家
	private Long giveUpUserId = null;


	private static final Logger logger = LoggerFactory.getLogger(BattleRoom.class);

	/** 战斗开始 */
	public void start() {
		nextTime = System.currentTimeMillis() + BattleConfig.startReadyTime;
		actor = 0;
		battleStatus = BattleStatus.start;
		startUserId = userMatchInfoList.get(actor).getUid();

		// 推送信息
		FairBattleLevelStartPush.Builder builder = FairBattleLevelStartPush.newBuilder();

		// 玩家信息
		for (UserMatchInfo userMatchInfo : userMatchInfoList) {
			PlayerMes.Builder playerBuilder = PlayerMes.newBuilder();
			playerBuilder.setUserId(userMatchInfo.getUid());
			builder.addPlayerMes(playerBuilder);
		}

		builder.setUserId(startUserId);
		builder.setRoomId(roomId);
		disrupAll(PushReqestName.FairBattleLevelStartPush, builder.build());
	}

	/** 处理玩家的开始请求 */
	public FairBattleLevelReadyResponse doRequest(UserMatchInfo userMatchInfo,
			FairBattleLevelReadyRequest battleMesRequest) {
		lock.lock();
		try {
			if (!battleStatus.equals(BattleStatus.start)) {
				return FairBattleLevelReadyResponse.newBuilder().build();
			}
			// 把所有人准备并且遍历所有玩家,如果所有玩家都准备发送准备完成推送
			dealUserMap.put(userMatchInfo.getUid(), new Object());

			if (dealUserMap.size() == userMatchInfoList.size()) {
				battleStatus = BattleStatus.fightWaiting;
				dealUserMap.clear();
				nextTime = System.currentTimeMillis() + BattleConfig.playTime;
				disrupAll(PushReqestName.FairBattleLevelReadyFinishedPush,
						FairBattleLevelReadyFinishedPush.newBuilder().build());
			}
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		return FairBattleLevelReadyResponse.newBuilder().build();
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
			fairBattleLevelEndRequestMap.put(userMatchInfo.getUid(), fairBattleLevelEndRequest);
			if (fairBattleLevelEndRequestMap.size() >= 2) {
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

			FairBattleLevelStartPush.Builder startBuilder = FairBattleLevelStartPush.newBuilder();
			// 玩家信息
			for (UserMatchInfo userMatchInfo : userMatchInfoList) {
				PlayerMes.Builder playerBuilder = PlayerMes.newBuilder();
				playerBuilder.setUserId(userMatchInfo.getUid());

				startBuilder.addPlayerMes(playerBuilder);
			}
			startBuilder.setUserId(startUserId);
			responseBuilder.setFairBattleLevelSatrt(startBuilder);
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
		FairBattleLevelEndPush.Builder builder = FairBattleLevelEndPush.newBuilder();
//		// 所有的桢信息
//		for (BattleMes battleMes : serverBattleMesList) {
//			builder.addBattleMes(battleMes);
//		}
		if (!isGiveUp) {
			builder.setResult(0);
			// 推送结束信息
			disrupAll(PushReqestName.FairBattleLevelEndPush, builder.build());
		} else {
			for (UserMatchInfo userMatchInfo : userMatchInfoList) {
				if (userMatchInfo.getUid().equals(giveUpUserId)) {
					builder.setResult(1);
				} else {
					builder.setResult(2);
				}
				disrupOne(PushReqestName.FairBattleLevelEndPush, userMatchInfo, builder.build());
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
					// 如果玩家长时间没有发送准备请求,那么战斗完成退出
					battleFinished();
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
