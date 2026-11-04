using System.Globalization;
using SucursalBancaria.Estructuras;

namespace SucursalBancaria.Calculadora
{
    /// <summary>
    /// Analizador léxico manual: recorre la cadena carácter por carácter y deposita los
    /// tokens, en orden de lectura, en una COLA SIMPLE (FIFO), que será la entrada del
    /// algoritmo Shunting-Yard.
    /// </summary>
    public static class Tokenizador
    {
        /// <param name="expresion">Texto, p. ej. "1500 * (1 + 0.12/12) ^ 12".</param>
        /// <param name="entrada">Cola destino (debe venir vacía).</param>
        /// <param name="error">Mensaje de error, o null si todo fue bien.</param>
        public static bool Tokenizar(string expresion, ColaSimple<Token> entrada, out string error)
        {
            error = null;
            if (expresion == null || expresion.Trim().Length == 0)
            {
                error = "La expresion esta vacia.";
                return false;
            }

            bool hayAnterior = false;
            Token anterior = new Token();
            int i = 0;

            while (i < expresion.Length)
            {
                char c = expresion[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                Token token;
                if (char.IsDigit(c) || c == '.' || c == ',')
                {
                    // --- número: dígitos con a lo más un separador decimal ('.' o ',')
                    int inicio = i;
                    bool haySeparador = false;
                    string numero = "";
                    while (i < expresion.Length && (char.IsDigit(expresion[i]) || expresion[i] == '.' || expresion[i] == ','))
                    {
                        if (expresion[i] == '.' || expresion[i] == ',')
                        {
                            if (haySeparador)
                            {
                                error = "Numero mal formado cerca de la posicion " + (inicio + 1) + ".";
                                return false;
                            }
                            haySeparador = true;
                            numero += ".";
                        }
                        else
                        {
                            numero += expresion[i];
                        }
                        i++;
                    }

                    decimal valor;
                    if (numero == "." || !decimal.TryParse(numero, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor))
                    {
                        error = "Numero invalido: \"" + expresion.Substring(inicio, i - inicio) + "\".";
                        return false;
                    }
                    token = Token.DeNumero(valor);
                }
                else if (c == '(' || c == '[' || c == '{')
                {
                    token = Token.DeSimbolo(TipoToken.AgrupacionAbre, c);
                    i++;
                }
                else if (c == ')' || c == ']' || c == '}')
                {
                    token = Token.DeSimbolo(TipoToken.AgrupacionCierra, c);
                    i++;
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '^' || c == 'x' || c == 'X')
                {
                    char simbolo = (c == 'x' || c == 'X') ? '*' : c;
                    i++;

                    // ¿Es un signo unario? Ocurre al inicio, tras un operador o tras una apertura.
                    bool posicionUnaria = !hayAnterior ||
                                          anterior.Tipo == TipoToken.Operador ||
                                          anterior.Tipo == TipoToken.AgrupacionAbre;
                    if (posicionUnaria && simbolo == '+')
                        continue; // el más unario no altera el valor: se ignora
                    if (posicionUnaria && simbolo == '-')
                        simbolo = Token.Negacion;

                    token = Token.DeSimbolo(TipoToken.Operador, simbolo);
                }
                else
                {
                    error = "Caracter no permitido '" + c + "' en la posicion " + (i + 1) + ".";
                    return false;
                }

                if (!entrada.Enqueue(token))
                {
                    error = "La expresion es demasiado larga (maximo " + entrada.Capacidad + " elementos).";
                    return false;
                }
                anterior = token;
                hayAnterior = true;
            }

            return true;
        }
    }
}
