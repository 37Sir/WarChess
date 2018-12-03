package com.zyd.common.unti;

/**
 * @Description: 字符转工具类
 * @author soloman
 * @date 2016年9月30日 上午11:19:25
 */

public class StringUtil {

	public static final char UNDERLINE = '_';

	// 驼峰转大写加下划线
	public static String camelToUnderline(String param) {
		if (param == null || "".equals(param.trim())) {
			return "";
		}
		int len = param.length();
		StringBuilder sb = new StringBuilder(len);
		for (int i = 0; i < len; i++) {
			char c = param.charAt(i);
			if (Character.isUpperCase(c) || Character.isDigit(c)) {
				sb.append(UNDERLINE);
				sb.append(Character.toUpperCase(c));
			} else {
				sb.append(c);
			}
		}

		int lenth2 = sb.length();
		StringBuilder sb2 = new StringBuilder();

		for (int i = 0; i < lenth2; i++) {
			char c = sb.charAt(i);
			sb2.append(Character.toUpperCase(c));
		}

		return sb2.toString();
	}

	/**
	 * 下划线转成驼峰格式(TRUE:大驼峰; FALSE:小驼峰)
	 * 
	 * @param param
	 * @param isBigOrLittle
	 * @return
	 */
	public static String underlineToCamel(String param, boolean isBigOrLittle) {
		String[] splitCols = param.split("_");
		String result = "";
		for (int i = 0; i < splitCols.length; i++) {
			String everySplit = splitCols[i];
			if (i == 0) {
				result += toCamel(everySplit, isBigOrLittle);
				continue;
			}
			result += toCamel(everySplit, true);
		}
		return result;
	}

	/**
	 * 转成驼峰
	 * 
	 * @param param
	 * @param isBigOrLittle
	 * @return
	 */
	private static String toCamel(String param, boolean isBigOrLittle) {
		String result = "";
		for (int j = 0; j < param.length(); j++) {
			char ch = param.charAt(j);
			if (j != 0) {
				result += Character.toLowerCase(ch);
			} else {
				if (isBigOrLittle) {
					result += Character.toUpperCase(ch);
				} else {
					result += Character.toLowerCase(ch);
				}
			}
		}
		return result;
	}

	public static void main(String[] args) {

		// String param = "isWin";
		//
		// String s1 = camelToUnderline(param);
		//
		// System.out.println(s1);
	}

}
