using SucursalBancaria.Estructuras;

namespace SucursalBancaria.Dominio
{
    /// <summary>Resultado de pedir a una ventanilla que llame al siguiente turno.</summary>
    public enum ResultadoLlamado
    {
        Llamado,
        VentanillaInvalida,
        VentanillaOcupada,
        FilaVacia
    }

    /// <summary>
    /// Sistema de atención en ventanilla.
    /// <list type="bullet">
    /// <item><b>fila</b>: COLA CIRCULAR de turnos en espera. El dispensador de fichas es un buffer
    /// de capacidad fija que se reutiliza todo el día sin desplazar datos.</item>
    /// <item><b>pantalla</b>: COLA CIRCULAR con los últimos llamados que se muestran en el monitor
    /// de la sala; al llenarse, el llamado más antiguo sale por el frente.</item>
    /// <item><b>ventanillas</b>: arreglo estático de tamaño fijo.</item>
    /// </list>
    /// </summary>
    public class SistemaTurnos
    {
        public const int CapacidadFila = 8;
        public const int NumeroVentanillas = 3;
        public const int LlamadosEnPantalla = 4;

        private readonly ColaCircular<Turno> fila = new ColaCircular<Turno>(CapacidadFila);
        private readonly ColaCircular<string> pantalla = new ColaCircular<string>(LlamadosEnPantalla);
        private readonly Ventanilla[] ventanillas = new Ventanilla[NumeroVentanillas];

        private int consecutivo;
        private int atendidos;
        private int rechazados;
        private double segundosEsperaAcumulados;

        public SistemaTurnos()
        {
            for (int i = 0; i < ventanillas.Length; i++)
                ventanillas[i] = new Ventanilla(i + 1);
        }

        public ColaCircular<Turno> Fila { get { return fila; } }
        public ColaCircular<string> Pantalla { get { return pantalla; } }
        public int Atendidos { get { return atendidos; } }
        public int Rechazados { get { return rechazados; } }

        public Ventanilla ObtenerVentanilla(int numero)
        {
            return ventanillas[numero - 1];
        }

        /// <summary>Espera promedio (segundos) de los turnos que ya pasaron a ventanilla.</summary>
        public double EsperaPromedio()
        {
            int iniciados = 0;
            for (int i = 0; i < ventanillas.Length; i++)
                iniciados += ventanillas[i].AtendidosEnElDia + (ventanillas[i].EstaLibre ? 0 : 1);
            return iniciados == 0 ? 0 : segundosEsperaAcumulados / iniciados;
        }

        /// <summary>Entrega un turno al cliente y lo forma al final de la fila.</summary>
        /// <returns>False si la fila está llena (el cliente debe esperar fuera / volver después).</returns>
        public bool EmitirTurno(string cliente, TipoOperacion operacion, out Turno turno)
        {
            turno = null;
            if (fila.IsFull())
            {
                rechazados++;
                return false;
            }

            consecutivo++;
            turno = new Turno(consecutivo, cliente, operacion);
            fila.Enqueue(turno);
            return true;
        }

        /// <summary>La ventanilla indicada llama al turno que está al frente de la fila.</summary>
        public ResultadoLlamado LlamarSiguiente(int numeroVentanilla, out Turno turno)
        {
            turno = null;
            if (numeroVentanilla < 1 || numeroVentanilla > ventanillas.Length)
                return ResultadoLlamado.VentanillaInvalida;

            Ventanilla ventanilla = ventanillas[numeroVentanilla - 1];
            if (!ventanilla.EstaLibre)
                return ResultadoLlamado.VentanillaOcupada;

            if (!fila.Dequeue(out turno))
                return ResultadoLlamado.FilaVacia;

            ventanilla.Atender(turno);
            segundosEsperaAcumulados += turno.SegundosDeEspera();
            PublicarEnPantalla(turno.Folio + " -> Ventanilla " + numeroVentanilla);
            return ResultadoLlamado.Llamado;
        }

        /// <summary>Concluye el trámite de la ventanilla indicada.</summary>
        public bool FinalizarAtencion(int numeroVentanilla, out Turno turno)
        {
            turno = null;
            if (numeroVentanilla < 1 || numeroVentanilla > ventanillas.Length)
                return false;

            turno = ventanillas[numeroVentanilla - 1].Liberar();
            if (turno == null)
                return false;

            atendidos++;
            return true;
        }

        /// <summary>Consulta quién es el siguiente sin sacarlo de la fila.</summary>
        public bool ConsultarSiguiente(out Turno turno)
        {
            return fila.Peek(out turno);
        }

        private void PublicarEnPantalla(string llamado)
        {
            string descartado;
            if (pantalla.IsFull())
                pantalla.Dequeue(out descartado);
            pantalla.Enqueue(llamado);
        }
    }
}
