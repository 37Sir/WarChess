using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework
{
    public class ChessAI
    {
        public delegate void DelegateCompleteSearch(Move move);
        DelegateCompleteSearch m_callback;
        static double[][] pawnEvalWhite =
        {
            new double[]{ 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
            new double[]{ 0.5,  1.0,  1.0, -2.0, -2.0,  1.0,  1.0,  0.5},
            new double[]{ 0.5, -0.5, -1.0,  0.0,  0.0, -1.0, -0.5,  0.5},
            new double[]{ 0.0,  0.0,  0.0,  2.0,  2.0,  0.0,  0.0,  0.0},
            new double[]{ 0.5,  0.5,  1.0,  2.5,  2.5,  1.0,  0.5,  0.5},
            new double[]{ 1.0,  1.0,  2.0,  3.0,  3.0,  2.0,  1.0,  1.0},
            new double[]{ 5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0},
            new double[]{ 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
        };

        static double[][] pawnEvalBlack =         
        {
            new double[]{ 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
            new double[]{ 5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0},
            new double[]{ 1.0,  1.0,  2.0,  3.0,  3.0,  2.0,  1.0,  1.0},
            new double[]{ 0.5,  0.5,  1.0,  2.5,  2.5,  1.0,  0.5,  0.5},
            new double[]{ 0.0,  0.0,  0.0,  2.0,  2.0,  0.0,  0.0,  0.0},
            new double[]{ 0.5, -0.5, -1.0,  0.0,  0.0, -1.0, -0.5,  0.5},
            new double[]{ 0.5,  1.0,  1.0, -2.0, -2.0,  1.0,  1.0,  0.5},
            new double[]{ 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
        };

        static double[][] knightEval =
        {
            new double[]{-5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0},
            new double[]{-4.0, -2.0,  0.0,  0.0,  0.0,  0.0, -2.0, -4.0},
            new double[]{-3.0,  0.5,  1.0,  1.5,  1.5,  1.0,  0.5, -3.0},
            new double[]{-3.0,  0.0,  1.5,  2.0,  2.0,  1.5,  0.0, -3.0},
            new double[]{-3.0,  0.0,  1.5,  2.0,  2.0,  1.5,  0.0, -3.0},
            new double[]{-3.0,  0.5,  1.0,  1.5,  1.5,  1.0,  0.5, -3.0},
            new double[]{-4.0, -2.0,  0.0,  0.0,  0.0,  0.0, -2.0, -4.0},
            new double[]{-5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0},
        };

        static double[][] bishopEvalWhite =
        {
            new double[]{-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0},
            new double[]{-1.0,  0.5,  0.0,  0.0,  0.0,  0.0,  0.5, -1.0},
            new double[]{-1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0, -1.0},
            new double[]{-1.0,  0.0,  1.0,  1.0,  1.0,  1.0,  0.0, -1.0},
            new double[]{-1.0,  0.5,  0.5,  1.0,  1.0,  0.5,  0.5, -1.0},
            new double[]{-1.0,  0.0,  0.5,  1.0,  1.0,  0.5,  0.0, -1.0},
            new double[]{-1.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -1.0},
            new double[]{-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0},
        };

        static double[][] bishopEvalBlack =
        {
            new double[]{-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0},
            new double[]{-1.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -1.0},
            new double[]{-1.0,  0.0,  0.5,  1.0,  1.0,  0.5,  0.0, -1.0},
            new double[]{-1.0,  0.5,  0.5,  1.0,  1.0,  0.5,  0.5, -1.0},
            new double[]{-1.0,  0.0,  1.0,  1.0,  1.0,  1.0,  0.0, -1.0},
            new double[]{-1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0, -1.0},
            new double[]{-1.0,  0.5,  0.0,  0.0,  0.0,  0.0,  0.5, -1.0},
            new double[]{-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0},
        };

        static double[][] rookEvalBlack =
        {
            new double[]{ 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
            new double[]{ 0.5,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{ 0.0,  0.0,  0.0,  0.5,  0.5,  0.0,  0.0,  0.0},
        };

        static double[][] rookEvalWhite =
        {
            new double[]{ 0.0,  0.0,  0.0,  0.5,  0.5,  0.0,  0.0,  0.0},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{-0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5},
            new double[]{ 0.5,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  0.5},
            new double[]{ 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0},
        };

        static double[][] evalQueen =
        {
            new double[]{-2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0},
            new double[]{-1.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -1.0},
            new double[]{-1.0,  0.0,  0.5,  0.5,  0.5,  0.5,  0.0, -1.0},
            new double[]{-0.5,  0.0,  0.5,  0.5,  0.5,  0.5,  0.0, -0.5},
            new double[]{ 0.0,  0.0,  0.5,  0.5,  0.5,  0.5,  0.0, -0.5},
            new double[]{-1.0,  0.5,  0.5,  0.5,  0.5,  0.5,  0.0, -1.0},
            new double[]{-1.0,  0.0,  0.5,  0.0,  0.0,  0.0,  0.0, -1.0},
            new double[]{-2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0},
        };

        static double[][] kingEvalBlack =
        {
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0},
            new double[]{-1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0},
            new double[]{ 2.0,  2.0,  0.0,  0.0,  0.0,  0.0,  2.0,  2.0},
            new double[]{ 2.0,  3.0,  1.0,  0.0,  0.0,  1.0,  3.0,  2.0},
        };

        static double[][] kingEvalWhite =
        {
            new double[]{ 2.0,  3.0,  1.0,  0.0,  0.0,  1.0,  3.0,  2.0},
            new double[]{ 2.0,  2.0,  0.0,  0.0,  0.0,  0.0,  2.0,  2.0},
            new double[]{-1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0},
            new double[]{-2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
            new double[]{-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
        };


        /// <summary>
        /// 简单AI
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Move GetSimpleNextMove(int color)
        {
            var moves = App.ChessLogic.GetAllMoves(color);
            if(moves.Count == 0)
            {
                return null;
            }
            int maxIndex = 0;
            double maxValue = -9999;
            int tempIndex = 0;
            foreach(var move in moves)
            {
                var tempValue = CalValueAfterMove(move);
                if(tempValue > maxValue)
                {
                    maxValue = tempValue;
                    maxIndex = tempIndex;
                }
                else if(tempValue == maxValue)
                {
                    System.Random ran = new System.Random();
                    int key = ran.Next(0, 10);
                    if (key > 5)
                    {
                        maxValue = tempValue;
                        maxIndex = tempIndex;
                    }
                }
                tempIndex++;
            }
            //System.Random ran = new System.Random();
            //int key = ran.Next(1, moves.Count);
            return moves[maxIndex];
        }

        /// <summary>
        /// 中等AI
        /// </summary>
        /// <param name="color">AI的颜色</param>
        /// <returns></returns>
        public Move GetNormalNextMove(int color)
        {
            int otherColor = 1 - color; //玩家的颜色
            int depth = 2;              //搜索树的深度
            var moves = App.ChessLogic.GetAllMoves(color);
            if(moves.Count == 0)
            {
                return null;
            }
            MoveSearchHelper root = new MoveSearchHelper(); //根节点
            root.board = App.ChessLogic.CopyBoard();        //原始盘面
            root.nextColor = color;
            var alpha = -9999;
            var beta = 9999;
            var bestSearch = SearchBestMove(depth, alpha, beta, root, true);
            
            return FindBestMove(bestSearch);
        }

        /// <summary>
        /// 困难AI
        /// </summary>
        /// <param name="color">AI的颜色</param>
        /// <returns></returns>
        public Move GetHardNextMove(int color)
        {
            int otherColor = 1 - color; //玩家的颜色
            int depth = 4;              //搜索树的深度

            MoveSearchHelper root = new MoveSearchHelper(); //根节点
            root.board = App.ChessLogic.CopyBoard();        //原始盘面
            root.nextColor = color;
            var alpha = -9999;
            var beta = 9999;
            var bestSearch = SearchBestMove(depth, alpha, beta, root, true);

            return FindBestMove(bestSearch);
        }

        private MoveSearchHelper SearchBestMove(int depth, double alpha, double beta, MoveSearchHelper root, bool isEval = false)
        {
            if(depth == 0)
            {
                root.value = CalBoardStaticValue(root.nextColor, root.board, isEval);
                return root;
            }

            var moves = App.ChessLogic.GetBoardAllMoves(root.board, root.nextColor);//可走位置
            var nextColor = 1 - root.nextColor;

            var bestSearch = new MoveSearchHelper();
            bestSearch.value = -9999;
            bestSearch.move = null;
            //白色取最小值
            if(root.nextColor == 0)
            {
                foreach (var move in moves)
                {
                    var nextLeaf = new MoveSearchHelper();
                    var nextBoard = GetBoardAfterMove(move, root.board);//移动完的盘面
                    nextLeaf.board = nextBoard;
                    nextLeaf.nextColor = nextColor;
                    nextLeaf.move = move;
                    nextLeaf.father = root;

                    var moveSearch = SearchBestMove(depth - 1, alpha, beta, nextLeaf, true);
                    if(bestSearch.move == null)
                    {
                        bestSearch = moveSearch;
                    }
                    if (moveSearch.value < bestSearch.value)
                    {
                        bestSearch = moveSearch;
                    }
                    //else if(moveSearch.value == bestSearch.value)
                    //{
                    //    //如果相等 而且不为空 随机选一个
                    //    System.Random ran = new System.Random();
                    //    int key = ran.Next(0, 10);
                    //    Debug.Log("White Key:============" + key);
                    //    if (key > 5)
                    //    {
                    //        bestSearch = moveSearch;
                    //    }
                    //}

                    //beta = bestSearch.value;
                    //if (beta <= alpha)
                    //{
                    //    return bestSearch;
                    //}
                }
            }
            //黑色取最大值
            else if(root.nextColor == 1)
            {
                foreach (var move in moves)
                {
                    var nextLeaf = new MoveSearchHelper();
                    var nextBoard = GetBoardAfterMove(move, root.board);//移动完的盘面
                    nextLeaf.board = nextBoard;
                    nextLeaf.nextColor = nextColor;
                    nextLeaf.move = move;
                    nextLeaf.father = root;

                    var moveSearch = SearchBestMove(depth - 1, alpha, beta, nextLeaf, true);
                    if (bestSearch.move == null)
                    {
                        bestSearch = moveSearch;
                    }
                    if (moveSearch.value > bestSearch.value)
                    {
                        bestSearch = moveSearch;
                    }
                    //else if (moveSearch.value == bestSearch.value)
                    //{
                    //    //如果相等 而且不为空 随机选一个
                    //    System.Random ran = new System.Random();
                    //    int key = ran.Next(0, 10);
                    //    Debug.Log("Key:============" + key);
                    //    if (key > 5)
                    //    {
                    //        bestSearch = moveSearch;
                    //    }
                    //}
                    //alpha = bestSearch.value;
                    //if (beta <= alpha)
                    //{
                    //    return bestSearch;
                    //}
                }
            }
            return bestSearch;
        }

        private Move FindBestMove(MoveSearchHelper moveSearch)
        {
            var tempSearch = moveSearch;
            var search = moveSearch;
            while (tempSearch.father != null)
            {
                search = tempSearch;
                tempSearch = tempSearch.father;
            }
            return search.move;
        }

        private double CalValueAfterMove(Move move)
        {
            int[,] tempBoard = App.ChessLogic.CopyBoard();
            var fromY = (int)move.From.y;
            var fromX = (int)move.From.x;
            var piece = tempBoard[fromY, fromX];

            var toY = (int)move.To.y;
            var toX = (int)move.To.x;

            var selfColor = piece / 10;
            tempBoard[fromY, fromX] = -1;
            tempBoard[toY, toX] = piece;

            var totalValue = CalBoardValue(selfColor, tempBoard);

            return totalValue;
        }

        /// <summary>
        /// 评估盘面局势
        /// </summary>
        /// <returns></returns>
        private double CalBoardValue(int selfColor, int[,] board, bool isEval = false)
        {
            double totalValue = 0;
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    var piece = board[y,x];
                    if (piece == -1) continue;
                    var pieceColor = piece / 10;
                    double value = Config.PieceValue[piece % 10];
                    if(isEval == true)
                    {
                        value += GetEval(x, y, piece);
                    }
                    if (pieceColor == selfColor)
                    {
                        totalValue += value;
                    }
                    else
                    {
                        totalValue -= value;
                    }
                }
            }
            return totalValue;
        }

        private double GetEval(int x, int y, int piece)
        {
            var pieceColor = piece / 10;
            var pieceType = piece % 10;
            switch (pieceType)
            {
                case 0:
                    return Config.PieceValue[pieceType] + (pieceColor == 0 ? pawnEvalWhite[y][x] : pawnEvalBlack[y][x]);
                case 1:
                    return Config.PieceValue[pieceType] + (knightEval[y][x]);
                case 2:
                    return Config.PieceValue[pieceType] + (pieceColor == 0 ? bishopEvalWhite[y][x] : bishopEvalBlack[y][x]);
                case 3:
                    return Config.PieceValue[pieceType] + (pieceColor == 0 ? rookEvalWhite[y][x] : rookEvalBlack[y][x]);
                case 4:
                    return Config.PieceValue[pieceType] + (evalQueen[y][x]);
                case 5:
                    return Config.PieceValue[pieceType] + (pieceColor == 0 ? kingEvalWhite[y][x] : kingEvalBlack[y][x]);
            }
            return 0;
        }

        /// <summary>
        /// 评估盘面局势
        /// </summary>
        /// <returns></returns>
        private double CalBoardStaticValue(int selfColor, int[,] board, bool isEval = false)
        {
            double totalValue = 0;
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    var piece = board[y, x];
                    if (piece == -1) continue;
                    var pieceColor = piece / 10;
                    double value = Config.PieceValue[piece % 10];
                    if (isEval == true)
                    {
                        value += GetEval(x, y, piece);
                    }
                    if (pieceColor == 1)//黑加白减
                    {
                        totalValue += value;
                    }
                    else
                    {
                        totalValue -= value;
                    }
                }
            }
            return totalValue;
        }

        private int[,] GetBoardAfterMove(Move move, int[,] board)
        {
            int[,] tempBoard = CopyBoard(board);
            var fromY = (int)move.From.y;
            var fromX = (int)move.From.x;
            var piece = tempBoard[fromY, fromX];

            var toY = (int)move.To.y;
            var toX = (int)move.To.x;

            var selfColor = piece / 10;
            tempBoard[fromY, fromX] = -1;
            tempBoard[toY, toX] = piece;

            return tempBoard;
        }

        private List<Move> GetNextMovesAfterMove(Move move, int color)
        {
            int[,] board = App.ChessLogic.CopyBoard();
            var tempBoard = GetBoardAfterMove(move, board);
            var nextMoves = App.ChessLogic.GetBoardAllMoves(tempBoard, color);
            return nextMoves;
        }

        public int[,] CopyBoard(int[,] board)
        {
            int[,] arr = new int[Config.Board.MaxY, Config.Board.MaxY];
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    arr[y, x] = board[y, x];
                }
            }
            return arr;
        }
    }

    public class Move
    {
        private Vector2 m_from;
        private Vector2 m_to;
        private int m_distance;

        public Move(Vector2 from, Vector2 to)
        {
            m_from = from;
            m_to = to;
        }

        public int Distance
        {
            get
            {
                return m_distance;
            }
            set
            {
                m_distance = value;
            }
        }

        public Vector2 From
        {
            get
            {
                return m_from;
            }
            set
            {
                m_from = value;
            }
        }

        public Vector2 To
        {
            get
            {
                return m_to;
            }
            set
            {
                m_to = value;
            }
        }
    }

    public class MoveSearchHelper
    {
        public int[,] board;
        public int alpha;
        public int beta;
        public double value;
        public int nextColor;
        public Move move;
        public List<MoveSearchHelper> sons;
        public MoveSearchHelper father;
    }

}
