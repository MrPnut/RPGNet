﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Piece
    {
        public static String[] Operators = { "+", "-", "/", "*", "=", "<>", ">", "<", ">=", "<=" };
        public enum Type
        {
            Operator, BIF, Procedure, Variable, Varchar, Packed, Int, Void
        }
        public static Type getType(String In)
        {
            switch (In.ToUpper())
            {
                case "VARCHAR":
                    return Type.Varchar;
                case "CHAR":
                    return Type.Varchar;
                case "INT":
                    return Type.Int;
                case "PACKED":
                    return Type.Packed;
            }
            return Type.Void;
        }

        private String _Value;
        private Type _Type;

        public Piece(String Val)
        {
            Val = Val.Trim();
            if (Val.EndsWith("'") && Val.StartsWith("'"))
            {
                _Type = Type.Varchar;
                Val = Val.Substring(1);
                Val = Val.Substring(0, Val.Length - 1);
                Val = '"' + Val + '"';
                Val = Val.Replace("''", "'");
            }
            else if (Val.StartsWith("%") && Val.EndsWith(")"))
            {
                _Type = Type.BIF;
            }
            else if (Operators.Contains(Val))
            {
                _Type = Type.Operator;
            }
            else
            {
                Boolean isVar = false;
                Boolean isInt = false;
                Boolean isPkd = false;
                foreach (char c in Val.ToCharArray())
                {
                    if (Char.IsDigit(c))
                    {
                        isInt = true;
                        continue;
                    }
                    if (c == '.')
                    {
                        isPkd = true;
                        continue;
                    }
                    if (Char.IsLetter(c))
                    {
                        isVar = true;
                        continue;
                    }
                }
                if (isVar == true)
                {
                    if (Val.EndsWith(")"))
                    {
                        _Type = Type.Procedure;
                    }
                    else
                    {
                        _Type = Type.Variable;
                    }
                }
                else if (isInt == true)
                {
                    if (isPkd == true)
                    {
                        _Type = Type.Packed;
                    }
                    else
                    {
                        _Type = Type.Int;
                    }
                }
            }
            _Value = Val;
        }

        public String getValue()
        {
            return _Value;
        }
        public Type getInstance()
        {
            return _Type;
        }
    }
}