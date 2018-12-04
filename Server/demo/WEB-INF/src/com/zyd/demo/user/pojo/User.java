package com.zyd.demo.user.pojo;

import java.util.Date;

public class User {
    private Integer id;

    private String userName;

    private Integer rank;

    private Integer gold;

    private Integer diamond;

    private Integer winCount;

    private Integer loseCount;

    private Date lastLoginTime;
    
    private Date updateTime;

    private Date createTime;

    public User(Integer id, String userName, Integer rank, Integer gold, Integer diamond, Integer winCount, 
        Integer loseCount,Date lastLoginTime, Date updateTime, Date createTime) {
        this.id = id;
        this.userName = userName;
        this.rank = rank;
        this.gold = gold;
        this.diamond = diamond;
        this.winCount = winCount;
        this.loseCount = loseCount;
        this.lastLoginTime = lastLoginTime;
        this.updateTime = updateTime;
        this.createTime = createTime;
    }

    public User() {
        super();
    }

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getUserName() {
        return userName;
    }

    public void setUserName(String userName) {
        this.userName = userName;
    }

    public Integer getRank() {
        return rank;
    }

    public void setRank(Integer rank) {
        this.rank = rank;
    }

    public Integer getGold() {
        return gold;
    }

    public void setGold(Integer gold) {
        this.gold = gold;
    }

    public Integer getDiamond() {
        return diamond;
    }

    public void setDiamond(Integer diamond) {
        this.diamond = diamond;
    }

    public Integer getWinCount() {
        return winCount;
    }

    public void setWinCount(Integer winCount) {
        this.winCount = winCount;
    }

    public Integer getLoseCount() {
        return loseCount;
    }

    public void setLoseCount(Integer loseCount) {
        this.loseCount = loseCount;
    }

    public Date getLastLoginTime() {
      return lastLoginTime;
    }

    public void setLastLoginTime(Date lastLoginTime) {
      this.lastLoginTime = lastLoginTime;
    }

    public Date getUpdateTime() {
        return updateTime;
    }

    public void setUpdateTime(Date updateTime) {
        this.updateTime = updateTime;
    }

    public Date getCreateTime() {
        return createTime;
    }

    public void setCreateTime(Date createTime) {
        this.createTime = createTime;
    }
}