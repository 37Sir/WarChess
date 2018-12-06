package com.zyd.proxy.client;

import io.netty.buffer.Unpooled;

import java.util.List;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.LinkedBlockingQueue;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.ClientProtocol.MessageHeaderResponse;
import com.zyd.common.rpc.Packet;
import com.zyd.proxy.pojo.PushTaskData;


public class ClientConnectPushUtil {
	public static BlockingQueue<String> queue = new LinkedBlockingQueue<String>();
	public static ConcurrentMap<String, PushTaskData> pushData = new ConcurrentHashMap<String,PushTaskData>();
	static Logger logger = LoggerFactory.getLogger(ClientConnectPushUtil.class.getName());
	public static ConcurrentMap<String, ClientConnectionHandler> clientConnectionHandlerMap = new ConcurrentHashMap<String, ClientConnectionHandler>();
	
	public static void init(){
		ExecutorService excutor = Executors.newSingleThreadExecutor();
		excutor.execute(new ClientPushService());
	}
		
	
	public static void pushToClient(Packet data, List<String> userTokenList,String name,String token){
		if(userTokenList != null && userTokenList.size() != 0){
			int size = data.buffers.size();
//			for(int i=0;i<size;i++){
//				data.buffers.set(i, Unpooled.wrappedBuffer(DesUtil.encrypt(data.buffers.get(i).array(), Config.pwd.substring(0, 8),Config.pwd.substring(Config.pwd.length()-8))));
//			}
			
			for(String userToken:userTokenList){
				ClientConnectionHandler handler = clientConnectionHandlerMap.get(userToken);
				if(handler != null && handler.getChannel().isActive()){
					logger.info("push data to client:{},responseName:{},handler:{},token:{}",userToken,name,handler,token);
					MessageHeaderResponse.Builder header = MessageHeaderResponse.newBuilder();
					header.setName(name);
					header.setError(0);
					Packet ddd = new Packet(header, data);
					handler.getChannel().writeAndFlush(ddd);
					
					ClientConnectPushUtil.pushData.remove(token);
				}else{
					ClientConnectPushUtil.pushData.remove(token);
					logger.warn("handler is null {} or active {} usertoken：{}",handler==null?true:handler.getChannel().isActive(),userToken);
				}
			}
		}
	}
}
