using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SucursalBancaria.Calculadora;
using SucursalBancaria.Dominio;
using SucursalBancaria.Estructuras;

namespace SucursalBancaria.UI
{
    /// <summary>
    /// Ventana principal: pestañas Ventanillas (Cola circular), Calculadora (Pilas + Colas simples)
    /// y Estructuras (punteros y celdas físicas).
    /// </summary>
    public class FormPrincipal : Form
    {
        private static readonly string[] Ejemplos =
        {
            "10000 * (1 + 0.115 / 12) ^ 12",
            "2500 + 1800 - 350 * 2",
            "{[ (15000 - 3000) / 6 ] * 1.16}",
            "-(500 - 1200) % 300",
            "(3 + 4"
        };

        private static readonly string[] DescripcionEjemplos =
        {
            "Interes compuesto mensual, 1 anio",
            "Movimientos de caja",
            "Abono mensual con IVA",
            "Negacion unaria y modulo",
            "Error de agrupacion"
        };

        private readonly SistemaTurnos sistema = new SistemaTurnos();
        private readonly EvaluadorExpresiones evaluador = new EvaluadorExpresiones();

        private TabControl tabs;
        private Label barra;

        // Ventanillas
        private TextBox txtCliente, txtFila, txtPantalla;
        private RadioButton[] radiosTramite = new RadioButton[4];
        private Label[] lblVentanillas = new Label[SistemaTurnos.NumeroVentanillas];
        private Button[] btnLlamar = new Button[SistemaTurnos.NumeroVentanillas];
        private Button[] btnFinalizar = new Button[SistemaTurnos.NumeroVentanillas];
        private Label lblUltimoLlamado, lblEstadisticas;

        // Calculadora
        private TextBox txtExpresion, txtTraza;
        private Label lblPostfija, lblResultado;
        private CheckBox chkTraza;

        // Estructuras
        private TextBox txtEstructuras;

        public FormPrincipal()
        {
            Controles.ConfigurarVentana(this, "Sucursal bancaria | Estructuras lineales estaticas", 900, 600);

            tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            Controls.Add(tabs);
            barra = Controles.BarraEstado(this);

            ConstruirVentanillas(Controles.Pestana(tabs, "Atencion en ventanilla"));
            ConstruirCalculadora(Controles.Pestana(tabs, "Calculadora del cajero"));
            ConstruirEstructuras(Controles.Pestana(tabs, "Estructuras internas"));

            tabs.SelectedIndexChanged += delegate { RefrescarTurnos(); };
            RefrescarTurnos();
        }

        // ================================================================ construcción

        private void ConstruirVentanillas(TabPage p)
        {
            GroupBox gN = Controles.Grupo(p, "Llega un cliente", 10, 5, 420, 135);
            Controles.Etiqueta(gN, "Nombre:", 10, 28).Font = Controles.Normal;
            txtCliente = Controles.Caja(gN, 75, 25, 330);
            txtCliente.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; EmitirTurno(); } };

            string[] nombres = { "Deposito", "Retiro", "Pago de servicios", "Consulta" };
            for (int i = 0; i < radiosTramite.Length; i++)
            {
                RadioButton r = new RadioButton();
                r.Text = nombres[i];
                r.Font = Controles.Normal;
                r.AutoSize = true;
                r.Location = new Point(10 + (i % 2) * 150, 60 + (i / 2) * 25);
                r.Checked = i == 0;
                gN.Controls.Add(r);
                radiosTramite[i] = r;
            }
            Button bE = Controles.Boton(gN, "Emitir turno", 305, 95, 100, delegate { EmitirTurno(); });
            bE.Font = Controles.Normal;

            GroupBox gF = Controles.Grupo(p, "Fila de espera (Cola circular)", 10, 145, 420, 360);
            txtFila = Controles.Area(gF, 10, 22, 400, 328);

            for (int i = 0; i < SistemaTurnos.NumeroVentanillas; i++)
            {
                int numero = i + 1; // copia local para el cierre (closure)
                GroupBox gV = Controles.Grupo(p, "Ventanilla " + numero, 440, 5 + i * 110, 215, 105);
                lblVentanillas[i] = Controles.Etiqueta(gV, "", 10, 22, 195);
                lblVentanillas[i].Font = Controles.Normal;
                lblVentanillas[i].Size = new Size(195, 38);
                btnLlamar[i] = Controles.Boton(gV, "Llamar siguiente", 10, 64, 105, delegate { Llamar(numero); });
                btnFinalizar[i] = Controles.Boton(gV, "Finalizar", 120, 64, 85, delegate { Finalizar(numero); });
                btnLlamar[i].Font = Controles.Normal;
                btnFinalizar[i].Font = Controles.Normal;
            }

            GroupBox gP = Controles.Grupo(p, "Pantalla de llamados", 665, 5, 210, 330);
            lblUltimoLlamado = Controles.Etiqueta(gP, "---", 10, 25, 190, Controles.Grande);
            lblUltimoLlamado.Size = new Size(190, 70);
            lblUltimoLlamado.TextAlign = ContentAlignment.MiddleCenter;
            lblUltimoLlamado.BackColor = Color.FromArgb(20, 30, 50);
            lblUltimoLlamado.ForeColor = Color.FromArgb(255, 210, 60);
            txtPantalla = Controles.Area(gP, 10, 105, 190, 215);
            txtPantalla.BackColor = Color.FromArgb(20, 30, 50);
            txtPantalla.ForeColor = Color.White;

            lblEstadisticas = Controles.Etiqueta(p, "", 440, 345, 435);
            lblEstadisticas.Size = new Size(435, 150);
        }

        private void ConstruirCalculadora(TabPage p)
        {
            Controles.Etiqueta(p, "Expresion (operadores + - * / % ^   agrupadores ( ) [ ] { }   decimales '.' o ','):", 10, 10);
            txtExpresion = Controles.Caja(p, 10, 33, 600);
            txtExpresion.Font = Controles.Mono;
            txtExpresion.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Evaluar(); } };
            Controles.Boton(p, "Evaluar", 620, 30, 120, delegate { Evaluar(); });
            Controles.Boton(p, "Limpiar", 750, 30, 120, delegate
            {
                txtExpresion.Clear();
                txtTraza.Clear();
                lblPostfija.Text = "Postfija:";
                lblResultado.Text = "= ";
                txtExpresion.Focus();
            });

            Controles.Etiqueta(p, "Ejemplos de la sucursal:", 10, 75);
            for (int i = 0; i < Ejemplos.Length; i++)
            {
                int indice = i;
                Button b = Controles.Boton(p, DescripcionEjemplos[i], 160 + i * 143, 68, 140, delegate
                {
                    txtExpresion.Text = Ejemplos[indice];
                    Evaluar();
                });
                b.Font = new Font("Segoe UI", 8F);
            }

            lblPostfija = Controles.Etiqueta(p, "Postfija:", 10, 112, 860);
            lblPostfija.Font = Controles.Mono;
            lblResultado = Controles.Etiqueta(p, "= ", 10, 140, 860, Controles.Grande);
            lblResultado.Size = new Size(860, 36);

            chkTraza = new CheckBox();
            chkTraza.Text = "Mostrar traza de la pila de operandos";
            chkTraza.Checked = true;
            chkTraza.AutoSize = true;
            chkTraza.Location = new Point(10, 185);
            p.Controls.Add(chkTraza);

            txtTraza = Controles.Area(p, 10, 210, 865, 295);
        }

        private void ConstruirEstructuras(TabPage p)
        {
            Controles.Etiqueta(p, "Punteros front / rear y celdas fisicas de las colas circulares", 10, 10, 0, Controles.Negrita);
            txtEstructuras = Controles.Area(p, 10, 35, 865, 470);
        }

        // ================================================================ ventanillas

        private void EmitirTurno()
        {
            string cliente = txtCliente.Text.Trim();
            if (cliente.Length == 0)
            {
                Controles.Mensaje(barra, "Escribe el nombre del cliente.", Controles.ColorAviso);
                txtCliente.Focus();
                return;
            }

            TipoOperacion tipo = TipoOperacion.Deposito;
            for (int i = 0; i < radiosTramite.Length; i++)
                if (radiosTramite[i].Checked)
                    tipo = (TipoOperacion)(i + 1);

            Turno turno;
            if (sistema.EmitirTurno(cliente, tipo, out turno))
            {
                Controles.Mensaje(barra, "Turno emitido: " + turno.Folio + " para " + cliente +
                                         ". Personas delante: " + (sistema.Fila.Cantidad - 1), Controles.ColorExito);
                txtCliente.Clear();
            }
            else
            {
                Controles.Mensaje(barra, "La fila esta llena (" + SistemaTurnos.CapacidadFila + "). El cliente debe regresar mas tarde.", Controles.ColorError);
            }
            txtCliente.Focus();
            RefrescarTurnos();
        }

        private void Llamar(int ventanilla)
        {
            Turno turno;
            switch (sistema.LlamarSiguiente(ventanilla, out turno))
            {
                case ResultadoLlamado.Llamado:
                    Controles.Mensaje(barra, "Turno " + turno.Folio + " (" + turno.Cliente + ") pasar a ventanilla " + ventanilla +
                                             ". Espero " + turno.SegundosDeEspera().ToString("0") + " s.", Controles.ColorExito);
                    System.Media.SystemSounds.Asterisk.Play();
                    break;
                case ResultadoLlamado.VentanillaOcupada:
                    Controles.Mensaje(barra, "La ventanilla " + ventanilla + " sigue ocupada. Finalice la atencion primero.", Controles.ColorAviso);
                    break;
                case ResultadoLlamado.FilaVacia:
                    Controles.Mensaje(barra, "No hay clientes en espera.", Controles.ColorAviso);
                    break;
                default:
                    Controles.Mensaje(barra, "Ventanilla invalida.", Controles.ColorError);
                    break;
            }
            RefrescarTurnos();
        }

        private void Finalizar(int ventanilla)
        {
            Turno turno;
            if (sistema.FinalizarAtencion(ventanilla, out turno))
                Controles.Mensaje(barra, "Ventanilla " + ventanilla + " termino con " + turno.Folio + ". Queda libre.", Controles.ColorExito);
            else
                Controles.Mensaje(barra, "La ventanilla " + ventanilla + " no estaba atendiendo a nadie.", Controles.ColorAviso);
            RefrescarTurnos();
        }

        private void RefrescarTurnos()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sistema.Fila.Cantidad; i++)
            {
                Turno t = sistema.Fila.ElementoEn(i);
                sb.AppendLine((i == 0 ? "front-> " : "        ") + (i + 1) + ". [celda " + sistema.Fila.IndiceFisico(i) + "] " +
                              t.Folio + "  " + Turno.Nombre(t.Operacion).PadRight(17) + " " + t.Cliente);
            }
            txtFila.Text = sb.Length == 0 ? "(no hay clientes en espera)" : sb.ToString();

            for (int i = 0; i < SistemaTurnos.NumeroVentanillas; i++)
            {
                Ventanilla v = sistema.ObtenerVentanilla(i + 1);
                lblVentanillas[i].Text = v.EstaLibre ? "LIBRE" : "Atendiendo " + v.TurnoActual.Folio + Environment.NewLine + v.TurnoActual.Cliente;
                lblVentanillas[i].ForeColor = v.EstaLibre ? Controles.ColorExito : Controles.ColorInfo;
                btnLlamar[i].Enabled = v.EstaLibre && !sistema.Fila.IsEmpty();
                btnFinalizar[i].Enabled = !v.EstaLibre;
            }

            // pantalla: el más reciente está al final de la cola circular
            sb = new StringBuilder();
            for (int i = sistema.Pantalla.Cantidad - 1; i >= 0; i--)
                sb.AppendLine(sistema.Pantalla.ElementoEn(i));
            txtPantalla.Text = sb.ToString();
            lblUltimoLlamado.Text = sistema.Pantalla.IsEmpty()
                ? "---"
                : sistema.Pantalla.ElementoEn(sistema.Pantalla.Cantidad - 1).Replace(" -> ", Environment.NewLine);

            lblEstadisticas.Text =
                "En fila: " + sistema.Fila.Cantidad + "/" + SistemaTurnos.CapacidadFila + Environment.NewLine +
                "Atendidos: " + sistema.Atendidos + Environment.NewLine +
                "Sin ficha (fila llena): " + sistema.Rechazados + Environment.NewLine +
                "Espera promedio: " + sistema.EsperaPromedio().ToString("0.0") + " s" + Environment.NewLine +
                sistema.Fila.DescribirIndices();

            txtEstructuras.Text = DescribirEstructuras();
        }

        // ================================================================ calculadora

        private void Evaluar()
        {
            ResultadoEvaluacion r = evaluador.Evaluar(txtExpresion.Text);

            lblPostfija.Text = "Postfija: " + (r.Postfija ?? "-");
            txtTraza.Text = chkTraza.Checked && r.Traza != null
                ? "Evaluacion con la pila de operandos:" + Environment.NewLine + r.Traza
                : "";

            if (r.Exito)
            {
                lblResultado.ForeColor = Controles.ColorExito;
                lblResultado.Text = "= " + EvaluadorExpresiones.Num(r.Valor) + "     ($" + r.Valor.ToString("N2") + ")";
                Controles.Mensaje(barra, "Expresion evaluada correctamente.", Controles.ColorExito);
            }
            else
            {
                lblResultado.ForeColor = Controles.ColorError;
                lblResultado.Text = "Error";
                Controles.Mensaje(barra, "Error: " + r.Error, Controles.ColorError);
            }
        }

        // ================================================================ estructuras

        private string DescribirEstructuras()
        {
            StringBuilder sb = new StringBuilder();
            AgregarCola(sb, "[ColaCircular] Fila de turnos", sistema.Fila);
            sb.AppendLine();
            sb.AppendLine("[ColaCircular] Pantalla de llamados  " + sistema.Pantalla.DescribirIndices());
            sb.AppendLine();
            sb.AppendLine("Las pilas y colas simples de la calculadora se crean en cada evaluacion");
            sb.AppendLine("(capacidad " + EvaluadorExpresiones.MaximoTokens + " tokens). La traza de la pestana Calculadora muestra la pila de operandos.");
            return sb.ToString();
        }

        private static void AgregarCola(StringBuilder sb, string titulo, ColaCircular<Turno> cola)
        {
            sb.AppendLine(titulo + "  " + cola.DescribirIndices());
            string[] celdas = new string[cola.Capacidad];
            for (int i = 0; i < celdas.Length; i++)
                celdas[i] = "[ ----- ]";
            for (int p = 0; p < cola.Cantidad; p++)
                celdas[cola.IndiceFisico(p)] = "[ " + cola.ElementoEn(p).Folio.PadRight(5) + " ]";

            sb.Append("   indice: ");
            for (int i = 0; i < celdas.Length; i++)
                sb.Append(("    " + i + "     ").Substring(0, 9));
            sb.AppendLine();
            sb.Append("   celdas: ");
            for (int i = 0; i < celdas.Length; i++)
                sb.Append(celdas[i]);
            sb.AppendLine();
            if (!cola.IsEmpty())
            {
                sb.Append("           ");
                for (int i = 0; i < celdas.Length; i++)
                {
                    string marca = (i == cola.Front ? "F" : "") + (i == cola.Rear ? "R" : "");
                    sb.Append(("    " + marca + "       ").Substring(0, 9));
                }
                sb.AppendLine("  (F = front, R = rear)");
            }
        }
    }
}
