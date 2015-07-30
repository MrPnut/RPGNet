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
        public static String getName()
        {
            return Program_Name;
        }

        #region Procedure methods
        private static Dictionary<String, Procedure> Procedures = new Dictionary<String, Procedure>();
        public static void addProcedure(String Name, Procedure Proc) {
            if (Procedures.ContainsKey(Name))
            {
                Procedures[Name] = Proc;
                Errors.throwNotice("Procedure replaced (using my procedure definition): " + Name);
            }
            else
            {
                Procedures.Add(Name, Proc);
                Errors.throwNotice("Procedure added: " + Name);
            }
        }
        public static Procedure getProcedure(String Name)
        {
            if (Procedures.ContainsKey(Name))
            {
                return Procedures[Name];
            }
            else
            {
                //Error
                Errors.throwNotice("Trying to load unknown procedure: " + Name);
                return null;
            }
        }
        #endregion

        #region Global methods
        private static Dictionary<String, Variable> Globals = new Dictionary<String, Variable>();
        public static Boolean globalExists(String Name)
        {
            return Globals.ContainsKey(Name);
        }
        public static string getGlobalTypeCIL(String Name)
        {
            return RPG.getCILType(Globals[Name].getType());
        }
        public static Piece.Type getGlobalType(String Name)
        {
            return Globals[Name].getType();
        }
        public static void addGlobal(String Name, Piece.Type Type, int Dim = 0)
        {
            Globals.Add(Name, new Variable(Type, Dim));
        }
        #endregion

        #region Label methods
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
        #endregion

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
                    case "DCL-PR": //DCL-PR Procname ReturnType
                        Proc = new Procedure(Pieces[1].getValue());
                        Proc.setReturn(Piece.getType(Pieces[2].getValue()));
                        break;
                    case "END-PR":
                        addProcedure(Proc.getName(), Proc);
                        Proc = null;
                        break;

                    case "DCL-PROC": //Dcl-Proc Name
                        Proc = new Procedure(Pieces[1].getValue());
                        break;
                    case "DCL-PI": //DCL-PI NAME RETURNTYPE
                        Proc.setReturn(Piece.getType(Pieces[2].getValue()));
                        break;
                    case "DCL-PARM": //DCL-PARM NAME, VALUE
                        //Also used for PR
                        Proc.addParam(Pieces[1].getValue(), Piece.getType(Pieces[2].getValue()));
                        break;
                    case "END-PI":
                        //Has no real use :(
                        break;
                    case "DCL-S": //DCL-S NAME TYPE
                        if (Proc != null)
                        {
                            Proc.addVariable(Pieces[1].getValue(), Piece.getType(Pieces[2].getValue()));
                        }
                        else
                        {
                            addGlobal(Pieces[1].getValue(), Piece.getType(Pieces[2].getValue()));
                        }
                        break;
                    case "RETURN":
                        Build = Interpreter.StringBuilder(Pieces, 1, Pieces.Length);
                        Proc.Expression(Build, Proc.getReturn());
                        Proc.addIL("ret");
                        break;
                    case "END-PROC":
                        addProcedure(Proc.getName(), Proc);
                        Proc = null;
                        break;

                    case "DSPLY":
                        Build = Interpreter.StringBuilder(Pieces, 1, Pieces.Length);
                        Proc.Expression(Build, Piece.Type.Varchar);
                        Proc.addIL("call void [mscorlib]System.Console::WriteLine(string)");
                        break;
                    case "WAIT":
                        Proc.addIL("call string [mscorlib]System.Console::ReadLine()");
                        Proc.addIL("pop");
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
                        Proc.addGoto(getLastScope());
                        break;

                    case "DOW":
                        Build = Interpreter.StringBuilder(Pieces, 1, Pieces.Length);

                        Proc.addGoto(getScope());
                        Labels.Add(getScope());
                        Scope++;

                        Proc.Expression(Build);
                        Proc.addIL("brfalse.s " + getScope());
                        break;
                    case "ENDDO":
                        Proc.addIL("br.s " + getLastScope());
                        Proc.addGoto(getScope());
                        Labels.Add(getScope());
                        Scope++;
                        break;

                    case "CALLP":
                        Proc.loadItem(Pieces[1]);
                        break;

                    case "MONITOR":
                        Proc.addIL(".try");
                        Proc.addIL("{");

                        break;
                    case "ON-ERROR":
                        Labels.Add(getScope());
                        Proc.addIL("leave.s " + getScope());
                        Scope++;
                        Proc.addIL("}");

                        Proc.addVariable("E", Piece.Type.Error);
                        Proc.addIL("catch [mscorlib]System.Exception");
                        Proc.addIL("{");
                        Proc.storeItem("E");
                        break;
                    case "ENDMON":
                        Labels.Add(getScope());
                        Proc.addIL("leave.s " + getScope());
                        Scope++;
                        Proc.addIL("}");
                        Proc.addGoto(getLastScope());
                        Proc.addGoto(getLastScope());
                        Proc.addIL("nop");
                        break;

                    default:
                        if (Pieces.Length > 1)
                        {
                            if (Pieces[1].getInstance() != Piece.Type.Operator) continue;
                            if (Pieces[0].getInstance() != Piece.Type.Variable) continue;
                            Proc.Expression(Interpreter.StringBuilder(Pieces, 2, Pieces.Length), Proc.getVarType(Pieces[0].getValue()));
                            Proc.storeItem(Pieces[0].getValue());
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
            foreach (var Var in Globals)
            {
                Out.Add(".field private static " + RPG.getCILType(Var.Value.getType()) + " " + Var.Key);
            }
            foreach (var Proc in Procedures)
            {
                Out.AddRange(Proc.Value.getIL());
            }
            Out.Add("}");

            return Out.ToArray();
        }

    }
}
