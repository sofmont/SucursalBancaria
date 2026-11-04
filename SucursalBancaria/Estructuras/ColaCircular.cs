using System;

namespace SucursalBancaria.Estructuras
{
    /// <summary>
    /// Cola circular (Circular Queue / Ring Buffer) FIFO sobre un arreglo estático de tamaño fijo.
    /// <para>
    /// Punteros de control: <c>front</c> (primer elemento), <c>rear</c> (último elemento) y
    /// <c>cantidad</c>. Ambos punteros avanzan con aritmética modular <c>(i + 1) % capacidad</c>,
    /// por lo que las celdas liberadas al inicio se reutilizan sin mover datos.
    /// </para>
    /// Complejidad: Enqueue, Dequeue, Peek, IsEmpty e IsFull son O(1).
    /// </summary>
    /// <typeparam name="T">Tipo de dato almacenado.</typeparam>
    public class ColaCircular<T>
    {
        private readonly T[] datos;
        private int front;    // posición del primer elemento
        private int rear;     // posición del último elemento
        private int cantidad; // elementos guardados (distingue vacía de llena cuando front == rear + 1)

        /// <summary>Crea una cola circular con capacidad fija.</summary>
        public ColaCircular(int capacidad)
        {
            if (capacidad <= 0)
                throw new ArgumentOutOfRangeException("capacidad", "La capacidad debe ser mayor que cero.");
            datos = new T[capacidad];
            front = 0;
            rear = -1;
            cantidad = 0;
        }

        public int Capacidad { get { return datos.Length; } }
        public int Cantidad { get { return cantidad; } }
        public int Front { get { return front; } }
        public int Rear { get { return rear; } }

        public bool IsEmpty()
        {
            return cantidad == 0;
        }

        public bool IsFull()
        {
            return cantidad == datos.Length;
        }

        /// <summary>Inserta al final, envolviendo el índice al inicio si es necesario.</summary>
        /// <returns><c>false</c> si la cola está llena.</returns>
        public bool Enqueue(T valor)
        {
            if (IsFull())
                return false;

            rear = (rear + 1) % datos.Length;
            datos[rear] = valor;
            cantidad++;
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
            front = (front + 1) % datos.Length;
            cantidad--;
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

        /// <summary>Vacía la cola reiniciando los punteros.</summary>
        public void Clear()
        {
            for (int i = 0; i < datos.Length; i++)
                datos[i] = default(T);
            front = 0;
            rear = -1;
            cantidad = 0;
        }

        /// <summary>Lectura de solo consulta. 0 = frente, Cantidad-1 = final.</summary>
        public T ElementoEn(int posicionLogica)
        {
            if (posicionLogica < 0 || posicionLogica >= cantidad)
                throw new ArgumentOutOfRangeException("posicionLogica");
            return datos[(front + posicionLogica) % datos.Length];
        }

        /// <summary>Índice físico del arreglo que corresponde a una posición lógica.</summary>
        public int IndiceFisico(int posicionLogica)
        {
            return (front + posicionLogica) % datos.Length;
        }

        public string DescribirIndices()
        {
            return "front = " + front + " | rear = " + rear + " | cantidad = " + cantidad + "/" + datos.Length +
                   " | vacia: " + IsEmpty() + " | llena: " + IsFull();
        }
    }
}
