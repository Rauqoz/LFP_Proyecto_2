using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2_LFP_2s2019 {
    class traduccion {
        public string palabra { get; set; }
        public string Referencia { get; set; }

        public Boolean saltoLinea { get; set; }

        public traduccion(string palabra, string referencia, bool saltoLinea) {
            this.palabra = palabra;
            Referencia = referencia;
            this.saltoLinea = saltoLinea;
        }
    }
}
