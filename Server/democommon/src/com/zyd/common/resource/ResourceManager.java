package com.zyd.common.resource;

import java.lang.reflect.InvocationTargetException;
import java.text.ParseException;
import java.util.Map;

import org.springframework.context.ApplicationContext;



/**
 * @Description:   配置总的加载类
 * @author          soloman
 * @Date           2016年9月20日 下午2:33:59 
 */

public class ResourceManager {

	@SuppressWarnings("rawtypes")
	public void init(ApplicationContext applicationContext) throws NoSuchMethodException, SecurityException, IllegalAccessException, IllegalArgumentException, InvocationTargetException, InstantiationException, ParseException {
		
		Map<String, ResourceLoader> initBeansMap = applicationContext.getBeansOfType(ResourceLoader.class);
		
		if(initBeansMap != null) {
			ResourceLoader[] initBeans = initBeansMap.values().toArray(new ResourceLoader[initBeansMap.size()]);
			for(ResourceLoader loader : initBeans) {
				loader.load();
			}
		}
		
	}
	
}
