package com.zyd.common.rpc;


public interface RpcResponseHandler {
    void onResponse(int error, Packet results);

}
