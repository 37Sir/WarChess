package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesRequest;
import com.zyd.common.proto.client.WarChess.PlayerBattleMesResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerBattleMes extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

      @Override
      public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
          logger.info("logRequest PlayerBattleMes {}, {}", TextFormat.printToUnicodeString(cui),
              TextFormat.printToUnicodeString(paramValues.parseProtobuf(PlayerBattleMesRequest.PARSER, 1)) );
      }
  
      @Override
      public void logResponse(Packet returnParam) {
          logger.info("logResponse PlayerBattleMes {}",
              TextFormat.printToUnicodeString(returnParam.parseProtobuf(PlayerBattleMesResponse.PARSER, 0)));  
      }
  
      @Override
      public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
          PlayerBattleMesRequest playerBattleMesRequest = paramValues.parseProtobuf(PlayerBattleMesRequest.PARSER, 1);
          PlayerBattleMesResponse playerBattleMesResponse = battleRoomManager.onRequest(user.getUserName(),playerBattleMesRequest);
          return new Packet(playerBattleMesResponse);
      }
    
    
}
