package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerCancelMatchRequest;
import com.zyd.common.proto.client.WarChess.PlayerCancelMatchResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerCancelMatch extends BaseClientServlet{
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    
    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerCancelMatch {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerCancelMatchRequest.PARSER, 1)) );
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerCancelMatch {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerCancelMatchResponse.PARSER, 0)));  
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType,User user) throws Exception {      
        matchService.removeWaitUser(user.getUserName(), user.getId());
        return new Packet(PlayerCancelMatchResponse.newBuilder());
    }    
    
}
