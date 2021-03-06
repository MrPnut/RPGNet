﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RPGNet
{
    class Interpreter
    {
        public static String getContent(String FileLoc)
        {
            List<String> Output = new List<String>();
            String Line = "";
            foreach (String L in File.ReadAllLines(FileLoc))
            {
                Line = L.Trim();
                if (Line.StartsWith("*") || Line.Trim().StartsWith("//"))
                {
                    //Do nothing..
                }
                else if (Line.StartsWith("/COPY"))
                {
                    Line = Line.Substring(5).Trim();
                    Output.Add(getContent(Line));
                    Errors.throwNotice("Copied in " + Line);
                }
                else
                {
                    Output.Add(Line);
                }
            }
            return String.Join(" ", Output);
        } 
        public static String[] toParts(String In)
        {
            List<String> Parts = new List<String>();
            String Current_Part = "";

            Boolean Inside_Speech = false;
            foreach (Char c in In.ToCharArray())
            {
                switch (c)
                {
                    case ';':
                        if (Inside_Speech == true)
                        {
                            Current_Part += c;
                        }
                        else
                        {
                            Parts.Add(Current_Part.Trim());
                            Current_Part = "";
                        }
                        break;
                    default:
                        if (c == '\'')
                        {
                            Inside_Speech = !Inside_Speech;
                        }
                        Current_Part += c;
                        break;
                }
            }
            if (Current_Part.Trim() != "")
            {
                Parts.Add(Current_Part.Trim());
            }
            return Parts.ToArray();
        }
        public static Piece[] getPieces(String Part)
        {
            //This gets the pieces for a statement - not BIFS or procs. 
            //See issue #6
            List<Piece> Pieces = new List<Piece>();
            String Current = "";

            Boolean SPEECH = false;
            int BracketScope = 0;
            foreach (Char c in Part.ToCharArray())
            {
                switch (c)
                {
                    case '\'':
                        SPEECH = !SPEECH;
                        Current += c;
                        break;
                    case '(':
                        BracketScope++;
                        Current += c;
                        break;
                    case ')':
                        BracketScope--;
                        Current += c;
                        break;
                    case ' ':
                        if (SPEECH == false && BracketScope == 0)
                        {
                            Pieces.Add(new Piece(Current));
                            Current = "";
                        }
                        else
                        {
                            Current += c;
                        }
                        break;
                    default:
                        Current += c;
                        break;
                }
            }
            if (Current != "") Pieces.Add(new Piece(Current));

            return Pieces.ToArray();
        }

        public static Piece[] StringBuilder(Piece[] Input, int Start, int End)
        {
            List<Piece> Output = new List<Piece>();
            for (int Index = Start; Index < End; Index++)
            {
                Output.Add(Input[Index]);
            }
            return Output.ToArray();
        }
        public static String[] parseCall(String Value)
        {
            String[] Out = new String[2];
            int Start, End;
            Start = Value.IndexOf('(') + 1;
            End = Value.LastIndexOf(')');
            if (Start < 0 || End < 0 || (Start - 1) < 0)
            {
                Errors.throwError("Trying to parse a call/BIF but seems to be failing: " + Value);
            }
            Out[1] = Value.Substring(Start, int.Parse(Math.Abs(Start - End).ToString())).Trim(); //Params
            Out[0] = Value.Substring(0, Start - 1).Trim(); //Name
            return Out;
        }
    }
}
