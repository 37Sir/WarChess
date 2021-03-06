﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework
{
    public class ChessLogic02
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
        new int[]{5, -1, -1, -1, -1, -1, -1, -1},
        new int[]{-1, -1, -1, -1, -1, -1, -1, -1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
        new int[]{-1, -1, -1, -1, -1, -1, -1, -1},
        new int[]{-1, -1, -1, -1, -1, -1, -1, 15},
    };

        private Vector2Int[] m_kings = {
        new Vector2Int(0, 0),
        new Vector2Int(7, 7),
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
                new int[]{5, -1, -1, -1, -1, -1, -1, -1},
                new int[]{-1, -1, -1, -1, -1, -1, -1, -1},
                new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
                new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
                new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
                new int[]{-1,-1,-1,-1,-1,-1,-1,-1},
                new int[]{-1, -1, -1, -1, -1, -1, -1, -1},
                new int[]{-1, -1, -1, -1, -1, -1, -1, 15},
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
        /// 王被将
        /// </summary>
        /// <returns></returns>
        public bool IsCheck(int kingColor)
        {
            Vector2Int kingPos = m_kings[kingColor];
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    int piece = GetPiece(x, y);
                    int color = piece / 10;
                    if (color == kingColor || piece == -1) continue;
                    if (CanMove(new Vector2(x, y), kingPos))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 被将的预判
        /// </summary>
        /// <returns>走完这步棋是否被将</returns>
        public bool IsCheckPre(int kingColor, Vector2 from, Vector2 to)
        {
            Vector2 kingPos;
            int fromX = (int)from.x;
            int fromY = (int)from.y;
            int toX = (int)to.x;
            int toY = (int)to.y;
            int tempPiece = GetPiece(fromX, fromY);
            if (tempPiece % 10 == (int)Config.PieceType.K)
            {
                kingPos = to;
            }
            else
            {
                kingPos = m_kings[kingColor];
            }
            int[,] temp = CopyBoard();
            temp[fromY, fromX] = -1;
            temp[toY, toX] = tempPiece;

            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    int piece = temp[y, x];
                    int color = piece / 10;
                    if (color == kingColor || piece == -1) continue;
                    if (CanMovePre(new Vector2(x, y), kingPos, temp))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
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
            int toType = GetPiece(toX, toY);
            int toColor;
            int toSquare = SQUARES[toY][toX];

            if (fromType < 0)
            {
                return false;
            }
            fromColor = fromType / 10;

            if (toType < 0)
            {
                toColor = -1;
            }
            else
            {
                toColor = toType / 10;
            }
            int difference = toSquare - fromSquare;
            int index = difference + 119;
            toType = toType % 10;
            fromType = fromType % 10;
            if (toColor == fromColor) return false;//todo如果王车易位 需要修改
            if (fromType == (int)Config.PieceType.P)//todo 小兵直走(新模式 上下左右走)
            {
                if(GetPiecesTotalDistance(from, to) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ((ATTACKS[index] & (1 << SHIFTS[fromType])) != 0)
            {
                //如果是王或者马 不必判断阻挡
                if (fromType == (int)Config.PieceType.N || fromType == (int)Config.PieceType.K)
                {
                    return true;
                }

                //判断阻挡
                int offset = RAYS[index];
                int j = toSquare + offset;
                bool blocked = false;
                while (j != fromSquare)
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
                if (!blocked)
                {
                    return true;
                }
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
            int fromColor = fromType / 10;
            int toX = (int)to.x;
            int toY = (int)to.y;
            int toType = GetPiece(toX, toY);
            if (CanMove(from, to))
            {
                m_board[fromY][fromX] = -1;
                m_board[toY][toX] = fromType;
                if (fromType % 10 == (int)Config.PieceType.P)
                {
                    m_pawn[fromColor][fromX] = 1;
                }
                if (fromType % 10 == (int)Config.PieceType.K)
                {
                    m_kings[fromColor] = new Vector2Int(toX, toY);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 可移动位置
        /// </summary>
        /// <returns></returns>
        public List<Vector2> GenerateMoves(Vector2 from)
        {
            List<Vector2> moves = new List<Vector2>();
            Vector2 to = new Vector2();
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    to.x = x;
                    to.y = y;
                    if (CanMove(from, to))
                    {
                        moves.Add(to);
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// 可移动位置
        /// </summary>
        /// <returns></returns>
        public List<Move> NewGenerateMoves(Vector2 from)
        {
            List<Move> moves = new List<Move>();
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    Vector2 to = new Vector2();
                    to.x = x;
                    to.y = y;
                    if (CanMove(from, to))
                    {
                        Move move = new Move(from, to);
                        move.Distance = GetPiecesTotalDistance(from, to);
                        moves.Add(move);
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// 兵晋升
        /// </summary>
        public void PPromoted(Vector2Int pos, int type)
        {
            if (pos.y == 7)
            {
                m_board[pos.y][pos.x] = type;
            }
            else
            {
                m_board[pos.y][pos.x] = type + 10;
            }
        }

        public int GetPiece(int x, int y)
        {
            return m_board[y][x];
        }

        public int GetPiece(float x, float y)
        {
            return m_board[(int)y][(int)x];
        }

        public void SetPiece(float x, float y, int type)
        {
            m_board[(int)y][(int)x] = type;
        }

        public int GetPieceValue(int selfColor, Vector2 piecePos)
        {
            var piece = GetPiece(piecePos.x, piecePos.y);
            var pieceColor = piece / 10;
            int value = Config.PieceValue[piece % 10];
            if (pieceColor == selfColor)
            {
                return value;
            }
            else
            {
                return -value;
            }
        }

        public void Undo(Vector2 from, Vector2 to, int eatType)
        {
            var temp = GetPiece(to.x - 1, to.y - 1);
            SetPiece(from.x - 1, from.y - 1, temp);
            SetPiece(to.x - 1, to.y - 1, eatType);
        }

        /// <summary>
        /// 可走预判
        /// </summary>
        /// <returns></returns>
        private bool CanMovePre(Vector2 from, Vector2 to, int[,] temp)
        {
            int fromX = (int)from.x;
            int fromY = (int)from.y;
            int fromType = temp[fromY, fromX];
            int fromColor;
            int fromSquare = SQUARES[fromY][fromX];

            int toX = (int)to.x;
            int toY = (int)to.y;
            int toType = temp[toY, toX];
            int toColor;
            int toSquare = SQUARES[toY][toX];

            if (fromType < 0)
            {
                return false;
            }
            fromColor = fromType / 10;

            if (toType < 0)
            {
                toColor = -1;
            }
            else
            {
                toColor = toType / 10;
            }
            int difference = toSquare - fromSquare;
            int index = difference + 119;
            toType = toType % 10;
            fromType = fromType % 10;

            if ((ATTACKS[index] & (1 << SHIFTS[fromType])) != 0)
            {
                //判断是否小兵往回走 *小兵直走斜吃
                if (fromType == (int)Config.PieceType.P)
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
                while (j != fromSquare)
                {
                    int y = 7 - j / 16;
                    int x = j % 16;
                    if (temp[y, x] >= 0)
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

        public int[,] CopyBoard()
        {
            int[,] arr = new int[Config.Board.MaxY, Config.Board.MaxY];
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    arr[y, x] = m_board[y][x];
                }
            }
            return arr;
        }

        /// <summary>
        /// 得到所有可行的走法
        /// </summary>
        /// <returns></returns>
        public List<Move> GetAllMoves(int color)
        {
            List<Move> allMoves = new List<Move>();
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    var piece = GetPiece(x, y);
                    var pieceColor = piece / 10;
                    if (pieceColor != color) continue;
                    var from = new Vector2(x, y);
                    var moves = GenerateMoves(from);//某个棋子可移动的位置集合
                    foreach (var move in moves)
                    {
                        Move temp = new Move(from, move);
                        allMoves.Add(temp);
                    }
                }
            }
            return allMoves;
        }

        public List<Vector2> GetSummonPoints(int color)
        {
            List<Vector2> points = new List<Vector2>();
            var kingPoint = m_kings[color];
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for(int x = 0; x < Config.Board.MaxX; x++)
                {
                    var temp = new Vector2(x, y);
                    var piece = GetPiece(x, y);
                    if (piece > -1) continue;
                    var distance = GetPiecesDistance(kingPoint, temp);
                    if (distance == 1)
                    {
                        points.Add(temp);
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// 召唤
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public bool DoSummon(Vector2 point, int type)
        {
            int color = type / 10;
            var kingPoint = m_kings[color];
            var piece = GetPiece(point.x, point.y);
            if (piece > -1) return false;
            var distance = GetPiecesDistance(kingPoint, point);
            if (distance == 1)
            {
                SetPiece(point.x, point.y, type);
                return true;
            } 
            else return false;
        }

        private int GetPiecesDistance(Vector2 from, Vector2 to)
        {
            int dx = (int)Math.Abs(from.x - to.x);
            int dy = (int)Math.Abs(from.y - to.y);
            if(dx > dy)
            {
                return dx;
            }
            else
            {
                return dy;
            }
        }

        private int GetPiecesTotalDistance(Vector2 from, Vector2 to)
        {
            int dx = (int)Math.Abs(from.x - to.x);
            int dy = (int)Math.Abs(from.y - to.y);

            return dx + dy;
        }
    }
}

