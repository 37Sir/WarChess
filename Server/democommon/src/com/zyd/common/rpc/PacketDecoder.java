package com.zyd.common.rpc;

import java.util.List;
import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.ByteToMessageDecoder;
import io.netty.handler.codec.CorruptedFrameException;
import io.netty.util.ReferenceCountUtil;

public class PacketDecoder extends ByteToMessageDecoder {


  Packet decoding = new Packet();

  @Override
  protected void decode(ChannelHandlerContext ctx, ByteBuf in, List<Object> out) throws Exception {
      in.markReaderIndex();

      int length = 0;
      int shift = 0;
      
      // Read variant32 length
      for (;;) {
          if (!in.isReadable()) {
              in.resetReaderIndex();
              return;
          }

          byte tmp = in.readByte();

          if (tmp >= 0) {
              length |= tmp << shift;
              break;
          }
          
          length |= (tmp & 0x7f) << shift;
          shift += 7;
          
          // invalid length
          if (length < 0) {
              throw new CorruptedFrameException("negative length: " + length);
          }
      }
          
      // packet ended.
      if (length > 0) {
          if (in.readableBytes() < length - 1) {
              in.resetReaderIndex();
              return;
          }
          decoding.buffers.add(in.readBytes(length - 1));
      } else {
          out.add(decoding);
          decoding = new Packet();
      }
      ReferenceCountUtil.release(out);
      ReferenceCountUtil.release(decoding.buffers);
  }

}
