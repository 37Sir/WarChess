package com.zyd.gw;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.Properties;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class AppConfig {
    private static Logger logger = LoggerFactory.getLogger(AppConfig.class.getName());
    public final static Properties properties = new Properties();
    public static String rootPath = System.getProperty("user.dir");

    static {
        try {
            properties.load(new FileInputStream(rootPath + "/conf/application.properties"));
        } catch (IOException e) {
            logger.error("load config file error",e);
        }
    }
    public final static String CLIENT_BIND_ADDRESS = properties.getProperty("clientBindAddress");
    public final static int CLIENT_BIND_PORT = Integer.parseInt(properties.getProperty("clientBindPort"));
    public final static String INFO_BIND_ADDRESS = properties.getProperty("infoBindAddress");
    public final static int INFO_BIND_PORT = Integer.parseInt(properties.getProperty("infoBindPort"));

    
    public final static int HEART_BEAT_SECONDS = Integer.parseInt(properties.getProperty("heartBeatTimeOut"));
    public final static int HEART_BEAT_SEND = Integer.parseInt(properties.getProperty("heartBeatSend"));
    public final static boolean HEART_BEAT_SWITCH = Integer.parseInt(properties.getProperty("heartBeatSwitch"))==1?true:false;
}
