package com.zyd.demo.common.enumuration;

public enum MysqlHandleType {
    UPDATE(1,"更新"),
    INSERT(2,"插入");
    MysqlHandleType(int type,String des){
       this.type = type;
       this.des = des;
    }
    private  int type;
    private String des;
    
    public int getType() {
      return type;
    }
    public String getDes() {
      return des;
    }        
}
