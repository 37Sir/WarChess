package com.zyd.gw.client;

import java.util.concurrent.atomic.AtomicInteger;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.rpc.PacketDecoder;
import com.zyd.common.rpc.PacketEncoder;
import com.zyd.gw.AppConfig;
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

public class ClientConnectionListener {
    private static ClientConnectionListener INSTANCE;
    private static final Logger logger = LoggerFactory.getLogger(ClientConnectionHandler.class.getName());
    private ClientConnectionListener(){};
    
    public static ClientConnectionListener getInstance(){
        if (INSTANCE == null) {
          INSTANCE = new ClientConnectionListener();
        }
        return INSTANCE;
    }
    
    //Threadsafe
    public static final AtomicInteger num_clients = new AtomicInteger(0);
    
    public void startup(EventLoopGroup eventLoopGroup) {
        ServerBootstrap bootstrap = new ServerBootstrap();
        bootstrap.group(eventLoopGroup);
        bootstrap.channel(NioServerSocketChannel.class);
        bootstrap.childOption(ChannelOption.TCP_NODELAY, false);
        bootstrap.childOption(ChannelOption.SO_KEEPALIVE, true);
        bootstrap.childHandler(new ChannelInitializer<SocketChannel>() {
            @Override
            protected void initChannel(SocketChannel ch) throws Exception {
                onChannelConnected(ch);
            }
          
        });

        try {
             ChannelFuture f = bootstrap.bind(AppConfig.CLIENT_BIND_ADDRESS,AppConfig.CLIENT_BIND_PORT).sync();
            logger.info("Client listener running at: " + f.channel().localAddress());
        }
        catch (Exception e) {
            logger.error("Client listener bind failed: " + e.getMessage(),e);
        }
    }

    protected void onChannelConnected(SocketChannel ch) {
        num_clients.incrementAndGet();
        final ClientConnectionHandler conn = new ClientConnectionHandler(ch);
        // remember this connection to manage it later.
        ch.closeFuture().addListener(new GenericFutureListener<ChannelFuture>() {
            @Override
            public void operationComplete(ChannelFuture future) throws Exception {
                num_clients.decrementAndGet();
                conn.onConnectionClosed();
                
            }
        });

        // initialize channel pipeline
        ChannelPipeline p = ch.pipeline();
        p.addLast("frameDecoder", new PacketDecoder());
        p.addLast("frameEncoder", new PacketEncoder());
        p.addLast("idleStateHandler", new IdleStateHandler(1, 0, 1));
        p.addLast("Handler", conn);
        
        logger.info("--------------------------------------Client connected: " + ch.remoteAddress());
        
    }
        
        
    
}
