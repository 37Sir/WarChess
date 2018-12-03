package com.zyd.demo.common;

import com.zyd.demo.common.nosql.NosqlService;
import com.zyd.demo.round.service.BattleRoomManager;

public class BaseService {
  
  protected NosqlService nosqlService;
  protected BattleRoomManager battleRoomManager;
  protected CommonService commonService;

  
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
