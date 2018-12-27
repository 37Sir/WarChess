package com.zyd.demo.stone.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerInitiativeEndRequest;
import com.zyd.common.proto.client.WarChess.PlayerInitiativeEndResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerInitiativeEnd extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
  
    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerInitiativeEnd {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerInitiativeEndRequest.PARSER, 1)) );
    }
  
    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerInitiativeEnd {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerInitiativeEndResponse.PARSER, 0)));  
    }
  
    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {

        battleRoomManager.endRound(user);
        return new Packet(PlayerInitiativeEndResponse.newBuilder());
    }
}
