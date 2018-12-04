package com.zyd.demo.common.enumuration;



@SuppressWarnings("rawtypes")
public enum TableName {
	USER(true, TableName.class,"user");

	private Boolean isFenBiao;
	private Class clazz;
	private String name; 
	
	private TableName(boolean isFenBiao, Class clazz, String name){
		this.isFenBiao = isFenBiao;
		this.clazz = clazz;
		this.name = name;
	}
	
	public boolean getIsFenBiao(){
		return isFenBiao;
	}
	
	public Class getClazz() {
		return clazz;
	}

	public static TableName get(String tableName){
		for (TableName tn : TableName.values()) {
			if(tn.name().equals(tableName) && tn.getIsFenBiao()){
				return tn;
			}
		}
		return null;
	}
	
    public String getTableName() {
      return name;
    }
}
