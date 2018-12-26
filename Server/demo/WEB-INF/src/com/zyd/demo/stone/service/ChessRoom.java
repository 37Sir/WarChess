package com.zyd.demo.stone.service;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.MessageLite;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.WarChess.ActiveInfo;
import com.zyd.common.proto.client.WarChess.BattleMes;
import com.zyd.common.proto.client.WarChess.NewServerBattleMesPush;
import com.zyd.common.proto.client.WarChess.OnePlayerReady;
import com.zyd.common.proto.client.WarChess.PlayNextPush;
import com.zyd.common.proto.client.WarChess.PlayerActiveRequest;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesRequest;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesResponse;
import com.zyd.common.proto.client.WarChess.PlayerCanNextPush;
import com.zyd.common.proto.client.WarChess.PlayerCanPaintingPush;
import com.zyd.common.proto.client.WarChess.PlayerEndPush;
import com.zyd.common.proto.client.WarChess.PlayerMes;
import com.zyd.common.proto.client.WarChess.PlayerNotReady;
import com.zyd.common.proto.client.WarChess.PlayerPaintingEndResponse;
import com.zyd.common.proto.client.WarChess.PlayerReadyFinishedPush;
import com.zyd.common.proto.client.WarChess.PlayerReadyRequest;
import com.zyd.common.proto.client.WarChess.PlayerStartPush;
import com.zyd.common.proto.client.WarChess.ServerBattleMesPush;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.CommonService;
import com.zyd.demo.common.enumuration.PushReqestName;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.round.pojo.UserMatchInfo;
import com.zyd.demo.round.service.BattleConfig;
import com.zyd.demo.round.service.BattleRoomManager;
import com.zyd.demo.round.service.ChessService;
import com.zyd.demo.user.pojo.User;

public class ChessRoom {
    // 战斗房间的id
    public long roomId;
    // 对战成员
    private List<User> userMatchInfoList;
    // 当前动作玩家的索引
    private int actor;
    // 当前操作时间
    private long nextTime = 0;
    //棋子移动后的等待时间
    private long waitTime = 0;    
    //
    private CommonService commonService;
    // 房间操作锁
    private Lock lock = new ReentrantLock();
    private int currentPlayNum = 0;
    //玩家1棋子index——type
    private Map<Integer, String> userOne = new HashMap<>();
    //玩家2棋子index——type
    private Map<Integer, String> userTwo = new HashMap<>();
    //玩家1可以召唤的位置
    private Map<Integer, String> userOneCan = new HashMap<>();
    //玩家2可以召唤的位置
    private Map<Integer, String> userTwoCan = new HashMap<>();
    //玩家刚召唤的棋子
    private Map<Integer, String> userNow = new HashMap<>();
    //玩家已经移动过的棋子
    private Map<Integer, String> userHavaMove = new HashMap<>();
    // 是否可以将房间移出
    public boolean canRemove = false;
    // 房间状态
    public BattleStatus battleStatus = BattleStatus.start;
    // 开始的玩家userId
    private int startUserId;
    //后手玩家ID
    private int afterUserId;
    // 完成操作的玩家
    private Map<Integer, Object> dealUserMap = new HashMap<>();    
    // 房间管理器
    private BattleRoomManager battleRoomManager;
    // 是否是主动放弃结束,0:正常结束，3：游戏错误
    private int isGiveUp = 0;
    // 主动放弃的玩家
    private Integer giveUpUserId = null;
    //胜利玩家Id
    private int winUserId = 0;
    //玩家回合对应的Mp初始值
    private int mp = 1 ;
    //玩家剩余的mp
    private int lastMp = 0;
    //一回合加的mp数量
    private int mpAdd = 1;
    //玩家1王的位置
    int kingOne = 57;
    //玩家2王的位置
    int kingTwo = 8;    
    private static final Logger logger = LoggerFactory.getLogger(ChessRoom.class);
    /** 匹配成功 */
    public void start() {
        nextTime = System.currentTimeMillis() + BattleConfig.startReadyTime;
        actor = 0;
        battleStatus = BattleStatus.start;
        //打乱顺序，避免每次先手方为同一玩家
        Collections.shuffle(userMatchInfoList);
        startUserId = userMatchInfoList.get(actor).getId();
        afterUserId = userMatchInfoList.get(actor+1).getId();
        PlayerStartPush.Builder builder = PlayerStartPush.newBuilder();

        // 构建匹配成功的玩家信息
        for (User user : userMatchInfoList) {
            PlayerMes.Builder playerBuilder = PlayerMes.newBuilder();
            playerBuilder.setUserId(user.getId());
            playerBuilder.setUserName(user.getUserName());
            builder.addPlayerMes(playerBuilder);
        }
        lastMp = mp;
        builder.setUserId(startUserId);
        builder.setRoomId(roomId);
        //信息的推送
        disrupAll(PushReqestName.PlayerStartPush, builder.build());
    }
    //玩家操作的行为
    public void doRequest(User user, PlayerActiveRequest playerActiveRequest) {
        lock.lock();
        try {
            ActiveInfo actInfo = playerActiveRequest.getActiveInfo();
            Map<Integer, String> my = getMyChess(user);
            Map<Integer, String> other = getOtherChess(user);
            int myKing = getMyChessKing(user);
            int otherKing = getOtherKing(user);
            if (battleStatus.equals(BattleStatus.fighting)) {
                logger.warn("PLAYER_ROOM_NOT_FINGHTING userName:{}",user.getUserName());
                throw new BaseException(ErrorCode.PLAYER_ROOM_NOT_FINGHTING_VALUE); 
            }
            if (userMatchInfoList.get(actor).getId() != user.getId()) {
                logger.warn("PLAYER_CALL_NOT_YOUR_ACTIVE userName:{}",user.getUserName());
                throw new BaseException(ErrorCode.PLAYER_CALL_NOT_YOUR_ACTIVE_VALUE);               
            }
            //召唤还是移动
            if (actInfo.getIsCall()) {
                int index = actInfo.getCallInfo().getIndex();
                int type = actInfo.getCallInfo().getType();
                if (my.containsKey(index) || other.containsKey(index)) {
                    logger.warn("PLAYER_INDEX_HAVA_CHESSuserName:{}",user.getUserName());
                    throw new BaseException(ErrorCode.PLAYER_INDEX_HAVA_CHESS_VALUE); 
                }
                //检查召唤位置
                checkIsCanCall(index,user);
                if (ChessService.cost.get(ChessService.getShiftsType(type)) > lastMp) {
                    logger.warn("PLAYER_CALL_NOT_ENOUGH_MP userName:{}",user.getUserName());
                    throw new BaseException(ErrorCode.PLAYER_CALL_NOT_ENOUGH_MP_VALUE);                    
                }
                lastMp -= ChessService.cost.get(ChessService.getShiftsType(type));
                userNow.put(index, ChessService.getShiftsType(type));
                NewServerBattleMesPush.Builder  build = NewServerBattleMesPush.newBuilder();
                build.setActiveInfo(actInfo);
                disrupOne(PushReqestName.NewServerBattleMesPush, userMatchInfoList.get(nextActorIndex()), build.build());
            } else {
                int from = actInfo.getMoveInfo().getFrom();
                int  to = actInfo.getMoveInfo().getTo();
                if (!my.containsKey(from)) {
                    logger.warn("PLAYER_INDEX_NOT_HAVA_CHESS userName:{}",user.getUserName());
                    throw new BaseException(ErrorCode.PLAYER_INDEX_NOT_HAVA_CHESS_VALUE);                  
                }
                if (my.containsKey(to)) {
                    logger.warn("PLAYER_INDEX_HAVA_CHESS userName:{}",user.getUserName());
                    throw new BaseException(ErrorCode.PLAYER_INDEX_HAVA_CHESS_VALUE);                      
                }
                //是否可以达到位置
                if (!ChessService.CheckStoneMove(from, to, other, my)) {
                    logger.warn("PLAYER_INDEX_CANT_MOVE userName:{}",user.getUserName());
                    throw new BaseException(ErrorCode.PLAYER_INDEX_CANT_MOVE_VALUE);                      
                }
                if (userHavaMove.containsKey(from) || userNow.containsKey(from)) {
                    logger.warn("PLAYER_INDEX_CAT_NOT_MOVE userName:{}",user.getUserName());
                    throw new BaseException(ErrorCode.PLAYER_INDEX_CAT_NOT_MOVE_VALUE);                     
                }
                //吃棋还是移动
                if (other.containsKey(to)) {
                    userHavaMove.put(to, my.get(from));
                    my.put(to, my.get(from));
                    my.remove(from);
                    other.remove(to);                    
                } else {
                    userHavaMove.put(to, my.get(from)); 
                    my.put(to, my.get(from));
                    my.remove(from); 
                }
                NewServerBattleMesPush.Builder  build = NewServerBattleMesPush.newBuilder();
                build.setActiveInfo(actInfo);
                if (to == otherKing) {
                    winUserId = user.getId();
                    nextTime = System.currentTimeMillis() + BattleConfig.playReadTime;
                    battleStatus = BattleStatus.waitFinish;
                } else {
                    battleStatus = BattleStatus.fightWaiting;
                    waitTime = System.currentTimeMillis() + BattleConfig.playReadTime;                  
                }
                disrupOne(PushReqestName.NewServerBattleMesPush, userMatchInfoList.get(nextActorIndex()), build.build());
            }
        } catch (Exception e) {
            e.printStackTrace();
            logger.error("", e);
        } finally {
            lock.unlock();
        }
    
    }
    //玩家主动结束回合
    public void endRound(User user) {
        lock.lock();
        try {
            if (user.getId() == afterUserId) {
                mp += mpAdd;
            }
            lastMp = mp;
            userNow.clear();
            userHavaMove.clear();
            actor = nextActorIndex();
            battleStatus = BattleStatus.roundWaiting;
            nextTime = System.currentTimeMillis() + BattleConfig.playReadTime;
            disrupAll(PushReqestName.PlayerCanPaintingPush, PlayerCanPaintingPush.newBuilder().build());                                    
        } catch (Exception e) {
          e.printStackTrace();
          logger.error("", e);
        } finally {
          lock.unlock();
        }   
    }
    private int getOtherKing(User user) {
        int s = 0;
        if (user.getId() == startUserId) {
            s = kingTwo;
        } else {
            s= kingOne;
        }      
        return s;
    }
    private int getMyChessKing(User user) {
        int s = 0;
        if (user.getId() == startUserId) {
            s = kingOne;
        } else {
            s= kingTwo;
        }
        return s;
    }
    //位置是否可以召唤
    private void checkIsCanCall(int index, User user) throws BaseException {
        if(user.getId() == startUserId) {
            if (!userOneCan.containsKey(index)) {
                logger.warn("PLYER_INDEX_CANT_CALL userName:{}",user.getUserName());
                throw new BaseException(ErrorCode.PLYER_INDEX_CANT_CALL_VALUE);                
            }
        } else {
            if (!userTwoCan.containsKey(index)) {
                logger.warn("PLYER_INDEX_CANT_CALL userName:{}",user.getUserName());
                throw new BaseException(ErrorCode.PLYER_INDEX_CANT_CALL_VALUE);                
            }            
        }
    }
    //获得自己的棋子
    private Map<Integer, String> getOtherChess(User user) {
        Map<Integer, String> map = new HashMap<>();
        if (user.getId() == startUserId) {
            map = userOne;
        } else {
            map = userTwo;
        }
        return map;
    }
    //获得对方的棋子
    private Map<Integer, String> getMyChess(User user) {
        Map<Integer, String> map = new HashMap<>();
        if (user.getId() == startUserId) {
            map = userTwo;
        } else {
            map = userOne;
        }        
        return map;
    }
    /** 更新方法,如果玩家操作超时 */
    public void onUpdate() {
        if (nextTime > System.currentTimeMillis()) {
            return;
        }
        lock.lock();
        try {
            if (System.currentTimeMillis() > nextTime) {
                // 根据游戏的状态进行超时处理
                if (battleStatus.equals(BattleStatus.start)) {
                    //有玩家长时间未准备，则退出此次匹配
                    havaPlayerNotReady();
                } else if (battleStatus.equals(BattleStatus.fighting)) {
                    actor = nextActorIndex();
                    battleStatus = BattleStatus.roundWaiting;
                    nextTime = BattleConfig.playReadTime;
                    if (userMatchInfoList.get(actor).getId() == afterUserId) {
                        mp += mpAdd;
                    }
                    lastMp = mp;
                    userNow.clear();
                    userHavaMove.clear();
                    actor = nextActorIndex();
                    battleStatus = BattleStatus.roundWaiting;
                    nextTime = System.currentTimeMillis() + BattleConfig.playReadTime;
                    disrupAll(PushReqestName.PlayerCanPaintingPush, PlayerCanPaintingPush.newBuilder().build());                    
                } else if (battleStatus.equals(BattleStatus.roundWaiting)) {
                    battleStatus = BattleStatus.fighting;
                    nextTime = BattleConfig.chessTime;
                    disrupAll(PushReqestName.PlayNextPush, PlayNextPush.newBuilder().build()); 
                } else if (battleStatus.equals(BattleStatus.waitFinish)) {
                    battleFinished(winUserId);
                }
            }
            if (battleStatus.equals(BattleStatus.fightWaiting)) {
                if (System.currentTimeMillis() > waitTime) {
                    dealUserMap.clear();
                    battleStatus = BattleStatus.fighting;
                    disrupOne(PushReqestName.PlayerCanNextPush, userMatchInfoList.get(actor), PlayerCanNextPush.newBuilder().build());
                } 
            }
        } catch (Exception e) {
            logger.error("", e);
        } finally {
            lock.unlock();
        }
    }
    
    /** 战斗结束 
     * @throws Exception */
    public void battleFinished(int userId) throws Exception {
        PlayerEndPush.Builder builder = PlayerEndPush.newBuilder();

        builder.setResult(0);
        builder.setWinUserId(userId);
        // 推送结束信息
        disrupAll(PushReqestName.PlayerEndPush, builder.build());        
        battleStatus = BattleStatus.finished;
        canRemove = true;
        battleRoomManager.removeChessRoom(this);
        return;
    }
    
      /**玩家未准备退出房间和匹配,并将未准备玩家信息发送给客户端*/
     private void havaPlayerNotReady() {
         PlayerNotReady.Builder playerNotReady = PlayerNotReady.newBuilder();
         for ( User user :userMatchInfoList) {
             if (!dealUserMap.containsKey(user.getId())) {
                 playerNotReady.addUserId(user.getId());
             }
         }
         //将信息推送给玩家
         disrupAll(PushReqestName.PlayerNotReady, playerNotReady.build());
         
         battleStatus = BattleStatus.finished;
         canRemove = true;
         battleRoomManager.removeChessRoom(this);
         return;
     }
     
     /** 处理玩家的开始请求 */
     public void doRequest(User user,PlayerReadyRequest req) {
         lock.lock();
         try {
             if (!battleStatus.equals(BattleStatus.start)) {
                 logger.warn("PLAYER_NOT_START userId:{}",user.getId());
                 throw new BaseException(ErrorCode.PLAYER_NOT_START_VALUE);
             }
             dealUserMap.put(user.getId(), new Object());
             //将玩家点击准备的动作发给对战的客户端
             OnePlayerReady.Builder onePlayerReady = OnePlayerReady.newBuilder();
             onePlayerReady.setUserId(user.getId());
             User toUserMatchInfo = null; 
             for (User u : userMatchInfoList) {
                 if (u.getId() != user.getId()) {
                     toUserMatchInfo = u;
                 }
             }
             disrupOne(PushReqestName.OnePlayerReady, toUserMatchInfo, onePlayerReady.build());
             
             // 把所有人准备并且遍历所有玩家,如果所有玩家都准备发送准备完成推送
             if (dealUserMap.size() == userMatchInfoList.size()) {
                 battleStatus = BattleStatus.fighting;
                 dealUserMap.clear();
                 initChess();
                 nextTime = System.currentTimeMillis() + BattleConfig.chessTime;
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
     /** 处理玩家回合结束的动画处理完成请求 */
     public PlayerPaintingEndResponse doRequest(User user) {
         lock.lock();
         try {
             if (battleStatus.equals(BattleStatus.fightWaiting)) {
                 dealUserMap.put(user.getId(), new Object());
                 if (dealUserMap.size() == userMatchInfoList.size()) {
                     dealUserMap.clear();
                     battleStatus = BattleStatus.fighting;
                     disrupOne(PushReqestName.PlayerCanNextPush, userMatchInfoList.get(actor), PlayerCanNextPush.newBuilder().build());
                 }
             } else if (battleStatus.equals(BattleStatus.roundWaiting)) {
                 dealUserMap.put(user.getId(), new Object());
                 if (dealUserMap.size() == userMatchInfoList.size()) {
                     battleStatus = BattleStatus.fighting;
                     nextTime = System.currentTimeMillis() + BattleConfig.chessTime;
                     disrupAll(PushReqestName.PlayNextPush, PlayNextPush.newBuilder().build());
                 }               
             } else if (battleStatus.equals(BattleStatus.waitFinish)) {
                 battleFinished(winUserId);
             }
         } catch (Exception e) {
             logger.error("", e);
         } finally {
             lock.unlock();
         }
         return PlayerPaintingEndResponse.newBuilder().build();
     }
    //初始化棋盘初始信息
    private void initChess() {
        //玩家1可以召唤的位置
        userOneCan.put(49, "49");
        userOneCan.put(50, "50");
        userOneCan.put(58, "58");
        //玩家2可以召唤的位置
        userTwoCan.put(7, "7");
        userTwoCan.put(15, "15");
        userTwoCan.put(16, "16");
        
    }

    public void disrupAll(PushReqestName pushRequestName, MessageLite messageLite) {
        List<String> userTokenList = new ArrayList<>();
        for (User user : userMatchInfoList) {
            userTokenList.add(user.getUserName());
        }
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
    public void disrupOne(PushReqestName pushRequestName, User user, MessageLite messageLite) {
        List<String> userTokenList = new ArrayList<>();
        userTokenList.add(user.getUserName());
        commonService.addProxyPushData(pushRequestName, new Packet(messageLite), userTokenList, messageLite.toString());
    }
    public List<User> getUserMatchInfoList() {
        return userMatchInfoList;
    }

    public void setUserMatchInfoList(List<User> userMatchInfoList) {
        this.userMatchInfoList = userMatchInfoList;
    }
    
    public ChessRoom(BattleRoomManager battleRoomManager, CommonService commonService,
        User... users) {
        this.battleRoomManager = battleRoomManager;
        this.commonService = commonService;
        userMatchInfoList = new ArrayList<>();
        for (User userMatchInfo : users) {
            userMatchInfoList.add(userMatchInfo);
        }
    }
    //房间状态
    enum BattleStatus {
        start, fighting, fightWaiting, roundWaiting,waitFinish, finished
    }

}