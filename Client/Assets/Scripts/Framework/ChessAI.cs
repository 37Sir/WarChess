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
            System.Random ran = new System.Random();
            int key = ran.Next(1, moves.Count);
            return moves[key];
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
