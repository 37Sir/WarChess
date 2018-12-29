package com.zyd.demo.user.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerChatRequest;
import com.zyd.common.proto.client.WarChess.PlayerChatResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerChat extends BaseClientServlet {
  private Logger logger = LoggerFactory.getLogger(this.getClass());

  @Override
  public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
      logger.info("logRequest PlayerChatRequest {}, {}", TextFormat.printToUnicodeString(cui),
          TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerChatRequest.PARSER, 1)) );
  }

  @Override
  public void logResponse(Packet returnParam) {
      logger.info("logResponse PlayerChatResponse {}",
          TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerChatResponse.PARSER, 0)));  
  }

  @Override
  public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
      PlayerChatRequest req = paramValues.parseProtobuf(PlayerChatRequest.PARSER, 1);
      int number = req.getNumber();
      battleRoomManager.onChat(user.getUserName(),number);
      return new Packet(PlayerChatResponse.newBuilder());
  }
}
