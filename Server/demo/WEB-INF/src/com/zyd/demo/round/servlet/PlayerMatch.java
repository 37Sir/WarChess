package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.LoginRequest;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.round.pojo.UserMatchInfo;

public class PlayerMatch extends BaseClientServlet{
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerMatch {}", TextFormat.printToUnicodeString(cui));  
    }

    @Override
    public void logResponse(Packet returnParam) {
      // TODO Auto-generated method stub
      super.logResponse(returnParam);
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        
        UserMatchInfo userMatchInfo = new UserMatchInfo();
        userMatchInfo.setUid(cui.getUserId());
        userMatchInfo.setToken(cui.getUserToken());
        System.out.println(cui.getUserId() + "-" + cui.getUserToken());
        matchService.addWaitUser(userMatchInfo);
        
        return new Packet();
    }
    
    
}
