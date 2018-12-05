package com.zyd.gw.test;

import com.zyd.common.proto.client.ClientProtocol.LoginResponse;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderResponse;
import com.zyd.common.proto.client.RpcProtocol.RpcHeader;
import com.zyd.common.rpc.Packet;
import io.netty.channel.ChannelHandler;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;

public class TestHandle extends SimpleChannelInboundHandler<Packet> {

  @Override
  protected void channelRead0(ChannelHandlerContext arg0, Packet arg1) throws Exception {
    MessageHeaderResponse req = arg1.parseProtobuf(MessageHeaderResponse.PARSER, 0);
    System.out.println(req.getError());
  }


}
