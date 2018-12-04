package com.zyd.demo.common;

import javax.servlet.ServletConfig;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.BeanFactory;
import org.springframework.context.ApplicationContext;
import org.springframework.web.context.support.WebApplicationContextUtils;
import com.zyd.common.resource.ResourceManager;
import com.zyd.demo.common.lock.ILock;
import com.zyd.demo.common.utils.ConfigurationUtil;
import com.zyd.demo.info.ProxyConnection;
import io.netty.channel.nio.NioEventLoopGroup;

public class InitServlet extends HttpServlet {
    private static final Logger logger = LoggerFactory.getLogger(InitServlet.class);
    NioEventLoopGroup group = new NioEventLoopGroup();
    ProxyConnection proxy;

    @Override
    public void init(ServletConfig config) throws ServletException {
        super.init(config);
        
        try {
            BeanFactory context = WebApplicationContextUtils.getWebApplicationContext(config.getServletContext());
            ConfigurationUtil.beanFactory = context;
            ResourceManager resourceManager = (ResourceManager) context.getBean("resourceManager");
            
            //load the system config
            resourceManager.init((ApplicationContext)context);
            
            ConfigurationUtil.infoLock = (ILock)context.getBean("ilock");
            proxy = new ProxyConnection(ConfigurationUtil.PROXY_ADDR, ConfigurationUtil.PROXY_PORT, group);
            proxy.start();
            logger.info("\n\n\n" +
                    "      _________ __                  __              ._.                         " + "\n" + 
                    "     /   _____/_| |_______ ________/  |_ __ ________| |                         " + "\n" + 
                    "     |_____  ||_  __||__  ||_  __ |   __|  |   | __/| |                         " + "\n" + 
                    "    /        | |  |  / __ ||  | |/ |  | |  |  /| |_> >|                         " + "\n" + 
                    "   /_______  / |__| (____ /|__|    |__| |____/ |  __/_|                         " + "\n" + 
                    "           |/           |/                     |__| |/        "
                    + "\n\n\n");
        }catch(Exception e){
            logger.error("Error in InitServlet: " , e);
        }
    }
    
    
    @Override
    public void destroy() {
        try {
            //关闭之前刷新缓存。
//            proxy.stop();
                        
//            proxy.close();
            logger.info("server is going to shut down,commit the cached data.");
            group.shutdownGracefully();
            //----------
            super.destroy();
            System.exit(0);
        } catch (Exception e) {
            logger.error("*****************KP Info destroy fail!*****************");
        }
    }
    
}
