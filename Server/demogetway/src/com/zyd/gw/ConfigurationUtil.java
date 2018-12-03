package com.zyd.gw;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.Properties;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class ConfigurationUtil {
    private static Logger logger = LoggerFactory.getLogger(ConfigurationUtil.class.getName());
    public final static Properties properties = new Properties();
    public static String rootPath = System.getProperty("user.dir");
    
    static {
        try {
            properties.load(new FileInputStream(rootPath + "/conf/config.properties"));
        } catch (IOException e) {
            logger.error("load config file error",e);
        }
    }
}
