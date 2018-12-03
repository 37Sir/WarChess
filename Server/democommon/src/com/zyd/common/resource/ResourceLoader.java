package com.zyd.common.resource;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.zyd.common.unti.DateTimeUtils;
import com.zyd.common.unti.StringUtil;


/**
 * @Description: 资源加载的抽象类
 * @author soloman
 * @Date 2016年9月20日 上午10:17:00
 */

public abstract class ResourceLoader<T> {
	protected Logger logger = LoggerFactory.getLogger(this.getClass().getName());

	public List<T> loadAll(Class<T> clazz) throws NoSuchMethodException, SecurityException, IllegalAccessException, IllegalArgumentException, InvocationTargetException, InstantiationException, ParseException {

		// 获取初始entity
		List<Map<String, String>> entityList = transFileToMapList(clazz.getSimpleName());
		List<T> needList = new ArrayList<T>();

		// 开始组装需要的pojo
		for (Map<String, String> entity : entityList) {

			T template = clazz.newInstance();

			Field[] fields = clazz.getDeclaredFields();

			for (Field f : fields) {

				f.setAccessible(true);

				// 根据参数名去配置里找啊
				String paramName = f.getName();
				String keyName = StringUtil.camelToUnderline(paramName);
				String value = entity.get(keyName);

				if (value != null && !value.equals("")) {
					if ("int".equals(f.getType().getSimpleName()) || "Integer".equals(f.getType().getSimpleName())) {
						f.set(template, new Integer(value));
					} else if ("long".equals(f.getType().getSimpleName()) || "Long".equals(f.getType().getSimpleName())) {
						f.set(template, new Long(value));
					} else if ("byte".equals(f.getType().getSimpleName()) || "Byte".equals(f.getType().getSimpleName())) {
						f.set(template, new Byte(value));
					} else if ("float".equals(f.getType().getSimpleName()) || "Float".equals(f.getType().getSimpleName())) {
						f.set(template, new Float(value));
					} else if ("double".equals(f.getType().getSimpleName()) || "Double".equals(f.getType().getSimpleName())) {
						f.set(template, new Double(value));
					} else if ("boolean".equals(f.getType().getSimpleName()) || "Boolean".equals(f.getType().getSimpleName())) {
						if (value.equals("0")) {
							f.set(template, new Boolean(false));
						} else if (value.equals("1")) {
							f.set(template, new Boolean(true));
						}
					} else if ("Date".equals(f.getType().getSimpleName())) {
						if (value.indexOf("-") != -1) {
							Date date = DateTimeUtils.dateFormat.parse(value);
							f.set(template, date);
						} else {
							Date date = DateTimeUtils.hourdateFormat.parse(value);
							f.set(template, date);
						}

					} else {
						f.set(template, value);
					}
					f.setAccessible(false);
				} else {
					if("String".equalsIgnoreCase(f.getType().getSimpleName())){
						f.set(template, "");
					}else{
						f.set(template, null);
					}
					
				}

			}

			needList.add(template);
		}

		return needList;

	}

	public List<Map<String, String>> transFileToMapList(String resName) {

		FileInputStream fis = null;

		BufferedReader br = null;

		InputStreamReader isr = null;

		List<Map<String, String>> entityList = new ArrayList<>();

		try {

			String ss = this.getClass().getResource("/").getPath();

			String path = ss.substring(0, ss.length() - 9);

			fis = new FileInputStream(path + "/resources/" + resName + ".bytes");
			isr = new InputStreamReader(fis, "gb2312");
			
			br = new BufferedReader(isr);

			String title = br.readLine();

			// 获取列名
			String[] titles = title.split("@");

			title = br.readLine();
			title = br.readLine();

			String line = br.readLine();
if("DataGrowthPlan".equals(resName)){
	System.out.println();
}
			while (line != null) {

				String[] params = line.split("@");

				Map<String, String> paramMap = new HashMap<>();

				for (int i = 0; i < params.length; i++) {
					paramMap.put(titles[i], params[i]);
				}

				entityList.add(paramMap);

				line = br.readLine();
			}

		} catch (Exception e) {

			e.printStackTrace();

		} finally {
			try {
				fis.close();
				br.close();
				isr.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}

		return entityList;
	}

	public abstract void load() throws NoSuchMethodException, SecurityException, IllegalAccessException, IllegalArgumentException, InvocationTargetException, InstantiationException, ParseException;

	public void check(List<?> list, Class<?> clazz) {
		if (list == null) {
			StackTraceElement ste = Thread.currentThread().getStackTrace()[2];
			logger.error("checkListNull :{} : is null", ste.toString());
			throw new NullPointerException();
		} else if (list.size() == 0) {
			// 这里检测到了严重的错误,这里打印标记性日志
			logger.error("dangerous! serious error ,no data in table {} \n\n\n" + " #######    #########   #########    ########    #########    " + "\n" + " #          #      ##   #      ##    #      #    #      ##    " + "\n" + " #          #########   #########    #      #    #########    " + "\n" + " ######     ##          ##           #      #    ##           " + "\n" + " #          #  ##       #  ##        #      #    #  ##        " + "\n" + " #          #    ##     #    ##      #      #    #    ##      " + "\n" + " #######    #      ##   #      ##    ########    #      ##    " + "\n\n\n", clazz.getName());

		} else {
			// 这里是校验成功的数据表,大致打印了条数,到时候可以初步查询.
			logger.info("success check data in table {} ,data's size = {}", clazz.getName(), list.size());
		}
	}

}
