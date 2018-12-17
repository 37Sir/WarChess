package com.zyd.demo.common;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.protobuf.TextFormat;
import com.zyd.common.proto.client.RpcProtocol.PushHeader;
import com.zyd.common.rpc.Packet;
import com.zyd.demo.common.enumuration.PushReqestName;
import com.zyd.demo.common.push.ProxyPushUtil;
import com.zyd.demo.common.push.PushTaskData;
import com.zyd.demo.user.pojo.User;


public class CommonService extends BaseService {
	private Logger logger = LoggerFactory.getLogger(CommonService.class.getName());
	/**
	 * @param name 与客户端约定的接口
	 * @param pushData 
	 * @param userTokenList 需要推送的玩家
	 */
	public void addProxyPushData(PushReqestName name,Packet pushData,String userToken, String content){
		
		List<String> list = new ArrayList<String>();
		list.add(userToken);
		addProxyPushData(name, pushData, list, content);
	}
	public void addProxyPushData(PushReqestName name,Packet pushData,List<String> userTokenList, String content){
		PushHeader.Builder pushHeader = PushHeader.newBuilder();
		pushHeader.addAllUserToken(userTokenList);
		String token = UUID.randomUUID().toString();
		ProxyPushUtil.pushData.put(token, new PushTaskData(name.getRequestName(), new Packet(pushHeader,pushData)));
		ProxyPushUtil.queue.add(token);
		logger.info("add push data to proxy , request:{},userTokenList:{} content:{}",name,TextFormat.printToString(pushHeader),content);
	}
	public User getUserById(int s) {
	    return userService.getUserById(s);
	}
	
	public void updateUser(User u) {
	    userService.updateUser(u);
	}
}
