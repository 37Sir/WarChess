package com.zyd.gw.pojo;


import com.zyd.common.rpc.Packet;
import com.zyd.gw.client.ClientConnectionHandler;
import com.zyd.gw.proxy.InfoProxyConnection;

public class ClientRpcJob {
    private Packet args;
    private ClientConnectionHandler client;
    private InfoProxyConnection proxy;

    public Packet getArgs() {
      return args;
    }
    public void setArgs(Packet args) {
      this.args = args;
    }
    public ClientConnectionHandler getClient() {
      return client;
    }
    public void setClient(ClientConnectionHandler client) {
      this.client = client;
    }
    public InfoProxyConnection getProxy() {
      return proxy;
    }
    public void setProxy(InfoProxyConnection proxy) {
      this.proxy = proxy;
    }
    
    
    
}
