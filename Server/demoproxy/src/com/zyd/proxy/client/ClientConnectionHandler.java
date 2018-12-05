package com.zyd.proxy.client;

import java.util.UUID;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderRequest;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderResponse;
import com.zyd.common.rpc.Packet;
import io.netty.buffer.Unpooled;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.channel.socket.SocketChannel;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;

public class ClientConnectionHandler extends SimpleChannelInboundHandler<Packet>{
    private SocketChannel channel;
    private static final Logger logger = LoggerFactory.getLogger(ClientConnectionHandler.class.getName());
    private long lastReceiveRPCTime = System.currentTimeMillis();
    private String channelToken = UUID.randomUUID().toString();
    
    public SocketChannel getChannel() {
      return channel;
   }

   public void setChannel(SocketChannel channel) {
      this.channel = channel;
   }
   
     private String calling_rpc_token;
     private String last_rpc;
     private MessageHeaderInfo user_info_ = MessageHeaderInfo.getDefaultInstance();
    public ClientConnectionHandler(SocketChannel ch) {
        this.channel = ch;
    }
    public void onConnectionClosed() {
      String token = user_info_.getUserToken();
      logger.warn("Client disconnected: {} channelToken:{}" , channel.remoteAddress(),channelToken);
      ClientConnectPushUtil.clientConnectionHandlerMap.remove(token);
    }
    
    public void responseRpcAgain(int error, Packet rets) {
      
      MessageHeaderResponse.Builder header = MessageHeaderResponse.newBuilder();
      header.setError(error);
      header.setRequestToken(last_rpc);
      int size = rets.buffers.size();
      logger.info("channelToken:{}--------------responseRpcAgain: error={} size={}" ,channelToken, error,size);
      lastReceiveRPCTime = System.currentTimeMillis();
      channel.writeAndFlush(new Packet(header, rets));
  }
    @Override
    protected void channelRead0(ChannelHandlerContext arg0, Packet msg) throws Exception {
        // the first buffer is always a header.
        MessageHeaderRequest messageHeader = msg.parseProtobuf(MessageHeaderRequest.PARSER, 0);
        String requestToken = messageHeader.getRequestToken();
        String requestName = messageHeader.getName();
        if("HeartBeat".equals(requestName)){
            logger.debug("heartbeat channelToken:{}",channelToken);
        }else{
            user_info_ = messageHeader.getUserInfo();
            if (user_info_.hasUserToken()) {
                ClientConnectPushUtil.clientConnectionHandlerMap.put(user_info_.getUserToken(), this);  
                logger.debug("add client token:{} ,size:{}",user_info_.getUserToken(),ClientConnectPushUtil.clientConnectionHandlerMap.size());  
            }
            calling_rpc_token = requestToken;
            MessageHeaderInfo.Builder b = MessageHeaderInfo.newBuilder();
            b.mergeFrom(user_info_);
            b.setRequestToken(requestToken);
            msg.buffers.set(0, Unpooled.wrappedBuffer(b.build().toByteArray()));
            // call 
            onRequestRpc(requestName, msg);
        }
      
    }
    
  public void responseRpc(int error, Packet rets) {
        if (error != 300) {
            last_rpc = calling_rpc_token;
            calling_rpc_token = null;
            logger.info("responseRpc####"+last_rpc);
            MessageHeaderResponse.Builder header = MessageHeaderResponse.newBuilder();
            header.setError(error);
            header.setRequestToken(last_rpc);
            lastReceiveRPCTime = System.currentTimeMillis();
            int size = rets.buffers.size();
            logger.info("channelToken:{}-------------ClientResponseRpc: error={} size={}" ,channelToken, error,size);
    //        for(int i=0;i<size;i++){
    //            rets.buffers.set(i, Unpooled.wrappedBuffer(DesUtil.encrypt(rets.buffers.get(i).array(), ConfigurationUtil.pwd.substring(0, 8),ConfigurationUtil.pwd.substring(ConfigurationUtil.pwd.length()-8))));
    //        }
            channel.writeAndFlush(new Packet(header, rets));
        }else{
          
            logger.info("responseRpc300####"+calling_rpc_token);
            MessageHeaderResponse.Builder header = MessageHeaderResponse.newBuilder();
            header.setError(error);
            header.setRequestToken(calling_rpc_token);
            lastReceiveRPCTime = System.currentTimeMillis();        
            last_rpc = null;
            calling_rpc_token = null;
            
            int size = rets.buffers.size();
            logger.info("channelToken:{}-------------ClientResponseRpc: error={} size={}" ,channelToken, error,size);
    //        for(int i=0;i<size;i++){
    //            rets.buffers.set(i, Unpooled.wrappedBuffer(DesUtil.encrypt(rets.buffers.get(i).array(), ConfigurationUtil.pwd.substring(0, 8),ConfigurationUtil.pwd.substring(ConfigurationUtil.pwd.length()-8))));
    //        }
            channel.writeAndFlush(new Packet(header, rets));
        }
    }
    protected void onRequestRpc(String name, Packet args){
        logger.debug("channelToken:{} InfoRequestRpc: {},\n args={}\n",channelToken, name, args.buffers.size());
        ClientRpcDispatcher.getInstance().addClientRequest(this, name, args);
    }
    
    @Override
    public void userEventTriggered(ChannelHandlerContext ctx, Object evt)
            throws Exception {
         if (evt instanceof IdleStateEvent) {
             IdleStateEvent event = (IdleStateEvent) evt;
             try {
                if (event.state().equals(IdleState.READER_IDLE)) {
                    // 发送心跳
                    MessageHeaderResponse.Builder header = MessageHeaderResponse.newBuilder();
                    header.setName("#HeartBeat");
                    header.setError(0);
                    ctx.channel().writeAndFlush(new Packet(header));
                    lastReceiveRPCTime = System.currentTimeMillis();
                    logger.debug("channelToken:{} sending heartbeat. {},lastHeartBeat:{} User:{}",channelToken,IdleState.READER_IDLE.name(),lastReceiveRPCTime,user_info_.getUserId());
                 } else if (event.state().equals(IdleState.WRITER_IDLE)) {
                     logger.debug(IdleState.WRITER_IDLE.name());
                 }                  
                     logger.debug("channelToken:{} client userEventTriggered:{},lastHeartBeat:{},between:{}",channelToken,IdleState.ALL_IDLE.name(),lastReceiveRPCTime,(System.currentTimeMillis()-lastReceiveRPCTime));
                     if (lastReceiveRPCTime != 0L) {
                         if ((System.currentTimeMillis()-lastReceiveRPCTime) > 9*1000l && ctx.channel().isActive()) {
                             logger.error("channelToken:{} No rpc call received from client {} in {} seconds, close channel.{} {}",channelToken,getChannel().remoteAddress().toString(),3,user_info_.getUserToken(),last_rpc);
                             ctx.close();
                         }
                     }
  
            } catch (Exception e) {
                logger.error("error happend in userEventTriggered",e);
            }
         }
        super.userEventTriggered(ctx, evt);
    }

    public boolean isConnected() {
      return true;
    }
}
