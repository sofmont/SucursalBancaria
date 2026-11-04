using System;

namespace SucursalBancaria.Estructuras
{
    /// <summary>
    /// Cola simple (Queue) FIFO lineal sobre un arreglo estático de tamaño fijo.
    /// <para>
    /// Punteros de control: <c>front</c> = índice del primer elemento, <c>rear</c> = índice del último
    /// (-1 si no hay). Los índices sólo avanzan; al vaciarse por completo se reinician (front = 0, rear = -1).
    /// </para>
    /// <para>
    /// Limitación clásica ("desbordamiento falso"): si <c>rear</c> llegó al final del arreglo pero
    /// <c>front</c> avanzó, quedan celdas libres al inicio que no se pueden usar. <see cref="Reorganizar"/>
    /// resuelve esto desplazando manualmente los elementos al inicio (O(n)). La cola circular
    /// resuelve el mismo problema en O(1) con aritmética modular.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Tipo de dato almacenado.</typeparam>
    public class ColaSimple<T>
    {
        private readonly T[] datos;
        private int front; // posición del primer elemento
        private int rear;  // posición del último elemento (-1 si no hay)

        /// <summary>Crea una cola simple con capacidad fija.</summary>
        public ColaSimple(int capacidad)
        {
            if (capacidad <= 0)
                throw new ArgumentOutOfRangeException("capacidad", "La capacidad debe ser mayor que cero.");
            datos = new T[capacidad];
            front = 0;
            rear = -1;
        }

        public int Capacidad { get { return datos.Length; } }

        /// <summary>Elementos almacenados (rear - front + 1).</summary>
        public int Cantidad { get { return rear - front + 1; } }

        public int Front { get { return front; } }
        public int Rear { get { return rear; } }

        public bool IsEmpty()
        {
            return front > rear;
        }

        /// <summary>
        /// En la cola simple "llena" significa que <c>rear</c> alcanzó la última celda del arreglo,
        /// aunque existan celdas libres antes de <c>front</c>.
        /// </summary>
        public bool IsFull()
        {
            return rear == datos.Length - 1;
        }

        /// <summary>
        /// <c>true</c> cuando la cola reporta estar llena pero hay celdas libres al inicio
        /// (situación que <see cref="Reorganizar"/> puede corregir).
        /// </summary>
        public bool HayDesbordamientoFalso()
        {
            return IsFull() && front > 0;
        }

        /// <summary>Inserta al final de la cola.</summary>
        /// <returns><c>false</c> si <c>rear</c> está en la última posición.</returns>
        public bool Enqueue(T valor)
        {
            if (IsFull())
                return false;

            rear++;
            datos[rear] = valor;
            return true;
        }

        /// <summary>Retira el elemento del frente.</summary>
        /// <returns><c>false</c> si la cola está vacía.</returns>
        public bool Dequeue(out T valor)
        {
            valor = default(T);
            if (IsEmpty())
                return false;

            valor = datos[front];
            datos[front] = default(T);
            front++;

            // si ya no quedan elementos, se reinician los índices
            if (front > rear)
            {
                front = 0;
                rear = -1;
            }
            return true;
        }

        /// <summary>Consulta el frente sin retirarlo.</summary>
        public bool Peek(out T valor)
        {
            valor = default(T);
            if (IsEmpty())
                return false;

            valor = datos[front];
            return true;
        }

        /// <summary>
        /// Compacta la cola moviendo los elementos al inicio del arreglo (front = 0).
        /// Corrige el desbordamiento falso a costa de O(n) movimientos.
        /// </summary>
        /// <returns>Número de celdas liberadas al final.</returns>
        public int Reorganizar()
        {
            if (front == 0)
                return 0;

            int liberadas = front;
            int destino = 0;
            for (int i = front; i <= rear; i++)
            {
                datos[destino] = datos[i];
                datos[i] = default(T);
                destino++;
            }
            rear = destino - 1;
            front = 0;
            return liberadas;
        }

        /// <summary>Vacía la cola reiniciando los punteros.</summary>
        public void Clear()
        {
            for (int i = 0; i < datos.Length; i++)
                datos[i] = default(T);
            front = 0;
            rear = -1;
        }

        /// <summary>Lectura de solo consulta. 0 = frente, Cantidad-1 = final.</summary>
        public T ElementoEn(int posicionLogica)
        {
            if (posicionLogica < 0 || posicionLogica >= Cantidad)
                throw new ArgumentOutOfRangeException("posicionLogica");
            return datos[front + posicionLogica];
        }

        public string DescribirIndices()
        {
            return "front = " + front + " | rear = " + rear + " | cantidad = " + Cantidad + "/" + datos.Length +
                   " | vacia: " + IsEmpty() + " | llena: " + IsFull() +
                   (HayDesbordamientoFalso() ? " (desbordamiento falso)" : "");
        }
    }
}
