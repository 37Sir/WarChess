package com.zyd.common.rpc;

import java.util.concurrent.ConcurrentHashMap;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.proto.client.RpcProtocol.RpcHeader;
import com.zyd.common.proto.client.RpcProtocol.RpcType;
import io.netty.channel.Channel;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;

public abstract class RpcMessageHandler extends SimpleChannelInboundHandler<Packet> {
  
    private static final Logger logger = LoggerFactory.getLogger(RpcMessageHandler.class.getName());

    //请求的回复
    private final ConcurrentHashMap<Integer, RpcResponseHandler> processing = new ConcurrentHashMap<Integer, RpcResponseHandler>();
    
    private final Channel channel;
    
    private final Object lock = new Object();
    
    private int request_id = 0;
    @Override
    protected void channelRead0(ChannelHandlerContext arg0, Packet msg) throws Exception {
        RpcHeader request = msg.parseProtobuf(RpcHeader.PARSER, 0);
        switch (request.getType().getNumber()) {
          case RpcType.RPC_REQUEST_VALUE:
              Packet args = new Packet(msg.buffers, 1, msg.buffers.size());
              logger.debug("request.getId:{}, request.getName:{}",request.getId(),request.getName());
              onRequestRpc(request.getId(), request.getName(), args,request.getQueueId());
              break;
              
          case RpcType.RPC_RESPONSE_VALUE:
              RpcResponseHandler handler = processing.remove(request.getId());
              if (handler != null) {
                  logger.debug("onResponse.getId:{}, onResponse.getName:{},{}",request.getId(),request.getName(),request.getError());
                  handler.onResponse(request.getError(), new Packet(msg.buffers, 1, msg.buffers.size()));
              }else{
                  logger.error("@RpcMessageHandler@ handler is null! and Request_id : " + request.getId());
              }
              break;
           case RpcType.RPC_SERVER_HEART_BEAT_REQUEST_VALUE:
              onRequestHeartBeatSend();
              break;
           case RpcType.RPC_SERVER_HEART_BEAT_RESPONSE_VALUE:
              onResponseHeartBeatSend();
              break;
          }
    }
    public RpcMessageHandler(Channel channel) {
      this.channel = channel;
    }
    
    public void responseRpc(int id, int error, Packet rets,RPCQueueIDEnum queueId) {
        RpcHeader.Builder header = RpcHeader.newBuilder();
        header.setType(RpcType.RPC_RESPONSE);
        header.setError(error);
        header.setId(id);
        header.setQueueId(queueId);
        logger.info("id="+id);
        channel.writeAndFlush(new Packet(header, rets));
    }
    
    // get channel
    public Channel getChannel() {
      return this.channel;
    }
    
    private int updateNextRequestId(){
      synchronized (lock) {
          request_id = (this.request_id % (Integer.MAX_VALUE - 1)) + 1;
          return request_id;
      }
    }
    public void requestRpc0(String name, Packet args,RpcResponseHandler handler,RPCQueueIDEnum queueId) {
        RpcHeader.Builder header = RpcHeader.newBuilder();
        header.setType(RpcType.RPC_REQUEST);
        header.setName(name);
        header.setId(0);
        if (handler != null) {
            header.setId(updateNextRequestId());
            processing.put(header.getId(), handler);
        }
        header.setQueueId(queueId);
        logger.debug("-------------GameRequestRpc: id={}  ,{}",header.getId(),header.getName());
        // write packet
        channel.writeAndFlush(new Packet(header, args));
    }
    public void requestRpc(String name, Packet args, RpcResponseHandler handler,RPCQueueIDEnum queueId) {
        requestRpc0(name, args, handler,queueId);
    }
    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        logger.error(cause.getMessage(),cause);
        ctx.close();
    }
    protected abstract void onRequestRpc(int id, String name, Packet args,RPCQueueIDEnum queueId);
    
    // called when remote send cancel message
    protected abstract void onCancelRpc(int id);
    
    protected abstract void onRequestHeartBeatSend();
    protected abstract void onResponseHeartBeatSend();
    //waiting for be overriden.
    public abstract void transmit(RpcResponseHandler rinfo, String name, Packet args);
    public void responseServerHeartBeat() {
      RpcHeader.Builder header = RpcHeader.newBuilder();
      header.setType(RpcType.RPC_SERVER_HEART_BEAT_RESPONSE);
      header.setName("HeartBeat");
      header.setId(0);
      header.setQueueId(RPCQueueIDEnum.MAIN_QUEUE);
      logger.debug("-------------responseServerHeartBeat: id={}  ,{}",header.getId(),header.getName());
      // write packet
      channel.writeAndFlush(new Packet(header));
      
    }
    public void requestServerHeartBeat(){
      RpcHeader.Builder header = RpcHeader.newBuilder();
      header.setType(RpcType.RPC_SERVER_HEART_BEAT_REQUEST);
      header.setName("HeartBeat");
      header.setId(0);
      header.setQueueId(RPCQueueIDEnum.MAIN_QUEUE);
//      logger.debug("-------------requestServerHeartBeat: id={}  ,{}",header.getId(),header.getName());
      // write packet
      channel.writeAndFlush(new Packet(header));
  }
}
