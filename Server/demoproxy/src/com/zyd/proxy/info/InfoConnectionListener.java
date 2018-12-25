package com.zyd.proxy.info;

import java.util.HashSet;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.rpc.PacketDecoder;
import com.zyd.common.rpc.PacketEncoder;
import com.zyd.proxy.ConfigurationUtil;
import io.netty.bootstrap.ServerBootstrap;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.handler.timeout.IdleStateHandler;
import io.netty.util.concurrent.GenericFutureListener;

public class InfoConnectionListener {
    private static InfoConnectionListener INSTANCE;
    private Logger logger = LoggerFactory.getLogger(InfoConnectionListener.class.getName());
    private InfoConnectionListener() {}
    public static InfoConnectionListener getInstance() {
        if (INSTANCE == null) {
            INSTANCE = new InfoConnectionListener();
        }
        return INSTANCE;
    }
    
    private final HashSet<InfoConnectionHandler> connections = new HashSet<InfoConnectionHandler>();
    
    public HashSet<InfoConnectionHandler> getConnectedServers() {
        return connections;
    }
    
    public void startup (EventLoopGroup eventLoopGroup) {
        ServerBootstrap boot = new ServerBootstrap();
        
        boot.group(eventLoopGroup);
        boot.channel(NioServerSocketChannel.class);
        boot.childOption(ChannelOption.TCP_NODELAY, true);
        boot.childOption(ChannelOption.SO_KEEPALIVE, true);
        //boot.handler(new LoggingHandler(LogLevel.INFO));
        boot.childHandler(new ChannelInitializer<SocketChannel>() {
          @Override
          protected void initChannel(SocketChannel ch) throws Exception {

              onChannelConnected(ch);
               
          }});
        try {
            ChannelFuture f = boot.bind(ConfigurationUtil.INFO_BIND_ADDRESS, ConfigurationUtil.INFO_BIND_PORT).sync();
            logger.info("Info listener running at: " + f.channel().localAddress());
        }
        catch (Exception e) {
            logger.error("Info listener bind failed: " + e.getMessage(),e);
        }
    }
    
     
    protected void onChannelConnected(SocketChannel ch) {
        final InfoConnectionHandler conn = new InfoConnectionHandler(ch);
        connections.add(conn);
        System.out.println(connections.size()+"------------------");
        ch.closeFuture().addListener(new GenericFutureListener<ChannelFuture>() {
          @Override
          public void operationComplete(ChannelFuture future) throws Exception {
              logger.error("Info server disconnected: " + future.channel().remoteAddress());
              connections.remove(conn);
              conn.onConnectionClosed();
          }
      });
        ChannelPipeline p = ch.pipeline();
        p.addLast("frameDecoder", new PacketDecoder());
        p.addLast("frameEncoder", new PacketEncoder());
        p.addLast("idleStateHandler", new IdleStateHandler(3, 0, 3));
        p.addLast("Handler", conn);
      
    }
    public synchronized InfoConnectionHandler getMostIdleConnection(RPCQueueIDEnum queueId) {
        InfoConnectionHandler result = null;
        double load = 0;
        // find a info connection with minimum load 
        for (InfoConnectionHandler c : connections) {
/*            if (c.canProcessJobs(queueId)) {
                if (result == null) {
                    result = c;
                    load = c.getJobLoad(queueId);
                }
                else {
                    double infoLoad = c.getJobLoad(queueId);
                    
                    // if a info is fully idle, dispatch to first one with maximum job load.
                    if (infoLoad == 0)
                        infoLoad = -c.getJobCapacity(queueId);
  
                    if (infoLoad < load) {
                        result = c;
                        load = infoLoad;
                    }
                }
             }*/
             result = c;
        }
        
        return result;
    }
}
