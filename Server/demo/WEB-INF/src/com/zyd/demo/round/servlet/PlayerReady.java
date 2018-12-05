package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerReadyRequest;
import com.zyd.common.proto.client.WarChess.PlayerReadyResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerReady extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerReady {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerReadyRequest.PARSER, 1))); 
    }
  
    @Override
    public Packet service(Packet paramValues, Integer deviceType,User user) throws Exception {
          PlayerReadyRequest playerReadyRequest = paramValues.parseProtobuf(PlayerReadyRequest.PARSER, 1);
          battleRoomManager.onRequest(user.getUserName(), playerReadyRequest);
          return new Packet(PlayerReadyResponse.newBuilder());
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerReady {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerReadyResponse.PARSER, 0))); 
    }
    
    
}
