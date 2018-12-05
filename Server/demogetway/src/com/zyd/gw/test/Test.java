package com.zyd.gw.test;

import java.util.Scanner;
import com.zyd.common.proto.client.ClientProtocol.LoginRequest;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderRequest;
import com.zyd.common.rpc.Packet;
import com.zyd.common.rpc.PacketDecoder;
import com.zyd.common.rpc.PacketEncoder;
import io.netty.bootstrap.Bootstrap;
import io.netty.buffer.ByteBuf;
import io.netty.buffer.Unpooled;
import io.netty.channel.Channel;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.channel.socket.nio.NioSocketChannel;
import io.netty.handler.timeout.IdleStateHandler;

public class Test {
    public void run () {
      Bootstrap bootstrap = new Bootstrap();
      EventLoopGroup eventLoopGroup = new NioEventLoopGroup();
      bootstrap.group(eventLoopGroup);
      bootstrap.channel(NioSocketChannel.class);
      bootstrap.option(ChannelOption.TCP_NODELAY, true);
      bootstrap.option(ChannelOption.SO_KEEPALIVE, true);
      bootstrap.handler(new ChannelInitializer<SocketChannel>() {
          @Override
          protected void initChannel(SocketChannel ch) throws Exception {
            ChannelPipeline p = ch.pipeline();
            p.addLast("frameDecoder", new PacketDecoder());
            p.addLast("frameEncoder", new PacketEncoder());
            p.addLast("idleStateHandler", new IdleStateHandler(1, 0, 10));
            p.addLast("Handler", new TestHandle());
          }
        
      });
      try {
        Channel ch = bootstrap.connect("127.0.0.1",10000).sync().channel();
        MessageHeaderRequest.Builder req = MessageHeaderRequest.newBuilder();
        MessageHeaderInfo.Builder mes = MessageHeaderInfo.newBuilder();
        mes.setUserId(1);
        mes.setUserToken("login");
        req.setUserInfo(mes);
        while(true){
          Scanner sc  = new Scanner(System.in);
          String s = sc.nextLine();
          req.setName(s);
          ch.writeAndFlush(new Packet(req,req));
        }
      } catch (InterruptedException e) {
        // TODO Auto-generated catch block
        e.printStackTrace();
      }
    }
    
    public static void main(String[] args) {
        new Test().run();
    }
}
