using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Piece
    {
        public static String[] Operators = { "+", "-", "/", "*", "=", "<>", ">", "<", ">=", "<=", "NOT" };
        public enum Type
        {
            Operator, BIF, Call, Variable, Varchar, Packed, Int, Indicator, Error, Void
            /*
             * Piece.Type descriptions
             *  - Operator can be any value from the Operators String array
             *  - A BIF is a piece that starts with % and ends with ) which will do a 'lower level' call
             *  - A call is currently a piece that ends with ) which calls procedures.. will eventually support arrays
             *  - Variable is a piece that indicates that the value may represent a variable
             *  - Varchar is the type of the value passed in
             *  - Int is the type of the value passed in
             *  - Indicator is the type of value passed in
             *  - Error is used for the Exceptione type in MSIL
             *  - Void represents no return for procedures.
             */
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
                case "IND":
                    return Type.Indicator;
                default:
                    Errors.throwNotice("Unknown RPG type: " + In);
                    return Type.Void;
            }
        }

        private String _Value;
        private Type _Type;

        public Piece(String Val)
        {
            Val = Val.Trim();
            Val = Val.Replace("$s", " ");
            if (Val.EndsWith("'") && Val.StartsWith("'"))
            {
                _Type = Type.Varchar;

                //Rid of the quote marks
                Val = Val.Substring(1);
                Val = Val.Substring(0, Val.Length - 1);

                Val = Val.Replace("\\", "\\\\");
                Val = Val.Replace("\"", "\\\"");
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
                        _Type = Type.Call;
                    }
                    else if (Val.StartsWith("*"))
                    {
                        _Type = Type.Indicator;
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
                else
                {
                    Errors.throwError("Unknown piece found: " + Val);
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
