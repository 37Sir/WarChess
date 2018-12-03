package com.zyd.demo.round.service;

import java.util.Map;
import java.util.Map.Entry;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelEndRequest;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelEndResponse;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelFattingFinishedRequest;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelFattingFinishedResponse;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelReadyRequest;
import com.zyd.common.proto.client.FireBattle.FairBattleLevelReadyResponse;
import com.zyd.common.proto.client.FireBattle.PlayerBattleMesRequest;
import com.zyd.common.proto.client.FireBattle.PlayerBattleMesResponse;
import com.zyd.common.proto.client.FireBattle.PlayerRequireBattleMesAgainRequest;
import com.zyd.common.proto.client.FireBattle.PlayerRequireBattleMesAgainResponse;
import com.zyd.demo.common.BaseService;
import com.zyd.demo.round.pojo.UserMatchInfo;


// 房间管理器
public class BattleRoomManager extends BaseService {
	
	private static final Logger logger = LoggerFactory.getLogger(BattleRoomManager.class);
	// 房间id与房间的映射
	private Map<Long, BattleRoom> battleRoomMap = new ConcurrentHashMap<>();
	// 用户key对应房间的id
	private Map<String, Long> userToRoomIdMap = new ConcurrentHashMap<>();
	// 用户key对应的用户信息
	private Map<String, UserMatchInfo> userMatchInfoMap = new ConcurrentHashMap<>();
	// 房间更新时长
	private static final int SCHEDULED_EXECULATE_INTERVAL_MILLISECOND = 200;
	// 定时器,启动线程进行房间玩家操作时间更新
	private ScheduledExecutorService updateTask = Executors.newSingleThreadScheduledExecutor();

	/** 玩家掉线 */
	public void playerDisConnect(String token) {
		UserMatchInfo userMatchInfo = userMatchInfoMap.get(token);
		if (userMatchInfo != null) {
			userMatchInfo.setConnect(false);
		}
	}

	/** 玩家重连 */
	public void playerConnect( String clientIp,String token) {
		String userKey = token;
		UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
		Long roomId = userToRoomIdMap.get(userKey);
		BattleRoom battleRoom = null;
		if(roomId!=null){
			battleRoom = battleRoomMap.get(roomId);
		}
		
		if (userMatchInfo != null && battleRoom !=null) {
			System.out.println("重新设置token : " + token);
			//重新设置token
			userMatchInfo.setToken(token);
			userMatchInfo.setConnect(true);
			userMatchInfo.setClientIp(clientIp);
		}
	}

	/** 添加一个新的房间到对应关系中 */
	public void addNewBattleRoom(BattleRoom battleRoom) {
		try {
			Long roomId = nosqlService.getNoSql().incr("FIRE_BATTLE_ROOM_INCR");
			System.out.println(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + roomId);
			battleRoom.roomId = roomId;
			battleRoomMap.put(roomId, battleRoom);
			for (UserMatchInfo userMatchInfo : battleRoom.getUserMatchInfoList()) {
				String userKey = getUserKey(userMatchInfo.getToken());
				userToRoomIdMap.put(userKey, roomId);
				userMatchInfoMap.put(userKey, userMatchInfo);
			}
			battleRoom.start();
		} catch (Exception e) {
			logger.error("can not get the room id");
		}
		
	}

	/** 移除一个战斗房间 */
	public void removeBattleRoom(BattleRoom battleRoom) {
		battleRoomMap.remove(battleRoom.roomId);
		for (UserMatchInfo userMatchInfo : battleRoom.getUserMatchInfoList()) {
			String userKey = getUserKey( userMatchInfo.getToken());
			userToRoomIdMap.remove(userKey);
			userMatchInfoMap.remove(userKey);
		}
	}

	/** 通过用户,移除一个战斗房间 */
	public void removeBattleRoom(String token) {
		Long roomId = userToRoomIdMap.get(token);
		if (roomId == null) {
			return;
		}
		BattleRoom battleRoom = battleRoomMap.get(roomId);
		if (battleRoom != null) {
			removeBattleRoom(battleRoom);
		}
	}

	/** 战斗请求发送 */
	public PlayerBattleMesResponse onRequest(String token, PlayerBattleMesRequest battleMesRequest) {
		String userKey = token;
		BattleRoom battleRoom = null;
		Long roomId = userToRoomIdMap.get(userKey);
		UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
		if (roomId != null) {
			battleRoom = battleRoomMap.get(roomId);
		}
		if (battleRoom == null || userMatchInfo == null) {
			PlayerBattleMesResponse.Builder builder = PlayerBattleMesResponse.newBuilder();
			builder.setRes(false);
			builder.setError("比赛已经结束!");
			return builder.build();
		}
		return battleRoom.doRequest(userMatchInfo, battleMesRequest);
	}

	/** 战斗信息请求 */
	public PlayerRequireBattleMesAgainResponse onRequest(long roomId, PlayerRequireBattleMesAgainRequest request) {
		BattleRoom battleRoom = battleRoomMap.get(roomId);
		if(battleRoom == null){
			// TODO 如果房间为空的情况,表示房间已经结束,数据从db中获取
			return null;
		}else{
			return battleRoom.doRequest(request);
		}
	}

	/** 准备请求 */
	public FairBattleLevelReadyResponse onRequest(String token ,FairBattleLevelReadyRequest battleMesRequest) {
		BattleRoom battleRoom = null;
		String userKey = token;
		Long roomId = userToRoomIdMap.get(userKey);
		UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
		if (roomId != null) {
			battleRoom = battleRoomMap.get(roomId);
		}
		if (battleRoom == null || userMatchInfo == null) {
			return null;
		}
		return battleRoom.doRequest(userMatchInfo, battleMesRequest);
	}
	
	/** 得到战斗房间*/
	public BattleRoom getBattleRoomByUser(String token) {
		String userKey = token;
		Long roomId = userToRoomIdMap.get(userKey);
		BattleRoom battleRoom = null;
		if (roomId != null) {
			battleRoom = battleRoomMap.get(roomId);
		}
		return battleRoom;
	}


	/** 战斗完成请求 */
	public FairBattleLevelFattingFinishedResponse onRequest(String token,
			FairBattleLevelFattingFinishedRequest request) {
		BattleRoom battleRoom = null;
		String userKey = token;
		Long roomId = userToRoomIdMap.get(userKey);
		UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
		if (roomId != null) {
			battleRoom = battleRoomMap.get(roomId);
		}
		if (battleRoom == null || userMatchInfo == null) {
			return null;
		}
		return battleRoom.doRequest(userMatchInfo, request);
	}

	/** 战斗结束请求 */
	public FairBattleLevelEndResponse onRequest(String token, FairBattleLevelEndRequest request) {
		BattleRoom battleRoom = null;
		String userKey = token;
		Long roomId = userToRoomIdMap.get(userKey);
		UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
		if (roomId != null) {
			battleRoom = battleRoomMap.get(roomId);
		}
		if (battleRoom == null || userMatchInfo == null) {
			return null;
		}
		return battleRoom.doRequest(userMatchInfo, request);
	}

	/** 初始化方法 */
	public void init() {
		onUpdate();
	}


	/** 得到用户的key zoneId_userId */
	public String getUserKey(String token) {
		return new StringBuilder().append(token).toString();
	}

	/** 更新方法 */
	public void onUpdate() {
		updateTask.scheduleWithFixedDelay(() -> {
			try{
				for (Entry<Long, BattleRoom> entry : battleRoomMap.entrySet()) {
					BattleRoom battleRoom = entry.getValue();
					if (battleRoom.canRemove) {
						removeBattleRoom(battleRoom);
					} else {
						// 更新
						battleRoom.onUpdate();
					}
				}
			}catch(Exception e){
				logger.error("", e);
			}
		
		}, 0, SCHEDULED_EXECULATE_INTERVAL_MILLISECOND, TimeUnit.MILLISECONDS);
	}


}
