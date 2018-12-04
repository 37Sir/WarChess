package com.zyd.demo.common.utils;

import com.google.common.base.Joiner;


public class DomeJoiner {
	private final Joiner joiner;
	
	private DomeJoiner(String SPLIT_CHAR){
		joiner = Joiner.on(SPLIT_CHAR);
	}
	
	public final static DomeJoiner on(String SPLIT_CHAR){
		return new DomeJoiner(SPLIT_CHAR);
	}
	
	public String join(Object...strs){
		return "|" + joiner.join(strs);
	}

}
