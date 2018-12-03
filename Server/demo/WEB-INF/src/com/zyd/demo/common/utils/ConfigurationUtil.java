package com.zyd.demo.common.utils;

import java.util.Iterator;
import org.apache.commons.configuration.Configuration;
import org.apache.commons.configuration.ConfigurationException;
import org.apache.commons.configuration.ConfigurationFactory;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.BeanFactory;
import com.zyd.demo.common.lock.ILock;

public class ConfigurationUtil {
    public static BeanFactory beanFactory; 
    private static Configuration config;
    public static ILock infoLock;
    private static final ConfigurationFactory factory = new ConfigurationFactory("propertyConfig.xml");
    private static final Logger logger = LoggerFactory.getLogger(ConfigurationUtil.class.getName());


    
    static {
      try {
          config = factory.getConfiguration();
      } catch (ConfigurationException e) {
          logger.error("", e);
      }
      logger.info("finished load configuration:");
      String temp = "";

      @SuppressWarnings("unchecked")
      Iterator<String> iterator = config.getKeys();
      while (iterator.hasNext()) {
          temp = iterator.next();
          logger.info(temp + " -->" + config.getString(temp));
      }

    }
                
    public static final int REDIS_TIME_OUT = config.getInt("redis_read_time_out", 20000);
    public static final int LOGIN_TASK=config.getInt("app.login_task");
    public static final int ROOM_GAME_TASK=config.getInt("app.room_game_task");
    public static final int THIRD_TASK=config.getInt("app.third_task");
    public static final int MAIN_TASK=config.getInt("app.main_task");
    public static final int TASK_COUNT = LOGIN_TASK+ROOM_GAME_TASK+THIRD_TASK+MAIN_TASK;
    public static final int CPU_OFFSET = config.getInt("app.cpu_offset");
    public static final String PROXY_ADDR=config.getString("app.proxy.addr");
    public static final int PROXY_PORT=StringUtil.toInt(config.getString("app.proxy.port"));
    
}
