package com.zyd.demo.common.utils;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import org.springframework.beans.factory.annotation.Autowired;
import com.zyd.demo.common.memcached.ScheduledCacheToMysql;

public class ServiceLocator {
    public static final ExecutorService cacheToMysql = Executors.newSingleThreadExecutor();
    
    public static final ScheduledCacheToMysql sc = new ScheduledCacheToMysql();
    
    static { 
        Executors.newScheduledThreadPool(1).scheduleAtFixedRate(new Runnable() {          
            @Override
            public void run() {
                try {
                    sc.doMusqlQueue();
                } catch (Exception e) {
                    
                }
            }
        }, 0, 1000 * 60 *5L ,TimeUnit.MILLISECONDS);
    }
}
