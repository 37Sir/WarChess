package com.zyd.demo.common.lock;

import net.rubyeye.xmemcached.CASOperation;
import net.rubyeye.xmemcached.GetsResponse;
import net.rubyeye.xmemcached.MemcachedClient;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class MemCasLockImpl implements ILock{
	private Logger logger = LoggerFactory.getLogger(MemCasLockImpl.class.getName()); 
	protected MemcachedClient mcc;
	
	private final String LOCK_PREIFX = "PK.LOCK_";
	private final String LOCK_INTERVAL = "|";
	
	//config
	private int tryNum;//3
	private int tryIntervalTime;//100ms
	private int expr;//60seconds
	
	public MemcachedClient getMcc() {
		return mcc;
	}
	
	public void setMcc(MemcachedClient mcc) {
		this.mcc = mcc;
	}
	
	public int getTryNum() {
		return tryNum;
	}
	
	public void setTryNum(int tryNum) {
		this.tryNum = tryNum;
	}

	public int getTryIntervalTime() {
		return tryIntervalTime;
	}
	
	public void setTryIntervalTime(int tryIntervalTime) {
		this.tryIntervalTime = tryIntervalTime;
	}
	
	public int getExpr() {
		return expr;
	}
	
	public void setExpr(int expr) {
		this.expr = expr;
	}

	/**
	 * 锁value部分。
	 */
	public String getLockContent(String key){
		return Thread.currentThread().getThreadGroup().getName() + LOCK_INTERVAL + String.valueOf(Thread.currentThread().getId()) + LOCK_INTERVAL + key;
	}
	
	/**
	 * 内部又进行了一道key的封装，确保key的唯一
	 */
	private String getKey(String userId){
		return LOCK_PREIFX + userId;
	}
	
	/* 
	 * 
	 * 
	 * lock realKey 
	 */
	@Override
	public boolean tryLock(String originalKey) {
		logger.debug("lockTrace tryLock userId:{}",originalKey);
		return tryLock(originalKey, expr);
	}
	
	@Override
	public boolean tryLock(String originalKey, int expireTime) {
		return tryLock(originalKey, expireTime, tryNum);
	}
	
	@Override
	public boolean tryLock(String originalKey, int expireTime, int numOfTry) {
		final String lockKey = getKey(originalKey);
		final String lockValue = getLockContent(lockKey);
		return tryLock(originalKey, expireTime, numOfTry, lockValue);
	}
	
	private boolean tryLock(String originalKey, final int expireTime, final int numOfTry, final String lockValue) {
		final String lockKey = getKey(originalKey);
		try {
			GetsResponse<String> gRes = mcc.gets(lockKey);
			if(gRes == null){
				mcc.set(lockKey, expireTime, "");
				gRes = mcc.gets(lockKey);
			}
			boolean res =  mcc.cas(lockKey, expireTime, new CASOperation<String>() {
					// 最大尝试cas操作numOfTry次。
					public int getMaxTries() {
						return numOfTry;
					}
					
					public String getNewValue(long currentCAS, String currentValue) {
						if(currentValue.equals("")){
							return lockValue;
						}
						return null;
					}
				});
			if(!res){
				Thread.sleep(tryIntervalTime);
				res =  mcc.cas(lockKey, expireTime, new CASOperation<String>() {
					// 最大尝试cas操作numOfTry次。
					public int getMaxTries() {
						return numOfTry;
					}
					
					public String getNewValue(long currentCAS, String currentValue) {
						if(currentValue.equals("")){
							return lockValue;
						}
						return null;
					}
				});
			}
			return res;
		} catch (Throwable e) {
			logger.warn("not get lock ",e);
			return false;
		}
	}

	@Override
	public boolean unLock(String userId) {
		logger.debug("lockTrace unLock userId:{}",userId);
		return unLock(userId, 0);
	}
	
	@Override
	public boolean unLock(String userId, int expireTime) {
		final String lockKey = getKey(userId);
		final String lockValue = getLockContent(lockKey);
		return unLock(userId, expireTime, lockValue);
	}
	
	private boolean unLock(String userId, int expireTime, final String lockValue) {
		final String lockKey = getKey(userId);
		GetsResponse<String> gRes = null;
		try {
			gRes = mcc.gets(lockKey);
			//第一次的时候。
			if(gRes == null || gRes.getValue() == null || gRes.getValue().equals("")){
				return true;
			}
			//解锁（前提是拿到了锁了）的时候没有竞争。
			//不需要cas。
			return mcc.cas(lockKey, expireTime, new CASOperation<String>() {
				// 最大尝试cas操作numOfTry次。
				public int getMaxTries() {
					return 1;
				}
				
				public String getNewValue(long currentCAS, String currentValue) {
					if(currentValue.equals(lockValue)){
						return "";
					}
					return null;
				}
			});
		} catch (Throwable e) {
			return false;
		}
	}

}
