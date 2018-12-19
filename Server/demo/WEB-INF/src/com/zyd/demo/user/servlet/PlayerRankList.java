package com.zyd.demo.user.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.ClientProtocol.PlayerRankListRequest;
import com.zyd.common.proto.client.ClientProtocol.PlayerRankListResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerRankList extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
  
    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerMutuallyFeedback {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerRankListRequest.PARSER, 1)) );
    }
    
    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerMutuallyFeedback {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerRankListResponse.PARSER, 0)));  
    }
  
    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
        PlayerRankListResponse.Builder res = userService.buildPlayerRankListResponse();
        return new Packet(res);
    }
}
