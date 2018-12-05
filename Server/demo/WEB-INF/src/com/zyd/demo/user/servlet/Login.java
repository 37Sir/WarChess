package com.zyd.demo.user.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.ClientProtocol.LoginRequest;
import com.zyd.common.proto.client.ClientProtocol.LoginResponse;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;

public class Login extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        logger.info("logRequest Login {}, {}", TextFormat.printToUnicodeString(cui),
            TextFormat.printToUnicodeString(paramValues.parseProtobuf(LoginRequest.PARSER, 1)));    
    }

    @Override
    public void logResponse(Packet returnParam) {
        logger.info("logResponse Login {}",
            TextFormat.printToUnicodeString(returnParam.parseProtobuf(LoginResponse.PARSER, 0)));    }

    @Override
    public Packet serviceBeforeLogin(Packet paramValues, String rpcName) throws Exception {
        MessageHeaderInfo mi = paramValues.parseProtobuf(MessageHeaderInfo.PARSER, 0);
        String token = mi.getUserToken();
        logRequest(mi, paramValues);
        LoginRequest req = paramValues.parseProtobuf(LoginRequest.PARSER, 1);
        String userName = req.getUserName();
        
        LoginResponse.Builder res = userService.login(userName,token);
        logResponse(new Packet(res));
        return new Packet(res);
    }
    

}
