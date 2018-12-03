package com.zyd.demo.round.service;

import java.util.Map.Entry;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.demo.common.BaseService;
import com.zyd.demo.round.pojo.UserMatchInfo;

// 玩家匹配队列
public class MatchService extends BaseService {

	private static final Logger logger = LoggerFactory.getLogger(MatchService.class);
	// 匹配队列
	private ConcurrentHashMap<String, UserMatchInfo> match = new ConcurrentHashMap<>();
	// 待加入的匹配队列
	private ConcurrentHashMap<String, UserMatchInfo> addPlayer = new ConcurrentHashMap<>();
	// 离线,取消匹配的玩家信息
	private ConcurrentHashMap<String, Object> cancelPlayer = new ConcurrentHashMap<>();
	// 定时器,启动线程进行玩家匹配
	private ScheduledExecutorService matchTask = Executors.newSingleThreadScheduledExecutor();
	// 匹配间隔时长
	private static final int SCHEDULED_EXECULATE_INTERVAL_MILLISECOND = 1000;
	// 原子标记量,判断匹配线程是否完成
	private AtomicBoolean isMatching = new AtomicBoolean(false);

	public void init() {
		System.out.println("MatchService 开始执行匹配");
		startMatchTask();
	}

	public void startMatchTask() {
		matchTask.scheduleWithFixedDelay(() -> {
			try {
				if (isMatching.get()) {
					logger.error("there is a matching thead not finished!");
					return;
				} else {
					isMatching.set(true);
				}
				// 得到最新的匹配队列
				// 将待添加的人员添加到map中
				for (Entry<String, UserMatchInfo> entry : addPlayer.entrySet()) {
					String key = entry.getKey();
					UserMatchInfo userMatchInfo = entry.getValue();
					addPlayer.remove(key);
					match.put(key, userMatchInfo);
				}
				// 将待移除的人员添加到map中
				for (Entry<String, Object> entry : cancelPlayer.entrySet()) {
					String key = entry.getKey();
					match.remove(key);
					cancelPlayer.remove(key);
				}
				// 将剩余的人员进行匹配,进行1对1的匹配
				if (match.size() < 2) {
					// logger.info("not found enough person to start!");
					isMatching.set(false);
					return;
				}

				for (Entry<String, UserMatchInfo> entry : match.entrySet()) {
					String key = entry.getKey();
					UserMatchInfo value = entry.getValue();
					for (Entry<String, UserMatchInfo> matchEntry : match.entrySet()) {
						String matchKey = matchEntry.getKey();
						UserMatchInfo matchValue = matchEntry.getValue();
						// 如果是相同的人
						if (key.equals(matchKey)) {
							continue;
						}
						// 将两个人的数据在匹配队列中删除
						// 如果两个人的ip相同 测试使用
						if (value.getClientIp().equals(matchValue.getClientIp())) {
							match.remove(key);
							match.remove(matchKey);
							// TODO 判断匹配规则
							// 进入匹配
							doMatch(value, matchValue);
						}
					}
				}
				isMatching.set(false);
			} catch (Exception e) {
				logger.error("", e);
			}

		}, 0, SCHEDULED_EXECULATE_INTERVAL_MILLISECOND, TimeUnit.MILLISECONDS);
	}

	public void disConnect(String token, Long userId) {
		String userKey = getUserKey(token);
		cancelPlayer.put(userKey, new Object());
	}

	private void doMatch(UserMatchInfo user, UserMatchInfo toUser) {
		// 检验玩家是否连接
		List<UserMatchInfo> userMatchInfos = new ArrayList<>();
		userMatchInfos.add(user);
		userMatchInfos.add(toUser);

		BattleRoom battleRoom = new BattleRoom(battleRoomManager,commonService, user, toUser);
		battleRoomManager.addNewBattleRoom(battleRoom);
	}

	// 添加玩家到匹配队列
	public void addWaitUser(UserMatchInfo userMatchInfo) {
		// TODO 测试
		battleRoomManager.removeBattleRoom(userMatchInfo.getToken());

		String userKey = getUserKey(userMatchInfo.getToken());
		// 移除即将删除的玩家队列
		cancelPlayer.remove(userKey);
		// 待加入信息
		addPlayer.put(userKey, userMatchInfo);
	}

	// 移除玩家匹配队列
	public void removeWaitUser(String token, Long userId) {
		String userKey = token;
		// 待加入信息
		cancelPlayer.put(userKey, userId);
		// 移除即将删除的玩家队列
		addPlayer.remove(userKey);
	}

	/** 得到用户的key zoneId_userId */
	public String getUserKey(String token) {
		return new StringBuilder().append(token).toString();
	}
}
