﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class RPG
    {
        public static string getCILType(Piece.Type Type)
        {
            switch (Type)
            {
                case Piece.Type.Int:
                    return "int32";
                case Piece.Type.Packed:
                    return "float64";
                case Piece.Type.Varchar:
                    return "string";
                case Piece.Type.Indicator:
                    return "bool";
            }
            return "void";
        }
    }
}
