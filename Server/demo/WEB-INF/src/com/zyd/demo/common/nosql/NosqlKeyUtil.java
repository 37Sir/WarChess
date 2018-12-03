package com.zyd.demo.common.nosql;

import com.zyd.demo.common.utils.KpJoiner;

public class NosqlKeyUtil {

    private static final String DELIMITER = "_";
    private static final String DEFAULT_SUFFIX = ".suffix";
    private static final KpJoiner joiner = KpJoiner.on(DELIMITER);
    //活动系列
    private static final String PLAYER_ACTIVITY_LIST_NOSQL = "A_LN";

   


    public static String getListNosqlPlayerActivity(long userId) {
        return joiner.join(PLAYER_ACTIVITY_LIST_NOSQL, userId);
    }

    

}