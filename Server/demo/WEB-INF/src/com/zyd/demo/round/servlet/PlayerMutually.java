package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerMutuallyRequest;
import com.zyd.common.proto.client.WarChess.PlayerMutuallyResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerMutually extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerMutually {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerMutuallyRequest.PARSER, 1)) );
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerMutually {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerMutuallyResponse.PARSER, 0)));  
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
        PlayerMutuallyRequest req = paramValues.parseProtobuf(PlayerMutuallyRequest.PARSER, 1);
        int type = req.getType();
        battleRoomManager.onMutually(type, user);
        return new Packet(PlayerMutuallyResponse.newBuilder());
    }
    
    
}
