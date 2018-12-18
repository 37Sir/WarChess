package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerMutuallyFeedbackRequest;
import com.zyd.common.proto.client.WarChess.PlayerMutuallyFeedbackResponse;
import com.zyd.common.proto.client.WarChess.PlayerMutuallyRequest;
import com.zyd.common.proto.client.WarChess.PlayerMutuallyResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerMutuallyFeedback extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerMutuallyFeedback {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerMutuallyFeedbackRequest.PARSER, 1)) );
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerMutuallyFeedback {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerMutuallyFeedbackResponse.PARSER, 0)));  
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
        PlayerMutuallyFeedbackRequest req = paramValues.parseProtobuf(PlayerMutuallyFeedbackRequest.PARSER, 1);
        boolean agree = req.getIsAgree();
        battleRoomManager.MutuallyFeedback(user,agree);
        return new Packet(PlayerMutuallyFeedbackResponse.newBuilder());
    }
    
    
}
