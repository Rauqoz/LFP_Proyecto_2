using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2_LFP_2s2019 {
    public partial class Form1 : Form {

        Analizador analizador = new Analizador();
        public Form1() {
            InitializeComponent();
            tAnalizar.Clear();
            tPython.Clear();
            tConsola.Clear();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void generarTraduccionToolStripMenuItem_Click(object sender, EventArgs e) {
            tConsola.Clear();
            tPython.Clear();
            analizador.Lexico(tAnalizar, tPython);
            if (tPython.Text.Length != 0) {
                analizador.guardarPython(tPython, tConsola);
            }
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e) {
            tAnalizar.Clear();
            tPython.Clear();
            tConsola.Clear();
            analizador.Leer(tAnalizar);
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void tablaDeTokensReconocidosToolStripMenuItem_Click(object sender, EventArgs e) {
            analizador.htmlTokens();
        }

        private void tablaDeSimbolosToolStripMenuItem_Click(object sender, EventArgs e) {
            analizador.htmlSimbolos();
        }

        private void acerdaDeToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show("Raul Quiñonez - 201503903");
            string manualTecnico = Path.Combine(Application.StartupPath, "Manual_Tecnico_Proyecto2.pdf");
            string manualUsuario = Path.Combine(Application.StartupPath, "Manual_Usuario_Proyecto2.pdf");
            Process abrir1 = new Process();
            abrir1.StartInfo.FileName = manualTecnico;
            Process abrir2 = new Process();
            abrir2.StartInfo.FileName = manualUsuario;
            abrir1.Start();
            abrir2.Start();
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e) {
            if (tAnalizar.Text.Length != 0) {
                analizador.guardarArchivo(tAnalizar);
            } else {
                MessageBox.Show("No hay Texto");
            }
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e) {
            analizador.htmlErrores();
        }

        private void archivoToolStripMenuItem_Click(object sender, EventArgs e) {

        }

        private void limpiarDocumentosResientesToolStripMenuItem_Click(object sender, EventArgs e) {
            tAnalizar.Clear();
            tPython.Clear();
            tConsola.Clear();
            analizador.borrarHistorial();
        }

        private void ayudaToolStripMenuItem_Click(object sender, EventArgs e) {

        }
    }
}
