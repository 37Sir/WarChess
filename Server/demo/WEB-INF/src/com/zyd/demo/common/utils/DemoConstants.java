package com.zyd.demo.common.utils;

import java.util.Random;

public class DemoConstants {
    public final static Integer PRIVATE_KEY = 93574451;
	public static final String LOCK_FLUSH_ALLCACHE_TO_DB = "L_F_A_T_DB";
	
	// XXX 压力测试
	// 体力自动恢复cd
	public static final int HEART_COOL_TIME_GAP_MIN = 6;// default 6
	
	public static final String ENCODING = "UTF-8";

	public static final String COMMON_DELIMITER = ",";

	public static final String COMMON_LIST_DELIMITER = ";";

	// 时间
	public static final long ONE_DAY_TIME = 24 * 60 * 60 * 1000l;

	public static final int ONE_DAY_SECS = 24 * 60 * 60;

	public static final long ONE_HOUR_TIME = 60 * 60 * 1000l;

	public static final int ONE_HOUR_SECS = 60 * 60;// memcache单位时间是秒。

	public static final int TWO_HOUR_SECS = 2 * 60 * 60;// memcache单位时间是秒。
	
	public static final int ONR_MIN_TIME = 60 * 1000;

	// mem的缓存时间
	public static final int CACHE_DATA_ONE_HOUR = 60 * 60;// memcache单位时间是秒。

	public static final int CACHE_DATA_DAY = 24 * 60 * 60;

	public static final int CACHE_DATA_WEEK = 24 * 60 * 60 * 7;

	// 保留1周
	public static final long PLAYER_MAIL_TIME_OUT = 60 * 60 * 1000;

	public static final int USER_NICK_NAME_MAX_LENGTH = 14;// 单位——汉字



	// user icon
	public static final String[] ICON = { "c001", "c002", "c003", "c004", "c005", "c006" };

	public static final String INIT_ICON() {
		Random rand = new Random();
		return ICON[rand.nextInt(ICON.length)];
	}

	
	
	
}