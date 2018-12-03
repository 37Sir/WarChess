package com.zyd.demo.common.lock;

public interface ILock {
	/**
	 * 仅在调用时锁为空闲状态才获取该锁。
	 * 默认超时间，在配置文件中配置的
	 * 
	 * @param key
	 * @return
	 */
	boolean tryLock(String key);
	
	/**
	 * 带有指定超时时间的加锁
	 * 
	 * @param key
	 * @param expireTime 以秒为单位
	 * @return
	 */
	boolean tryLock(String key, int expireTime);
	
	/**
	 * 带有指定超时时间的加锁
	 * 指定自旋次数的加锁
	 * 
	 * @param key
	 * @param expireTime 以秒为单位
	 * @return
	 */
	boolean tryLock(String key, int expireTime, int numOfTry);

	/**
	 * 仅在拥有锁的情况下才释放锁。
	 */
	boolean unLock(String key);
	
	/**
	 * 仅在拥有锁的情况下才释放锁。
	 * 带有指定超时时间的释放锁。
	 */
	boolean unLock(String key, int expireTime);
}
