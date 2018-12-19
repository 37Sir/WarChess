package com.zyd.demo.common.memcached;

import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeoutException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.esotericsoftware.kryo.Kryo;
import com.esotericsoftware.kryo.io.Input;
import com.esotericsoftware.kryo.io.Output;
import com.zyd.demo.common.enumuration.TableName;
import com.zyd.demo.common.jdbc.MapperHelper;
import com.zyd.demo.common.jdbc.MapperHelper.DBCacheMissHandler;
import com.zyd.demo.common.utils.DemoConstants;
import com.zyd.demo.user.pojo.User;
import net.rubyeye.xmemcached.MemcachedClient;
import net.rubyeye.xmemcached.exception.MemcachedException;

public class MemcachedHandler {
    private static final Logger logger = LoggerFactory.getLogger(MemcachedHandler.class);
    //mcc的操作不超过100ms
    private static final long opTimeout = 5000l;
    protected MemcachedClient mcc;
    protected MapperHelper mapperHelper;
    
    public MemcachedClient getMcc() {
      return mcc;
    }
    
    public MapperHelper getMapperHelper() {
      return mapperHelper;
    }

    public void setMapperHelper(MapperHelper mapperHelper) {
      this.mapperHelper = mapperHelper;
    }

    public void setMcc(MemcachedClient mcc) {
      this.mcc = mcc;
    }

    public <T> T loadValue(String key, DBCacheMissHandler handler, Class<T> clazz) {
      return loadValue(key, handler, DemoConstants.CACHE_DATA_DAY, clazz);
    }
    /**
     * 存储str。
     */
    public String loadValueStr(String key, DBCacheMissHandler handler) {
        return loadValueStr(key, handler, DemoConstants.CACHE_DATA_DAY);
    }

    public String loadValueStr(String key) {
        return loadValueStr(key, null);
    }

    /*
     * base load string value method
     */
    private String loadValueStr(final String key, DBCacheMissHandler handler, int expr) {
        String result = null;
        try {
            byte[] serBytes = getMccValueTimeOut(key);
            if (serBytes == null && handler != null) {
                result = handler.loadFromDB();
                if (result == null) {
                    logger.warn("not find this string in db!---" + "\tkey:" + key);
                    return null;
                }
                setCacheStrNoDB(key, expr, result);
            } else if (serBytes != null) {
                result = new String(serBytes);
            }
        } catch (Exception e) {
            logger.error("error happend during get key from memcache" + "\tkey:" + key + " : ", e);
        }
        return result;
    }
    
    public boolean setCacheStrNoDB(String key, int expr, final String obj) {
        return setStrValue(key, expr, obj.getBytes());
    }
    
    //存string
    private boolean setStrValue(String key, int expr, final byte[] bytes) {
      try {
          if (bytes.length > 2048) {
              logger.warn("MemcachedHandler key : " + key + " : value length : " + bytes.length);
          }
          return mcc.set(key, expr, bytes, opTimeout);
      } catch (TimeoutException | InterruptedException | MemcachedException e) {
          logger.error("error happened in MemcachedHandler setMccValueTimeOut", e);
      }
      return false;
    }
    
    //存对象
    protected <T> T loadValue(String key, DBCacheMissHandler handler, int expr, Class<T> clazz) {
        T result = null;
        try {
            byte[] serBytes = getMccValueTimeOut(key);
            if (serBytes == null && handler != null) {
                result = handler.loadFromDB();
                if (result == null) {
                    logger.warn(":not find  this object in db!---" + "\tkey:" + key);
                    return null;
                }
                setCacheEncodeNoDB(key, expr, result);
            } else if (serBytes != null) {
                result = decode(serBytes, clazz);
                if (result == null && handler != null) {
                    result = handler.loadFromDB();
                    if (result == null) {
                        logger.debug(":There is not have this object in db!---" + "\tkey:" + key);
                        return null;
                    }
                    setCacheEncodeNoDB(key, expr, result);
                }
            }
        } catch (Exception e) {
            logger.error("error happend during get key from memcache " + "\tkey:" + key + " : ", e);
        }
        return result;
    }
    
    /**
     * 获取数据是通用的
     */
    protected byte[] getMccValueTimeOut(String key) {
        try {
            return mcc.get(key, opTimeout);
        } catch (TimeoutException | InterruptedException | MemcachedException e) {
            logger.error("error happened in MemcachedHandler getMccValueTimeOut", e);
        }
        return null;
    }
    //更新数据库和缓存
    public boolean setCacheEncodeWithDB(String cacheKey, String tableName, int expr, final Object object) {
        if (tableName.equals(TableName.USER.getTableName())) {
            User obj = (User) object;
            mapperHelper.getUserMapper().updateByPrimaryKey(obj);
        } 
  
        return setEncodeValue(cacheKey, expr, object);
    }
    
    public boolean setCacheEncodeWithDB(String key, String tableName, final Object obj) {
        return setCacheEncodeWithDB(key, tableName, DemoConstants.CACHE_DATA_DAY, obj);
    }
    
    public boolean setCacheEncodeNoDB(String key, int expr, final Object obj) {
        return setEncodeValue(key, expr, obj);
    }
    
    public boolean setEncodeValue(String key, int expr, final Object obj) {
        try {
            byte[] value = encode(obj);
            if (value.length > 2048) {
                logger.warn("MemcachedHandler key : " + key + " : value length : " + value.length);
            }
            return mcc.set(key, expr, value, opTimeout);
        } catch (TimeoutException | InterruptedException | MemcachedException e) {
            logger.error("error happened in MemcachedHandler setMccValueTimeOutBeEncode", e);
        }
        return false;
    }
    
    //根据多个id取出对应数据
    protected <T> Map<Integer, T> loadValues(Class<T> clazz, Collection<String> keyCollections) {
        Map<Integer, T> result = new HashMap<Integer, T>();
        for (Map.Entry<String, T> entry : loadValues0(clazz, keyCollections).entrySet()) {
            result.put(
                Integer.parseInt(entry.getKey().substring(entry.getKey().lastIndexOf(CacheKeyUtil.SPLIT_CHAR) + 1)),
                entry.getValue());
        }
        return result;
    }

    private <T> Map<String, T> loadValues0(Class<T> clazz, Collection<String> keyCollections) {
        Map<String, T> mapResult = new HashMap<String, T>();
        try {
            //一块从memcache中取得一匹key。
            Map<String, byte[]> mapStrBytes = getCollectionMccValueTimeOut(keyCollections);
            if (mapStrBytes != null) {
                for (Map.Entry<String, byte[]> entry : mapStrBytes.entrySet()) {
                    if (entry.getValue() != null) {
                        String key = entry.getKey();
                        mapResult.put(key, decode(entry.getValue(), clazz));
                    }
                }
            }
  
        } catch (Exception e) {
            logger.error("error happend during get key from memcache ", e);
        }
        return mapResult;
    }
    
    protected Map<String, byte[]> getCollectionMccValueTimeOut(Collection<String> keyCollections) {
      try {
          return getMcc().get(keyCollections, opTimeout);
      } catch (TimeoutException | InterruptedException | MemcachedException e) {
          logger.error("error happened in MemcachedHandler getCollectionMccValueTimeOut", e);
      }
      return null;
    }
    
    protected <T> Map<Long, T> loadValuesLong0(Class<T> clazz, Collection<String> keyCollections) {
        Map<Long, T> mapResult = new HashMap<Long, T>();
        try {
            //一块从memcache中取得一匹key。
            Map<String, byte[]> mapStrBytes = getCollectionMccValueTimeOut(keyCollections);
            if (mapStrBytes != null) {
                for (Map.Entry<String, byte[]> entry : mapStrBytes.entrySet()) {
                    if (entry.getValue() != null) {
                        String key = entry.getKey();
                        mapResult.put(Long.parseLong(key.substring(key.lastIndexOf(CacheKeyUtil.SPLIT_CHAR) + 1)),
                            decode(entry.getValue(), clazz));
                    }
                }
            }
  
        } catch (Exception e) {
            logger.error("error happend during get key from memcache ", e);
        }
        return mapResult;
    }
    
    //排行版信息
    public List<User> getRankList (){
        return mapperHelper.getUserMapper().selectByRank();
    }
    
    
    
    /**
     * 对象转byte
     * @param obj
     * @return
     */
    public byte[] encode(Object obj) {
        Kryo kryo = new Kryo();
        kryo.register(obj.getClass());
        byte[] result = null;
        Output baos = null;
        try {
            baos = new Output(512, 1024 * 1024);
            kryo.writeObject(baos, obj);
            result = baos.toBytes();
        } finally {
            if (baos != null) {
                baos.close();
            }
        }
        return result;
    }
    /**
     * byte转对象
     * @param bytes
     * @param type
     * @return
     */
    public <T> T decode(byte[] bytes, Class<T> type) {
        Kryo kryo = new Kryo();
        kryo.register(type);
        T result = null;
        Input in = null;
        try {
            in = new Input(bytes);
            result = kryo.readObject(in, type);
        } catch (Exception e) {
            logger.error("decode : ", e);
        } finally {
            if (in != null) {
                in.close();
            }
        }
        return result;
  }
    
}
