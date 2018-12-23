using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework
{
    public class ChessAI
    {
        /// <summary>
        /// 简单AI
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Move GetSimpleNextMove(int color)
        {
            var moves = App.ChessLogic.GetAllMoves(color);
            int maxIndex = 0;
            int maxValue = 0;
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

        private int CalValueAfterMove(Move move)
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
        private int CalBoardValue(int selfColor, int[,] board)
        {
            int totalValue = 0;
            for (int y = 0; y < Config.Board.MaxY; y++)
            {
                for (int x = 0; x < Config.Board.MaxX; x++)
                {
                    var piece = board[y,x];
                    if (piece == -1) continue;
                    var pieceColor = piece / 10;
                    int value = Config.PieceValue[piece % 10];
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
    }

    public class Move
    {
        private Vector2 m_from;
        private Vector2 m_to;

        public Move(Vector2 from, Vector2 to)
        {
            m_from = from;
            m_to = to;
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
}
