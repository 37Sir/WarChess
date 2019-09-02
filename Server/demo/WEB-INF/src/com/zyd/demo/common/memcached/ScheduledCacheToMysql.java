package com.zyd.demo.common.memcached;

import java.util.concurrent.LinkedBlockingDeque;
import org.springframework.beans.factory.annotation.Autowired;
import com.zyd.demo.common.enumuration.MysqlHandleType;
import com.zyd.demo.common.enumuration.TableName;
import com.zyd.demo.common.jdbc.MapperHelper;
import com.zyd.demo.user.pojo.User;


public class ScheduledCacheToMysql {
    public  final LinkedBlockingDeque<SynchronizationMysql> queue = new LinkedBlockingDeque<SynchronizationMysql>();
    @Autowired
    protected MapperHelper mapperHelper;
        
    public void add (SynchronizationMysql synchronizationMysql) {
        queue.add(synchronizationMysql);
    }
    
    public void updateMysql(SynchronizationMysql synchronizationMysql) {
        if (synchronizationMysql.getTableName().equals(TableName.USER.getTableName())) {
            User obj = (User) synchronizationMysql.getObject();
            mapperHelper.getUserMapper().updateByPrimaryKey(obj);
        }
    }
    
    /**
             * 插入不太好做，因为自增的ID没有返回 ，先不做
     * @param synchronizationMysql
     */
    public void insertMysql (SynchronizationMysql synchronizationMysql) {
        
    }
    
    public  void doMusqlQueue() throws InterruptedException {
        SynchronizationMysql s = queue.take();
        if (s != null) {
            if (s.getType() == MysqlHandleType.UPDATE.getType()) {
                updateMysql(s);
            } else if (s.getType() == MysqlHandleType.INSERT.getType()) {
                insertMysql(s);
            }
        }
    }
}
