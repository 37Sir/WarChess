package com.zyd.demo.common.nosql;

public interface NoSql {
    public void set(String key, String value) throws Exception;
    public String get(String key) throws Exception;
    public Long incr(String key) throws Exception;
    public void decr(String key,int decr) throws Exception;
    public boolean exists(String key)throws Exception;
    public void shutdown();
}
