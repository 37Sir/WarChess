package com.zyd.demo.common.jdbc;

import com.zyd.demo.user.dao.UserMapper;

public class MapperHelper {
    private UserMapper userMapper;
    
    
  
  
  
    public UserMapper getUserMapper() {
      return userMapper;
    }

    public void setUserMapper(UserMapper userMapper) {
      this.userMapper = userMapper;
    }


    /**
     * 从数据库中加载，查询。
     */
    public interface DBCacheMissHandler {
        <T> T loadFromDB() throws Exception;
    }
}
