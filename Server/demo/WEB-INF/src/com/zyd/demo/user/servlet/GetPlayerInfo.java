package com.zyd.demo.user.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerInfoRequest;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerInfoResponse;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerRankRequest;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerRankResopnse;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class GetPlayerInfo extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    
    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerMutuallyFeedback {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(GetPlayerInfoRequest.PARSER, 1)) );
    }
    
    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerMutuallyFeedback {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(GetPlayerInfoResponse.PARSER, 0)));  
    }
  
    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
        GetPlayerInfoResponse.Builder res = userService.buildGetPlayerInfoResponse(user);
        return new Packet(res);
    }
}
