package com.zyd.gw.pojo;

public class ProxyPojo {
  private String host;
  private int port;
  
  public ProxyPojo(String host,int port) {
      this.host = host;
      this.port = port;
  }

  public String getHost() {
      return host;
  }
  public void setHost(String host) {
      this.host = host;
  }
  public int getPort() {
      return port;
  }
  public void setPort(int port) {
      this.port = port;
  }
}