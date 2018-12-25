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
import com.zyd.common.proto.client.WarChess.OnePlayerReady;
import com.zyd.common.proto.client.WarChess.PlayNextPush;
import com.zyd.common.proto.client.WarChess.PlayerMes;
import com.zyd.common.proto.client.WarChess.PlayerNotReady;
import com.zyd.common.proto.client.WarChess.PlayerReadyFinishedPush;
import com.zyd.common.proto.client.WarChess.PlayerReadyRequest;
import com.zyd.common.proto.client.WarChess.PlayerStartPush;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.CommonService;
import com.zyd.demo.common.enumuration.PushReqestName;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.round.pojo.UserMatchInfo;
import com.zyd.demo.round.service.BattleConfig;
import com.zyd.demo.round.service.BattleRoomManager;
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
    //
    private CommonService commonService;
    // 房间操作锁
    private Lock lock = new ReentrantLock();
    private int currentPlayNum = 0;
    //玩家1棋子index——type
    private Map<Integer, String> userOne = new HashMap<>();
    //玩家2棋子index——type
    private Map<Integer, String> userTwo = new HashMap<>();
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
    private int winUserId = 0;
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
            User u = user;
            builder.addPlayerMes(playerBuilder);
        }
        builder.setUserId(startUserId);
        builder.setRoomId(roomId);
        //信息的推送
        disrupAll(PushReqestName.PlayerStartPush, builder.build());
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
                    giveUpUserId = userMatchInfoList.get(actor).getId();
                    winUserId = userMatchInfoList.get(nextActorIndex()).getId();
                    isGiveUp = 2;
//                    battleFinished(userMatchInfoList.get(nextActorIndex()).getId());

                } else if (battleStatus.equals(BattleStatus.fightWaiting)) {
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
    
    //初始化棋盘初始信息
    private void initChess() {
      // TODO Auto-generated method stub
      
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
        start, fighting, fightWaiting, waitFinish, finished,mutually
    }
}
