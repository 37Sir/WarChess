package com.zyd.demo.common.nosql.impl;

import org.apache.commons.pool.impl.GenericObjectPool;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.demo.common.nosql.NoSql;
import com.zyd.demo.common.utils.ConfigurationUtil;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;

public class Redis implements NoSql {
  private static final Logger logger = LoggerFactory.getLogger("nosqlLogger");
  
    static GenericObjectPool.Config cfg = new GenericObjectPool.Config();
    static{
        cfg.maxActive = 40;
        cfg.maxWait = 2000;//time out
        cfg.minIdle = 10;
        cfg.timeBetweenEvictionRunsMillis = 2 * 60 * 1000;//connection will be checked per 2minutes
        cfg.minEvictableIdleTimeMillis = 10 * 60 * 1000;// an object will be dropped if it's idle last 10mins
        cfg.testWhileIdle = true;
    }

      private JedisPool pool;
      
      private String host;
      private int port;
      private String password;
      long logStartTime = System.currentTimeMillis();
      
      public void init(){//this method should be called in spring
          if(ConfigurationUtil.IS_OVER_SEA){
              pool = new JedisPool(cfg, host, port, ConfigurationUtil.REDIS_TIME_OUT);
              logger.info("launch jedis " + host + ":" + port);
          } else {
              pool = new JedisPool(cfg, host, port, ConfigurationUtil.REDIS_TIME_OUT,password);
              logger.info("launch jedis " + host + ":" + port + "password :" + password);        
          }
          logger.info("jedis init successfully");
    }

      @Override
      public void set(String key, String value) throws Exception {
          logStartTime = System.currentTimeMillis();
          Jedis jedis = pool.getResource();
          try{
              jedis.set(key, value);
          }catch(Exception e){
              logger.error("redis error: " + e);
              pool.returnBrokenResource(jedis);
              throw e;
          }finally{
              if(null != jedis){
                  pool.returnResource(jedis);
              }
              logger.info("{}\t set use:\t{}\tms",key,System.currentTimeMillis()-logStartTime);
          }        
      }

      @Override
      public String get(String key) throws Exception {
          logStartTime = System.currentTimeMillis();
          Jedis jedis = pool.getResource();
          try{
              String result = jedis.get(key);
              return result;
          }catch(Exception e){
              logger.error("redis error: " + e);
              pool.returnBrokenResource(jedis);
              throw e;
          }finally{
              if(null != jedis){
                  pool.returnResource(jedis);
              } 
              logger.info("{}\t get use:\t{}\tms",key,System.currentTimeMillis()-logStartTime);
          }
      }
      
      @Override
      public Long incr(String key) throws Exception {
          logStartTime = System.currentTimeMillis();
          Jedis jedis = pool.getResource();
          try {
              return jedis.incr(key);
          } catch (Exception e) {
              logger.error("redis error: ", e);
              pool.returnBrokenResource(jedis);
              throw e;
          } finally {
              if (null != jedis) {
                  pool.returnResource(jedis);
              }
              logger.debug("{}\t incr use:\t{}\tms", key, System.currentTimeMillis() - logStartTime);
          }
      }
      
      @Override
      public void decr(String key, int decr) throws Exception {
          logStartTime = System.currentTimeMillis();
          Jedis jedis = pool.getResource();
          try {
              jedis.decrBy(key, decr);
          } catch (Exception e) {
              logger.error("redis error: ", e);
              pool.returnBrokenResource(jedis);
              throw e;
          } finally {
              if (null != jedis) {
                  pool.returnResource(jedis);
              }
              logger.debug("{}\t decr use:\t{}\tms", key, System.currentTimeMillis() - logStartTime);
          }
      }  
      @Override
      public boolean exists(String key) throws Exception {
          logStartTime = System.currentTimeMillis();
          Jedis jedis = null;
          try {
              jedis = pool.getResource();
              return jedis.exists(key);
          } catch (Exception e) {
              logger.error("redis error: " + e);
              pool.returnBrokenResource(jedis);
              throw e;
          }finally{
              if(null != jedis){
                  pool.returnResource(jedis);
              }
              logger.info("{}\t exists use:\t{}\tms",key,System.currentTimeMillis()-logStartTime);
          }
      }
      @Override
      public void shutdown() {
          logStartTime = System.currentTimeMillis();
          try {
              pool.destroy();
              logger.info("redis destory successfully");
          } catch (Exception e) {
              logger.error("redis pool destory fail " + e);
          }
      }

      public static GenericObjectPool.Config getCfg() {
        return cfg;
      }

      public static void setCfg(GenericObjectPool.Config cfg) {
        Redis.cfg = cfg;
      }

      public JedisPool getPool() {
        return pool;
      }

      public void setPool(JedisPool pool) {
        this.pool = pool;
      }

      public String getHost() {
        return host;
      }

      public void setHost(String host) {
        this.host = host;
      }

      public int getPort() {
        return port;
      }

      public void setPort(int port) {
        this.port = port;
      }

      public String getPassword() {
        return password;
      }

      public void setPassword(String password) {
        this.password = password;
      }
      
      
}
