using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class DataStructure
    {
        private Dictionary<String, Piece.Type> Variables;
        private Boolean Qualified;

        public DataStructure(Boolean qualified)
        {
            Variables = new Dictionary<String, Piece.Type>();
            Qualified = qualified;
        }

        public void addVar(String Name, Piece.Type Var)
        {
            Variables.Add(Name, Var);
        }

        public Boolean isQualified()
        {
            return Qualified;
        }
        public string[] getVars()
        {
            return Variables.Keys.ToArray();
        }
        public Piece.Type getType(String Var)
        {
            return Variables[Var];
        }
    }
}
