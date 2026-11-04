using System;
using System.Windows.Forms;
using SucursalBancaria.UI;

namespace SucursalBancaria
{
    /// <summary>
    /// APLICACIÓN 2 (Windows Forms) — Sucursal bancaria: turnos de ventanilla (Cola circular)
    /// y calculadora del cajero (Pilas + Colas simples, algoritmo Shunting-Yard).
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormPrincipal());
        }
    }
}
