package com.zyd.demo.common.utils;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;


public class StringUtil {
	private static final Logger logger = LoggerFactory.getLogger(StringUtil.class.getName());
	public static final String encoding(String input) {
		try {
			byte[] bytes = input.getBytes("ISO-8859-1");
			return new String(bytes, "utf-8");
		} catch (Exception ex) {
			logger.error("error happened in StringUtil encoding",ex);
		}

		return null; 
	}
	public static final String printfString(String[] paramValues) {
		StringBuilder sb=new StringBuilder();
		if(paramValues!=null){
			for(String str:paramValues){
				sb.append(str).append(",");
			}
		}
		return sb.toString(); 
	}
	
	public static boolean checkEmptyString(String input){
		return input == null || "".equals(input.trim());
	}
	public static boolean checkNull(Object input){
		return input == null;
	}
	
	public static final int toInt(String input) {
		int output = 0;
		try{
			if (input != null) {
				output = Integer.parseInt(input.trim());
			}
		} catch (NumberFormatException e) {
			throw e;
		}
		return output;
	}
	
	public static final float toFloat(String input) {
		float output = 0;
		try{
			if (input != null) {
				output = Float.parseFloat(input.trim());
			}
		} catch (NumberFormatException e) {
			throw e;
		}
		return output;
	}
	
	public static final double toDouble(String input) {
		double output = 0;
		try{
			if (input != null) {
				output = Double.parseDouble(input.trim());
			}
		} catch (NumberFormatException e) {
			throw e;
		}
		return output;
	}
	
	public static final long toLong(String input) {
		long output = 0;
		try{
			if (input != null) {
				output = Long.parseLong(input.trim());
			}
		} catch (NumberFormatException e) {
			throw e;
		}
		return output;
	}
	
	public static final int getIntParam(String input,int defaultValue) {
		int output = 0;
		if("".equals(input) || input==null) 
			return defaultValue;
		try{
			if (input != null) {
				output = Integer.parseInt(input.trim());
			}
		} catch (NumberFormatException e) {
			return defaultValue;
		}
		return output;
	}
	public static final String getStringParam(String input,String defaultValue) {
		if("".equals(input)) 
			return defaultValue;
		
		return input;
	}
	public static final String[] getStringArrayParam(String input,String[] defaultValue) {
		if("".equals(input)) 
			return defaultValue;
		
		return input.split("\\|");
	}
	public static final boolean filter(String input) {
		String	regEx = "[\'\" ~!@#$%^&*()+`{}|\\\\,\\./<>?;:]";
		return filter(input, regEx);
	}
	
	public static final boolean filter(String input, String regEx) {
		Pattern pat = Pattern.compile(regEx);
		Matcher mat = pat.matcher(input);
		boolean rs = mat.find();
		return rs;
	}
	public static final String escapeIndex(String input){
		String	regEx = "[\'\"*+`{}|\\\\/<>]";
		Pattern pat = Pattern.compile(regEx);
		Matcher mat = pat.matcher(input);
		boolean rs = mat.find();
		
		if(rs){
			return mat.replaceAll("").trim();
		}else{
			return input;
		}
		
	}
	public static final String escape(String input){
		String	regEx = "[\'\"~!@#$%^&*()+`{}|\\\\,./<>?;:]";
		Pattern pat = Pattern.compile(regEx);
		Matcher mat = pat.matcher(input);
		boolean rs = mat.find();
		if(rs){
			return mat.replaceAll("").trim();
		}else{
			return input;
		}
	}
	public static final String escapeEnter(String input){
		String	regEx = "[\n]";
		Pattern pat = Pattern.compile(regEx);
		Matcher mat = pat.matcher(input);
		boolean rs = mat.find();
		if(rs){
			return mat.replaceAll("").trim();
		}else{
			return input;
		}
	}
	public static final String escape(String input,String replaceString) throws Exception{
		String	regEx = "[\\^\\$\\*\\+\\|\\.\\?]";
		Pattern pat = Pattern.compile(regEx);
		Matcher mat = pat.matcher(input);
		
		boolean rs = mat.find();
		if(rs){
			try {
				input=mat.replaceAll("\\"+mat.group()).trim();
				return input;
			} catch (Exception e) {
				throw e;
			}
		}else{
			return input;
		}
	}
	public static final String stringToAscii(String input) throws UnsupportedEncodingException{
		byte[] str=input.getBytes("UTF-8");
		StringBuffer sb=new StringBuffer();
		for(byte b:str){
			int c =b&0xff;
			sb.append("\\").append(c);
		}
		return sb.toString();
	}
	public static final String asciiToString(String input) throws UnsupportedEncodingException{
		return null;
	}
	
	public static final String getUnitString(int input){
		return input==1?"":String.valueOf(input);
	}
	
	public static String replaceByIndex(String str,int index,char c){
		if(str.length()<=index){
			index = str.length() -1;
		}
		char[] chars = str.toCharArray();
		chars[index] = c;
		return String.valueOf(chars);
		
	}
	
	public static List<Integer> strToIntegerList(String str){
		List<Integer> result=new ArrayList<>();
		String []  arr=str.split(",");
		for (int i = 0; i < arr.length; i++) {
			result.add(Integer.parseInt(arr[i]));
		}
		return result;
	}
	public static void main(String[] args) throws UnsupportedEncodingException {
		
	}
}
