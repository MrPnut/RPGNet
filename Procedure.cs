using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Procedure
    {
        private String ProcName;
        private Dictionary<String, Piece.Type> Parameters;
        private Dictionary<String, Piece.Type> Variables;
        private Piece.Type ReturnType;
        private List<String> ILCode;

        public Procedure(String Name)
        {
            ProcName = Name;
            Parameters = new Dictionary<String, Piece.Type>();
            Variables = new Dictionary<String, Piece.Type>();
            ReturnType = Piece.Type.Void;
            ILCode = new List<String>();
        }

        public String getName()
        {
            return ProcName;
        }
        public void setReturn(Piece.Type Type)
        {
            ReturnType = Type;
        }
        public void addParam(String Name, Piece.Type Type)
        {
            Parameters.Add(Name, Type);
        }
        public void addVariable(String Name, Piece.Type Type)
        {
            Variables.Add(Name, Type);
        }
        public Piece.Type getVarType(String Name) {
            if (Variables.ContainsKey(Name))
            {
                return Variables[Name];
            }
            else
            {
                return Piece.Type.Int;
            }
        }
        public void addIL(String IL)
        {
            ILCode.Add(IL);
        }

        public String[] getIL()
        {
            List<String> Out = new List<String>();
            List<String> Vars = new List<string>();

            Out.Add(".method static " + RPG.getCILType(ReturnType) + " " + getName() + " (");
            foreach (var Parm in Parameters)
            {
                Vars.Add(RPG.getCILType(Parm.Value) + " " + Parm.Key);
            }
            Out.Add(String.Join(", ", Vars));
            Out.Add(")");
            Out.Add("{");

            Vars.Clear();
            if (getName() == Module.getName()) Out.Add(".entrypoint");
            Out.Add(".maxstack " + Variables.Count);
            Out.Add(".locals init (");
            foreach (var Variable in Variables)
            {
                Vars.Add(RPG.getCILType(Variable.Value) + " " + Variable.Key);
            }
            Out.Add(String.Join(", ", Vars));
            Out.Add(")");

            foreach (String Piece in ILCode)
            {
                Out.Add(Piece);
            }
            Out.Add("}");

            return Out.ToArray();
        }

        public void Expression(Piece[] In)
        {
            String Out = "";
            String OP = "";

            foreach (Piece Token in In)
            {
                switch (Token.getValue())
                {
                    case "==":
                    case "!=":
                    case ">":
                    case "<":
                    case "<=":
                    case ">=":
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                        OP = Token.getValue();
                        break;
                    default:
                        switch (OP)
                        {
                            case "=":
                                break;
                            case "<>":
                                break;
                            case ">":
                                break;
                            case ">=":
                                break;
                            case "<":
                                break;
                            case "<=":
                                break;
                            case "+":
                                loadItem(Token);
                                if (this.getVarType(Token.getValue()) == Piece.Type.Varchar || Token.getInstance() == Piece.Type.Varchar)
                                {
                                    addIL("call string [mscorlib]System.String::Concat(string, string)");
                                }
                                else
                                {
                                    addIL("add");
                                }
                                break;
                            case "-":
                                loadItem(Token);
                                addIL("sub");
                                break;
                            case "*":
                                loadItem(Token);
                                addIL("mul");
                                break;
                            case "/":
                                loadItem(Token);
                                addIL("div");
                                break;
                            default:
                                loadItem(Token);
                                break;
                        }
                        break;
                }
            }
        }
        public String loadItem(Piece Item)
        {
            switch (Item.getInstance())
            {
                case Piece.Type.Int:
                    addIL("ldc.i4 " + Item.getValue());
                    break;
                case Piece.Type.Packed:
                    addIL("ldc.r8 " + Item.getValue());
                    break;
                case Piece.Type.Varchar:
                    addIL("ldstr " + Item.getValue());
                    break;
                case Piece.Type.Variable:
                    if (Parameters.ContainsKey(Item.getValue()))
                    {
                        addIL("ldarg " + Item.getValue());
                    }
                    else if (Variables.ContainsKey(Item.getValue()))
                    {
                        addIL("ldloc " + Item.getValue());
                    }
                    break;
                case Piece.Type.Procedure:
                    break;
                case Piece.Type.BIF:
                    break;
            }
            return "";
        }
    }
}
