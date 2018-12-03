package com.zyd.demo.common.jdbc;


public class MapperHelper {
  /**
   * 从数据库中加载，查询。
   */
  public interface DBCacheMissHandler {
      <T> T loadFromDB() throws Exception;
  }
}
