package com.zyd.gw.client;

import java.util.concurrent.LinkedBlockingDeque;
import java.util.concurrent.TimeUnit;
import com.zyd.common.rpc.Packet;
import com.zyd.common.rpc.RpcResponseHandler;
import com.zyd.gw.pojo.ClientRpcJob;
import com.zyd.gw.proxy.InfoProxyConnection;
import io.netty.channel.nio.NioEventLoopGroup;


public class ClientRpcDispatcher {
  
    private final static ClientRpcDispatcher INSTANCE = new ClientRpcDispatcher();
    public static ClientRpcDispatcher getInstance() {
      return INSTANCE;
    }
    private ClientRpcDispatcher() {
    }
    private final RpcQueue clientRpcQueue = new RpcQueue("clientRpcQueue");

    public void addClientRequest(ClientConnectionHandler client, Packet args,InfoProxyConnection proxy) {
        ClientRpcJob job = new ClientRpcJob();
        job.setClient(client);
        job.setArgs(args);
        job.setProxy(proxy);
        clientRpcQueue.addJob(job);
        System.out.println("连接加入队列");
    }
    
    
    public void startup(NioEventLoopGroup group) {

      group.scheduleWithFixedDelay(new Runnable() {
          @Override
          public void run() {
//              logger.info("ClientRpcDispatcher starting");
              dispacher();
          }
      },0, 100, TimeUnit.MILLISECONDS);
      
     }
    
    protected void dispacher() {
      while ( true ) {
        try {
            final ClientRpcJob job  = clientRpcQueue.queue.take();
            InfoProxyConnection conn = job.getProxy();
            if (conn == null) {
                continue;
            }
            System.out.println("开始转发");
            conn.getHandler().requestRPC(job.getArgs(), new  RpcResponseHandler() {
              
              @Override
              public void onResponse(int error, Packet rets) {
                  // TODO Auto-generated method stub
                  job.getClient().responseRpc(error, rets);
              }
            });
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
      }
    }

    public class RpcQueue {
      public final LinkedBlockingDeque<ClientRpcJob> queue = new LinkedBlockingDeque<ClientRpcJob>();
      public int capacity = 10000;
      public int processing = 0;
      private String queueName;

      public RpcQueue(String name) {
          this.queueName = name;
      }
      
      // add a job to this queue
      public boolean addJob(ClientRpcJob job) {
          if (queue.size() >= capacity) {
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
