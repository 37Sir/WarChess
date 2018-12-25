package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerMatchRequest;
import com.zyd.common.proto.client.WarChess.PlayerMatchResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.round.pojo.UserMatchInfo;
import com.zyd.demo.user.pojo.User;

public class PlayerMatch extends BaseClientServlet{
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerMatch {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerMatchRequest.PARSER, 1))); 
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerMatch {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerMatchResponse.PARSER, 0)));  
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType,User user) throws Exception {
        MessageHeaderInfo cui = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        PlayerMatchRequest req = paramValues.parseProtobuf(PlayerMatchRequest.PARSER, 1);
        UserMatchInfo userMatchInfo = new UserMatchInfo();
        userMatchInfo.setUid(cui.getUserId());
        userMatchInfo.setToken(cui.getUserToken());
        matchService.addWaitUser(userMatchInfo,req.getType(),user);
        
        return new Packet(PlayerMatchResponse.newBuilder());
    }
    
    
}
