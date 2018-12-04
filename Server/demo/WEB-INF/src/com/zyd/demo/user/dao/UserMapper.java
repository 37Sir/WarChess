package com.zyd.demo.user.dao;

import com.zyd.demo.user.pojo.User;

public interface UserMapper {
    int insert(User record);

    int insertSelective(User record);

    User selectByPrimaryKey(Integer id);

    Integer selectByName(String userName);
    
    int updateByPrimaryKeySelective(User record);

    int updateByPrimaryKey(User record);
}