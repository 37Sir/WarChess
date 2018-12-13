package com.zyd.demo.common;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.common.utils.ConfigurationUtil;
import com.zyd.demo.round.service.BattleRoomManager;
import com.zyd.demo.round.service.ChessService;
import com.zyd.demo.round.service.MatchService;
import com.zyd.demo.user.pojo.User;
import com.zyd.demo.user.service.UserService;

public class BaseClientServlet {
    protected static Logger logger = LoggerFactory.getLogger(BaseClientServlet.class.getName());

    
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest BaseClientServlet {}", TextFormat.printToUnicodeString(cui));
    }
    
    
    public void logResponse(Packet returnParam) {
    }
    
    //登录的方法
    public Packet serviceLogin(Packet paramValues, String rpcName) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        String userToken = cui.getUserToken();
        Packet returnParam = serviceBeforeLogin(paramValues, rpcName);
        return returnParam;
    }
    //登录调用的方法
    public Packet serviceBeforeLogin(Packet paramValues, String rpcName) throws Exception {
        return paramValues;
    }
    // request from client
    public Packet serviceWithViladate(Packet paramValues, String rpcName) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        return checkInit(cui, paramValues, rpcName);
    }
    // game request from client
    public Packet serviceWithViladate0(Packet paramValues, String rpcName) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        return checkInit0(cui, paramValues, rpcName);
    }
    //房间操作不加锁
    private Packet checkInit0(MessageHeaderInfo cui, Packet paramValues, String rpcName) throws Exception {
      int userId = cui.getUserId();
      Packet returnvalue = null;

      User user = userService.getUserById(userId);
      logRequest(cui, paramValues);
      returnvalue = service(paramValues,1,user);
      logResponse(returnvalue);                         
      
      if (returnvalue == null || returnvalue.buffers.size() == 0) {
        logger.error("lockTrace returnParam not get lock value userId:{}", userId);
      } else {
          logResponse(returnvalue);                         
      }
      return returnvalue;
  }
    public Packet service(Packet paramValues, Integer deviceType,User user) throws Exception {
        return paramValues;
    }
    private Packet checkInit(MessageHeaderInfo cui, Packet paramValues, String rpcName) throws Exception {
        int userId = cui.getUserId();
        Packet returnvalue = null;
        if (ConfigurationUtil.infoLock.tryLock(String.valueOf(userId))) {
            try {
                User user = userService.getUserById(userId);
                logRequest(cui, paramValues);
                returnvalue = service(paramValues,1,user);
                logResponse(returnvalue);            
            } catch (BaseException e) {
                logger.error("BaseException happened {}  ---  {}", e.getErrrorCode(),
                    ErrorCode.valueOf(e.getErrrorCode()).name());
                throw e;
            } catch (Exception e) {
                logger.error("error happened", e);
                throw e;
            } finally {
                ConfigurationUtil.infoLock.unLock(String.valueOf(userId));
            }
        } else {
            logger.error("lockTrace not get lock value userId:{}", userId);
            throw new BaseException(ErrorCode.NOT_GET_LOCK_VALUE);
        }
        
        if (returnvalue == null || returnvalue.buffers.size() == 0) {
          logger.error("lockTrace returnParam not get lock value userId:{}", userId);
        }
        return returnvalue;
    }
    public void serviceUpdateUserData(Packet msg, String string) {
      
    }
    protected MatchService matchService;
    protected UserService userService;
    protected BattleRoomManager battleRoomManager;
    protected ChessService chessService;

    
    public ChessService getChessService() {
      return chessService;
    }


    public void setChessService(ChessService chessService) {
      this.chessService = chessService;
    }


    public BattleRoomManager getBattleRoomManager() {
      return battleRoomManager;
    }

    public void setBattleRoomManager(BattleRoomManager battleRoomManager) {
      this.battleRoomManager = battleRoomManager;
    }

    public MatchService getMatchService() {
      return matchService;
    }

    public void setMatchService(MatchService matchService) {
      this.matchService = matchService;
    }

    public UserService getUserService() {
        return userService;
    }

    public void setUserService(UserService userService) {
        this.userService = userService;
    }
        

}
