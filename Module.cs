using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Module
    {
        private static String Program_Name = "";
        private static Dictionary<String, Procedure> Procedures = new Dictionary<String, Procedure>();
        public static void addProcedure(String Name, Procedure Proc) {
            Procedures.Add(Name, Proc);
        }
        public static String getName()
        {
            return Program_Name;
        }

        public static List<String> Labels = new List<String>();
        private static int Scope = 0;
        public static String getScope()
        {
            return "SCOPE" + Scope.ToString();
        }
        public static String getLastScope()
        {
            String Out = Labels[Labels.Count - 1];
            Labels.RemoveAt(Labels.Count - 1);
            return Out;
        }
        
        public static void Run(String Name, String Code)
        {
            Program_Name = Name; 
            Procedure Proc = null;
            Piece[] Pieces;
            Piece[] Build;
            String forElse;
            foreach (String Part in Interpreter.toParts(Code))
            {
                Pieces = Interpreter.getPieces(Part);
                if (Proc != null) Proc.addIL("");
                if (Proc != null) Proc.addIL("//" + Part);
                switch (Pieces[0].getValue().ToUpper())
                {
                    case "DCL-PROC": //Dcl-Proc Name
                        Proc = new Procedure(Pieces[1].getValue());
                        break;
                    case "DCL-PI": //DCL-PI NAME RETURNTYPE
                        Proc.setReturn(Piece.getType(Pieces[2].getValue()));
                        break;
                    case "DCL-PARM": //DCL-PARM NAME, VALUE
                        Proc.addParam(Pieces[1].getValue(), Piece.getType(Pieces[2].getValue()));
                        break;
                    case "END-PI":
                        break;
                    case "DCL-S": //DCL-S NAME TYPE
                        Proc.addVariable(Pieces[1].getValue(), Piece.getType(Pieces[2].getValue()));
                        break;
                    case "END-PROC":
                        addProcedure(Proc.getName(), Proc);
                        Proc = null;
                        break;

                    case "DSPLY":
                        Proc.loadItem(Pieces[1]);
                        Proc.addIL("call void [mscorlib]System.Console::WriteLine(string)");
                        break;
                    case "WAIT":
                        Proc.addIL("call string [mscorlib]System.Console::ReadLine()");
                        break;

                    case "SELECT":
                        Labels.Add(getScope());
                        Scope++;
                        break;
                    case "WHEN":
                        forElse = getLastScope();

                        Labels.Add(getScope());
                        Build = Interpreter.StringBuilder(Pieces, 1, Pieces.Length);
                        Proc.Expression(Build);
                        Proc.addIL("brfalse.s " + getScope()); Scope++;

                        Proc.addGoto(forElse);
                        break;
                    case "ENDSEL":
                        Proc.addGoto(getLastScope()); Scope++;
                        break;
                    case "IF":
                        Build = Interpreter.StringBuilder(Pieces, 1, Pieces.Length);
                        Proc.Expression(Build);

                        Labels.Add(getScope());
                        Proc.addIL("brfalse.s " + getScope());
                        Scope++;
                        break;
                    case "ELSE":
                        forElse = getLastScope();

                        Labels.Add(getScope()); 
                        Proc.addIL("br.s " + getScope()); Scope++;

                        Proc.addGoto(forElse);
                        break;
                    case "ELSEIF":
                        forElse = getLastScope();

                        Labels.Add(getScope());
                        Build = Interpreter.StringBuilder(Pieces, 1, Pieces.Length);
                        Proc.Expression(Build);
                        Proc.addIL("brfalse.s " + getScope()); Scope++;

                        Proc.addGoto(forElse);
                        break;
                    case "ENDIF":
                        Proc.addGoto(getLastScope()); Scope++;
                        break;
                    default:
                        if (Pieces.Length > 1)
                        {
                            if (Pieces[1].getInstance() != Piece.Type.Operator) continue;
                            if (Pieces[0].getInstance() != Piece.Type.Variable) continue;
                            Proc.Expression(Interpreter.StringBuilder(Pieces, 2, Pieces.Length));
                            Proc.addIL("stloc " + Pieces[0].getValue());
                        }
                        break;
                }
            }
        }

        public static String[] getCode()
        {
            List<String> Out = new List<string>();

            Out.Add(".assembly " + Program_Name + " {}");
            Out.Add(".assembly extern mscorlib {}");
            Out.Add(".module " + Program_Name + ".exe");

            Out.Add(".class private auto ansi " + Program_Name + ".Program extends [mscorlib]System.Object {");
            foreach (var Proc in Procedures)
            {
                foreach (String Piece in Proc.Value.getIL())
                {
                    Out.Add(Piece);
                }
            }
            Out.Add("}");

            return Out.ToArray();
        }

    }
}
