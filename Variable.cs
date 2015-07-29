using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGNet
{
    class Variable
    {
        private Piece.Type _Type;
        private int _Dim;

        public Variable(Piece.Type Type, int Dim = 0)
        {
            _Type = Type;
            _Dim = Dim;
        }

        public Piece.Type getType()
        {
            return _Type;
        }
        public int getDim()
        {
            return _Dim;
        }
    }
}
