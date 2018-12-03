package com.zyd.demo.common;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.EDeviceType;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.rpc.Packet;

public class BaseClientServlet {
    protected static Logger logger = LoggerFactory.getLogger(BaseClientServlet.class.getName());
    
    public boolean isGuildLock() {
      return false;
    }
    
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest BaseClientServlet {}", TextFormat.printToUnicodeString(cui));
    }
    
    
    public void logResponse(Packet returnParam) {
    }

    //登录的方法
    public Packet serviceLogin(Packet paramValues, String rpcName) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        String userToken = cui.getUserToken();
        EDeviceType enu = cui.getUserDevice();
        Packet returnParam = serviceBeforeLogin(paramValues, rpcName);
        return returnParam;
    }
    //登录调用的方法
    public Packet serviceBeforeLogin(Packet paramValues, String rpcName) throws Exception {
        return paramValues;
    }
    // request from client
    public Packet serviceWithViladate(Packet paramValues, String rpcName, boolean isGuildLock) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        return checkInit(cui, paramValues, rpcName, isGuildLock);
    }
    public Packet service(Packet paramValues, Integer deviceType) throws Exception {
        return paramValues;
    }
    private Packet checkInit(MessageHeaderInfo cui, Packet paramValues, String rpcName, boolean isGuildLock) throws Exception {
        Packet returnvalue = null;
        returnvalue = service(paramValues,1);
        return null;
    }

    public void serviceUpdateUserData(Packet msg, String string) {
      
    }

}
