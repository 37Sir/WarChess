package com.zyd.demo.common;

import com.zyd.demo.common.memcached.CacheJDBCHandler;
import com.zyd.demo.common.nosql.NosqlService;
import com.zyd.demo.round.service.BattleRoomManager;
import com.zyd.demo.user.service.UserService;

public class BaseService {
  
    protected NosqlService nosqlService;
    protected BattleRoomManager battleRoomManager;
    protected CommonService commonService;
    protected UserService userService;
    protected CacheJDBCHandler cacheJDBCHandler;

    
    
    public CacheJDBCHandler getCacheJDBCHandler() {
      return cacheJDBCHandler;
    }

    public void setCacheJDBCHandler(CacheJDBCHandler cacheJDBCHandler) {
      this.cacheJDBCHandler = cacheJDBCHandler;
    }

    public UserService getUserService() {
      return userService;
    }
  
    public void setUserService(UserService userService) {
      this.userService = userService;
    }
  
    public CommonService getCommonService() {
      return commonService;
    }
  
    public void setCommonService(CommonService commonService) {
      this.commonService = commonService;
    }
  
    public BattleRoomManager getBattleRoomManager() {
      return battleRoomManager;
    }
  
    public void setBattleRoomManager(BattleRoomManager battleRoomManager) {
      this.battleRoomManager = battleRoomManager;
    }
  
    public NosqlService getNosqlService() {
      return nosqlService;
    }
  
    public void setNosqlService(NosqlService nosqlService) {
      this.nosqlService = nosqlService;
    }

}
