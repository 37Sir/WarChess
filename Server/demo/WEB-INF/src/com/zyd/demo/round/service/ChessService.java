package com.zyd.demo.round.service;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import com.zyd.demo.common.BaseService;
/**
 * 判断规则的类
 * @author ZhaoYiDing
 *
 */
public class ChessService extends BaseService {
  /*对应增量的棋子集合，用于判断棋子可走*/
  int[] attacks = new int[]{
    20, 0, 0, 0, 0, 0, 0, 24,  0, 0, 0, 0, 0, 0,20, 0,
     0,20, 0, 0, 0, 0, 0, 24,  0, 0, 0, 0, 0,20, 0, 0,
     0, 0,20, 0, 0, 0, 0, 24,  0, 0, 0, 0,20, 0, 0, 0,
     0, 0, 0,20, 0, 0, 0, 24,  0, 0, 0,20, 0, 0, 0, 0,
     0, 0, 0, 0,20, 0, 0, 24,  0, 0,20, 0, 0, 0, 0, 0,
     0, 0, 0, 0, 0,20, 2, 24,  2,20, 0, 0, 0, 0, 0, 0,
     0, 0, 0, 0, 0, 2,53, 56, 53, 2, 0, 0, 0, 0, 0, 0,
    24,24,24,24,24,24,56,  0, 56,24,24,24,24,24,24, 0,
     0, 0, 0, 0, 0, 2,53, 56, 53, 2, 0, 0, 0, 0, 0, 0,
     0, 0, 0, 0, 0,20, 2, 24,  2,20, 0, 0, 0, 0, 0, 0,
     0, 0, 0, 0,20, 0, 0, 24,  0, 0,20, 0, 0, 0, 0, 0,
     0, 0, 0,20, 0, 0, 0, 24,  0, 0, 0,20, 0, 0, 0, 0,
     0, 0,20, 0, 0, 0, 0, 24,  0, 0, 0, 0,20, 0, 0, 0,
     0,20, 0, 0, 0, 0, 0, 24,  0, 0, 0, 0, 0,20, 0, 0,
    20, 0, 0, 0, 0, 0, 0, 24,  0, 0, 0, 0, 0, 0,20
  };
  
  /*用户判断是否有阻挡*/
  int[] rays = new int[]{
     17,  0,  0,  0,  0,  0,  0, 16,  0,  0,  0,  0,  0,  0, 15, 0,
      0, 17,  0,  0,  0,  0,  0, 16,  0,  0,  0,  0,  0, 15,  0, 0,
      0,  0, 17,  0,  0,  0,  0, 16,  0,  0,  0,  0, 15,  0,  0, 0,
      0,  0,  0, 17,  0,  0,  0, 16,  0,  0,  0, 15,  0,  0,  0, 0,
      0,  0,  0,  0, 17,  0,  0, 16,  0,  0, 15,  0,  0,  0,  0, 0,
      0,  0,  0,  0,  0, 17,  0, 16,  0, 15,  0,  0,  0,  0,  0, 0,
      0,  0,  0,  0,  0,  0, 17, 16, 15,  0,  0,  0,  0,  0,  0, 0,
      1,  1,  1,  1,  1,  1,  1,  0, -1, -1,  -1,-1, -1, -1, -1, 0,
      0,  0,  0,  0,  0,  0,-15,-16,-17,  0,  0,  0,  0,  0,  0, 0,
      0,  0,  0,  0,  0,-15,  0,-16,  0,-17,  0,  0,  0,  0,  0, 0,
      0,  0,  0,  0,-15,  0,  0,-16,  0,  0,-17,  0,  0,  0,  0, 0,
      0,  0,  0,-15,  0,  0,  0,-16,  0,  0,  0,-17,  0,  0,  0, 0,
      0,  0,-15,  0,  0,  0,  0,-16,  0,  0,  0,  0,-17,  0,  0, 0,
      0,-15,  0,  0,  0,  0,  0,-16,  0,  0,  0,  0,  0,-17,  0, 0,
    -15,  0,  0,  0,  0,  0,  0,-16,  0,  0,  0,  0,  0,  0,-17
  };
    //兵的位移增量集合
    Map<String, List<Integer>> pawn_offsets = new HashMap<>();
    //b为后手方，w为先手方
    List<Integer> b = new ArrayList<Integer>();
    List<Integer> w = new ArrayList<Integer>();
    //除兵外的位移增量集合
    Map<String, List<Integer>> piece_offsets = new HashMap<>();
    List<Integer> n = new ArrayList<Integer>();
    List<Integer> b1 = new ArrayList<Integer>();
    List<Integer> r = new ArrayList<Integer>();
    List<Integer> q = new ArrayList<Integer>();
    List<Integer> k = new ArrayList<Integer>();
    //各棋子的初始化值
    HashMap<String, Integer> shifts = new HashMap<String, Integer>();
    //初始化
    {
        shifts.put("p", 0);  
        shifts.put("n", 1); 
        shifts.put("b", 2); 
        shifts.put("r", 3); 
        shifts.put("q", 4); 
        shifts.put("k", 5);
        b.add(16);
        b.add(32);
        b.add(17);
        b.add(15);
        w.add(-16);
        w.add(-32);
        w.add(-17);
        w.add(-15);
        pawn_offsets.put("b", b);
        pawn_offsets.put("w", w);
        n.add(-18);
        n.add(-33);
        n.add(-31);
        n.add(-14);
        n.add(-18);
        n.add(33);
        n.add(31);
        n.add(-14);
        piece_offsets.put("n", n);
        b1.add(-17);
        b1.add(-15);
        b1.add(17);
        b1.add(15);
        piece_offsets.put("b", b1);
        r.add(-16);
        r.add(1);
        r.add(16);
        r.add(-1);
        piece_offsets.put("r", r);
        q.add(-17);
        q.add(-16);
        q.add(-15);
        q.add(1);
        q.add(17);
        q.add(16);
        q.add(15);
        q.add(-1);
        piece_offsets.put("q", q);
        k.add(-17);
        k.add(-16);
        k.add(-15);
        k.add(1);
        k.add(17);
        k.add(16);
        k.add(15);
        k.add(-1);
        piece_offsets.put("k", k);
      

    }

  /*棋盘对应index的值*/
  int[] SQUARES =new int[]{
        0,  1, 2, 3, 4, 5, 6, 7,
        16, 17, 18, 19, 20, 21, 22, 23,
        32, 33, 34, 35, 36, 7, 38, 39,
        48, 49, 50, 51, 52, 53, 54, 55,
        64, 65, 66, 67, 68, 69, 70, 71,
        80, 81, 82, 83, 84, 85, 86, 87,
        96, 97, 98, 99, 100, 101, 102, 103,
        112, 113, 114, 115, 116, 117, 118, 119
  };
  
  public int getSquares(int index) {
      if (index >= 1 && index <= 64) return SQUARES[index -1];
      return -1;
  }
  
  public int getIndex(int squares) {
      for (int i= 0 ; i < 64; i++) {
          if (SQUARES[i] == squares ) {
              return i+1;
          }
      }
      return -1;
  }
  
  /**
   *OtherUserChess:敌方棋子
   * isFirst :敌方是否为先手方
   */
   public Boolean attacked(Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess, int square,Boolean isFirst) {
     for (Map.Entry<Integer, String> entry : OtherUserChess.entrySet()) {
       //16*8棋盘位置关系位置转换
       square = getSquares(square);
       int value = getSquares(entry.getKey());
       int difference = value - square;
       int index = difference + 119;
       if ((attacks[index] & (1 << shifts.get(entry.getValue()))) >0) {
         if (entry.getValue() == "p") {
           if (difference > 0) {
             if (isFirst) return true;
           } else {
             if (!isFirst) return true;
           }
           continue;
         }
         /* 棋子是国王和马不会被阻挡 */
         if (entry.getValue() == "n" || entry.getValue() == "k") return true;
  
         int offset = rays[index];
         int  j = value + offset;
         boolean blocked = false;
         
         while (j != square) {
           int key =getIndex(j);
           if (OtherUserChess.containsKey(key) || myChess.containsKey(key)) {
             blocked = true;
             break;
           }
           j += offset;
         }
           
         if (!blocked) return true;
       }
     }
  
     return false;
   }
   
   /**
    * 移动的校验
    * @param from 初始位置
    * @param to 移动位置
    * @param OtherUserChess 我方棋子
    * @param myChess 对方棋子
    * @param isFirst 对方是否是先手方
    * @return
    */
   public boolean checkMove(int from ,int to ,Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess,Boolean isFirst){
       String us = "";
       if (isFirst) {
           us = "b";
       } else {
           us = "w";
       }
       if (OtherUserChess.get(from) == "p") {
           if ((from <= 56 && from >= 49) || (from <= 16 && from >= 9)) {
               if (getSquares(to) == (getSquares(from)+pawn_offsets.get(us).get(0))
                   || getSquares(to) == (getSquares(from)+pawn_offsets.get(us).get(1))) {
                   return true;
               }
           } else {
               if (getSquares(to) == (getSquares(from)+pawn_offsets.get(us).get(0))){
                   return true;
               }
           }
           return false;
       } else {
           return CheckNotPawnMove(from, to, OtherUserChess, myChess);
       }       
   }
   /**
    * 除兵外的移动检验
    * @return
    */
   public boolean CheckNotPawnMove(int from ,int to ,Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess){
       int square = getSquares(to);
       int value = getSquares(from);
       int difference = value - square;
       int index = difference + 119;
       if ((attacks[index] & (1 << shifts.get(OtherUserChess.get(from)))) >0) {
           /* 棋子是国王和马不会被阻挡 */
           if (OtherUserChess.get(from) == "n" || OtherUserChess.get(from) == "k") return true;    
           int offset = rays[index];
           int  j = value + offset;
           boolean blocked = false;
           
           while (j != square) {
             int key =getIndex(j);
             if (OtherUserChess.containsKey(key) || myChess.containsKey(key)) {
               blocked = true;
               break;
             }
             j += offset;
           }           
           if (!blocked) return true;
       }
       return false;
   }
}
