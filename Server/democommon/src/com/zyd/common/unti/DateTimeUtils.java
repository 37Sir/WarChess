package com.zyd.common.unti;

import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

/**
 * @Description: 时间日期工具类
 * @author soloman
 * @date 2016年10月9日 下午2:34:40
 */

public class DateTimeUtils {

	// --------------------根据findBug提示修改--------------------------
	private static final String DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";
	private static final String HOUR_DATE_FORMAT = "HH:mm:ss";
	private static final String DAY_DATE_FORMAT = "yyyy-MM-dd";

	private static final String DAY_HOUR_DATE_FORMAT = "yyyy-MM-dd HH";

	// --------------------根据findBug提示修改--------------------------


	public static final DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");

	public static final DateFormat hourdateFormat = new SimpleDateFormat("HH:mm:ss");

	public static final DateFormat dayDateFormat = new SimpleDateFormat("yyyy-MM-dd");


	public static final String getDateFormat(Date date){
		return new SimpleDateFormat(DATE_FORMAT).format(date);
	}


	public static final Date parseStringDate(String date) throws ParseException {
		return new SimpleDateFormat(DATE_FORMAT).parse(date);
	}
	public static final Date parseDayHourDateFormat(String date) throws ParseException {
		return new SimpleDateFormat(DAY_HOUR_DATE_FORMAT).parse(date);
	}


	public static final String getHourDateFormat(Date date){
		return new SimpleDateFormat(HOUR_DATE_FORMAT).format(date);
	}

	public static final String getDayDateFormat(Date date){
		return new SimpleDateFormat(DAY_DATE_FORMAT).format(date);
	}

	/**
	 * 判断当前时间是否在两个时间之间
	 * 
	 * @param date1
	 * @param date2
	 * @return
	 */
	public static boolean isBetween(Date date1, Date date2) {
		return System.currentTimeMillis() > date1.getTime() && System.currentTimeMillis() < date2.getTime();
	}

	/**
	 * 判断当前时间是否在两个时间之间(HH:mm:ss)
	 * 
	 * @param time1
	 * @param time2
	 * @return
	 */
	public static boolean isBetweenByTime(Date time1, Date time2) {
		Calendar now = Calendar.getInstance();
		int nowYear = now.get(Calendar.YEAR);
		int nowMonth = now.get(Calendar.MONTH);
		int nowDay = now.get(Calendar.DAY_OF_YEAR);

		now.setTime(time1);
		now.set(Calendar.YEAR, nowYear);
		now.set(Calendar.MONTH, nowMonth);
		now.set(Calendar.DAY_OF_YEAR, nowDay);
		Date newTime1 = now.getTime();

		now.setTime(time2);
		now.set(Calendar.YEAR, nowYear);
		now.set(Calendar.MONTH, nowMonth);
		now.set(Calendar.DAY_OF_YEAR, nowDay);
		Date newTime2 = now.getTime();
		return isBetween(newTime1, newTime2);
	}

	/**
	 * 判断是否在当前时间之前(HH:mm:ss)
	 * 
	 * @param time
	 * @return
	 */
	public static boolean isBeforeNowByTime(Date time) {
		Calendar temp = Calendar.getInstance();
		int nowYear = temp.get(Calendar.YEAR);
		int nowMonth = temp.get(Calendar.MONTH);
		int nowDay = temp.get(Calendar.DAY_OF_YEAR);
		temp.setTime(time);
		temp.set(Calendar.YEAR, nowYear);
		temp.set(Calendar.MONTH, nowMonth);
		temp.set(Calendar.DAY_OF_YEAR, nowDay);
		Date tempDate = temp.getTime();
		Date now = new Date();
		return tempDate.before(now);
	}

	/**
	 * 判断是否在当前时间之后(HH:mm:ss)
	 * 
	 * @param time
	 * @return
	 */
	public static boolean isAfterNowByTime(Date time) {
		Calendar temp = Calendar.getInstance();
		int nowYear = temp.get(Calendar.YEAR);
		int nowMonth = temp.get(Calendar.MONTH);
		int nowDay = temp.get(Calendar.DAY_OF_YEAR);
		temp.setTime(time);
		temp.set(Calendar.YEAR, nowYear);
		temp.set(Calendar.MONTH, nowMonth);
		temp.set(Calendar.DAY_OF_YEAR, nowDay);
		Date tempDate = temp.getTime();
		Date now = new Date();
		return tempDate.after(now);
	}

	/**
	 * 获取当前时间(秒)
	 * 
	 * @return
	 */
	public static int getCurrentTime() {
		int now = (int) (System.currentTimeMillis() / 1000);
		return now;
	}

	/**
	 * 获取指定时的时间,如果当前时间超过该时则返回第二天该时的时间(0分、0秒、0毫秒)
	 * 
	 * @param hourOfDay
	 * @return
	 */
	public static Calendar getTimeOfDayOrNext(int hourOfDay) {
		Calendar now = Calendar.getInstance();
		Calendar c = Calendar.getInstance();
		if (now.get(Calendar.HOUR_OF_DAY) >= hourOfDay) {
			c.add(Calendar.DAY_OF_YEAR, 1);
		}
		c.set(Calendar.HOUR_OF_DAY, hourOfDay);
		c.set(Calendar.MINUTE, 0);
		c.set(Calendar.SECOND, 0);
		c.set(Calendar.MILLISECOND, 0);
		return c;
	}

	/**
	 * 获取当前时间到下一分钟的秒数
	 * 
	 * @return
	 */
	public static int getSecsToNextMinute() {
		Calendar c = Calendar.getInstance();
		c.add(Calendar.MINUTE, 1);
		c.set(Calendar.SECOND, 0);
		c.set(Calendar.MILLISECOND, 0);

		long now = System.currentTimeMillis();
		return (int) ((c.getTimeInMillis() - now) / 1000);
	}

	/**
	 * 获取到下一个当前时的毫秒数
	 * 
	 * @param hourOfDay
	 * @return
	 */
	public static long getMillSecondsToNext(int hourOfDay) {
		Calendar c = getTimeOfDayOrNext(hourOfDay);
		long now = System.currentTimeMillis();
		return c.getTimeInMillis() - now;
	}

	/**
	 * 获取当前时间在当天中的小时数
	 * 
	 * @return
	 */
	public static int getCurrentHour() {
		Calendar c = Calendar.getInstance();
		return c.get(Calendar.HOUR_OF_DAY);
	}

	/**
	 * 获取下一个小时的时刻
	 * 
	 * @return
	 */
	public static int getNextHourOfDay() {
		Calendar c = Calendar.getInstance();
		c.add(Calendar.HOUR_OF_DAY, 1);
		return c.get(Calendar.HOUR_OF_DAY);
	}

	/**
	 * 两个时间相差的秒数
	 * 
	 * @param start
	 * @param end
	 * @return
	 */
	public static int secondsBetween(long start, long end) {
		return (int) ((end - start) / 1000);
	}

	/**
	 * 两个时间相差的秒数
	 * 
	 * @param start
	 * @param end
	 * @return
	 */
	public static int secondsBetween(Date start, Date end) {
		return (int) ((end.getTime() - start.getTime()) / 1000);
	}

	/**
	 * 判断两个时间是否是同一周(以周日24点为分界点)
	 * 
	 * @param time1
	 * @param time2
	 * @return
	 */
	public static boolean isSameWeek(Date time1, Date time2) {
		Calendar now = Calendar.getInstance();
		if (time1.before(time2)) {
			now.setTime(time1);
		} else {
			now.setTime(time2);
		}
		now.set(Calendar.DAY_OF_WEEK, 1);
		now.set(Calendar.HOUR_OF_DAY, 24);
		now.set(Calendar.MINUTE, 00);
		now.set(Calendar.SECOND, 00);
		now.set(Calendar.MILLISECOND, 00);
		Date sunday = now.getTime();
		now.add(Calendar.DAY_OF_YEAR, 7);
		Date about7Day = now.getTime();
		if ((time1.after(sunday) && time2.after(sunday) && time1.before(about7Day) && time2.before(about7Day)) || (time1.before(sunday) && time2.before(sunday))) {// 前面一个或判断是基于不是周日的日期后面一个或判断是基于周日的日期
			return true;
		}
		return false;
	}
	
	public static boolean isToday(Date date)
	{
		if(date == null)
		{
			return false;
		}

		//"yyyyMMdd_HHmmss"
		String date_str = GetDateString(date);
		String cur_date_str = GetCurDateString();
		if(date_str.substring(0, 8).equals(cur_date_str.substring(0, 8)))
		{
			return true;
		}
		return false;
	}
	
	public static String GetDateString(Date date)
	{	
		if(date == null)
		{
			return null;
		}
		//"yyyy-MM-dd HH:mm:ss"
		SimpleDateFormat simpleDateFormat = new SimpleDateFormat( "yyyyMMdd_HHmmss"); 
		String str = simpleDateFormat.format(date); 
		return str;
	}
	
	public static String GetCurDateString()
	{
		Date date = new Date();
		return GetDateString(date);
	}
}
