using System;

namespace SucursalBancaria.Estructuras
{
    /// <summary>
    /// Pila (Stack) LIFO implementada desde cero sobre un arreglo estático de tamaño fijo.
    /// <para>
    /// Puntero de control: <c>top</c> = índice del último elemento insertado (-1 si está vacía).
    /// No utiliza ninguna colección nativa del lenguaje (List, Stack, ArrayList, etc.).
    /// </para>
    /// Complejidad: Push, Pop, Peek, IsEmpty e IsFull son O(1).
    /// </summary>
    /// <typeparam name="T">Tipo de dato almacenado.</typeparam>
    public class Pila<T>
    {
        private readonly T[] datos; // memoria estática: el tamaño no cambia tras el constructor
        private int top;            // índice de la cima (-1 = vacía)

        /// <summary>Crea una pila con capacidad fija.</summary>
        /// <param name="capacidad">Número máximo de elementos (mayor que cero).</param>
        public Pila(int capacidad)
        {
            if (capacidad <= 0)
                throw new ArgumentOutOfRangeException("capacidad", "La capacidad debe ser mayor que cero.");
            datos = new T[capacidad];
            top = -1;
        }

        /// <summary>Capacidad máxima del arreglo.</summary>
        public int Capacidad { get { return datos.Length; } }

        /// <summary>Número de elementos actualmente almacenados.</summary>
        public int Cantidad { get { return top + 1; } }

        /// <summary>Valor actual del puntero <c>top</c> (solo lectura, para depuración).</summary>
        public int Top { get { return top; } }

        /// <summary>Indica si la pila no contiene elementos.</summary>
        public bool IsEmpty()
        {
            return top == -1;
        }

        /// <summary>Indica si la pila alcanzó su capacidad máxima.</summary>
        public bool IsFull()
        {
            return top == datos.Length - 1;
        }

        /// <summary>Inserta un elemento en la cima.</summary>
        /// <returns><c>false</c> si la pila está llena (desbordamiento / overflow).</returns>
        public bool Push(T valor)
        {
            if (IsFull())
                return false;

            top++;
            datos[top] = valor;
            return true;
        }

        /// <summary>Retira el elemento de la cima.</summary>
        /// <param name="valor">Elemento retirado, o <c>default</c> si estaba vacía.</param>
        /// <returns><c>false</c> si la pila está vacía (subdesbordamiento / underflow).</returns>
        public bool Pop(out T valor)
        {
            valor = default(T);
            if (IsEmpty())
                return false;

            valor = datos[top];
            datos[top] = default(T); // libera la referencia para el recolector de basura
            top--;
            return true;
        }

        /// <summary>Consulta la cima sin retirarla.</summary>
        /// <returns><c>false</c> si la pila está vacía.</returns>
        public bool Peek(out T valor)
        {
            valor = default(T);
            if (IsEmpty())
                return false;

            valor = datos[top];
            return true;
        }

        /// <summary>Vacía la pila reiniciando el puntero <c>top</c>.</summary>
        public void Clear()
        {
            while (top >= 0)
            {
                datos[top] = default(T);
                top--;
            }
        }

        /// <summary>
        /// Elimina el elemento del FONDO desplazando el resto una posición hacia abajo (O(n)).
        /// Operación auxiliar para historiales acotados (p. ej. deshacer con límite):
        /// cuando la pila está llena se descarta el registro más antiguo.
        /// </summary>
        /// <returns><c>false</c> si la pila está vacía.</returns>
        public bool EliminarFondo(out T valor)
        {
            valor = default(T);
            if (IsEmpty())
                return false;

            valor = datos[0];
            for (int i = 0; i < top; i++)
                datos[i] = datos[i + 1];
            datos[top] = default(T);
            top--;
            return true;
        }

        /// <summary>
        /// Lectura de solo consulta para recorridos/visualización.
        /// </summary>
        /// <param name="desdeCima">0 = cima, 1 = el de abajo, ... , Cantidad-1 = fondo.</param>
        public T ElementoDesdeCima(int desdeCima)
        {
            if (desdeCima < 0 || desdeCima > top)
                throw new ArgumentOutOfRangeException("desdeCima");
            return datos[top - desdeCima];
        }

        /// <summary>Estado interno de los punteros (equivalente a MostrarIndices del repositorio base).</summary>
        public string DescribirIndices()
        {
            return "top = " + top + " | cantidad = " + Cantidad + "/" + datos.Length +
                   " | vacia: " + IsEmpty() + " | llena: " + IsFull();
        }
    }
}
