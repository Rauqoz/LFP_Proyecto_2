using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2_LFP_2s2019 {
    class Tokens_Error {

        public string palabra { get; set; }
        public int fila { get; set; }
        public int columna { get; set; }

        public string tipoError { get; set; }

        public Tokens_Error(string palabra, int fila, int columna, string tipoError) {
            this.palabra = palabra;
            this.fila = fila;
            this.columna = columna;
            this.tipoError = tipoError;
        }
    }
}
