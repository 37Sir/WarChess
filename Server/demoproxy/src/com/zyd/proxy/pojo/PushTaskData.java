package com.zyd.proxy.pojo;

import java.util.ArrayList;
import java.util.List;
import com.zyd.common.rpc.Packet;

public class PushTaskData {
	private String name;
	private Packet result;
	private String token;
	private Long lastPushTime;
	private List<String> userTokenList = new ArrayList<String>();
	public PushTaskData(String name,Packet result,String token,Long lastPushTime,List<String> userTokenList) {
		this.name = name;
		this.result = result;
		this.token = token;
		this.lastPushTime = lastPushTime;
		this.userTokenList = userTokenList;
	}
	
	public List<String> getUserTokenList() {
		return userTokenList;
	}

	public void setUserTokenList(List<String> userTokenList) {
		this.userTokenList = userTokenList;
	}

	public Long getLastPushTime() {
		return lastPushTime;
	}

	public void setLastPushTime(Long lastPushTime) {
		this.lastPushTime = lastPushTime;
	}

	public String getName() {
		return name;
	}
	public void setName(String name) {
		this.name = name;
	}
	public Packet getResult() {
		return result;
	}
	public void setResult(Packet result) {
		this.result = result;
	}
	public String getToken() {
		return token;
	}
	public void setToken(String token) {
		this.token = token;
	}
	

}
