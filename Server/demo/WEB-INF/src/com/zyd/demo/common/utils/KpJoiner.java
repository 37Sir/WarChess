package com.zyd.demo.common.utils;

import com.google.common.base.Joiner;


public class KpJoiner {
	private final Joiner joiner;
	
	private KpJoiner(String SPLIT_CHAR){
		joiner = Joiner.on(SPLIT_CHAR);
	}
	
	public final static KpJoiner on(String SPLIT_CHAR){
		return new KpJoiner(SPLIT_CHAR);
	}
	
	public String join(Object...strs){
		return "|" + joiner.join(strs);
	}

}
