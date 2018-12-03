package com.zyd.common.rpc;

import java.util.List;
import java.util.Vector;
import com.google.protobuf.MessageLite;
import com.google.protobuf.Parser;
import io.netty.buffer.ByteBuf;
import io.netty.buffer.Unpooled;

public class Packet {
  public final List<ByteBuf> buffers;
  
  // constructs a new packet, using a default buffer list
  public Packet() {
      buffers = new Vector<ByteBuf>();
  }
  
  // constructs a new packet, using a sublist.
  public Packet(List<ByteBuf> from, int fromIndex, int toIndex) {
      buffers = from.subList(fromIndex, toIndex);
  }
  
  // construct a new packet with a list of buffer.
  public Packet(Object... args) {
      buffers = new Vector<ByteBuf>();
      for (Object obj : args) {
          if (obj instanceof ByteBuf)
              add((ByteBuf)obj);
          else if (obj instanceof MessageLite)
              add((MessageLite)obj);
          else if (obj instanceof MessageLite.Builder)
              add((MessageLite.Builder)obj);
          else if (obj instanceof Packet)
              add((Packet)obj);
      }
  }
  // add a buffer to the packet list directly.
  public void add(ByteBuf buff) {
      buffers.add(buff);
  }
  
  // add a message to the packet list.
  public void add(MessageLite msg) {
      add(Unpooled.wrappedBuffer(msg.toByteArray()));
  }

  // add a message builder to the packet list.
  public void add(MessageLite.Builder builder) {
      add(Unpooled.wrappedBuffer(builder.build().toByteArray()));
  }
  
  // add another packet
  public void add(Packet packet) {
      if (packet != null)
          buffers.addAll(packet.buffers);
  }
  
  // parse buffer to an object
  public <T extends MessageLite> T parseProtobuf(Parser<T> parser, int id) {
      try {
          final ByteBuf msg = buffers.get(id);
          final byte[] array;
          final int offset;
          final int length = msg.readableBytes();
          if (msg.hasArray()) {
              array = msg.array();
              offset = msg.arrayOffset() + msg.readerIndex();
          } else {
              array = new byte[length];
              msg.getBytes(msg.readerIndex(), array, 0, length);
              offset = 0;
          }
          return parser.parseFrom(array, offset, length);
      }
      catch (Exception e) {
          return null;
      }
  }
}