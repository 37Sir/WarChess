package com.zyd.demo.round.servlet;

import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;

public class test extends BaseClientServlet {

  @Override
  public Packet service(Packet paramValues, Integer deviceType) throws Exception {
    System.out.println("-----------------------------------test");
    return super.service(paramValues, deviceType);
  }

}
