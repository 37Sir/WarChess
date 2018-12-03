package com.zyd.demo.common.memcached;

import java.util.HashSet;
import java.util.List;
import java.util.Map;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.demo.common.enumuration.TableName;
import com.zyd.demo.common.jdbc.MapperHelper.DBCacheMissHandler;
import com.zyd.demo.common.utils.KpConstants;
import com.zyd.demo.common.utils.StringUtil;

public class CacheJDBCHandler extends MemcachedHandler {
    private static Logger logger = LoggerFactory.getLogger("memcachedFlush");
    @SuppressWarnings("unchecked")
    public HashSet<Integer> getIdsByCache(final String tableName, final Long objId) {
        try {
            String cacheKey = CacheKeyUtil.getIdsCacheKey(tableName, objId);
            // 记录key。
            String idsStr = loadValueStr(cacheKey, new DBCacheMissHandler() {
                @Override
                public String loadFromDB() throws Exception {
                    String result = "";
                    if ("tab".equals(tableName)) {
                        List<Integer> ids = null;
                        for (final Integer id : ids) {
                            result += (id + KpConstants.COMMON_DELIMITER);
                        }
                    } else {
                        logger.error("not found table:{}", tableName);
                    }
                    // logger.debug("result:"+result);
                    return result;
                }
            });
            HashSet<Integer> set = new HashSet<Integer>();
            
            if (idsStr != null && !"".equals(idsStr.trim()) && !"null".equals(idsStr.trim())) {
                for (String s : idsStr.split(KpConstants.COMMON_DELIMITER)) {
                    if (!"null".equals(s.trim()) && !"".equals(s.trim())) {
                        set.add(StringUtil.toInt(s.trim()));
                    }
                }
            }
            return set;
        } catch (Exception e) {
            logger.error("error happened in CacheJDBCMapper getIdsByCache", e);
        }
        return new HashSet<Integer>();
    }
    
    
    private String idsToString(HashSet<Integer> ids) {
        String res = "";
        for (Integer ins : ids) {
            res += (ins + KpConstants.COMMON_DELIMITER);
        }
        return res;
    }
    
    
    public <T> Map<Integer, T> getCollectionByCache(final Class<T> clazz, final HashSet<String> setKeys) {
        return loadValues(clazz, setKeys);
    }
    
    public <T> Map<Long, T> getCollectionByCacheLong0(final Class<T> clazz, final HashSet<String> setKeys) {
        return loadValuesLong0(clazz, setKeys);
    }
    
    /**
     * 这个接口，缓存获取的是不分表的表数据。
     */
    public <T> T getDataByCacheNoSplit(final String tableName, final Long objId, Class<T> clazz) {
        try {
            String cacheKey = CacheKeyUtil.getCacheKeyNoRoute(tableName, String.valueOf(objId));
            DBCacheMissHandler cmHandler = new DBCacheMissHandler() {

                @SuppressWarnings("unchecked")
                @Override
                public Object loadFromDB() throws Exception {
                    Object result = null;
                    if (tableName.equals(TableName.USER.name())) {
//                        result = mapperHelper.getUserMapper().selectByPrimaryKey(objId);
                    }
                    return result;
                }
            };
            return (T) loadValue(cacheKey, cmHandler, clazz);
        } catch (Exception e) {
            logger.error("error happened in CacheJDBCMapper getUserByCache", e);
        }
        return null;
    }
    
    
    public HashSet<Integer> create(String tableName, Object obj) {
        HashSet<Integer> setIds = null;
        String idsCacheKey = null;
        String cacheKey = null;
        if (TableName.USER.name().equals(tableName)) {
//            User objUser = ((User) obj);
//            mapperHelper.getUserMapper().insert(objUser);
//            cacheKey = CacheKeyUtil.getCacheKey(tableName, String.valueOf(objUser.getId()), user.getId());
            // 新建user，同时缓存user
            setCacheEncodeWithDB(cacheKey, tableName, obj);
            return null;
        } else {
          logger.error("not found table:{}", tableName);
        }
        return setIds;
    }
    
    
}
