package com.zyd.demo.round.service;

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
import com.zyd.common.proto.client.WarChess.BattleMes;
import com.zyd.common.proto.client.WarChess.PlayerEndPush;
import com.zyd.common.proto.client.WarChess.FairBattleLevelEndRequest;
import com.zyd.common.proto.client.WarChess.FairBattleLevelEndResponse;
import com.zyd.common.proto.client.WarChess.PlayerReadyFinishedPush;
import com.zyd.common.proto.client.WarChess.PlayerStartPush;
import com.zyd.common.proto.client.WarChess.PlayerUndoInfoPush;
import com.zyd.common.proto.client.WarChess.PlayerUndoPush;
import com.zyd.common.proto.client.WarChess.OnePlayerReady;
import com.zyd.common.proto.client.WarChess.PlayNextPush;
import com.zyd.common.proto.client.WarChess.PlayUndoNextPush;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesRequest;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesResponse;
import com.zyd.common.proto.client.WarChess.PlayerMes;
import com.zyd.common.proto.client.WarChess.PlayerNotAgreePush;
import com.zyd.common.proto.client.WarChess.PlayerNotReady;
import com.zyd.common.proto.client.WarChess.PlayerPaintingEndResponse;
import com.zyd.common.proto.client.WarChess.PlayerReadyRequest;
import com.zyd.common.proto.client.WarChess.PlayerRequireBattleMesAgainRequest;
import com.zyd.common.proto.client.WarChess.PlayerRequireBattleMesAgainResponse;
import com.zyd.common.proto.client.WarChess.ServerBattleMesPush;
import com.zyd.common.proto.client.WarChess.UndoInfo;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.CommonService;
import com.zyd.demo.common.enumuration.PushReqestName;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.round.pojo.UserMatchInfo;
import com.zyd.demo.user.pojo.User;


// 对战房间
public class BattleRoom {
	// 战斗房间的id
	public long roomId;
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
	public List<BattleMes> serverBattleMesList = new ArrayList<>();
	// 当前操作桢的索引
	private int currentPlayNum = 0;
	//玩家1棋子index——type
	private Map<Integer, String> userOne = new HashMap<>();
	//玩家2棋子index——type
    private Map<Integer, String> userTwo = new HashMap<>();
	// 结束阶段发送的用户 Uid-FairBattleLevelEndRequest
	private Map<Integer, FairBattleLevelEndRequest> battleEndRequestMap = new HashMap<>();
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
	//吃掉的棋子回合-（userID_位置_棋子类型）
	private Map<Integer, String> captureMap = new HashMap<>();
	//兵升变的回合(回合-userId_from)
	private Map<Integer, String> promotionMap = new HashMap<>();
	// 房间管理器
	private BattleRoomManager battleRoomManager;
	// 是否是主动放弃结束,0:正常结束，1：有，2：超时 3：游戏错误 4 逼和 5平局
	private int isGiveUp = 0;
	// 主动放弃的玩家
	private Integer giveUpUserId = null;
	private int winUserId = 0;
	//玩家1王的位置
	int kingOne = 61;
	//玩家2王的位置
	int kingTwo = 5;
	//玩家被连续将军的次数(userID-count)
	Map<Integer, Integer> genera = new HashMap<>();
	//玩家1是否被将军
	boolean jiangjunOne = false;
	//顽家2是否被将军
	boolean jiangjunTwo = false;
	//请求交互方ID
	private int undoUserId;
	//玩家交互状态（1位悔棋 2为和局）
	private int mutaully = 0;
	//交互时的剩余时间
	private int lastTime  = 0;
	
	private static final Logger logger = LoggerFactory.getLogger(BattleRoom.class);

	/** 战斗开始 */
	public void start() {
		nextTime = System.currentTimeMillis() + BattleConfig.startReadyTime;
		actor = 0;
		battleStatus = BattleStatus.start;
		//打乱顺序，避免每次先手方为同一玩家
		Collections.shuffle(userMatchInfoList);
		startUserId = userMatchInfoList.get(actor).getUid();
		afterUserId = userMatchInfoList.get(actor+1).getUid();
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

	/** 处理玩家发送的桢请求 */
	public PlayerBattleMesResponse doRequest(UserMatchInfo userMatchInfo,
			PlayerBattleMesRequest playerBattleMesRequest) {
		lock.lock();
		try {
            PlayerBattleMesResponse.Builder battleMesResponse = PlayerBattleMesResponse.newBuilder();
			if (!battleStatus.equals(BattleStatus.fighting)) {
                isGiveUp = 3;
                battleFinished(winUserId);
				battleMesResponse.setRes(false);
                battleMesResponse.setError("非法操作1！");
				return battleMesResponse.build();
			}
			BattleMes battleMes = playerBattleMesRequest.getBattleMes();
			// 如果关键帧不是自增的
			if (battleMes.getPlayNum() != currentPlayNum + 1) {
			    isGiveUp = 3;
			    battleFinished(winUserId);
				battleMesResponse.setRes(false);
                battleMesResponse.setError("非法操作2！");
				return battleMesResponse.build();
			}

			if (!userMatchInfo.getUid().equals(userMatchInfoList.get(actor).getUid())) {
                isGiveUp = 3;
                battleFinished(winUserId);			  
				battleMesResponse.setRes(false);
				battleMesResponse.setError("非法操作3！");
				return battleMesResponse.build();
			}
	        int from = battleMes.getFrom();
	        int to = battleMes.getTo();
	        int promotion = battleMes.getPromption();
	        int otherUserId = userMatchInfoList.get(nextActorIndex()).getUid();
	        Map<Integer, String> OtherUserChess = getOtherPlayerChess(userMatchInfo.getUid());
	        Map<Integer, String> myChess = getNowPlayerChess(userMatchInfo.getUid());
	        Boolean isFirst = false;
	        if (userMatchInfo.getUid().intValue() != startUserId)  isFirst = true;
//	        checkFirstPlayer(OtherUserChess,myChess,userMatchInfo.getUid(),isFirst);
	        //操作位置上是否有棋子
	        if (!myChess.containsKey(from)) {
                isGiveUp = 3;
                battleFinished(winUserId);	          
                battleMesResponse.setRes(false);
                battleMesResponse.setError("操作位置无棋子");
                return battleMesResponse.build();
	        }
	        //兵升变的条件是否满足
	        if (promotion > 0) {
	            int second = 0; 
	            if (otherUserId == startUserId) {
	                second = 6;
	            } else {
	                second = 1;
	            }
                if (ChessService.getRank(from) != second
                    || myChess.get(from) != "p" || promotion >= 5){
                    isGiveUp = 3;
                    battleFinished(winUserId);            
                    battleMesResponse.setRes(false);
                    battleMesResponse.setError("不满足兵升变条件");
                    return battleMesResponse.build();                  
                }
	        }
	        //移动还是吃子
			if (!userOne.containsKey(to) && !userTwo.containsKey(to)) {
			    //是否移动成功
			    if (!ChessService.checkMove(from, to, OtherUserChess, myChess, isFirst)) {
		             battleMesResponse.setRes(false);
		             battleMesResponse.setError("移动操作有误");
		             return battleMesResponse.build();
			    } else {
			          myChess.put(to, myChess.get(from));
			          if (myChess.get(from) == "k") {
			              if (userMatchInfo.getUid() == startUserId) {
			                  kingOne = to;  
			              } else {
			                  kingTwo = to;
			              }
			          }
			          myChess.remove(from);
			    }
			} else {
			    //是否成功吃子
			    if (!ChessService.CheckNotPawnMove(from, to, OtherUserChess, myChess)) {
                    battleMesResponse.setRes(false);
                    battleMesResponse.setError("吃子操作有误");
                    return battleMesResponse.build();
			    } else {
                    myChess.put(to, myChess.get(from));
                    if (myChess.get(from) == "k") {
                        if (userMatchInfo.getUid() == startUserId) {
                            kingOne = to;  
                        } else {
                            kingTwo = to;
                        }
                   }
                   captureMap.put(currentPlayNum,String.valueOf(otherUserId)+
                       "_"+String.valueOf(to) + "_" + OtherUserChess.get(to));
                   myChess.remove(from);
                   OtherUserChess.remove(to);
			    }
			}
			//操作方国王位置
            int myKing = 0;
            //对方国王的位置
            int otherKing = 0;
            if (userMatchInfo.getUid() == startUserId) {
                myKing = kingOne;
                otherKing = kingTwo;
            } else {
                myKing = kingTwo;
                otherKing = kingOne;
            }
            //移动后自己是否存在被将军的情况
            if (ChessService.attacked(OtherUserChess, myChess, myKing, isFirst)) {
                isGiveUp = 3;
                battleFinished(winUserId);
                battleMesResponse.setRes(false);
                battleMesResponse.setError("移动后会被将军");
                return battleMesResponse.build();
            }
            //处理兵升变的情况
            doPromotion(to, promotion, myChess);
            promotionMap.put(currentPlayNum, userMatchInfo.getUid() +"_"+ from);
            battleStatus = BattleStatus.fightWaiting;
            nextTime = System.currentTimeMillis() + BattleConfig.playReadTime;
            currentPlayNum = currentPlayNum + 1;
            //长将的处理
            checkGenera(otherUserId, OtherUserChess, myChess, isFirst, otherKing); 
            // 保存桢
            saveBattleMes(battleMes);
            
            ServerBattleMesPush.Builder builder = ServerBattleMesPush.newBuilder();
            builder.setBattleMes(battleMes);
            builder.setCurrentTime(System.currentTimeMillis());
            builder.setNextTime(nextTime);
            
            // 操作信息推送
//            disrupAll(PushReqestName.ServerBattleMesPush, builder.build());
            disrupOne(PushReqestName.ServerBattleMesPush, userMatchInfoList.get(nextActorIndex()), builder.build());
            //国王被吃，游戏结束(正常结束)
            if (!OtherUserChess.containsKey(otherKing)) {
                battleStatus = BattleStatus.waitFinish;
                winUserId = userMatchInfoList.get(actor).getUid();
            }
            //对方是否无棋可走
            List<String> move = ChessService.generate_moves(myChess,OtherUserChess,otherKing, !isFirst);
            //如果对方无棋可走，游戏结束
            if (move.size() == 0) {
                if (ChessService.attacked(myChess,OtherUserChess,  otherKing, !isFirst)) {
                    battleStatus = BattleStatus.waitFinish;
                    winUserId = userMatchInfoList.get(actor).getUid(); 
                } else {
                    isGiveUp = 4;
                    battleStatus = BattleStatus.waitFinish;
                }
                
            }
            // 下一步操作赋值
            actor = nextActorIndex();
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
	/**长将的处理*/
    public void checkGenera(int otherUserId, Map<Integer, String> OtherUserChess,
        Map<Integer, String> myChess, Boolean isFirst, int otherKing) {
        //移动后对方是否存在被将军的情况
        if (ChessService.attacked(myChess,OtherUserChess,  otherKing, !isFirst)) {
            genera.put(otherUserId, genera.get(otherUserId) +1 );   
            
        } else {
            genera.put(otherUserId, 0 );
        }
        //连续将军次数大于5次，则平局
        if (genera.get(otherUserId).intValue() >= 5) {
            isGiveUp = 5 ;
            battleStatus = BattleStatus.waitFinish;
        }
    }
	/**处理兵升变*/
    public void doPromotion(int to, int promotion, Map<Integer, String> myChess) {
        if (promotion > 0 && myChess.get(to) == "p") {
            //移动后的兵是否在第1行或者第8行
            if (ChessService.getRank(to) == 0 || ChessService.getRank(to) == 7){
                String type = ChessService.getShiftsType(promotion);
                myChess.put(to, type);
            }
        }
    }
	
	/**判断等待的人是否为先手方*/
	private void checkFirstPlayer(Map<Integer, String> otherUserChess, Map<Integer, String> myChess,
        Integer uid, Boolean isFirst) {
	    if ( uid.intValue() != startUserId) {
	        otherUserChess = userOne;
	        myChess = userTwo;
	        isFirst = true;
	    } else {
	        otherUserChess = userTwo;
	        myChess = userOne;
	        isFirst = false;
	    }
    }
	/**得到操作方的棋子*/
	private Map<Integer, String> getNowPlayerChess(Integer uid) {
	    if (uid.intValue() == startUserId) {
	        return userOne;
	    }
	    return userTwo;
	}
	/**得到对方棋子*/
    private Map<Integer, String> getOtherPlayerChess(Integer uid) {
      if (uid.intValue() == startUserId) {
          return userTwo;
      }
      return userOne;
    }	
    /** 处理玩家发送的桢处理完成请求 */
	public PlayerPaintingEndResponse doRequest(UserMatchInfo userMatchInfo) {
		lock.lock();
		try {
			if (battleStatus.equals(BattleStatus.fightWaiting)) {
				dealUserMap.put(userMatchInfo.getUid(), new Object());
				if (dealUserMap.size() == userMatchInfoList.size()) {
				    if (mutaully == 0) {
    					dealUserMap.clear();
    					nextTime = System.currentTimeMillis() + BattleConfig.playTime;
    					battleStatus = BattleStatus.fighting;
    					disrupAll(PushReqestName.PlayNextPush, PlayNextPush.newBuilder().build());
				    } else {
				         //悔棋动作结束的推送
				         mutaully = 0;
	                     dealUserMap.clear();
	                     nextTime = System.currentTimeMillis() + BattleConfig.playTime;
	                     battleStatus = BattleStatus.fighting;
	                     disrupAll(PushReqestName.PlayUndoNextPush, PlayUndoNextPush.newBuilder().build()); 
				    }
				}
			} else if (battleStatus.equals(BattleStatus.waitFinish)) {
                dealUserMap.put(userMatchInfo.getUid(), new Object());
                if (dealUserMap.size() == userMatchInfoList.size()) {
                    battleFinished(winUserId);
                }			    
			}
		} catch (Exception e) {
			logger.error("", e);
		} finally {
			lock.unlock();
		}
		return PlayerPaintingEndResponse.newBuilder().build();
	}

	/** 战斗结束请求 */
	public FairBattleLevelEndResponse doRequest(UserMatchInfo userMatchInfo,
			FairBattleLevelEndRequest fairBattleLevelEndRequest) {
		lock.lock();
		try {

			// 主动结束
			if (fairBattleLevelEndRequest.getIsGiveUp()) {
				this.isGiveUp = 1;
				this.giveUpUserId = userMatchInfo.getUid();
				// 战斗结束
//				battleFinished();
				return FairBattleLevelEndResponse.newBuilder().build();
			}

			battleStatus = BattleStatus.waitFinish;
			battleEndRequestMap.put(userMatchInfo.getUid(), fairBattleLevelEndRequest);
			if (battleEndRequestMap.size() >= 2) {
				// 战斗结束
//				battleFinished();
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
	public void battleFinished(int userId) {
		PlayerEndPush.Builder builder = PlayerEndPush.newBuilder();
		// 所有的桢信息
//		for (BattleMes battleMes : serverBattleMesList) {
//			builder.addBattleMes(battleMes);
//		}
		//正常结束
		User u1 = commonService.getUserById(startUserId);
		User u2 = commonService.getUserById(afterUserId);
		if (isGiveUp == 0) {
		    if (u1.getId() == userId) {
		        u1.setWinCount(u1.getWinCount()+1);
		        u1.setRank(u1.getRank() + 20);
		        u2.setLoseCount(u2.getLoseCount()+1);
		        u2.setRank(u2.getRank() - 20);
		    } else {
                u2.setWinCount(u2.getWinCount()+1);
                u1.setRank(u1.getRank() - 20);
                u1.setLoseCount(u1.getLoseCount()+1);
                u2.setRank(u2.getRank() + 20);
		    }
		    builder.setWinRank(20);
		    builder.setLoseRank(20);
			builder.setResult(0);
			builder.setWinUserId(userId);
			// 推送结束信息
			disrupAll(PushReqestName.PlayerEndPush, builder.build());
		} else if (isGiveUp == 2) {
            if (u1.getId() == userId) {
                u1.setWinCount(u1.getWinCount()+1);
                u1.setRank(u1.getRank() + 20);
                u2.setLoseCount(u2.getLoseCount()+1);
                u2.setRank(u2.getRank() - 20);
            } else {
                u2.setWinCount(u2.getWinCount()+1);
                u1.setRank(u1.getRank() - 20);
                u1.setLoseCount(u1.getLoseCount()+1);
                u2.setRank(u2.getRank() + 20);
            }
            for (UserMatchInfo userMatchInfo : userMatchInfoList) {
                if (userMatchInfo.getUid().equals(giveUpUserId)) {
                    builder.setWinUserId(winUserId);
                    builder.setWinRank(20);
                    builder.setLoseRank(20);
                    builder.setResult(3);
                } else {
                    builder.setWinUserId(winUserId);
                    builder.setWinRank(20);
                    builder.setLoseRank(20);
                    builder.setResult(4);
                }
                disrupOne(PushReqestName.PlayerEndPush, userMatchInfo, builder.build());
            }		    
		} else if (isGiveUp == 3) {
		    builder.setResult(5);
            disrupAll(PushReqestName.PlayerEndPush, builder.build());
		} else if (isGiveUp == 4) {
            u1.setDrawCount(u1.getDrawCount()+1);
            u2.setDrawCount(u2.getDrawCount()+1);
            builder.setResult(6);
            disrupAll(PushReqestName.PlayerEndPush, builder.build());		    
		} else if (isGiveUp == 5) {
		    u1.setDrawCount(u1.getDrawCount()+1);
		    u2.setDrawCount(u2.getDrawCount()+1);
            builder.setResult(7);
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
        commonService.updateUser(u1);
        commonService.updateUser(u2);
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
				// 根据游戏的状态进行超时处理
				if (battleStatus.equals(BattleStatus.start)) {
				    //有玩家长时间未准备，则退出此次匹配
				    havaPlayerNotReady();
				} else if (battleStatus.equals(BattleStatus.fighting)) {
  				    giveUpUserId = userMatchInfoList.get(actor).getUid();
  				    winUserId = userMatchInfoList.get(nextActorIndex()).getUid();
  				    isGiveUp = 2;
				    battleFinished(userMatchInfoList.get(nextActorIndex()).getUid());
				} else if (battleStatus.equals(BattleStatus.fightWaiting)) {
				    if (mutaully == 0) {
    					dealUserMap.clear();
    					nextTime = System.currentTimeMillis() + BattleConfig.playTime;
    					battleStatus = BattleStatus.fighting;
    					disrupAll(PushReqestName.PlayNextPush, PlayNextPush.newBuilder().build());
				    } else {
                        dealUserMap.clear();
                        mutaully = 0 ;
                        nextTime = System.currentTimeMillis() + lastTime;
                        battleStatus = BattleStatus.fighting;
                        disrupAll(PushReqestName.PlayUndoNextPush, PlayUndoNextPush.newBuilder().build());
				    }
				} else if (battleStatus.equals(BattleStatus.waitFinish)) {
				    battleFinished(winUserId);
				} else if (battleStatus.equals(BattleStatus.mutually)) {
				    battleStatus = BattleStatus.fighting;
				    mutaully = 0;
				    nextTime = System.currentTimeMillis() + lastTime;
                    disrupAll(PushReqestName.PlayerNotAgreePush, PlayerNotAgreePush.newBuilder().build());
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
	/**玩家的交互请求
	 * @throws BaseException */
    public void onMutually(int type, UserMatchInfo userMatchInfo) throws BaseException {
        //1为悔棋
        if (type == 1) {
            if (userMatchInfo.getUid().intValue() == startUserId) {
                if (currentPlayNum < 2 || currentPlayNum % 2 != 0) {
                    //不符合悔棋条件
                    logger.error("PLAYER_CAN_NOT_UNDO_VALUE userId:{}", userMatchInfo.getUid());
                    throw new BaseException(ErrorCode.PLAYER_CAN_NOT_UNDO_VALUE);
                }
                lastTime = (int)(nextTime - System.currentTimeMillis());                
            } else {
                if (currentPlayNum < 3 || currentPlayNum % 2 != 1) {
                    //不符合悔棋条件
                    logger.error("PLAYER_CAN_NOT_UNDO_VALUE userId:{}", userMatchInfo.getUid());
                    throw new BaseException(ErrorCode.PLAYER_CAN_NOT_UNDO_VALUE);
                }
                lastTime = (int)(nextTime - System.currentTimeMillis());                
            }
            battleStatus = BattleStatus.mutually;
            nextTime = System.currentTimeMillis() + BattleConfig.unDoTime;
            mutaully = 1;
            undoUserId = userMatchInfo.getUid();
            PlayerUndoPush.Builder p = PlayerUndoPush.newBuilder();
            p.setType(1);
            disrupOne(PushReqestName.PlayerUndoPush, userMatchInfoList.get(nextActorIndex()), p.build());
          //2为和局
        } else if (type == 2) {
            nextTime = System.currentTimeMillis() + BattleConfig.unDoTime;
            mutaully = 2;
            undoUserId = userMatchInfo.getUid();
            battleStatus = BattleStatus.mutually;
            //推送玩家和局的请求
            PlayerUndoPush.Builder p = PlayerUndoPush.newBuilder();
            p.setType(2);
            disrupOne(PushReqestName.PlayerUndoPush, userMatchInfoList.get(nextActorIndex()), p.build());
        }
    }
    /**悔棋回复棋盘的方法
     * my:悔棋方棋子
     * */
    public void undoRevent(Map<Integer, String> my , Map<Integer, String> other ,int  current) {
        BattleMes b = serverBattleMesList.get(current -1);
        int to = b.getFrom();
        int from = b.getTo();
        //是否被吃棋
        boolean isEat = captureMap.containsKey(current -1);
        //是否兵升变
        boolean isChang = b.getPromption() > 0 ? true : false;
        if (!isEat && !isChang) {
            other.put(to, userTwo.get(from));
            other.remove(from);
        } else if (isEat && !isChang) {
            other.put(to, userTwo.get(from));
            other.remove(from);
            String chessType = captureMap.get(current -1).split("_")[2];
            my.put(from, chessType);
        } else if ( !isEat && isChang) {
            other.put(to, "p");
            other.remove(from);
        } else if (isEat && isChang) {
            other.put(to, "p");
            other.remove(from);
            String chessType = captureMap.get(current -1).split("_")[2];
            my.put(from, chessType);
        }
    }
    
    /**玩家交互的回复
     * @throws BaseException */
    public void onMutuallyFeedback(boolean agree, UserMatchInfo userMatchInfo) throws BaseException {
        if (mutaully <= 0) {
            //没有交互信息请求
        } else if (!agree) {
            mutaully = 0;
            nextTime = System.currentTimeMillis() + lastTime;
            battleStatus = BattleStatus.fighting;
            PlayerNotAgreePush.Builder p = PlayerNotAgreePush.newBuilder();
            disrupAll(PushReqestName.PlayerNotAgreePush, p.build());
            return ;
        } else {
            //1:悔棋
            if (mutaully == 1) {
                if (undoUserId == startUserId) {
                    if (currentPlayNum < 2 || currentPlayNum % 2 != 0) {
                        //不符合悔棋条件
                        logger.error("PLAYER_CAN_NOT_UNDO_VALUE userId:{}", userMatchInfo.getUid());
                        throw new BaseException(ErrorCode.PLAYER_CAN_NOT_UNDO_VALUE);
                    }
                    battleStatus = BattleStatus.fightWaiting;
                    PlayerUndoInfoPush.Builder p = buildPlayerUndoInfoPush(userOne, userTwo,userMatchInfo);
                    disrupAll(PushReqestName.PlayerUndoInfoPush, p.build());
                    
                } else {
                    if (currentPlayNum < 3 || currentPlayNum % 2 != 1) {
                        //不符合悔棋条件
                        logger.error("PLAYER_CAN_NOT_UNDO_VALUE userId:{}", userMatchInfo.getUid());
                        throw new BaseException(ErrorCode.PLAYER_CAN_NOT_UNDO_VALUE);
                    }
                    battleStatus = BattleStatus.fightWaiting;
                    PlayerUndoInfoPush.Builder p = buildPlayerUndoInfoPush(userTwo,userOne,userMatchInfo);
                    disrupAll(PushReqestName.PlayerUndoInfoPush, p.build());
                } 
            } else if (mutaully == 2) {
                isGiveUp = 5;
                battleFinished(winUserId);
            }
        }
    }
    /**构建悔棋信息
     * my: 悔棋方棋子
     * */
    public PlayerUndoInfoPush.Builder buildPlayerUndoInfoPush(Map<Integer, String> my ,Map<Integer, String> other ,UserMatchInfo userMatchInfo) {
        PlayerUndoInfoPush.Builder playerUndoInfoPush= PlayerUndoInfoPush.newBuilder();
        undoRevent(my, other, currentPlayNum);
        UndoInfo.Builder undoInfo = UndoInfo.newBuilder();
        undoInfo.setIsEat(false);
        BattleMes b = serverBattleMesList.get(currentPlayNum -1);
        undoInfo.setBattleMes(b);
        if (captureMap.containsKey(currentPlayNum-1)) {
            undoInfo.setIsEat(true);
            undoInfo.setType(ChessService.shifts.get(captureMap.get(currentPlayNum-1).split("_")[2]));
            undoInfo.setUserId(userMatchInfo.getUid());
            captureMap.remove(currentPlayNum-1);
        }
        playerUndoInfoPush.addUndoInfo(undoInfo);
        undoRevent(other, my, currentPlayNum-1);
        UndoInfo.Builder undoInfo1 = UndoInfo.newBuilder();
        undoInfo1.setIsEat(false);
        BattleMes b1 = serverBattleMesList.get(currentPlayNum -2);
        undoInfo1.setBattleMes(b1);
        if (captureMap.containsKey(currentPlayNum-2)) {
            undoInfo1.setIsEat(true);
            undoInfo1.setType(ChessService.shifts.get(captureMap.get(currentPlayNum-2).split("_")[2]));
            undoInfo1.setUserId(undoUserId);
            captureMap.remove(currentPlayNum-2);
        }
        playerUndoInfoPush.addUndoInfo(undoInfo1);
        serverBattleMesList.remove(currentPlayNum-1);
        serverBattleMesList.remove(currentPlayNum-2);
        currentPlayNum = currentPlayNum -2;
        return playerUndoInfoPush;
    }
    /** 保存战斗桢 */
  	public void saveBattleMes(BattleMes battleMes) {
  	  		serverBattleMesList.add(battleMes);
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
	
	/**
	 * 初始化玩家棋子，固定的写死
	 * p:兵  r ：车 n：马  b：象 q：王后  k：王
	 */
	public void initChess() {
	    for (int i = 1; i <= 6; i++) {
	        if (i==1) {
	            for (int y = 9; y<=16; y++ ) {
	                userTwo.put(y, "p");
                    userOne.put(y+40, "p");
	            }
	        } else if (i==2) {
	            userTwo.put(1, "r");
  	            userTwo.put(8, "r");
  	            userOne.put(57, "r");
  	            userOne.put(64, "r");
	        } else if (i == 3 ) {
	            userTwo.put(2, "n");
	            userTwo.put(7, "n");
	            userOne.put(58, "n");
	            userOne.put(63, "n");	            
	        } else if (i == 4) {
	            userTwo.put(3, "b");
	            userTwo.put(6, "b");
                userOne.put(59, "b");
                userOne.put(62, "b");  	            
	        } else if (i == 5) {
	            userTwo.put(4, "q");
	            userOne.put(60, "q");	            
	        } else if (i == 6) {
	            userTwo.put(5, "k");
	            userOne.put(61, "k"); 	            
	        }
	    }
	    genera.put(startUserId, 0);
	    genera.put(afterUserId, 0);
	}

    public List<UserMatchInfo> getUserMatchInfoList() {
  		return userMatchInfoList;
  	}

	public void setUserMatchInfoList(List<UserMatchInfo> userMatchInfoList) {
		this.userMatchInfoList = userMatchInfoList;
	}		

	public Map<Integer, String> getUserOne() {
        return userOne;
	}

	public void setUserOne(Map<Integer, String> userOne) {
	  this.userOne = userOne;
    }

    public Map<Integer, String> getUserTwo() {
        return userTwo;
    }

    public void setUserTwo(Map<Integer, String> userTwo) {
        this.userTwo = userTwo;
    }

    enum BattleStatus {
  		start, fighting, fightWaiting, waitFinish, finished,mutually
  	}


    
    
}
