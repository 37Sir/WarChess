package com.zyd.common.rpc;

import com.google.protobuf.CodedOutputStream;
import io.netty.buffer.ByteBuf;
import io.netty.buffer.ByteBufOutputStream;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.MessageToByteEncoder;

public class PacketEncoder extends MessageToByteEncoder<Packet> {
  @Override
  protected void encode( ChannelHandlerContext ctx, Packet msg, ByteBuf out) throws Exception {

      CodedOutputStream headerOut = CodedOutputStream.newInstance(new ByteBufOutputStream(out));

      // write buffs
      for (ByteBuf buff : msg.buffers) {
          int bodyLen = buff.readableBytes();
          int headerLen = CodedOutputStream.computeRawVarint32Size(bodyLen + 1);
          out.ensureWritable(headerLen + bodyLen);

          headerOut.writeRawVarint32(bodyLen + 1);
          headerOut.flush();

          out.writeBytes(buff, buff.readerIndex(), bodyLen);
      }
      
      // write packet end
      out.writeByte(0);
  }
}
