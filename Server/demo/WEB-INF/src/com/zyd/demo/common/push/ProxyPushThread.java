package com.zyd.demo.common.push;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class ProxyPushThread implements Runnable {
	private final Logger logger = LoggerFactory.getLogger(ProxyPushThread.class.getName());
	@Override
	public void run() {

		while(true){
			try {
				 String token = ProxyPushUtil.queue.take();
				 logger.info("start push  data to proxy:{}",token);
				 PushTaskData data = ProxyPushUtil.pushData.remove(token);
				 if(data == null){
					 continue;
				 }
				 ProxyPushUtil.pushToProxy(data.getName(), token, data.getResult());
			} catch (Exception e) {
				logger.error(e.getMessage(),e);
			}
		}
	

	}

}
