package com.zyd.demo.common.exception;

public class BaseException extends Exception {
  private static final long serialVersionUID = -1807094022186456487L;
  protected int errorCode;

  public int getErrrorCode() {
      return errorCode;
  }

  public void setErrrorCode(int errorCode) {
      this.errorCode = errorCode;
  }

  public BaseException(int errorCode) {
      super(String.valueOf(errorCode));
      this.errorCode = errorCode;
  }
}
