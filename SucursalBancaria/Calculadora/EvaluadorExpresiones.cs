using System;
using System.Globalization;
using System.Text;
using SucursalBancaria.Estructuras;

namespace SucursalBancaria.Calculadora
{
    /// <summary>Resultado completo de una evaluación (valor, notación postfija y traza didáctica).</summary>
    public sealed class ResultadoEvaluacion
    {
        public bool Exito { get; internal set; }
        public decimal Valor { get; internal set; }
        public string Postfija { get; internal set; }
        public string Traza { get; internal set; }
        public string Error { get; internal set; }
    }

    /// <summary>
    /// Evaluador de expresiones aritméticas en tres fases, todas con estructuras estáticas propias:
    /// <list type="number">
    /// <item><b>Léxico</b>: texto → COLA SIMPLE de tokens (entrada).</item>
    /// <item><b>Shunting-Yard (Dijkstra)</b>: infija → postfija usando una PILA de operadores y
    /// una COLA SIMPLE de salida. Aquí también se valida el balance de ( ) [ ] { }.</item>
    /// <item><b>Evaluación postfija</b>: PILA de operandos.</item>
    /// </list>
    /// Operadores: + - * / % ^ y negación unaria. Agrupadores: ( ) [ ] { }.
    /// </summary>
    public class EvaluadorExpresiones
    {
        public const int MaximoTokens = 64;

        /// <summary>Evalúa la expresión. No lanza excepciones: los errores se reportan en el resultado.</summary>
        public ResultadoEvaluacion Evaluar(string expresion)
        {
            ResultadoEvaluacion r = new ResultadoEvaluacion();
            string error;

            // Fase 1: análisis léxico
            ColaSimple<Token> entrada = new ColaSimple<Token>(MaximoTokens);
            if (!Tokenizador.Tokenizar(expresion, entrada, out error))
                return Fallo(r, error);

            // Fase 2: infija -> postfija
            ColaSimple<Token> postfija = new ColaSimple<Token>(MaximoTokens);
            if (!ConvertirAPostfija(entrada, postfija, out error))
                return Fallo(r, error);
            r.Postfija = Describir(postfija);

            // Fase 3: evaluación
            decimal valor;
            string traza;
            if (!EvaluarPostfija(postfija, out valor, out traza, out error))
            {
                r.Traza = traza;
                return Fallo(r, error);
            }

            r.Exito = true;
            r.Valor = valor;
            r.Traza = traza;
            return r;
        }

        /// <summary>
        /// Algoritmo Shunting-Yard. Además valida la sintaxis con un autómata de dos estados
        /// (esperaOperando = true/false) y el emparejamiento de agrupadores con la pila.
        /// </summary>
        private static bool ConvertirAPostfija(ColaSimple<Token> entrada, ColaSimple<Token> salida, out string error)
        {
            error = null;
            Pila<Token> operadores = new Pila<Token>(MaximoTokens);
            bool esperaOperando = true;
            Token token;
            Token cima;

            while (entrada.Dequeue(out token))
            {
                switch (token.Tipo)
                {
                    case TipoToken.Numero:
                        if (!esperaOperando) { error = "Falta un operador antes de " + token + "."; return false; }
                        salida.Enqueue(token);
                        esperaOperando = false;
                        break;

                    case TipoToken.AgrupacionAbre:
                        if (!esperaOperando) { error = "Falta un operador antes de '" + token.Simbolo + "'."; return false; }
                        operadores.Push(token);
                        break;

                    case TipoToken.Operador:
                        if (token.EsOperadorUnario)
                        {
                            // prefijo: no tiene operando izquierdo, se apila directamente
                            operadores.Push(token);
                            break;
                        }
                        if (esperaOperando) { error = "El operador '" + token.Simbolo + "' no tiene operando a su izquierda."; return false; }

                        // desapilar mientras la cima tenga mayor prioridad (o igual y asociatividad izquierda)
                        while (operadores.Peek(out cima) && cima.Tipo == TipoToken.Operador &&
                               (cima.Precedencia > token.Precedencia ||
                                (cima.Precedencia == token.Precedencia && !token.AsociativoDerecha)))
                        {
                            operadores.Pop(out cima);
                            salida.Enqueue(cima);
                        }
                        operadores.Push(token);
                        esperaOperando = true;
                        break;

                    case TipoToken.AgrupacionCierra:
                        if (esperaOperando) { error = "Agrupacion vacia o incompleta antes de '" + token.Simbolo + "'."; return false; }

                        bool encontrada = false;
                        while (operadores.Pop(out cima))
                        {
                            if (cima.Tipo == TipoToken.AgrupacionAbre)
                            {
                                if (cima.Simbolo != Pareja(token.Simbolo))
                                {
                                    error = "Agrupadores cruzados: se abrio '" + cima.Simbolo + "' y se cerro con '" + token.Simbolo + "'.";
                                    return false;
                                }
                                encontrada = true;
                                break;
                            }
                            salida.Enqueue(cima);
                        }
                        if (!encontrada) { error = "Sobra un '" + token.Simbolo + "' sin su apertura."; return false; }
                        esperaOperando = false;
                        break;
                }
            }

            if (esperaOperando)
            {
                error = "La expresion esta incompleta (termina en operador o esta vacia).";
                return false;
            }

            while (operadores.Pop(out cima))
            {
                if (cima.Tipo == TipoToken.AgrupacionAbre)
                {
                    error = "Falta cerrar '" + cima.Simbolo + "'.";
                    return false;
                }
                salida.Enqueue(cima);
            }
            return true;
        }

        /// <summary>Evalúa la notación postfija consumiendo la cola y usando una pila de operandos.</summary>
        private static bool EvaluarPostfija(ColaSimple<Token> postfija, out decimal resultado, out string traza, out string error)
        {
            resultado = 0;
            error = null;
            StringBuilder sb = new StringBuilder();
            Pila<decimal> operandos = new Pila<decimal>(MaximoTokens);
            Token token;

            try
            {
                while (postfija.Dequeue(out token))
                {
                    if (token.Tipo == TipoToken.Numero)
                    {
                        operandos.Push(token.Valor);
                        sb.AppendLine("  leer " + Num(token.Valor) + "  -> Push            | pila: " + PilaComoTexto(operandos));
                        continue;
                    }

                    decimal b, a;
                    if (token.EsOperadorUnario)
                    {
                        operandos.Pop(out a);
                        operandos.Push(-a);
                        sb.AppendLine("  neg " + Num(a) + " = " + Num(-a) + "       | pila: " + PilaComoTexto(operandos));
                        continue;
                    }

                    operandos.Pop(out b); // el segundo operando sale primero (LIFO)
                    operandos.Pop(out a);
                    decimal parcial;
                    if (!Aplicar(token.Simbolo, a, b, out parcial, out error))
                    {
                        traza = sb.ToString();
                        return false;
                    }
                    operandos.Push(parcial);
                    sb.AppendLine("  " + Num(a) + " " + token.Simbolo + " " + Num(b) + " = " + Num(parcial) + "  | pila: " + PilaComoTexto(operandos));
                }
            }
            catch (OverflowException)
            {
                traza = sb.ToString();
                error = "Desbordamiento numerico: el resultado es demasiado grande.";
                return false;
            }
            catch (DivideByZeroException)
            {
                traza = sb.ToString();
                error = "El resultado es demasiado pequeno para representarse (division entre cero interna).";
                return false;
            }

            traza = sb.ToString();
            // la sintaxis ya fue validada, así que debe quedar exactamente un valor
            if (operandos.Cantidad != 1 || !operandos.Pop(out resultado))
            {
                error = "Expresion invalida.";
                return false;
            }
            return true;
        }

        private static bool Aplicar(char operador, decimal a, decimal b, out decimal resultado, out string error)
        {
            resultado = 0;
            error = null;
            switch (operador)
            {
                case '+': resultado = a + b; return true;
                case '-': resultado = a - b; return true;
                case '*': resultado = a * b; return true;
                case '/':
                    if (b == 0) { error = "Division entre cero."; return false; }
                    resultado = a / b; return true;
                case '%':
                    if (b == 0) { error = "Modulo entre cero."; return false; }
                    resultado = a % b; return true;
                case '^':
                    return Potencia(a, b, out resultado, out error);
                default:
                    error = "Operador desconocido '" + operador + "'.";
                    return false;
            }
        }

        /// <summary>Potencia con exponente entero por exponenciación rápida (cuadrados sucesivos).</summary>
        private static bool Potencia(decimal baseValor, decimal exponente, out decimal resultado, out string error)
        {
            resultado = 0;
            error = null;
            if (decimal.Truncate(exponente) != exponente)
            {
                error = "Solo se admiten exponentes enteros.";
                return false;
            }
            if (Math.Abs(exponente) > 10000)
            {
                error = "Exponente fuera de rango (maximo 10000).";
                return false;
            }
            int n = (int)exponente;
            if (n < 0 && baseValor == 0)
            {
                error = "Cero elevado a un exponente negativo.";
                return false;
            }

            int restante = Math.Abs(n);
            decimal acumulado = 1;
            decimal factor = baseValor;
            while (restante > 0)
            {
                if ((restante & 1) == 1)
                    acumulado *= factor;
                restante >>= 1;
                if (restante > 0)
                    factor *= factor;
            }
            if (n < 0 && acumulado == 0)
            {
                error = "El resultado es demasiado grande para representarse.";
                return false;
            }
            resultado = n < 0 ? 1 / acumulado : acumulado;
            return true;
        }

        private static char Pareja(char cierre)
        {
            switch (cierre)
            {
                case ')': return '(';
                case ']': return '[';
                default: return '{';
            }
        }

        private static string Describir(ColaSimple<Token> cola)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cola.Cantidad; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(cola.ElementoEn(i).ToString());
            }
            return sb.ToString();
        }

        private static string PilaComoTexto(Pila<decimal> pila)
        {
            // de fondo a cima, para leerla de izquierda a derecha
            StringBuilder sb = new StringBuilder("[");
            for (int i = pila.Cantidad - 1; i >= 0; i--)
            {
                sb.Append(Num(pila.ElementoDesdeCima(i)));
                if (i > 0) sb.Append(", ");
            }
            return sb.Append("]").ToString();
        }

        /// <summary>Formatea un decimal con hasta 6 decimales, sin ceros sobrantes.</summary>
        public static string Num(decimal valor)
        {
            return Math.Round(valor, 6).ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static ResultadoEvaluacion Fallo(ResultadoEvaluacion r, string error)
        {
            r.Exito = false;
            r.Error = error;
            return r;
        }
    }
}
