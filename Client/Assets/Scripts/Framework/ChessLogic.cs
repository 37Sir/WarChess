using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChessLogic
{
    int[] ATTACKS = {
       20, 0, 0, 0, 0, 0, 0, 24, 0, 0, 0, 0, 0, 0, 20, 0,
       0, 20, 0, 0, 0, 0, 0, 24, 0, 0, 0, 0, 0, 20, 0, 0,
       0, 0, 20, 0, 0, 0, 0, 24, 0, 0, 0, 0, 20, 0, 0, 0,
       0, 0, 0, 20, 0, 0, 0, 24, 0, 0, 0, 20, 0, 0, 0, 0,
       0, 0, 0, 0, 20, 0, 0, 24, 0, 0, 20, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 20, 2, 24, 2, 20, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 2, 53, 56, 53, 2, 0, 0, 0, 0, 0, 0,
  24, 24, 24, 24, 24, 24, 56, 0, 56, 24, 24, 24, 24, 24, 24, 0,
       0, 0, 0, 0, 0, 2, 53, 56, 53, 2, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 20, 2, 24, 2, 20, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 20, 0, 0, 24, 0, 0, 20, 0, 0, 0, 0, 0,
       0, 0, 0, 20, 0, 0, 0, 24, 0, 0, 0, 20, 0, 0, 0, 0,
       0, 0, 20, 0, 0, 0, 0, 24, 0, 0, 0, 0, 20, 0, 0, 0,
       0, 20, 0, 0, 0, 0, 0, 24, 0, 0, 0, 0, 0, 20, 0, 0,
       20, 0, 0, 0, 0, 0, 0, 24, 0, 0, 0, 0, 0, 0, 20,
     };
    int[] RAYS = {
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
    int[] SHIFTS = { 0, 1, 2, 3, 4, 5 };//p n b r q k
    int[][] SQUARES = {
        new int[]{112, 113,114,115,116,117,118,119},
        new int[]{ 96,  97, 98, 99,100,101,102,103},
        new int[]{ 80,  81, 82, 83, 84, 85, 86, 87},
        new int[]{ 64,  65, 66, 67, 68, 69, 70, 71},
        new int[]{ 48,  49, 50, 51, 52, 53, 54, 55},
        new int[]{32, 33,34,35,36,37,38,39},
        new int[]{16, 17,18,19,20,21,22,23},
        new int[]{0, 1,2,3,4,5,6,7}
    };

    //0:p, 1:n, 2:b, 3:r, 4:q, 5:k,
    //black + 10 
    private int[][] m_board =
    {
        new int[]{3, 1, 2, 4, 5, 2, 1, 3},
        new int[]{0, 0, 0, 0, 0, 0, 0, 0},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{10, 10, 10, 10, 10, 10, 10, 10},
        new int[]{13, 11, 12, 14, 15, 12, 11, 13},
    };

    private int[][] m_pawn =
    {
        new int[]{0, 0, 0, 0, 0, 0, 0, 0},
        new int[]{0, 0, 0, 0, 0, 0, 0, 0},
    };

    public int[][] Board
    {
        get
        {
            return m_board;
        }
        set
        {
            m_board = value;
        }
    }

    private void AddMove()
    {

    }

    public void Init()
    {
        int[][] board =
        {
            new int[]{3, 1, 2, 4, 5, 2, 1, 3},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0},
            new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
            new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
            new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
            new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
            new int[]{10, 10, 10, 10, 10, 10, 10, 10},
            new int[]{13, 11, 12, 14, 15, 12, 11, 13},
        };

        int[][] pawn =
        {
            new int[]{2, 2, 2, 2, 2, 2, 2, 2},//white
            new int[]{2, 2, 2, 2, 2, 2, 2, 2},//black
        };
        m_pawn = pawn;
        Board = board;
    }

    /// <summary>
    /// 目标位置是否可走
    /// </summary>
    /// <returns></returns>
    public bool CanMove(Vector2 from, Vector2 to)
    {
        int fromX = (int)from.x;
        int fromY = (int)from.y;
        int fromType = GetPiece(fromX, fromY);
        int fromColor;
        int fromSquare = SQUARES[fromY][fromX];

        int toX = (int)to.x;
        int toY = (int)to.y;
        int toType = GetPiece(toX, toY) % 10;
        int toColor;
        int toSquare = SQUARES[toY][toX];

        if (fromType < 0)
        {
            return false;
        }
        fromColor = fromType / 10;
        if(toType < 0)
        {
            toColor = -1;
        }
        else
        {
            toColor = toType / 10;
        }
        int difference = toSquare - fromSquare;
        int index = difference + 119;

        if (toColor == fromColor) return false;//todo如果王车易位 需要修改

        if (fromType == (int)Config.PieceType.P)//todo 小兵直走
        {
            if (toType >= 0) return false;
            //小兵第一次移动可以走两步 之后只有一步
            if(Math.Abs(from.y - to.y) > m_pawn[fromColor][fromX])
            {
                return false;
            }
            if (difference < 0)
            {
                if (fromColor == (int)Config.PieceColor.WHITE && from.x == to.x)
                {
                    return true;
                }
            }
            else
            {
                if (fromColor == (int)Config.PieceColor.BLACK && from.x == to.x)
                {
                    return true;
                }
            }
        }

        if ((ATTACKS[index] & (1 << SHIFTS[fromType]))!=0)
        {
            //判断是否小兵往回走 *小兵直走斜吃
            if(fromType == (int)Config.PieceType.P)
            {
                if (toType < 0 || toColor == fromColor) return false;
                if (difference > 0)
                {
                    if (fromColor == (int)Config.PieceColor.WHITE) return true;
                }
                else
                {
                    if (fromColor == (int)Config.PieceColor.BLACK) return true;
                }
            }
            //如果是王或者马 不必判断阻挡
            if (fromType == (int)Config.PieceType.N || fromType == (int)Config.PieceType.K) return true;

            //判断阻挡
            int offset = RAYS[index];
            int j = toSquare + offset;
            bool blocked = false;
            while(j != fromSquare)
            {
                int y = 7 - j / 16;
                int x = j % 16;
                if (GetPiece(x, y) >= 0)
                {
                    blocked = true;
                    break;
                }
                j += offset;
            }
            if (!blocked) return true;
        }
        return false;
    }

    /// <summary>
    /// 移动到目标位置
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public bool DoMove(Vector2 from, Vector2 to)
    {
        int fromX = (int)from.x;
        int fromY = (int)from.y;
        int fromType = GetPiece(fromX, fromY);
        int fromColor = fromType/10;
        int toX = (int)to.x;
        int toY = (int)to.y;
        int toType = GetPiece(toX, toY);
        if (CanMove(from, to))
        {
            m_board[fromY][fromX] = -1;
            m_board[toY][toX] = fromType;
            if(fromType == (int)Config.PieceType.P)
            {
                m_pawn[fromColor][fromX] = 1;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 可移动位置
    /// </summary>
    /// <returns></returns>
    public List<Vector2> GenerateMoves(Vector2 from) {
        List<Vector2> moves = new List<Vector2>();
        Vector2 to = new Vector2();
        for (int y = 0; y < Config.Board.MaxY; y++)
        {
            for (int x = 0; x < Config.Board.MaxX; x++)
            {
                to.x = x;
                to.y = y;
                if(CanMove(from, to))
                {
                    moves.Add(to); 
                }
            }
        }
        return moves;
    }

    public int GetPiece(int x, int y)
    {
        return m_board[y][x];
    }
}

