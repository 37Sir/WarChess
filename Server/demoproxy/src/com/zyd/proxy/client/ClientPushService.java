package com.zyd.proxy.client;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.proxy.pojo.PushTaskData;

public class ClientPushService implements Runnable {
	static Logger logger = LoggerFactory.getLogger(ClientPushService.class.getName());
	@Override
	public void run() {
		logger.info("\n\n"
				+ "push service start!"
				+ "\n\n");
		while(true){
			try {
				 String token = ClientConnectPushUtil.queue.take();
				 logger.error("start push  data:{}",token);
				 PushTaskData data = ClientConnectPushUtil.pushData.get(token);
				 if(data == null){
					 continue;
				 }
				 logger.debug("start push  data:{} lastPush:{}",token,data.getLastPushTime());
				 if(data.getLastPushTime()!=null && (System.currentTimeMillis()-data.getLastPushTime())<3*1000l){
					 continue;
				 }
				 System.out.println(token+"---------------------------------------------"+data.toString());
				ClientConnectPushUtil.pushToClient(data.getResult(), data.getUserTokenList(),data.getName(),token);
			} catch (Exception e) {
				logger.error("error happened in push service ",e);
			}
		}
	

	}

}
