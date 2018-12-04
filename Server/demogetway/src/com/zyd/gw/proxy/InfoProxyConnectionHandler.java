package com.zyd.gw.proxy;

import java.util.UUID;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderRequest;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.common.rpc.RpcResponseHandler;
import io.netty.channel.Channel;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;

public class InfoProxyConnectionHandler extends SimpleChannelInboundHandler<Packet> {
    private Logger logger = LoggerFactory.getLogger(InfoProxyConnectionHandler.class.getName());
    private RpcResponseHandler handler;
    private final Channel channel;
    public InfoProxyConnectionHandler(Channel channel) {
        this.channel = channel;
    }
    @Override
    protected void channelRead0(ChannelHandlerContext arg0, Packet msg) throws Exception {
        MessageHeaderResponse res = msg.parseProtobuf(MessageHeaderResponse.PARSER, 0);
        if(res.getName() != null && "#HeartBeat".equals(res.getName())){
          logger.debug("accept heartbeat from proxy");
          requestHearBeat();
        }else{
          responseRPC(res.getError(), msg);
        }

    }
    
    public void requestHearBeat(){
      
      MessageHeaderRequest.Builder request = MessageHeaderRequest.newBuilder();
      request.setName("HeartBeat");
      request.setRequestToken(UUID.randomUUID().toString());
      logger.debug("sending heartbeat to proxy");
      channel.writeAndFlush(new Packet(request));
  }
    private void responseRPC(int error, Packet results) {
      System.out.println("response------------");
        handler.onResponse(error, results);
      
    }
    public Channel getChannel() {
      return channel;
  }
    
    public void requestRPC(Packet args,RpcResponseHandler handler){
      if (handler != null) {
          this.handler = handler;
      }
      // write packet
      channel.writeAndFlush(args);
    }

}
