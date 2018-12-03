package com.zyd.demo.common.push;

import com.zyd.common.rpc.Packet;

public class PushTaskData {
	private String name;
	private Packet result;
	public PushTaskData(String name,Packet result) {
		this.name = name;
		this.result = result;
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
	

}
