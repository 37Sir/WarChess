package com.zyd.demo.common.memcached;

/**
 * 封装同步数据库 的操作，更新缓存后，异步定时同步数据用。
 * @author ZhaoYiDing
 *
 */
public class SynchronizationMysql {
    private String tableName;
    private Object object;
    private Integer type; //是什么操作(更新，或者插入)
    public SynchronizationMysql(String tableName, Object object,Integer type) {
        super();
        this.tableName = tableName;
        this.object = object;
        this.type = type;
    }
    
    public String getTableName() {
      return tableName;
    }
    public void setTableName(String tableName) {
      this.tableName = tableName;
    }
    public Object getObject() {
      return object;
    }
    public void setObject(Object object) {
      this.object = object;
    }

    public Integer getType() {
      return type;
    }

    public void setType(Integer type) {
      this.type = type;
    }
    
    
    
}
