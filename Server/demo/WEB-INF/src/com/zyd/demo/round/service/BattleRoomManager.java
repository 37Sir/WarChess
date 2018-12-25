package com.zyd.demo.round.service;

import java.util.Map;
import java.util.Map.Entry;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.WarChess.FairBattleLevelEndRequest;
import com.zyd.common.proto.client.WarChess.FairBattleLevelEndResponse;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesRequest;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesResponse;
import com.zyd.common.proto.client.WarChess.PlayerPaintingEndResponse;
import com.zyd.common.proto.client.WarChess.PlayerReadyRequest;
import com.zyd.common.proto.client.WarChess.PlayerRequireBattleMesAgainRequest;
import com.zyd.common.proto.client.WarChess.PlayerRequireBattleMesAgainResponse;
import com.zyd.demo.common.BaseService;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.round.pojo.UserMatchInfo;
import com.zyd.demo.stone.service.ChessRoom;
import com.zyd.demo.user.pojo.User;


// 房间管理器
public class BattleRoomManager extends BaseService {
	
	private static final Logger logger = LoggerFactory.getLogger(BattleRoomManager.class);
	// 房间id与房间的映射
	private Map<Long, BattleRoom> battleRoomMap = new ConcurrentHashMap<>();
    // 房间id与新模式房间的映射
    private Map<Long, ChessRoom> chessRoomMap = new ConcurrentHashMap<>();	
	// 用户key对应房间的id
	private Map<String, Long> userToRoomIdMap = new ConcurrentHashMap<>();
	// 用户key对应的用户信息
	private Map<String, UserMatchInfo> userMatchInfoMap = new ConcurrentHashMap<>();
    // 新模式用户key对应房间的id
    private Map<String, Long> userToRoomIdMap0 = new ConcurrentHashMap<>();
    // 新模式用户key对应的用户信息
    private Map<String, User> userMatchInfoMap0 = new ConcurrentHashMap<>();
	// 房间更新时长
	private static final int SCHEDULED_EXECULATE_INTERVAL_MILLISECOND = 200;
	// 定时器,启动线程进行房间玩家操作时间更新
	private ScheduledExecutorService updateTask = Executors.newSingleThreadScheduledExecutor();


	/** 添加一个新的房间到对应关系中 */
	public void addNewBattleRoom(BattleRoom battleRoom) {
		try {
			Long roomId = nosqlService.getNoSql().incr("FIRE_BATTLE_ROOM_INCR");
			battleRoom.roomId = roomId;
			battleRoomMap.put(roomId, battleRoom);
			for (UserMatchInfo userMatchInfo : battleRoom.getUserMatchInfoList()) {
				String userKey = getUserKey(userMatchInfo.getToken());
				userToRoomIdMap.put(userKey, roomId);
				userMatchInfoMap.put(userKey, userMatchInfo);
			}
			battleRoom.start();
		} catch (Exception e) {
			logger.error("can not get the room id"+e);
		}
		
	}
    /** 添加一个新模式的房间到对应关系中 */
    public void addNewChessRoom(ChessRoom chessRoom) {
        try {
            Long roomId = nosqlService.getNoSql().incr("CHESS_ROOM_INCR");
            chessRoom.roomId = roomId;
            chessRoomMap.put(roomId, chessRoom);
            for (User user : chessRoom.getUserMatchInfoList()) {
                String userKey = user.getUserName();
                userToRoomIdMap0.put(userKey, roomId);
                userMatchInfoMap0.put(userKey, user);
            }
            chessRoom.start();
        } catch (Exception e) {
            logger.error("can not get the room id"+e);
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
    /** 通过用户,移除一个新模式房间 */
    public void removeChessRoom(String token) {
        Long roomId = userToRoomIdMap0.get(token);
        if (roomId == null) {
            return;
        }
        ChessRoom chessRoom = chessRoomMap.get(roomId);
        if (chessRoom != null) {
            removeChessRoom(chessRoom);
        }
    }
    /** 移除一个新模式战斗房间 */
    public void removeChessRoom(ChessRoom chessRoom) {
        battleRoomMap.remove(chessRoom.roomId);
        for (User user : chessRoom.getUserMatchInfoList()) {
            String userKey = user.getUserName();
            userToRoomIdMap0.remove(userKey);
            userMatchInfoMap0.remove(userKey);
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

	/** 准备请求 0为普通 1为新模式
	 * @throws BaseException */
	public void onRequest(String token ,PlayerReadyRequest request) throws BaseException {
	    int type = request.getType(); 
	    if(type == 0) {
    		BattleRoom battleRoom = null;
    		String userKey = token;
    		Long roomId = userToRoomIdMap.get(userKey);
    		UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
    		if (roomId != null) {
    			battleRoom = battleRoomMap.get(roomId);
    		}
    		if (battleRoom == null || userMatchInfo == null) {
                logger.warn("PLAYER_NOT_MATCH_SUCCESS userName:{}",token);
                throw new BaseException(ErrorCode.PLAYER_NOT_MATCH_SUCCESS_VALUE); 
    		}
    		battleRoom.doRequest(userMatchInfo, request);
	    } else {
	        ChessRoom chessRoom = null;
	        String userKey = token;
	        Long roomId = userToRoomIdMap0.get(userKey);
	        User user  = userMatchInfoMap0.get(userKey);
            if (roomId != null) {
                chessRoom = chessRoomMap.get(roomId);
            }
            if (chessRoom == null || user == null) {
                logger.warn("PLAYER_NOT_MATCH_SUCCESS userName:{}",token);
                throw new BaseException(ErrorCode.PLAYER_NOT_MATCH_SUCCESS_VALUE); 
            }	         
	    }
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
	public PlayerPaintingEndResponse onRequest(String token) {
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
		return battleRoom.doRequest(userMatchInfo);
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
	
	/**玩家交互的请求
	 * @throws BaseException */
	public void onMutually(int type ,User user) throws BaseException {
	    BattleRoom battleRoom = null;
	    String userKey = user.getUserName();
        Long roomId = userToRoomIdMap.get(userKey);
        UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
        if (roomId != null) {
          battleRoom = battleRoomMap.get(roomId);
        }
        if (battleRoom == null || userMatchInfo == null) {
            //游戏已经结束
            logger.error("PLAYER_ROOM_NOT_HAVA userId:{}", userMatchInfo.getUid());
            throw new BaseException(ErrorCode.PLAYER_ROOM_NOT_HAVA_VALUE);
        }
        battleRoom.onMutually(type,userMatchInfo);
	}
	
	/**玩家交互的请求
	 * @throws Exception */
	public void MutuallyFeedback(User user, boolean agree) throws Exception {
        BattleRoom battleRoom = null;
        String userKey = user.getUserName();
        Long roomId = userToRoomIdMap.get(userKey);
        UserMatchInfo userMatchInfo = userMatchInfoMap.get(userKey);
        if (roomId != null) {
          battleRoom = battleRoomMap.get(roomId);
        }
        if (battleRoom == null || userMatchInfo == null) {
            //游戏已经结束
            logger.error("PLAYER_ROOM_NOT_HAVA userId:{}", userMatchInfo.getUid());
            throw new BaseException(ErrorCode.PLAYER_ROOM_NOT_HAVA_VALUE);
        }
        battleRoom.onMutuallyFeedback(agree,userMatchInfo);
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
