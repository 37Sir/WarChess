package com.zyd.gw.client;

import java.util.Random;
import java.util.UUID;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderRequest;
import com.zyd.common.rpc.Packet;
import com.zyd.gw.AppConfig;
import com.zyd.gw.proxy.InfoProxyConnection;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;

public class ClientConnectionHandler extends SimpleChannelInboundHandler<Packet>{
    private static final Logger logger = LoggerFactory.getLogger(ClientConnectionHandler.class.getName());
    private SocketChannel channel;
    InfoProxyConnection connect = null;
    private long lastReceiveRPCTime = System.currentTimeMillis();
    private String channelToken = UUID.randomUUID().toString();
    
    public SocketChannel getChannel() {
        return channel;
    }
    
    public ClientConnectionHandler(SocketChannel ch) {
        this.channel = ch;
        logger.warn("new client handler init. channelToken:{}",channelToken);
    }
    public void setChannel(SocketChannel channel) {
        this.channel = channel;
    }
    
    public void onConnectionClosed() {
        if(connect != null && connect.getHandler().getChannel().isActive()){
            connect.close();
        }
    }
    @Override
    protected void channelRead0(ChannelHandlerContext arg0, Packet msg) throws Exception {
        MessageHeaderRequest messageHeader = msg.parseProtobuf(MessageHeaderRequest.PARSER, 0);
        if("HeartBeat".equals(messageHeader.getName())){
            logger.info("heartbeat channelToken:{}",channelToken);
        }else{
            checkVailableInfoProxy();
            ClientRpcDispatcher.getInstance().addClientRequest(this, msg,connect);
        }
        lastReceiveRPCTime = System.currentTimeMillis();
    }
    private InfoProxyConnection checkVailableInfoProxy(){
        if (connect == null) {
        try {
          connect = new InfoProxyConnection(AppConfig.INFO_BIND_ADDRESS, AppConfig.INFO_BIND_PORT, new NioEventLoopGroup(1));
          connect.start();
          Thread.sleep(10 + (new Random().nextInt(10)));
          int i=0;
          while (connect.getHandler() == null
                  || !connect.getHandler().getChannel().isActive()){
              Thread.sleep(10 + (new Random().nextInt(10)));
              if (connect.getHandler() != null
                      && connect.getHandler().getChannel().isActive()){
                  break;
              }
              if (i >= 10) {
                  connect = null;
                  break;
              }
              logger.warn("try "+ i+" times for InfoProxyConnection start");
              i++;
          }
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
      }
        return connect;
    }
    
    public void responseRpc(int error, Packet rets) {
          channel.writeAndFlush(rets);
    }
    
    @Override
    public void userEventTriggered(ChannelHandlerContext ctx, Object evt)
            throws Exception {
        if (evt instanceof IdleStateEvent) {
            IdleStateEvent event = (IdleStateEvent) evt;
            try {
                if (event.state().equals(IdleState.READER_IDLE) && AppConfig.HEART_BEAT_SWITCH) {
                  
                } else if (event.state().equals(IdleState.WRITER_IDLE)) {
                    logger.debug(IdleState.WRITER_IDLE.name());
                }

                if (AppConfig.HEART_BEAT_SWITCH) {
                    logger.debug("channelToken:{} client userEventTriggered:{},lastHeartBeat:{},between:{}",
                            channelToken, IdleState.ALL_IDLE.name(),lastReceiveRPCTime,(System.currentTimeMillis() - lastReceiveRPCTime));
                    if (lastReceiveRPCTime != 0L) {
                        if ((System.currentTimeMillis() - lastReceiveRPCTime) > AppConfig.HEART_BEAT_SECONDS * 1000l
                                && ctx.channel().isActive()) {
                            logger.error("channelToken:{} No rpc call received from client {} in {} seconds, close channel",
                                    channelToken, getChannel().remoteAddress().toString(),AppConfig.HEART_BEAT_SECONDS);
                            ctx.close();
                        }
                    }
                }
            } catch (Exception e) {
                logger.error("error happend in userEventTriggered", e);
            }
        }
        super.userEventTriggered(ctx, evt);
    }
}
