using System.Globalization;

namespace SucursalBancaria.Calculadora
{
    /// <summary>Categorías léxicas de una expresión aritmética.</summary>
    public enum TipoToken
    {
        Numero,
        Operador,
        AgrupacionAbre,   // ( [ {
        AgrupacionCierra  // ) ] }
    }

    /// <summary>
    /// Unidad léxica. Es un struct (tipo valor) para que las pilas/colas de tokens
    /// guarden los datos directamente en el arreglo estático.
    /// </summary>
    public struct Token
    {
        /// <summary>Símbolo interno del menos unario (negación), distinto de la resta binaria.</summary>
        public const char Negacion = '~';

        public TipoToken Tipo { get; private set; }
        public decimal Valor { get; private set; }
        public char Simbolo { get; private set; }

        public static Token DeNumero(decimal valor)
        {
            Token t = new Token();
            t.Tipo = TipoToken.Numero;
            t.Valor = valor;
            return t;
        }

        public static Token DeSimbolo(TipoToken tipo, char simbolo)
        {
            Token t = new Token();
            t.Tipo = tipo;
            t.Simbolo = simbolo;
            return t;
        }

        public bool EsOperadorUnario
        {
            get { return Tipo == TipoToken.Operador && Simbolo == Negacion; }
        }

        /// <summary>Prioridad: ^ (4) &gt; negación (3) &gt; * / % (2) &gt; + - (1).</summary>
        public int Precedencia
        {
            get
            {
                switch (Simbolo)
                {
                    case '^': return 4;
                    case Negacion: return 3;
                    case '*':
                    case '/':
                    case '%': return 2;
                    case '+':
                    case '-': return 1;
                    default: return 0;
                }
            }
        }

        /// <summary>La potencia y la negación se agrupan de derecha a izquierda.</summary>
        public bool AsociativoDerecha
        {
            get { return Simbolo == '^' || Simbolo == Negacion; }
        }

        public override string ToString()
        {
            if (Tipo == TipoToken.Numero)
                return Valor.ToString(CultureInfo.InvariantCulture);
            if (EsOperadorUnario)
                return "neg";
            return Simbolo.ToString();
        }
    }
}
