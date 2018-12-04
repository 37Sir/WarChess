package com.zyd.demo.common.memcached;

import java.util.Collection;
import java.util.HashSet;
import com.zyd.demo.common.utils.DomeJoiner;
/**
 * 缓存的key值转换操作
 *
 */
public class CacheKeyUtil {
	public static final String SPLIT_CHAR 								= "|";
	public static final String SPLIT_CHAR_2 							= "_";
	public static final DomeJoiner joiner = DomeJoiner.on(SPLIT_CHAR);
	public static final DomeJoiner joiner_2 = DomeJoiner.on(SPLIT_CHAR_2);

	
	public static HashSet<String> getCacheKeys(String tableName, Collection<Integer> listKeys, Integer userId) {
	     HashSet<String> result = new HashSet<String>();
	     for (Integer key : listKeys) {
	         result.add(getCacheKey(tableName, String.valueOf(key), userId));
	     }
	     return result;
	}
	/**
	 * 得到所有缓存的ID字段的key。
	 * 
	 * @param tableName
	 * @param key
	 * @return
	 */
	public static String getIdsCacheKey(String tableName, Long userId){
		return joiner.join(tableName,".ID.", userId);
	}
	/**
	 * @param tableName 分表名
	 * @param ojbId 分表的主键自增ID
	 * @param
	 * @return 缓存user的数据，与数据表一样
	 */
	public static String getCacheKey(String tableName, String ojbId, Integer userId){
		if(tableName != null){
			tableName = tableName;
		}
		return joiner.join(tableName, ojbId);
	}
	
	public static String getCacheKeyNoRoute(String tableName, String ojbId){
		return joiner.join(tableName, ojbId);
	}
	

	
}