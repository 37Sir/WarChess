package com.zyd.demo.info;

import java.util.concurrent.TimeUnit;
import io.netty.channel.nio.NioEventLoopGroup;

public class InfoServer {
  private ProxyConnection proxy;

  public InfoServer() {
  }

  public void run() throws Exception {
      NioEventLoopGroup group = new NioEventLoopGroup(1);

      proxy = new ProxyConnection("127.0.0.1", 8089, group);
      
      proxy.start();

      try {
          while (!group.awaitTermination(1, TimeUnit.SECONDS)) {
              continue;
          }
      }
      finally {
          group.shutdownGracefully();
      }
  }

  public static void main(String[] args) throws Exception {
      new InfoServer().run();
  }
}
