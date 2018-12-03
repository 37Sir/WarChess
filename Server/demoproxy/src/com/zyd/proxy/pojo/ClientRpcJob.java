package com.zyd.proxy.pojo;

import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.rpc.Packet;
import com.zyd.proxy.client.ClientConnectionHandler;

public class ClientRpcJob {
  public String name;
  public Packet args;
  public long request_time;
  
  public ClientConnectionHandler client;
  private RPCQueueIDEnum queueId;
  
  
  public RPCQueueIDEnum getQueueId() {
      return queueId;
  }
  public void setQueueId(RPCQueueIDEnum queueId) {
      this.queueId = queueId;
  }
  public String getName() {
      return name;
  }
  public void setName(String name) {
      this.name = name;
  }
  public Packet getArgs() {
      return args;
  }
  public void setArgs(Packet args) {
      this.args = args;
  }
  public long getRequest_time() {
      return request_time;
  }
  public void setRequest_time(long request_time) {
      this.request_time = request_time;
  }
  public ClientConnectionHandler getClient() {
      return client;
  }
  public void setClient(ClientConnectionHandler client) {
      this.client = client;
  }

}
