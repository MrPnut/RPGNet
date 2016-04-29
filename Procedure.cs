using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Procedure
    {
        private int MaxStack;
        private String ProcName;
        private Dictionary<String, Piece.Type> Parameters;
        private Dictionary<String, String> DataStructures;
        private Dictionary<String, Variable> Variables;
        private Piece.Type ReturnType;
        private List<String> ILCode;

        public Procedure(String Name)
        {
            MaxStack = 0;
            ProcName = Name;
            Parameters = new Dictionary<String, Piece.Type>();
            DataStructures = new Dictionary<string, string>();
            Variables = new Dictionary<String, Variable>();
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
        public Piece.Type getReturn()
        {
            return ReturnType;
        }

        public void addParam(String Name, Piece.Type Type)
        {
            if (Parameters.ContainsKey(Name))
            {
                Errors.throwError("Trying to add paramater " + Name + " to " + getName() + " which has already be defined.");
            }
            else
            {
                Parameters.Add(Name, Type);
            }
        }
        public Piece.Type[] getParams()
        {
            List<Piece.Type> Out = new List<Piece.Type>();
            foreach (var Param in Parameters)
            {
                Out.Add(Param.Value);
            }
            return Out.ToArray();
        }
        public void addDS(String Name, String DSTemp)
        {
            DataStructures.Add(Name, DSTemp);
        }
        public void addVariable(String Name, Piece.Type Type, int Dim = 0)
        {
            if (Variables.ContainsKey(Name))
            {
                Errors.throwError("Trying to redefine variable " + Name + ".");
            }
            else
            {
                Variables.Add(Name, new Variable(Type, Dim));
            }
        }
        public Piece.Type getVarType(String Name) {
            if (Variables.ContainsKey(Name))
            {
                return Variables[Name].getType();
            }
            else if (Module.globalExists(Name))
            {
                return Module.getGlobalType(Name);
            }
            else
            {
                Errors.throwError("Trying to find type of an unknown variable: " + Name);
                return Piece.Type.Void;
            }
        }
        public int getVarDim(String Name)
        {
            if (Variables.ContainsKey(Name))
            {
                return Variables[Name].getDim();
            }
            else if (Module.globalExists(Name))
            {
                return Module.getGlobalDim(Name);
            }
            else
            {
                Errors.throwError("Trying to find dim of an unknown variable: " + Name);
                return 0;
            }
        }

        public void addIL(String IL)
        {
            ILCode.Add(IL);
        }
        public void addNot()
        {
            addIL("ldc.i4.0");
            addIL("ceq");
        }
        public void addGoto(String GOTO = "")
        {
            if (GOTO != "")
            {
                addIL(GOTO + ":");
            }
            else
            {
                addIL(Module.getScope() + ":");
            }
        }

        public String[] getIL()
        {
            List<String> Out = new List<String>();
            List<String> Vars = new List<string>();
            Boolean isArray = false;
            Out.Add("");
            Out.Add("");
            Out.Add("//************************************************************************");
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
            Out.Add(".maxstack " + MaxStack.ToString());
            Out.Add(".locals init (");
            foreach (var Varu in Variables)
            {
                isArray = (Varu.Value.getDim() > 0);
                Vars.Add(RPG.getCILType(Varu.Value.getType()) + (isArray ? "[]" : "") + " " + Varu.Key);
            }
            foreach (var DS in DataStructures)
            {
                Vars.Add("valuetype " + Module.getName() + ".Program/" + DS.Value + " " + DS.Key);
            }
            Out.Add(String.Join(", ", Vars));
            Out.Add(")");

            Out.AddRange(ILCode);
            Out.Add("}");
            Out.Add("//************************************************************************");

            return Out.ToArray();
        }

        public void Expression(Piece[] In, Piece.Type TypeOut = Piece.Type.Void)
        {
            Boolean NOT = false;
            String OP = "";

            MaxStack = Math.Max(In.Length, MaxStack);
            foreach (Piece Token in In)
            {
                switch (Token.getValue())
                {
                    case "NOT":
                        NOT = !NOT;
                        break;
                    case "=":
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
                        loadItem(Token);
                        switch (OP)
                        {
                            case "=":
                                addIL("ceq");
                                break;
                            case "<>":
                                addIL("ceq");
                                addNot();
                                break;
                            case ">":
                                addIL("cgt");
                                break;
                            case ">=":
                                addIL("clt");
                                addNot();
                                break;
                            case "<":
                                addIL("clt");
                                break;
                            case "<=":
                                addIL("cgt");
                                addNot();
                                break;
                            case "+":
                                if (TypeOut == Piece.Type.Varchar)
                                {
                                    addIL("call string [mscorlib]System.String::Concat(string, string)");
                                }
                                else
                                {
                                    addIL("add");
                                }
                                break;
                            case "-":
                                addIL("sub");
                                break;
                            case "*":
                                addIL("mul");
                                break;
                            case "/":
                                addIL("div");
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
            if (NOT) addNot();
        }
        public void doBIF(String Name, String InsideBrackets)
        {
            List<Piece> Pieces = new List<Piece>();
            foreach (String Parm in InsideBrackets.Split(':'))
            {
                if (Parm.Trim() != "") Pieces.Add(new Piece(Parm));
            }

            MaxStack = Math.Max(Pieces.Count, MaxStack);

            addIL("// " + Name + "(" + InsideBrackets + ") ---------");
            Name = Name.Substring(1); //Rid of the %
            switch (Name.ToUpper())
            {
                case "CHAR":
                    loadItem(Pieces[0]);
                    if (Pieces[0].getValue().EndsWith(")")) Pieces[0] = new Piece(Interpreter.parseCall(Pieces[0].getValue())[0]);
                    switch (Pieces[0].getInstance())
                    {
                        case Piece.Type.Variable:
                            addIL("call string [mscorlib]System.Convert::ToString(" + RPG.getCILType(getVarType(Pieces[0].getValue())) + ")");
                            break;
                        default:
                            addIL("call string [mscorlib]System.Convert::ToString(" + RPG.getCILType(Pieces[0].getInstance()) + ")");
                            break;
                    }
                    break;
                case "INT":
                    loadItem(Pieces[0]);
                    if (Pieces[0].getValue().EndsWith(")")) Pieces[0] = new Piece(Interpreter.parseCall(Pieces[0].getValue())[0]);
                    switch (Pieces[0].getInstance())
                    {
                        case Piece.Type.Variable:
                            addIL("call int32 [mscorlib]System.Convert::ToInt32(" + RPG.getCILType(getVarType(Pieces[0].getValue())) + ")");
                            break;
                        default:
                            addIL("call int32 [mscorlib]System.Convert::ToInt32(string)");
                            break;
                    }
                    break;
                case "DEC": //Pass in string
                    loadItem(Pieces[0]);
                    addIL("call float64 [mscorlib]System.Convert::ToDouble(string)");
                    break;
                case "LEN": //Pass in string
                    loadItem(Pieces[0]);
                    addIL("callvirt instance int32 [mscorlib]System.String::get_Length()");
                    break;
                case "SCAN": //%Scan(' ':String)
                    loadItem(Pieces[1]);
                    loadItem(Pieces[0]);
                    addIL("callvirt instance int32 [mscorlib]System.String::IndexOf(string)");
                    loadItem(new Piece("1"));
                    addIL("add");
                    break;
                case "TRIM": //Trim(string);
                    loadItem(Pieces[0]);
                    addIL("callvirt instance string [mscorlib]System.String::Trim()");
                    break;
                case "TRIML": //TrimL(string);
                    //TrimStart in mscorlib requires a character array, so we create a system.char array
                    loadItem(Pieces[0]);
                    loadItem(new Piece("0"));
                    addIL("newarr [mscorlib]System.Char");
                    addIL("callvirt instance string [mscorlib]System.String::TrimStart(char[])");
                    break;
                case "TRIMR": //TrimL(string);
                    //same as TrimEnd, so we create a system.char array
                    loadItem(Pieces[0]);
                    loadItem(new Piece("0"));
                    addIL("newarr [mscorlib]System.Char");
                    addIL("callvirt instance string [mscorlib]System.String::TrimEnd(char[])");
                    break;
                case "SUBST": //Subst(Var, startindex, length)
                    loadItem(Pieces[0]);

                    loadItem(Pieces[1]);
                    loadItem(new Piece("1"));
                    addIL("sub"); //We must take one from the second param to match RPGs arrays (start from 1)

                    loadItem(Pieces[2]);
                    addIL("callvirt instance string [mscorlib]System.String::Substring(int32, int32)");
                    break;
                case "THIS":
                    loadItem(new Piece("'" + getName() + "'"));
                    break;
                default:
                    Errors.throwError("Calling unknown built-in function: " + Name);
                    break;
            }
            addIL("// ---------------");
        }
        public void callProc(String Name, String InsideBrackets)  
        {
            Procedure Calling = Module.getProcedure(Name);

            if (Variables.ContainsKey(Name)) //Array check
            {
                MaxStack = Math.Max(1, MaxStack);
                if (Variables[Name].getDim() > 0)
                {
                    loadItem(new Piece(Name));
                    loadItem(new Piece(InsideBrackets));
                    loadItem(new Piece("1")); addIL("sub"); //For RPGLE indexs - 1
                    addIL("ld" + RPG.getCILArray(getVarType(Name)));
                }
                else
                {
                    //Error?
                    Errors.throwError("Tried loading element from array that doesn't exist: " + InsideBrackets);
                }
            }
            else
            {
                String ReturnCIL = RPG.getCILType(Piece.Type.Void);
                List<Piece> PassedInPieces = new List<Piece>();
                String[] PassedIn = InsideBrackets.Split(':');
                if (Calling == null)
                {
                    Errors.throwNotice("Calling " + Name + " but it hasn't been defined yet. Return is void automatically.");
                }
                else
                {
                    ReturnCIL = RPG.getCILType(Calling.getReturn());
                }
                foreach (String Parm in PassedIn)
                {
                    if (Parm != "")
                    {
                        PassedInPieces.Add(new Piece(Parm));
                        loadItem(new Piece(Parm));
                    }
                }
                MaxStack = Math.Max(PassedIn.Length, MaxStack);

                addIL("call " + ReturnCIL + " " + Module.getName() + ".Program::" + Name + " (");
                List<String> Params = new List<String>();
                foreach (Piece Param in PassedInPieces.ToArray())
                {
                    Params.Add(RPG.getCILType(Param.getInstance()));
                }
                addIL(String.Join(", ", Params));
                addIL(")");
            }
        }
        public void storeItem(String Var) //Will accept globals
        {
            String[] forVar;
            if (Module.globalExists(Var))
            {
                addIL("stsfld " + Module.getGlobalTypeCIL(Var) + " " + Module.getName() + ".Program::" + Var);
            }
            else if (Variables.ContainsKey(Var))
            {
                addIL("stloc " + Var);
            }
            else if (Var.Contains('.')) //DS
            {
                forVar = Var.Split('.'); 
            }
            else if (Var.Contains("(") && Var.Contains(")"))
            {
                forVar = Interpreter.parseCall(Var);
            }
            else
            {
                Errors.throwError("Trying to store in an unknown variable: " + Var);
            }
        }
        public String loadItem(Piece Item)
        {
            String[] forCall;
            switch (Item.getInstance())
            {
                case Piece.Type.Indicator:
                    switch (Item.getValue().ToUpper())
                    {
                        case "*ON":
                            addIL("ldc.i4 1");
                            break;
                        case "*OFF":
                            addIL("ldc.i4 0");
                            break;
                    }
                    break;
                case Piece.Type.Int:
                    addIL("ldc.i4 " + Item.getValue());
                    break;
                case Piece.Type.Packed:
                    addIL("ldc.r8 " + Item.getValue());
                    break;
                case Piece.Type.Varchar:
                    addIL("ldstr " + Item.getValue());
                    break;
                case Piece.Type.DataStructure:
                    forCall = Item.getValue().Split('.');
                    addIL("ldloc " + forCall[0]);
                    addIL("ldfld " + Module.getDSFieldType(DataStructures[forCall[0]], forCall[1]) + " " + Module.getName() + ".Program/" + DataStructures[forCall[0]] + "::" + forCall[1]);
                    break;
                case Piece.Type.Variable:
                    if (Parameters.ContainsKey(Item.getValue()))
                    {
                        addIL("ldarg " + Item.getValue());
                    }
                    else if (DataStructures.ContainsKey(Item.getValue()))
                    {
                        addIL("ldloca " + Item.getValue());
                    }
                    else if (Variables.ContainsKey(Item.getValue()))
                    {
                        addIL("ldloc " + Item.getValue());
                    }
                    else if (Module.globalExists(Item.getValue()))
                    {
                        addIL("ldsfld " + Module.getGlobalTypeCIL(Item.getValue()) + " " + Module.getName() + ".Program::" + Item.getValue());
                    }
                    else
                    {
                        Errors.throwNotice("Trying to find an unknown variable: " + Item.getValue() + ".");
                    }
                    break;
                case Piece.Type.Call: //Will pass in Procedure(etc)
                    forCall = Interpreter.parseCall(Item.getValue());
                    callProc(forCall[0], forCall[1]);
                    break;
                case Piece.Type.BIF:
                    forCall = Interpreter.parseCall(Item.getValue());

                    doBIF(forCall[0], forCall[1]);
                    break;
            }
            return "";
        }
    }
}
