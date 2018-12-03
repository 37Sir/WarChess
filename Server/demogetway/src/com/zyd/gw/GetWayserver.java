package com.zyd.gw;

import com.zyd.gw.client.ClientConnectionListener;
import com.zyd.gw.client.ClientRpcDispatcher;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;

public class GetWayserver {
    public GetWayserver(){
      
    }
    
    public void run() {
      
      int cpuNum = Runtime.getRuntime().availableProcessors();
      EventLoopGroup mainGroup = new NioEventLoopGroup(cpuNum);
      NioEventLoopGroup dispatcherGroup = new NioEventLoopGroup(1);
      
      ClientConnectionListener.getInstance().startup(mainGroup);
      ClientRpcDispatcher.getInstance().startup(dispatcherGroup);


    }
    
    public static void main(String[] args) throws Exception {
      GetWayserver instance = new GetWayserver();
      
      try {
          instance.run();
      }
      catch (Exception err) {
//          logger.error(err.getMessage(),err);
      }
  }
}
