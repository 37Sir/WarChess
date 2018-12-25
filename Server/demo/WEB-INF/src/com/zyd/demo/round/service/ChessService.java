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
  static int[] attacks = new int[]{
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
  static int[] rays = new int[]{
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
    static Map<String, List<Integer>> pawn_offsets = new HashMap<>();
    //b为后手方，w为先手方
    static List<Integer> b = new ArrayList<Integer>();
    static List<Integer> w = new ArrayList<Integer>();
    //除兵外的位移增量集合
    static Map<String, List<Integer>> piece_offsets = new HashMap<>();
    static List<Integer> n = new ArrayList<Integer>();
    static List<Integer> b1 = new ArrayList<Integer>();
    static List<Integer> r = new ArrayList<Integer>();
    static  List<Integer> q = new ArrayList<Integer>();
    static List<Integer> k = new ArrayList<Integer>();
    static List<Integer> p = new ArrayList<Integer>();
    //各棋子的初始化值
    static HashMap<String, Integer> shifts = new HashMap<String, Integer>();
    //初始化
    static {
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
        p.add(-16);
        p.add(1);
        p.add(16);
        p.add(-1);
        piece_offsets.put("p", p);

    }

  /*棋盘对应index的值*/
  static int[] squares =new int[]{
        0,  1, 2, 3, 4, 5, 6, 7,
        16, 17, 18, 19, 20, 21, 22, 23,
        32, 33, 34, 35, 36, 37, 38, 39,
        48, 49, 50, 51, 52, 53, 54, 55,
        64, 65, 66, 67, 68, 69, 70, 71,
        80, 81, 82, 83, 84, 85, 86, 87,
        96, 97, 98, 99, 100, 101, 102, 103,
        112, 113, 114, 115, 116, 117, 118, 119
  };
  
  public static int getSquares(int index) {
      if (index >= 1 && index <= 64) return squares[index -1];
      return -1;
  }
  
  public static int getIndex(int square) {
      for (int i= 0 ; i < 64; i++) {
          if (squares[i] == square ) {
              return i+1;
          }
      }
      return -1;
  }
  
  /**
   * 判断某个棋子是否会被攻击
   *OtherUserChess:敌方棋子
   * isFirst :敌方是否为先手方
   */
   public static Boolean attacked(Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess,
       int squares,Boolean isFirst) {
       for (Map.Entry<Integer, String> entry : OtherUserChess.entrySet()) {
         //16*8棋盘位置关系位置转换
         int square = getSquares(squares);
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
           //棋子是国王和马不会被阻挡 
           if (entry.getValue() == "n" || entry.getValue() == "k") return true;
    
           int offset = rays[index];
           int  j = value + offset;
           boolean blocked = false;
           //根据最小增量递加，看是否有棋子阻挡
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
   
   /** 判断在第几行*/
   public static int getRank(int index) {
       return (index-1) >> 3;
   }
   /**
    * 移动的校验
    * @param from 初始位置
    * @param to 移动位置
    * @param OtherUserChess 对方棋子
    * @param myChess 我方棋子
    * @param isFirst 对方是否是先手方
    * @return
    */
   public static boolean checkMove(int from ,int to ,Map<Integer, String> OtherUserChess,
       Map<Integer, String>  myChess,Boolean isFirst){
       String us = "";
       int second = 0;
       if (isFirst) {
           us = "b";
           second = 1;
       } else {
           us = "w";
           second = 6;
       }
       if (!myChess.containsKey(from)) return false;
       if ( "p".equals(myChess.get(from))) {
           int pIndex = getSquares(from)+pawn_offsets.get(us).get(0);
           if (!OtherUserChess.containsKey(getIndex(pIndex)) && !myChess.containsKey(getIndex(pIndex))) {
               if (getRank(from) == second) {
                   if (getSquares(to) == (pIndex)
                       || getSquares(to) == (getSquares(from)+pawn_offsets.get(us).get(1))) {
                       return true;
                   }
               } else {
                   if (getSquares(to) == (pIndex)){
                       return true;
                   }
               }
           }
           return false;
       } else {
           return CheckNotPawnMove(from, to, OtherUserChess, myChess);
       }       
   }
   /**
    * 除兵外的移动检验以及所有的吃子校验
    * @return
    */
   public static boolean CheckNotPawnMove(int from ,int to ,Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess){
       int square = getSquares(to);
       int value = getSquares(from);
       int difference = value - square;
       int index = difference + 119;
       if ((attacks[index] & (1 << shifts.get(myChess.get(from)))) >0) {
           /* 棋子是国王和马不会被阻挡 */
           if (myChess.get(from) == "n" || myChess.get(from) == "k") return true;    
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
   
   /**能走的棋子list集合
    *String格式：from_to_type 
    */
   public static List<String> generate_moves(Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess,
       int kingIndex ,Boolean isFirst){
       String us = "";
       int second = 0;
       if (isFirst) {
           us = "b";
           second = 1;
       } else {
           us = "w";
           second = 6;
       }
       List<String> move = new ArrayList<>();
       //遍历己方棋子，根据增量判断是否可以走
       for (Map.Entry<Integer, String> entry : myChess.entrySet()) {
           //兵吃棋和走棋不一样，特殊处理
           if (entry.getValue() == "p") {
               //走棋
               int from = entry.getKey().intValue();
               int pIndex = getSquares(from) + pawn_offsets.get(us).get(0);
               
               if (!OtherUserChess.containsKey(getIndex(pIndex)) && !myChess.containsKey(getIndex(pIndex))) {
                   move.add(String.valueOf(from) + "_" +String.valueOf(getIndex(pIndex)) +"_" + entry.getValue());
                   
                   pIndex = getSquares(from) + pawn_offsets.get(us).get(1);
                   if (!OtherUserChess.containsKey(getIndex(pIndex)) && !myChess.containsKey(getIndex(pIndex)) 
                       && getRank(from) == second ) {
                       move.add(String.valueOf(from) + "_" +String.valueOf(getIndex(pIndex)) +"_" + entry.getValue());
                   } 
               }
               //吃子
               for (int j = 2; j < 4; j++) {
                   pIndex = getSquares(from) + pawn_offsets.get(us).get(j);
                   //不在棋盘上
                   if ((pIndex & 0x88) > 0) continue;
                   if (OtherUserChess.containsKey(getIndex(pIndex))) {
                       move.add(String.valueOf(from) + "_" +String.valueOf(getIndex(pIndex)) +"_" + entry.getValue());
                   }                   
               }
           } else {
               for( int j = 0 ; j < piece_offsets.get(entry.getValue()).size(); j++) {
                   int offset = piece_offsets.get(entry.getValue()).get(j);
                   int square = getSquares(entry.getKey().intValue());
                   int from = entry.getKey().intValue();
                   while (true) {
                       square += offset;
                       if ((square & 0x88) > 0) break;
                       if (myChess.containsKey(getIndex(square))) break;
                       move.add(String.valueOf(from) + "_" +String.valueOf(getIndex(square)) +"_" + entry.getValue());
                       //马和国王只能移动一格，直接返回
                       if ("n".equals(entry.getValue()) || "k".equals(entry.getValue())) break;
                   }
               }
           }
       }
       List<String> last_move = new ArrayList<>();
       //如果移动后会被将军，则不能移动，否则加入返回的移动集合
        for (int i =0; i < move.size() ; i++ ) {
            String[] from_to_type= move.get(i).split("_");
            int from = Integer.parseInt(from_to_type[0]);
            int to = Integer.parseInt(from_to_type[1]);
            String type = from_to_type[2];
            if (type.equals("k")) kingIndex = to;
            Map<Integer, String> my = new HashMap<>(myChess);
            Map<Integer, String> other = new HashMap<>(OtherUserChess);
            my.put(to, my.get(from));
            my.remove(from);
            if (other.containsKey(to)) other.remove(to);
            if (!attacked(other, my, kingIndex, isFirst)){
                last_move.add(move.get(i));
            }
        }
       return last_move;
   }
   //得到对应的棋子类型
   public static String getShiftsType(int value) {
       String type = "";
       for (Map.Entry<String, Integer> entry : shifts.entrySet()) {
            if (value == entry.getValue().intValue()) {
                type = entry.getKey();
            }
       }
       return type;
   }
   
   /**
          * 新模式判断某个棋子是否会被攻击
    *OtherUserChess:敌方棋子
    */
   public static boolean CheckStoneMove(int from ,int to ,Map<Integer, String> OtherUserChess,Map<Integer, String>  myChess){
     int square = getSquares(to);
     int value = getSquares(from);
     int difference = value - square;
     int index = difference + 119;
     if ((attacks[index] & (1 << shifts.get(myChess.get(from)))) >0) {
         /* 棋子是国王和马和兵不会被阻挡 */
         if (myChess.get(from) == "n" || myChess.get(from) == "k" || myChess.get(from) == "p") return true;    
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
     //测试有无棋可以走的方法
   public static void main(String[] args) {
     //玩家1棋子index——type
     Map<Integer, String> userOne = new HashMap<>();
     //玩家2棋子index——type
     Map<Integer, String> userTwo = new HashMap<>();
     for (int i = 1; i <= 6; i++) {
       if (i==1) {
           for (int y = 9; y<=16; y++ ) {
               userTwo.put(y, "p");
               userOne.put(y+40, "p");
           }
       } else if (i==2) {
           userTwo.put(1, "r");
           userTwo.put(8, "r");
           userOne.put(57, "r");
           userOne.put(64, "r");
       } else if (i == 3 ) {
           userTwo.put(2, "n");
           userTwo.put(7, "n");
           userOne.put(58, "n");
           userOne.put(63, "n");               
       } else if (i == 4) {
           userTwo.put(3, "b");
           userTwo.put(6, "b");
           userOne.put(59, "b");
           userOne.put(62, "b");               
       } else if (i == 5) {
           userTwo.put(4, "q");
           userOne.put(60, "q");               
       } else if (i == 6) {
           userTwo.put(5, "k");
           userOne.put(61, "k");               
       }
   }
     long time = System.currentTimeMillis();
      ChessService s = new ChessService();
      List<String> list =s.generate_moves(userTwo, userOne, 61, false);
      System.out.println(s.checkMove(52, 36, userTwo, userOne, false));
      System.out.println(System.currentTimeMillis() - time);
   }
}
