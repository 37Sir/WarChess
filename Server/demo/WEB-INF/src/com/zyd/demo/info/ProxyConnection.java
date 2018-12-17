package com.zyd.demo.info;

import java.util.concurrent.TimeUnit;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.BeanFactory;
import org.springframework.context.ApplicationContext;
import com.zyd.common.proto.client.ClientProtocol;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.rpc.Packet;
import com.zyd.common.rpc.PacketDecoder;
import com.zyd.common.rpc.PacketEncoder;
import com.zyd.common.rpc.RpcMessageHandler;
import com.zyd.common.rpc.RpcResponseHandler;
import com.zyd.demo.common.BaseClientServlet;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.common.push.ProxyPushUtil;
import com.zyd.demo.common.utils.ConfigurationUtil;
import io.netty.bootstrap.Bootstrap;
import io.netty.channel.Channel;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioSocketChannel;
import io.netty.handler.timeout.IdleStateHandler;
import io.netty.util.concurrent.GenericFutureListener;

public class ProxyConnection {
    private static final BeanFactory beanFactory = ConfigurationUtil.beanFactory;
    private final String host;
    private final int port;    
    private MessageHandler handler;
    private long lastReceiveRPCTime = System.currentTimeMillis();
    private final EventLoopGroup group;
    private boolean connected;

//    private static final BeanFactory beanFactory = ConfigurationUtil.beanFactory;
    static Logger logger = LoggerFactory.getLogger(ProxyConnection.class.getName());
    static Logger infoHeartBeat = LoggerFactory.getLogger("infoToProxyHeartBeat");
    private NioEventLoopGroup mainJobRunningGroup;
    private NioEventLoopGroup loginJobRunningGroup;
    private NioEventLoopGroup roomGameRunningGroup;
    
    int mainJobNum = ConfigurationUtil.MAIN_TASK;
    int loginJobNum = ConfigurationUtil.LOGIN_TASK;
    int roomGameNum = ConfigurationUtil.ROOM_GAME_TASK;
    private int getJobNum(int taskConfig){
      if(taskConfig <= 0){
          return 0;
      }
      
      int cpuNum = Runtime.getRuntime().availableProcessors();
      Double f = cpuNum*ConfigurationUtil.CPU_OFFSET*(taskConfig*1.0/ConfigurationUtil.TASK_COUNT);
      if(f<1){
          return 1;
      }else{
          return f.intValue();
      }
    }
    public ProxyConnection(String host, int port, EventLoopGroup group) {
        this.host = host;
        this.port = port;
        this.group = group;
        if(ConfigurationUtil.TASK_COUNT!=0){
            mainJobNum = getJobNum(ConfigurationUtil.MAIN_TASK);
            loginJobNum = getJobNum(ConfigurationUtil.LOGIN_TASK);
            roomGameNum = getJobNum(ConfigurationUtil.ROOM_GAME_TASK);
            mainJobRunningGroup = new NioEventLoopGroup(mainJobNum);
            loginJobRunningGroup = new NioEventLoopGroup(loginJobNum);
            roomGameRunningGroup = new NioEventLoopGroup(roomGameNum);
        }else{
            logger.warn("TASK NUM CONFIG ERROR");
            mainJobRunningGroup = new NioEventLoopGroup(1);
            loginJobRunningGroup = new NioEventLoopGroup(1);
            roomGameRunningGroup = new NioEventLoopGroup(1);
        }        
    }
    
    public void start() {
      logger.info("..info proxyConnnection start..");
      lastReceiveRPCTime = System.currentTimeMillis();
      // Configure the client.
      Bootstrap boot = new Bootstrap();
      boot.group(group);
      boot.channel(NioSocketChannel.class);
      boot.option(ChannelOption.TCP_NODELAY, true);
      boot.option(ChannelOption.SO_KEEPALIVE, true);
      boot.handler(new ChannelInitializer<SocketChannel>() {

          @Override
          protected void initChannel(SocketChannel ch) throws Exception {
            handler = new MessageHandler(ch);
            ChannelPipeline p = ch.pipeline();
            p.addLast("frameDecoder", new PacketDecoder());
            p.addLast("frameEncoder", new PacketEncoder());
            p.addLast("idleStateHandler", new IdleStateHandler(3, 0, 3));
            p.addLast("Handler", handler);
          }   
        
      });
      ChannelFuture f = boot.connect("127.0.0.1", 8089);
      f.addListener(new GenericFutureListener<ChannelFuture>() {

        @Override
        public void operationComplete(ChannelFuture future)
                throws Exception {
            try {
                future.sync();
                connected = true;
                ProxyPushUtil.proxyhandler = handler;
            } catch (Exception e) {
                logger.info("ProxyConnection: " + e.getMessage(),e);
                connected = false;
            }
        }

    });
      f.channel().closeFuture()
                  .addListener(new GenericFutureListener<ChannelFuture>() {
                    @Override
                    public void operationComplete(ChannelFuture future)
                            throws Exception {
                        if (connected) {
                            logger.info("ProxyConnection disconnected: "+ future.channel().remoteAddress());
                            onDisconnected();
                        }
                        group.schedule(new Runnable() {
                            @Override
                            public void run() {
                                start();
                            }
                        }, 100, TimeUnit.MILLISECONDS);
                    }
                  });

      
    }
    protected void onDisconnected() {
    }
    public class MessageHandler extends RpcMessageHandler {

      public MessageHandler(Channel channel) {
          super(channel);
      }
      private void updateUserGuide(final Packet msg) throws BaseException{
          if(msg.buffers.size()==3){
              try {
                  msg.buffers.set(1, msg.buffers.get(2));
//                  BaseClientServlet servlet = (BaseClientServlet) beanFactory.getBean("UpdateUserGuide");
//                  servlet.serviceUpdateUserData(msg, "UpdateUserGuide");
              } catch (Exception e) {
                  logger.error("UpdateUserGuide | base error happened in onRequestRpc ",e);
              }
          }
          
      }
      @Override
      protected void onRequestRpc(final int id, final String name,final Packet args,RPCQueueIDEnum queueId) {
          final MessageHandler handler = this;
          lastReceiveRPCTime = System.currentTimeMillis();
          logger.info("request.getId:{}, request.getName:{},queueId:{}",id,name,queueId.name());
          switch (queueId) {
            case LOGIN_QUEUE:
              Runnable r = new Runnable() {
                  @Override
                  public void run() {
                      try {
                          final long startTime = System.currentTimeMillis();
                          Packet result = null;
                          BaseClientServlet servlet = (BaseClientServlet) beanFactory.getBean(name);
                          result = servlet.serviceLogin(args, name);
                          logGapTimeWithOutRes(name, id, startTime);
                          handler.responseRpc(id, 0, result,RPCQueueIDEnum.LOGIN_QUEUE);
                      } catch (BaseException e) {
                          logger.warn(name + "|base error happened in onRequestRpc : " +e.getErrrorCode()+" "+ErrorCode.valueOf(e.getErrrorCode()).name(), e);
                          handler.responseRpc(id, e.getErrrorCode(), null,RPCQueueIDEnum.LOGIN_QUEUE);
                      } catch (Exception e) {
                          logger.error(name + "|base error happened in onRequestRpc ",e);
                          handler.responseRpc(id, ClientProtocol.ErrorCode.SERVER_ERROR_VALUE, null,RPCQueueIDEnum.LOGIN_QUEUE);
                      }
                  }
              };
              loginJobRunningGroup.execute(r);
              break;
          case ROOM_GAME_QUEUE:
               r = new Runnable() {
                  @Override
                  public void run() {
                      try {
                          final long startTime = System.currentTimeMillis();
                          Packet result = null;
                          BaseClientServlet servlet = (BaseClientServlet) beanFactory.getBean(name);
                          result = servlet.serviceWithViladate0(args, name);
                          logGapTimeWithOutRes(name, id, startTime);
                          handler.responseRpc(id, 0, result,RPCQueueIDEnum.ROOM_GAME_QUEUE);
                      } catch (BaseException e) {
                          logger.warn(name + "|base error happened in onRequestRpc : " +e.getErrrorCode()+" "+ErrorCode.valueOf(e.getErrrorCode()).name(),e);
                          handler.responseRpc(id, e.getErrrorCode(), null,RPCQueueIDEnum.ROOM_GAME_QUEUE);
                      } catch (Exception e) {
                          logger.error(name + "|base error happened in onRequestRpc ",e);
                          handler.responseRpc(id, ClientProtocol.ErrorCode.SERVER_ERROR_VALUE, null,RPCQueueIDEnum.ROOM_GAME_QUEUE);
                      }
                  }
              };
              roomGameRunningGroup.execute(r);
              break;
          case MAIN_QUEUE:
             r= new Runnable() {
                  public void run() {
                      try {
                          
                            final long startTime = System.currentTimeMillis();
                            Packet result = null;
                            logHeardMessage(name, id, args);
                            BaseClientServlet servlet = (BaseClientServlet) beanFactory.getBean(name);
                            result = servlet.serviceWithViladate(args, name);
                            logGapTimeWithOutRes(name, id, startTime);
                            updateUserGuide(args);                             
                            handler.responseRpc(id, 0, result,RPCQueueIDEnum.MAIN_QUEUE);
                          
                      } catch(BaseException e){
                          if(e.getErrrorCode() == ErrorCode.SERVER_ERROR_VALUE){
                              logger.warn(name + "|base error happened in onRequestRpc : " +e.getErrrorCode()+" "+ErrorCode.valueOf(e.getErrrorCode()).name(),e);
                          }else{
                              logger.warn(name + "|base error happened in onRequestRpc : " +e.getErrrorCode()+" "+ErrorCode.valueOf(e.getErrrorCode()).name());
                          }                          
                          handler.responseRpc(id, e.getErrrorCode(), null,RPCQueueIDEnum.MAIN_QUEUE);
                      } catch (Exception e) {
                          logger.error(name + "|error happened in onRequestRpc : ", e);
                          handler.responseRpc(id, ClientProtocol.ErrorCode.SERVER_ERROR_VALUE, null,RPCQueueIDEnum.MAIN_QUEUE);
                      }
                  }
              };
              mainJobRunningGroup.execute(r);
              break;  
          default:
              break;
          }
      }
      
      private void logGapTimeWithOutRes(String operName, int operId, long startTime){
          logger.info("{}",new StringBuilder().append("request end.name:").append(operName).append(",request_ID:").append(operId).append(" use:").append((System.currentTimeMillis() - startTime)).append("ms").toString());
      }

      private void logHeardMessage(String operName, int operId, Packet args){
//        logger.debug("{}",new StringBuilder().append("request in.name:").append(operName).append(",request_ID:").append(operId).append(" \nMessageHeaderInfo: \n").append(args.parseProtobuf(ClientProtocol.MessageHeaderInfo.PARSER, 0)).toString());
      }
      
      @Override
      protected void onCancelRpc(int id) {

      }

      @Override
      public void transmit(RpcResponseHandler rinfo, String name, Packet args) {
          
      }

      @Override
      protected void onRequestHeartBeatSend() {
          lastReceiveRPCTime = System.currentTimeMillis();
          super.responseServerHeartBeat();
//          infoHeartBeat.info("-------------infoToProxyHeartBeat response HeartBeat");
      }

      @Override
      protected void onResponseHeartBeatSend() {
          
      }

      @Override
      public void userEventTriggered(ChannelHandlerContext ctx, Object evt)
              throws Exception {
          if (lastReceiveRPCTime != 0L) {
              if ((System.currentTimeMillis()-lastReceiveRPCTime) > 9*1000l && ctx.channel().isActive()) {
                  infoHeartBeat.error("No rpc call received from info proxy {} in {} seconds, close channel.isActive:{}",9,ctx.channel().isActive());                  
                  ctx.close();
              }
          }
      }


  }
}
