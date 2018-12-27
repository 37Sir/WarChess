package com.zyd.demo.user.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.ClientProtocol.PlayerDownRequest;
import com.zyd.common.proto.client.ClientProtocol.PlayerDownResopnse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerDown extends BaseClientServlet {
  private Logger logger = LoggerFactory.getLogger(this.getClass());
  
  @Override
  public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
      logger.info("logRequest PlayerDown {}, {}", TextFormat.printToUnicodeString(cui),
          TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerDownRequest.PARSER, 1)) );
  }
  
  @Override
  public void logResponse(Packet returnParam) {
      logger.info("logResponse PlayerDown {}",
          TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerDownResopnse.PARSER, 0)));  
  }

  @Override
  public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
      battleRoomManager.down(user);
      return new Packet(PlayerDownResopnse.newBuilder());
  }
}
