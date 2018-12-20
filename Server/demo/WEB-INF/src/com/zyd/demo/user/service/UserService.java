package com.zyd.demo.user.service;

import java.text.DecimalFormat;
import java.util.Date;
import java.util.List;
import com.zyd.common.proto.client.ClientProtocol.ErrorCode;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerInfoResponse;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerRankResopnse;
import com.zyd.common.proto.client.ClientProtocol.GetPlayerRankResopnse.Builder;
import com.zyd.common.proto.client.ClientProtocol.LoginResponse;
import com.zyd.common.proto.client.ClientProtocol.PlayerInfo;
import com.zyd.common.proto.client.ClientProtocol.PlayerRankListResponse;
import com.zyd.common.proto.client.ClientProtocol.RankInfo;
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
    public LoginResponse.Builder login(String userName,String token) throws Exception {
        LoginResponse.Builder req = LoginResponse.newBuilder();
        //验证用户名的合法性
        if (userName == null || "".equals(userName) || userName.length() > DemoConstants.USER_NICK_NAME_MAX_LENGTH) {
            throw new BaseException(ErrorCode.SERVER_ERROR_VALUE);
        }
        
        Integer  userId = getUserByName(userName);
        User user = null;
        if (userId == null) {
            user = buildNewUser(null,userName,1000,0,0,0,0,0,0,0,new Date(),new Date(), new Date());
        } else {
            user = getUserById(userId);
        }
        
        userId = user.getId();
        //锁住玩家操作
        if (ConfigurationUtil.infoLock.tryLock(String.valueOf(userId))) {
            try {
                user.setLastLoginTime(new Date());
                PlayerInfo.Builder playerInfo = buildeplayerInfo(user);
                 
                playerInfo.setFirstWin(getFitstWin());
                int r = getUserRank(user);
                playerInfo.setUserRank(r);
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
    /**计算先手胜率
     * @throws Exception */
    public double getFitstWin() throws Exception {
        if (!nosqlService.getNoSql().exists("FIRST_WIN_COUNT")
            || !nosqlService.getNoSql().exists("FIRST_LOSE_COUNT")) {
            commonService.insertFirstLoseCount();
            commonService.insertFirstWinCount();
        }
        Long win = commonService.getFirstWinCount();
        Long lose = commonService.getFirstLoseCount();
        DecimalFormat df = new DecimalFormat("#.00");
        double d = (double)win / (win +  lose);
        String s = df.format(d*100);
        return Double.parseDouble(s);
    }
    
    /**构建玩家返回信息*/
    public PlayerInfo.Builder buildeplayerInfo(User user){
        PlayerInfo.Builder playerInfo = PlayerInfo.newBuilder();
        playerInfo.setUserId(user.getId());
        playerInfo.setUserName(user.getUserName());
        playerInfo.setRank(user.getRank());
        playerInfo.setLosing(user.getLosingCount());
        playerInfo.setWinning(user.getWinningCount());
        playerInfo.setWinCount(user.getWinCount());
        playerInfo.setLoseCount(user.getLoseCount());
        playerInfo.setDraw(user.getDrawCount());
        playerInfo.setAllCount(user.getWinCount() + user.getLoseCount() + user.getDrawCount() );
        return playerInfo;
    }
    public User getUserById(Integer userId) {
        User result = cacheJDBCHandler.getDataByCacheNoSplit(TableName.USER.getTableName(), userId, User.class);
        return result;
    }

    private User buildNewUser(Integer id, String userName,int rank, int gold, int diamond, int winCount, int loseCount, 
        int drawCount , int winningCount, int losingCount ,Date lastLoginTime,Date updateTime, Date createTime) {
        User user = new User(id,userName,rank,gold,diamond,winCount,loseCount,drawCount,winningCount,losingCount,lastLoginTime,updateTime,createTime);
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
    
    public List<User> getRankList(){
        return cacheJDBCHandler.getRankList();
    }
    //玩家排名
    public int getUserRank(User user) {
      return cacheJDBCHandler.getUserRankList(user);
    }
    
    public PlayerRankListResponse.Builder buildPlayerRankListResponse(User user) {
        PlayerRankListResponse.Builder res = PlayerRankListResponse.newBuilder();
        List<User> rankList = getRankList();
        for (int i = 0; i< rankList.size() ; i++) {
            User u = rankList.get(i);
            RankInfo.Builder rankInfo = RankInfo.newBuilder();
            rankInfo.setName(u.getUserName());
            rankInfo.setRank(u.getRank());
            rankInfo.setRanking(i+1);
            res.addRankInfo(rankInfo);
        }
        res.setRank(user.getRank());
        res.setUserRank(getUserRank(user));
        return res;
    }
    public Builder buildGetPlayerRankResopnse(User user) {
        GetPlayerRankResopnse.Builder res = GetPlayerRankResopnse.newBuilder();
        res.setRank(getUserRank(user));
        return res;
    }
    public GetPlayerInfoResponse.Builder buildGetPlayerInfoResponse(
        User user) throws Exception {
        GetPlayerInfoResponse.Builder  res  = GetPlayerInfoResponse.newBuilder();
        PlayerInfo.Builder playerInfo = buildeplayerInfo(user);      
        playerInfo.setFirstWin(getFitstWin());
        int r = getUserRank(user);
        playerInfo.setUserRank(r);
        res.setPlayerInfo(playerInfo);
        return res;
    }
}
