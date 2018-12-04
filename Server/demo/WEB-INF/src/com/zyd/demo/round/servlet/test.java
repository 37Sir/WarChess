package com.zyd.demo.round.servlet;

import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.common.memcached.MemcachedHandler;

public class test extends BaseClientServlet {

  @Override
  public Packet service(Packet paramValues, Integer deviceType) throws Exception {
      System.out.println("-----------------------------------test1111111");
      return super.service(paramValues, deviceType);
  }

}
