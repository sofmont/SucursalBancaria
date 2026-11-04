using System;

namespace SucursalBancaria.Dominio
{
    /// <summary>Tipo de trámite; define la letra del folio del turno.</summary>
    public enum TipoOperacion
    {
        Deposito = 1,
        Retiro = 2,
        PagoServicios = 3,
        Consulta = 4
    }

    /// <summary>Entidad: turno (ficha) entregado a un cliente al llegar a la sucursal.</summary>
    public sealed class Turno
    {
        public int Consecutivo { get; private set; }
        public string Cliente { get; private set; }
        public TipoOperacion Operacion { get; private set; }
        public DateTime Llegada { get; private set; }
        public DateTime? InicioAtencion { get; private set; }

        public Turno(int consecutivo, string cliente, TipoOperacion operacion)
        {
            Consecutivo = consecutivo;
            Cliente = cliente;
            Operacion = operacion;
            Llegada = DateTime.Now;
        }

        /// <summary>Folio visible, p. ej. "R-007" para un retiro.</summary>
        public string Folio
        {
            get { return Letra(Operacion) + "-" + Consecutivo.ToString("D3"); }
        }

        public void IniciarAtencion()
        {
            InicioAtencion = DateTime.Now;
        }

        /// <summary>Segundos transcurridos entre la llegada y el inicio de la atención.</summary>
        public double SegundosDeEspera()
        {
            DateTime fin = InicioAtencion ?? DateTime.Now;
            return (fin - Llegada).TotalSeconds;
        }

        public static string Letra(TipoOperacion operacion)
        {
            switch (operacion)
            {
                case TipoOperacion.Deposito: return "D";
                case TipoOperacion.Retiro: return "R";
                case TipoOperacion.PagoServicios: return "P";
                default: return "C";
            }
        }

        public static string Nombre(TipoOperacion operacion)
        {
            switch (operacion)
            {
                case TipoOperacion.Deposito: return "Deposito";
                case TipoOperacion.Retiro: return "Retiro";
                case TipoOperacion.PagoServicios: return "Pago de servicios";
                default: return "Consulta";
            }
        }

        public override string ToString()
        {
            return Folio + " | " + Cliente + " | " + Nombre(Operacion) + " | llego " + Llegada.ToString("HH:mm:ss");
        }
    }
}
