package com.zyd.proxy.info;

import java.util.HashMap;
import java.util.Map;
import java.util.UUID;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.RpcProtocol.PushHeader;
import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.rpc.Packet;
import com.zyd.common.rpc.RpcMessageHandler;
import com.zyd.common.rpc.RpcResponseHandler;
import com.zyd.common.unti.Constants;
import com.zyd.proxy.client.ClientConnectPushUtil;
import com.zyd.proxy.pojo.PushTaskData;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.socket.SocketChannel;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;

public class InfoConnectionHandler extends RpcMessageHandler{
  private long lastReceiveRPCTime = System.currentTimeMillis();
  private final static Logger logger = LoggerFactory.getLogger(InfoConnectionHandler.class);

  private Map<Integer, Integer> capacityMap = new HashMap<Integer, Integer>();
  private Map<Integer, JobRunning> jobRunningMap = new HashMap<Integer, JobRunning>();
  
  // for monitor
  private int job_finished = 0;
  private int job_process_time = 0;
  private int job_running = 0;
  
  public InfoConnectionHandler(SocketChannel ch) {
      super(ch);
      capacityMap.put(RPCQueueIDEnum.LOGIN_QUEUE_VALUE, 0);
      capacityMap.put(RPCQueueIDEnum.ROOM_GAME_QUEUE_VALUE, 0);
      capacityMap.put(RPCQueueIDEnum.THIRD_QUEUE_VALUE, 0);
      capacityMap.put(RPCQueueIDEnum.MAIN_QUEUE_VALUE, 7);
      jobRunningMap.put(RPCQueueIDEnum.LOGIN_QUEUE_VALUE, new JobRunning(0, 0));
      jobRunningMap.put(RPCQueueIDEnum.ROOM_GAME_QUEUE_VALUE, new JobRunning(0, 0));
      jobRunningMap.put(RPCQueueIDEnum.THIRD_QUEUE_VALUE, new JobRunning(0, 0));
      jobRunningMap.put(RPCQueueIDEnum.MAIN_QUEUE_VALUE, new JobRunning(0, 0));
  }
  
  public void onConnectionClosed() {
//    super.allTimeout();
  }

  @Override
  protected void onRequestRpc(int id, String name, Packet args, RPCQueueIDEnum queueId) {
      if(name.startsWith("#")){
        PushHeader pushHeader = args.parseProtobuf(PushHeader.PARSER, 0);
        String token = UUID.randomUUID().toString();
        
//        System.out.println(name);
//        for(String u:pushHeader.getUserTokenList()){
//            System.out.println(u);
//        }
        
        ClientConnectPushUtil.pushData.put(token,new PushTaskData(name, new Packet(args.buffers.get(1)), token, null, pushHeader.getUserTokenList()));
        ClientConnectPushUtil.queue.add(token);
      }else if (id != 0)
        responseRpc(id, 0, null,queueId);    
  }

  @Override
  protected void onCancelRpc(int id) {
    // TODO Auto-generated method stub
    
  }

  @Override
  protected void onRequestHeartBeatSend() {
      super.requestServerHeartBeat();
    
  }

  @Override
  protected void onResponseHeartBeatSend() {
      lastReceiveRPCTime = System.currentTimeMillis();    
  }

  @Override
  public void transmit(RpcResponseHandler rinfo, String name, Packet args) {
    // TODO Auto-generated method stub
    
  }
  @Override
  public void userEventTriggered(ChannelHandlerContext ctx, Object evt)
          throws Exception {
       if (evt instanceof IdleStateEvent) {
           IdleStateEvent event = (IdleStateEvent) evt;
           if (event.state().equals(IdleState.READER_IDLE)) {
              // 发送心跳
              logger.debug("sending hearbeat");
              onRequestHeartBeatSend();
           } else if (event.state().equals(IdleState.WRITER_IDLE)) {
               logger.debug(IdleState.WRITER_IDLE.name());
           } 
           logger.debug("info userEventTriggered:{},lastHeartBeat:{},between:{}",IdleState.ALL_IDLE.name(),lastReceiveRPCTime,(System.currentTimeMillis()-lastReceiveRPCTime));
           if (lastReceiveRPCTime != 0L) {
               if ((System.currentTimeMillis()-lastReceiveRPCTime) > Constants.SERVER_HEART_BEAT_SECONDS*1000l && ctx.channel().isActive()) {
                   logger.error("No rpc call received from info server {}, in {} seconds, close channel.",getChannel().remoteAddress().toString(),Constants.SERVER_HEART_BEAT_SECONDS);
                   System.out.println("No rpc call received from info server");
                   ctx.close();
               }
           }
           
       }
      super.userEventTriggered(ctx, evt);
  }

  public void requestRpc(String name, Packet args, RpcResponseHandler handler,
          RPCQueueIDEnum queueId) {
      job_running++;
      final JobRunning running = jobRunningMap.get(queueId.getNumber());
      int jobRunning = running.getJobRunning();
      int jobRunningMax = running.getJobRunningMax();
      running.setJobRunning(++jobRunning);
      if (jobRunningMax < jobRunning)
          jobRunningMax = jobRunning;
      final long startTime = System.currentTimeMillis();
      super.requestRpc(name, args, new RpcResponseHandler() {
          @Override
          public void onResponse(int error, Packet results) {
              running.setJobRunning(running.getJobRunning()-1);
              job_running--;
              job_finished++;
              long processTime = System.currentTimeMillis() - startTime;
              job_process_time += (int)processTime;
              lastReceiveRPCTime = System.currentTimeMillis();
              if (handler != null){
                  handler.onResponse(error, results);
              }
          }
      },queueId);
    
  }

  public boolean canProcessJobs(RPCQueueIDEnum queueIDEnum) {
      return getJobCapacity(queueIDEnum) > 0 && jobRunningMap.get(queueIDEnum.getNumber()).getJobRunning() < getJobCapacity(queueIDEnum);

  }

  public int getJobCapacity(RPCQueueIDEnum queueIDEnum) {
     return capacityMap.get(queueIDEnum.getNumber());

  }

  public double getJobLoad(RPCQueueIDEnum queueIDEnum) {
      if (getJobCapacity(queueIDEnum) > 0) {
        return (double)jobRunningMap.get(queueIDEnum.getNumber()).getJobRunning() / (double)getJobCapacity(queueIDEnum);
      }
      return Double.MAX_VALUE;
  }
}
