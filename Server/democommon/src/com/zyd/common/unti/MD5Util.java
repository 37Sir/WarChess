package com.zyd.common.unti;

import java.io.UnsupportedEncodingException;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

public class MD5Util {

	private static MessageDigest md5;

	private static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
	static {
		try {
			md5 = MessageDigest.getInstance("MD5");
		} catch (NoSuchAlgorithmException e) {
			e.printStackTrace();
		}
	}

	private static String md5(String str, String charset) {
		StringBuilder sb = new StringBuilder();
		try {
			byte[] bb = md5.digest(str.getBytes(charset));
			for (byte b : bb) {
				sb.append(hexDigits[b >>> 4 & 0xf]);
				sb.append(hexDigits[b & 0xf]);
			}
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		}

		return sb.toString();
	}
	public static String md5(byte[] data){
		StringBuilder sb = new StringBuilder();
		byte[] bb = md5.digest(data);
		for (byte b : bb) {
			sb.append(hexDigits[b >>> 4 & 0xf]);
			sb.append(hexDigits[b & 0xf]);
		}
		return sb.toString().toUpperCase();
	}
	public static String md5(String str) {
		return md5(str, "UTF-8").toUpperCase();
	}

	// http://localhost:8080/bj/gm/reduceFc?userId=LIFENGYANG&orderId=D2011070621541254314&amount=100&time=20120517145924&sign=BAC3198AE96FBE0311D31BFCC69D8DF7
	// String encodeStr = MD5Util.md5(timestamp + userId + orderId + amount +
	// key);

	public static void main(String[] args) {
		String orderId = "D2011070621541254314";
		String userId = "LIFENGYANG";
		String timestamp = "20120517145924";
		int amount = 100;
		String key = "xlyouxi@pde";
		String sign = md5(timestamp + userId + orderId + amount + key);
		System.out.println(sign);
		System.out.println("E98B573542F7D26B6782B753B5A56769");
	}
}
