package com.zyd.demo.stone.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerActiveRequest;
import com.zyd.common.proto.client.WarChess.PlayerActiveResponse;

import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerActive extends BaseClientServlet {
  private Logger logger = LoggerFactory.getLogger(this.getClass());

  @Override
  public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
      logger.info("logRequest PlayerPaintingEndRequest {}, {}", TextFormat.printToUnicodeString(cui),
          TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerActiveRequest.PARSER, 1)) );
  }

  @Override
  public void logResponse(Packet returnParam) {
      logger.info("logResponse PlayerPaintingEndResponse {}",
          TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerActiveResponse.PARSER, 0)));  
  }

  @Override
  public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
      PlayerActiveRequest req = paramValues.parseProtobuf(PlayerActiveRequest.PARSER, 1);
      battleRoomManager.onRequest(user.getUserName(),req);
      return new Packet(PlayerActiveResponse.newBuilder());
  }
}
