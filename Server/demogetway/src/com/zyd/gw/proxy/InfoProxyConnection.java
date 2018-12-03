package com.zyd.gw.proxy;

import com.zyd.common.rpc.PacketDecoder;
import com.zyd.common.rpc.PacketEncoder;
import com.zyd.gw.pojo.ProxyPojo;
import io.netty.bootstrap.Bootstrap;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioSocketChannel;
import io.netty.handler.timeout.IdleStateHandler;
import io.netty.util.concurrent.GenericFutureListener;

public class InfoProxyConnection {
  
  private final String host;
  private final int port;
  private final EventLoopGroup group;
  private InfoProxyConnectionHandler handler;
  private boolean connected;

  public InfoProxyConnection(String host, int port,  EventLoopGroup group) {
    this.host = host;
    this.port = port;
    this.group = group;
  }
  
  public InfoProxyConnection(ProxyPojo pojo,EventLoopGroup group){
    this.host = pojo.getHost();
    this.port = pojo.getPort();
    this.group = group;

  }

  public InfoProxyConnectionHandler getHandler() {
    return handler;
  }

  public void setHandler(InfoProxyConnectionHandler handler) {
    this.handler = handler;
  }
  
  public void start() {
    // Configure the client.
    Bootstrap boot = new Bootstrap();

    boot.group(group);
    boot.channel(NioSocketChannel.class);
    boot.option(ChannelOption.TCP_NODELAY, true);
    boot.option(ChannelOption.SO_KEEPALIVE, true);
    boot.option(ChannelOption.CONNECT_TIMEOUT_MILLIS, 1000);
    
    boot.handler(new ChannelInitializer<SocketChannel>() {
        @Override
        public void initChannel(SocketChannel ch) throws Exception {
            handler = new InfoProxyConnectionHandler(ch);
            ChannelPipeline p = ch.pipeline();
            p.addLast("frameDecoder", new PacketDecoder());
            p.addLast("frameEncoder", new PacketEncoder());
            p.addLast("Handler", handler);
        }
    });

    // Start the client.
    ChannelFuture f = boot.connect(host, port);
    f.addListener(new GenericFutureListener<ChannelFuture>() {

        @Override
        public void operationComplete(ChannelFuture future)
                throws Exception {
            try {
                future.sync();
                connected = true;
                onConnected();
            } catch (Exception e) {
//                logger.error("ProxyConnection connected: " + e.getMessage(),e);
                connected = false;
            }
        }

    });

    f.channel().closeFuture().addListener(new GenericFutureListener<ChannelFuture>() {

                @Override
                public void operationComplete(ChannelFuture future) throws Exception {
                    try {
//                        logger.error("ProxyConnection ++{}",future.channel().isActive());
                        if (!connected) {
                          onDisconnected();
                        }
                    } catch (Exception e) {
//                        logger.error("ProxyConnection disconnected: " + e.getMessage(),e);
                    }
                }
            });
}

    /**
     * 1停止服务。
     * @throws InterruptedException 
     * 
     */
    public void close(){
        if(handler != null && handler.getChannel().isActive()){
            handler.getChannel().close();
        }
        group.shutdownGracefully();
    }
    
    protected void onConnected() {
//      logger.info("Connect to infoProxyConnect success! {}:{} serverId:{}",host,port,serverId);
        System.out.println("Connect to infoProxyConnect success! "+host+port);
    }
    
    protected void onDisconnected() {
//      logger.info("infoProxyConnect disconnected!  {}:{} serverId:{}",host,port,serverId);
        System.out.println("infoProxyConnect disconnected!  {}:{} serverId:{}"+host+port);
        group.shutdownGracefully();
    }

}
