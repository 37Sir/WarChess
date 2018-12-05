package com.zyd.demo.user.service;

import java.util.Date;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.ClientProtocol.LoginResponse;
import com.zyd.common.proto.client.ClientProtocol.PlayerInfo;
import com.zyd.common.unti.MD5Util;
import com.zyd.demo.common.BaseService;
import com.zyd.demo.common.enumuration.TableName;
import com.zyd.demo.common.exception.BaseException;
import com.zyd.demo.common.memcached.CacheKeyUtil;
import com.zyd.demo.common.utils.ConfigurationUtil;
import com.zyd.demo.common.utils.DemoConstants;
import com.zyd.demo.user.pojo.User;

public class UserService extends BaseService {

    @SuppressWarnings("unused")
    public LoginResponse.Builder login(String userName,String token) throws BaseException {
        LoginResponse.Builder req = LoginResponse.newBuilder();
        //验证用户名的合法性
        if (userName == null || "".equals(userName) || userName.length() > DemoConstants.USER_NICK_NAME_MAX_LENGTH) {
            throw new BaseException(ErrorCode.SERVER_ERROR_VALUE);
        }
        
        Integer  userId = getUserByName(userName);
        User user = null;
        if (userId == null) {
            user = buildNewUser(null,userName,1000,0,0,0,0,new Date(),new Date(), new Date());
        } else {
            user = getUserById(userId);
        }
        
        userId = user.getId();
        //锁住玩家操作
        if (ConfigurationUtil.infoLock.tryLock(String.valueOf(userId))) {
            try {
                user.setLastLoginTime(new Date());
                PlayerInfo.Builder playerInfo = PlayerInfo.newBuilder();
                playerInfo.setUserId(userId);
                playerInfo.setUserName(userName);
                req.setPlayerInfo(playerInfo);
                String md5String = MD5Util.md5(token + userId+DemoConstants.PRIVATE_KEY); 
                req.setSign(md5String);
                updateUser(user);
            } finally {
                ConfigurationUtil.infoLock.unLock(String.valueOf(userId));
            }
        }
        
        return req;
    }

    public User getUserById(Integer userId) {
        User result = cacheJDBCHandler.getDataByCacheNoSplit(TableName.USER.getTableName(), userId, User.class);
        return result;
    }

    private User buildNewUser(Integer id, String userName,int rank, int gold, int diamond, int winCount, int loseCount, 
        Date lastLoginTime,Date updateTime, Date createTime) {
        User user = new User(id,userName,rank,gold,diamond,winCount,loseCount,lastLoginTime,updateTime,createTime);
        cacheJDBCHandler.create(TableName.USER.getTableName(), user, user);
        return user;
    }

    private Integer  getUserByName(String userName) {
        return cacheJDBCHandler.getUserIdByName(userName);
    }
    
    public void updateUser(User user) {
        String cacheKey = CacheKeyUtil.getCacheKey(TableName.USER.getTableName(), user.getId().toString(), user.getId());
        cacheJDBCHandler.setCacheEncodeWithDB(cacheKey, TableName.USER.getTableName(), user);
    }

}
