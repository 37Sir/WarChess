package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerPaintingEndRequest;
import com.zyd.common.proto.client.WarChess.PlayerPaintingEndResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;
/*玩家图像渲染完成，可以开始下一回合的接口*/
public class PlayerPaintingEnd extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest PlayerPaintingEndRequest {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerPaintingEndRequest.PARSER, 1)) );
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse PlayerPaintingEndResponse {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerPaintingEndResponse.PARSER, 0)));  
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
        PlayerPaintingEndResponse res = battleRoomManager.onRequest(user.getUserName());
        return new Packet(res);
    }
        
}
