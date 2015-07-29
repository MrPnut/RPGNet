﻿using System;
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
            if (Parameters.ContainsKey(Name))
            {
                Errors.throwNotice("Trying to add paramater " + Name + " to " + getName() + " which has already be defined.");
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

        public void addVariable(String Name, Piece.Type Type)
        {
            if (Variables.ContainsKey(Name))
            {
                Errors.throwNotice("Trying to redefine variable " + Name + ".");
            }
            else
            {
                Variables.Add(Name, Type);
            }
        }
        public Piece.Type getVarType(String Name) {
            if (Variables.ContainsKey(Name))
            {
                return Variables[Name];
            }
            else if (Module.globalExists(Name))
            {
                return Module.getGlobalType(Name);
            }
            else
            {
                Errors.throwNotice("Trying to find type of an unknown variable: " + Name);
                return Piece.Type.Void;
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

            Out.AddRange(ILCode);
            Out.Add("}");

            return Out.ToArray();
        }

        public void Expression(Piece[] In, Boolean Varchar = false)
        {
            Boolean NOT = false;
            String OP = "";

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
                                if (Varchar)
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

            addIL("// " + Name + "(" + InsideBrackets + ") ---------");
            Name = Name.Substring(1); //Rid of the %
            switch (Name.ToUpper())
            {
                case "CHAR":
                    loadItem(Pieces[0]);
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
            /*
             * This function will also support arrays.
             * Like so:
             * if (Calling != Null) {
             *   //doCalling
             * }
             * else if (varExists) { //The array
             *   //load array element onto stack
             * } else {
             *   //Call a void procedure (like what Calling == null does now)
             * }
             */
            Procedure Calling = Module.getProcedure(Name);

            if (Calling == null)
            {
                addIL("call void " + Module.getName() + ".Program::" + Name + " ()");
                Errors.throwNotice(Name + " is assumed to exist, as no prototype or procedure is defined yet.");
            }
            else
            {
                foreach (String Parm in InsideBrackets.Split(':'))
                {
                    loadItem(new Piece(Parm));
                }

                addIL("call " + RPG.getCILType(Calling.ReturnType) + " " + Module.getName() + ".Program::" + Name + " (");
                List<String> Params = new List<String>();
                foreach (Piece.Type Param in Calling.getParams())
                {
                    Params.Add(RPG.getCILType(Param));
                }
                addIL(String.Join(", ", Params));
                addIL(")");
            }
        }
        public void storeItem(String Var) //Will accept globals
        {
            if (Module.globalExists(Var))
            {
                addIL("stsfld " + Module.getGlobalTypeCIL(Var) + " " + Module.getName() + ".Program::" + Var);
            }
            else if (Variables.ContainsKey(Var))
            {
                addIL("stloc " + Var);
            }
            else
            {
                Errors.throwError("Trying to store in an unknown variable: " + Var);
            }
        }
        public String loadItem(Piece Item)
        {
            String Value = "", Name = "", InsideBrackets = "";
            int Start, End;
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
                case Piece.Type.Variable:
                    if (Parameters.ContainsKey(Item.getValue()))
                    {
                        addIL("ldarg " + Item.getValue());
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
                    Value = Item.getValue();

                    Start = Value.IndexOf('(') + 1;
                    End = Value.LastIndexOf(')');
                    if (Start < 0 || End < 0 || (Start-1) < 0)
                    {
                        Errors.showDefinedNotice("spacing");
                        Errors.throwError("Trying to call a procedure but seems to be failing: " + Value);
                    }
                    InsideBrackets = Item.getValue().Substring(Start, int.Parse(Math.Abs(Start - End).ToString())).Trim();
                    Name = Value.Substring(0, Start-1).Trim();

                    callProc(Name, InsideBrackets);
                    break;
                case Piece.Type.BIF:
                    Value = Item.getValue();

                    Start = Value.IndexOf('(') + 1;
                    End = Value.LastIndexOf(')');
                    if (Start < 0 || End < 0)
                    {
                        Errors.showDefinedNotice("spacing");
                        Errors.throwError("Trying to call a built-in function but seems to be failing: " + Value);
                    }
                    InsideBrackets = Item.getValue().Substring(Start, int.Parse(Math.Abs(Start - End).ToString())).Trim();
                    Name = Value.Substring(0, Start-1).Trim();

                    doBIF(Name, InsideBrackets);
                    break;
            }
            return "";
        }
    }
}
