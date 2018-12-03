package com.zyd.proxy.info;

import java.io.Serializable;

public class JobRunning implements Serializable {
  private static final long serialVersionUID = -6430072332700676895L;
  public JobRunning(int jobRunning,int jobRunningMax) {
      this.jobRunning = jobRunning;
      this.jobRunningMax = jobRunningMax;
  }
  private int jobRunning;
  private int jobRunningMax;
  public int getJobRunning() {
      return jobRunning;
  }
  public void setJobRunning(int jobRunning) {
      this.jobRunning = jobRunning;
  }
  public int getJobRunningMax() {
      return jobRunningMax;
  }
  public void setJobRunningMax(int jobRunningMax) {
      this.jobRunningMax = jobRunningMax;
  }
}
