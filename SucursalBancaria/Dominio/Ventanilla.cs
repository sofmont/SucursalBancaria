namespace SucursalBancaria.Dominio
{
    /// <summary>Ventanilla de atención: puede estar libre o atendiendo un turno.</summary>
    public sealed class Ventanilla
    {
        public int Numero { get; private set; }
        public Turno TurnoActual { get; private set; }
        public int AtendidosEnElDia { get; private set; }

        public Ventanilla(int numero)
        {
            Numero = numero;
        }

        public bool EstaLibre
        {
            get { return TurnoActual == null; }
        }

        public void Atender(Turno turno)
        {
            TurnoActual = turno;
            turno.IniciarAtencion();
        }

        /// <summary>Termina la atención actual y deja la ventanilla libre.</summary>
        public Turno Liberar()
        {
            Turno terminado = TurnoActual;
            TurnoActual = null;
            if (terminado != null)
                AtendidosEnElDia++;
            return terminado;
        }

        public override string ToString()
        {
            return "Ventanilla " + Numero + ": " + (EstaLibre ? "LIBRE" : "atendiendo " + TurnoActual.Folio + " (" + TurnoActual.Cliente + ")");
        }
    }
}
