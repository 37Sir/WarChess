package com.zyd.proxy.client;

import java.util.Arrays;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.LinkedBlockingDeque;
import java.util.concurrent.TimeUnit;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol;
import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.rpc.Packet;
import com.zyd.common.rpc.RpcResponseHandler;
import com.zyd.proxy.ConfigurationUtil;
import com.zyd.proxy.info.InfoConnectionHandler;
import com.zyd.proxy.info.InfoConnectionListener;
import com.zyd.proxy.pojo.ClientRpcJob;
import com.zyd.proxy.proto.ServiceProto;
import com.zyd.proxy.proto.ServiceProto.QueueStatus;
import io.netty.channel.nio.NioEventLoopGroup;

public class ClientRpcDispatcher {
    private static final Logger logger = LoggerFactory.getLogger(
            ClientRpcDispatcher.class.getName());
    private final static ClientRpcDispatcher INSTANCE = new ClientRpcDispatcher();
    public static ClientRpcDispatcher getInstance() {
      return INSTANCE;
    }
    
    // for monitor
    private int num_client_requests;
    private int num_client_response_ok;
  
    // RPC queues
    // RPC queues
    private final RpcQueue loginQueue = new RpcQueue("LoginQueue");
    private final RpcQueue roomGameQueue = new RpcQueue("RoomGameQueue");
    private final RpcQueue mainQueue = new RpcQueue("MainQueue");
    private final Map<RPCQueueIDEnum, RpcQueue> queueMap = new HashMap<RPCQueueIDEnum, ClientRpcDispatcher.RpcQueue>();
    
    
    public void addClientRequest(ClientConnectionHandler client, String name, Packet args) {
        num_client_requests++;
        if (name.length() == 0) {
            client.responseRpc(ClientProtocol.RpcErrorCode.INVALID_ARG_VALUE, null);
            return;
        }    
        // a dot name means this is a special command that proxy server can handle directly
        if (name.charAt(0) == '.') {
            switch (name) {
            case ".echo":
                num_client_response_ok++;
                client.responseRpc(0, args);
                return;
                
            default:
                client.responseRpc(ClientProtocol.RpcErrorCode.INVALID_ARG_VALUE, null);
                return;
            }
        } else {
            ClientRpcJob job = new ClientRpcJob();
            job.client = client;
            job.name = name;
            job.args = args;
            job.request_time = System.currentTimeMillis();
            job.setQueueId(RPCQueueIDEnum.MAIN_QUEUE);
            if(Arrays.asList(ConfigurationUtil.LOGIN_QUEUE).contains(name)){
                job.setQueueId(RPCQueueIDEnum.LOGIN_QUEUE);
                loginQueue.addJob(job);
            }else if(Arrays.asList(ConfigurationUtil.ROOM_GAME_SERVER_QUEUE).contains(name)){
                job.setQueueId(RPCQueueIDEnum.ROOM_GAME_QUEUE);
                roomGameQueue.addJob(job);
            }else {
                job.setQueueId(RPCQueueIDEnum.MAIN_QUEUE);
                mainQueue.addJob(job);
            }
        }
    }
    
    public void dispatchLogin() {
        dispacher(loginQueue);
    }
  
    public void dispatchGameRoom() {
         dispacher(roomGameQueue);
    }
  
    public void dispatchMain() {
        dispacher(mainQueue);
    }
    
    public void dispacher(RpcQueue clientRcpQueue){
        int tag=0;
        while (true) {
            try {
                ClientRpcJob job = clientRcpQueue.queue.take();
                if (job == null){
                    continue;
                }

                // at this time, client may already disconnected because of timeout.
                if (!job.client.isConnected()) {    
                    continue;
                }
                final RpcQueue rpcQueue = queueMap.get(job.getQueueId());
                // find a most idle info connection
                final InfoConnectionHandler conn = InfoConnectionListener.getInstance().getMostIdleConnection(job.getQueueId());
                
                if (conn == null) {    
                    rpcQueue.numNotHaveIdleConn ++ ;
                    if(tag==0){
                        logger.warn("Not Have Idle InfoConn in ClientRpcDispatcher,{} times in last second.queueId:{},requestName:{}",
                                rpcQueue.numNotHaveIdleConn,job.getQueueId(),job.name);
                        tag=1;
                    }
                    clientRcpQueue.queue.addFirst(job);
                    continue;
                }else{
                    tag=0;
                }
                // dispatching job
                final ClientConnectionHandler client = job.client;
                final long request_time = job.request_time;
                final long enqueue_time = System.currentTimeMillis();
    
                logger.info("dispatching client RPC: {} to info: {} request_time:{}", job.name, conn.getChannel().remoteAddress().toString(),job.request_time);
                conn.requestRpc(job.name, job.args, new RpcResponseHandler() {
                    @Override
                    public void onResponse(int error, Packet results) {

                        client.responseRpc(error, results);
                        
                    }
                },job.getQueueId());
            
              } catch (Exception e) {
                logger.error("error happened in ClientRpcDispatcher",e);
            }
        }
    }
  
    public void startup() {
        NioEventLoopGroup group = new NioEventLoopGroup(4);
        
        group.scheduleWithFixedDelay(new Runnable() {
            
            @Override
            public void run() {
                logger.info("ClientRpcDispatcher starting");
                dispatchLogin();
            }
        },0, 10, TimeUnit.MILLISECONDS);
        group.scheduleWithFixedDelay(new Runnable() {
                    
                    @Override
                    public void run() {
                        logger.info("ClientRpcDispatcher starting");
                        dispatchGameRoom();
                    }
                },0, 10, TimeUnit.MILLISECONDS);
        group.scheduleWithFixedDelay(new Runnable() {
            
            @Override
            public void run() {
                logger.info("ClientRpcDispatcher starting");
                dispatchMain();
            }
        },0, 10, TimeUnit.MILLISECONDS);
    }
    public class RpcQueue {
      public final LinkedBlockingDeque<ClientRpcJob> queue = new LinkedBlockingDeque<ClientRpcJob>();
      public int capacity = 20000;
      private String queueName;
  

      public int numNotHaveIdleConn = 0;
      private ServiceProto.QueueStatus.Builder queueBuilder = QueueStatus.newBuilder();
      public RpcQueue(String name) {
          this.queueName = name;
      }
      
      // add a job to this queue
      public boolean addJob(ClientRpcJob job) {
          
          if (queue.size() >= capacity) {
  //            logger.error("queue {} is full",queueName);
              return false;
          }
  
          queue.add(job);
  
          
          return true;
      }
    
  
      public String getQueueName() {
          return queueName;
      }
  
      public void setQueueName(String queueName) {
          this.queueName = queueName;
      }
      
  }
  
    

}
