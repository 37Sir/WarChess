package com.zyd.demo.round.servlet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderInfo;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.user.pojo.User;

public class PlayerMutuallyFeedback extends BaseClientServlet {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public void logRequest(MessageHeaderInfo cui, Packet paramValues) {
        super.logRequest(cui, paramValues);
    }

    @Override
    public void logResponse(Packet returnParam) {
        super.logResponse(returnParam);
    }

    @Override
    public Packet service(Packet paramValues, Integer deviceType, User user) throws Exception {
        return super.service(paramValues, deviceType, user);
    }
    
    
}
