using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2_LFP_2s2019 {
    class Tokens {

        public string palabra { get; set; }
        public string tipoPalabra { get; set; }


        public Tokens(string palabra, string tipoPalabra) {
            this.palabra = palabra;
            this.tipoPalabra = tipoPalabra;
        }
    }
}
