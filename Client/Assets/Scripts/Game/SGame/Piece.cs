using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Piece
{
    public Config.PieceColor color;
    public Config.PieceType type;
    public int x;
    public int y;

    public Piece(Config.PieceColor color, Config.PieceType type, int x, int y)
    {
        this.color = color;
        this.type = type;
        this.x = x;
        this.y = y;
    }
}

