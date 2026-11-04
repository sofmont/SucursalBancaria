using System;
using System.Drawing;
using System.Windows.Forms;

namespace SucursalBancaria.UI
{
    /// <summary>Fábrica de controles con estilo uniforme (posicionamiento absoluto).</summary>
    internal static class Controles
    {
        public static readonly Font Normal = new Font("Segoe UI", 9F);
        public static readonly Font Negrita = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font Mono = new Font("Consolas", 9.5F);
        public static readonly Font Grande = new Font("Segoe UI", 16F, FontStyle.Bold);

        public static readonly Color ColorExito = Color.FromArgb(0, 120, 60);
        public static readonly Color ColorError = Color.FromArgb(190, 30, 30);
        public static readonly Color ColorAviso = Color.FromArgb(170, 110, 0);
        public static readonly Color ColorInfo = Color.FromArgb(0, 90, 160);

        public static Button Boton(Control padre, string texto, int x, int y, int ancho, EventHandler clic)
        {
            Button b = new Button();
            b.Text = texto;
            b.Font = Normal;
            b.Location = new Point(x, y);
            b.Size = new Size(ancho, 30);
            b.Click += clic;
            padre.Controls.Add(b);
            return b;
        }

        public static Label Etiqueta(Control padre, string texto, int x, int y, int ancho = 0, Font fuente = null)
        {
            Label l = new Label();
            l.Text = texto;
            l.Font = fuente ?? Normal;
            l.Location = new Point(x, y);
            if (ancho > 0)
                l.Size = new Size(ancho, 22);
            else
                l.AutoSize = true;
            padre.Controls.Add(l);
            return l;
        }

        public static TextBox Caja(Control padre, int x, int y, int ancho)
        {
            TextBox t = new TextBox();
            t.Font = Normal;
            t.Location = new Point(x, y);
            t.Width = ancho;
            padre.Controls.Add(t);
            return t;
        }

        /// <summary>Área de texto multilínea de solo lectura con fuente monoespaciada.</summary>
        public static TextBox Area(Control padre, int x, int y, int ancho, int alto)
        {
            TextBox t = new TextBox();
            t.Multiline = true;
            t.ReadOnly = true;
            t.WordWrap = false;
            t.ScrollBars = ScrollBars.Both;
            t.BackColor = Color.White;
            t.Font = Mono;
            t.Location = new Point(x, y);
            t.Size = new Size(ancho, alto);
            padre.Controls.Add(t);
            return t;
        }

        public static GroupBox Grupo(Control padre, string texto, int x, int y, int ancho, int alto)
        {
            GroupBox g = new GroupBox();
            g.Text = texto;
            g.Font = Negrita;
            g.Location = new Point(x, y);
            g.Size = new Size(ancho, alto);
            padre.Controls.Add(g);
            return g;
        }

        public static NumericUpDown Numero(Control padre, int x, int y, int ancho, int min, int max, int valor)
        {
            NumericUpDown n = new NumericUpDown();
            n.Font = Normal;
            n.Location = new Point(x, y + 3);
            n.Width = ancho;
            n.Minimum = min;
            n.Maximum = max;
            n.Value = valor;
            padre.Controls.Add(n);
            return n;
        }

        public static TabPage Pestana(TabControl tabs, string texto)
        {
            TabPage p = new TabPage(texto);
            p.Font = Normal;
            p.BackColor = SystemColors.Control;
            tabs.TabPages.Add(p);
            return p;
        }

        /// <summary>Aplica la configuración común de ventana (tamaño fijo, centrada).</summary>
        public static void ConfigurarVentana(Form f, string titulo, int ancho, int alto)
        {
            f.Text = titulo;
            f.Font = Normal;
            f.ClientSize = new Size(ancho, alto);
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>Barra de estado inferior para mensajes al usuario.</summary>
        public static Label BarraEstado(Form f)
        {
            Label l = new Label();
            l.Dock = DockStyle.Bottom;
            l.Height = 28;
            l.Font = Negrita;
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.Padding = new Padding(8, 0, 0, 0);
            l.BorderStyle = BorderStyle.FixedSingle;
            l.Text = "Listo.";
            f.Controls.Add(l);
            return l;
        }

        public static void Mensaje(Label barra, string texto, Color color)
        {
            barra.ForeColor = color;
            barra.Text = texto;
        }
    }
}
