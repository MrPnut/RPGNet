using System;
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
                case Piece.Type.Void:
                    return "void";
                case Piece.Type.Error:
                    return "class [mscorlib]System.Exception";
                default:
                    throw new Exception("shit");
                    Errors.throwError("Found an unknown type for CIL: " + Type.ToString());
                    return "void";
            }
        }

        public static string getCILTypeClass(Piece.Type Type) 
        {
            switch (Type)
            {
                case Piece.Type.Int:
                    return "[mscorlib]System.Int32";
                case Piece.Type.Packed:
                    return "[mscorlib]System.Double";
                case Piece.Type.Varchar:
                    return "[mscorlib]System.String";
                case Piece.Type.Indicator:
                    return "[mscorlib]System.Boolean";
                case Piece.Type.Void:
                    Errors.throwError("Unable to have a void array.");
                    return "void";
                default:
                    Errors.throwNotice("Found an unknown type for arrays: " + Type.ToString());
                    return "void";
            }
        }

        public static string getCILArray(Piece.Type Type)
        {
            switch (Type)
            {
                case Piece.Type.Int:
                    return "elem.i4";
                case Piece.Type.Packed:
                    return "elem.r8";
                case Piece.Type.Varchar:
                    return "elem.ref";
                case Piece.Type.Indicator:
                    return "elem.i1";
                case Piece.Type.Void:
                    Errors.throwError("Unable to have a void array.");
                    return "void";
                default:
                    Errors.throwNotice("Found an unknown type for arrays: " + Type.ToString());
                    return "void";
            }
        }
    }
}
