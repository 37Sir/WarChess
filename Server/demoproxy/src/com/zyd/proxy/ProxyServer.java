package com.zyd.proxy;

import com.zyd.proxy.client.ClientConnectionListener;
import com.zyd.proxy.client.ClientRpcDispatcher;
import com.zyd.proxy.info.InfoConnectionListener;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;

public class ProxyServer {
    public void run() {
    int cpuNum = Runtime.getRuntime().availableProcessors();
    EventLoopGroup mainGroup = new NioEventLoopGroup(cpuNum*3);
    EventLoopGroup httpGroup = new NioEventLoopGroup(1);
    
    InfoConnectionListener.getInstance().startup(mainGroup);
    ClientConnectionListener.getInstance().startup(mainGroup);
    ClientRpcDispatcher.getInstance().startup();


    }
    
    public static void main(String[] args) {
        ProxyServer p = new ProxyServer();
        p.run();
    }
}
