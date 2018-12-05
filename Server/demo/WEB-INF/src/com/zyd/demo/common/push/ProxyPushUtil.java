package com.zyd.demo.common.push;

import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.LinkedBlockingQueue;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.proto.client.RpcProtocol.RPCQueueIDEnum;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.info.ProxyConnection;

public class ProxyPushUtil {
	private static final  Logger logger = LoggerFactory.getLogger(ProxyPushUtil.class.getName());
	public static BlockingQueue<String> queue = new LinkedBlockingQueue<String>();
	public static ConcurrentMap<String, PushTaskData> pushData = new ConcurrentHashMap<String,PushTaskData>();
	public static ProxyConnection.MessageHandler proxyhandler;
	
	public static void init(){
		logger.info("game push service start");
		ExecutorService excutor = Executors.newFixedThreadPool(2);
		excutor.execute(new ProxyPushThread());
	}
	
	public static void pushToProxy(String name,String token,Packet data){
	        System.out.println(proxyhandler.toString() + proxyhandler.getChannel().isActive() );
			if(proxyhandler != null && proxyhandler.getChannel().isActive()){
				logger.info("push data from server to proxy. name:{},token:{}",name,token);
				proxyhandler.requestRpc("#"+name, new Packet(data),null, RPCQueueIDEnum.MAIN_QUEUE);
			}else{
				logger.error("proxy handler not available,pushing fail,remove push data.name:{},token:{}",name,token);
				pushData.remove(token);
			}
	}
}
