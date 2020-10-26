using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Proyecto2_LFP_2s2019 {
    class Analizador {

        public List<Tokens> lexicoBuenas = new List<Tokens>();
        public List<Tokens_Error> lexicoMalas = new List<Tokens_Error>();
        string[] tipoPalabra = { "Numero", "Reservada", "Identificador", "Cadena", "Comentario", "Simbolo", "Caracter" };
        string[] palabrasReservadas = { "int", "float", "char", "string", "bool", "new", "class", "static", "void", "main", "args", "console", "writeline", "graficarvector", "if", "else", "switch", "case", "break", "default", "for", "while", "true", "false" };
        Boolean armoPalabra = false;
        OpenFileDialog archivo;
        SaveFileDialog guardar;
        int ilex;
        string nombreDelArchivoEntrada = "";

        string output = "";
        string error = "";


        List<String> Pila = new List<String>();
        List<traduccion> traduccionFinal = new List<traduccion>();
        List<traduccion> pre_traduccion = new List<traduccion>();
        public List<traduccion> erroresSintacticos = new List<traduccion>();
        List<traduccion> pre_erroresSintacticos = new List<traduccion>();


        public Analizador() {

        }

        public void Leer(RichTextBox caja) {
            archivo = new OpenFileDialog();
            archivo.DefaultExt = "txt";
            archivo.Filter = "Archivos de texto (*.txt)|*.txt";

            if (archivo.ShowDialog() == DialogResult.OK) {
                nombreDelArchivoEntrada = archivo.SafeFileName;
                StreamReader leer;
                leer = new StreamReader(archivo.FileName);
                while (leer.Peek() > -1) {
                    //peek revisa el siguiente caracter y tiene que ser mayor a -1 que representa que no hay nada
                    String linea = leer.ReadLine().Trim();
                    if (!String.IsNullOrEmpty(linea)) {
                        caja.AppendText(linea + "\n");

                    }
                }

            } else {
                MessageBox.Show("Error Archivo no Seleccionado");

            }

        }

        public void guardarArchivo(RichTextBox caja) {

            guardar = new SaveFileDialog();
            guardar.Filter = "Archivos de texto (*.cs)|*.cs";
            guardar.DefaultExt = "cs";
            guardar.ShowDialog();
            if (guardar.FileName != "") {
                File.WriteAllText(guardar.FileName, caja.Text);
            } else {
                MessageBox.Show("No hay Nombre");
            }

        }

        public void guardarPython(RichTextBox caja, RichTextBox consola) {
            consola.Clear();
            string archivoPython = Path.Combine(Application.StartupPath, "python.py");
            File.WriteAllText(archivoPython, caja.Text);
            string command = "python " + archivo;

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            Process process = Process.Start(processInfo);

            // *** Read the streams ***
            process.WaitForExit();
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
            consola.AppendText(output);
            consola.AppendText(error);
            process.Close();
            output = "";
            error = "";


        }

        public void Lexico(RichTextBox caja, RichTextBox python) {
            python.Clear();
            if (caja.Text.Length == 0) {
                MessageBox.Show("Caja Vacia");
            } else {

                lexicoBuenas.Clear();
                lexicoMalas.Clear();

                double opcion = 0;
                String palabraArmada = "";
                int filaE = 0;
                int columnaE = 0;
                filaE += 1;
                char[] letra = caja.Text.ToArray();
                for (int i = 0; i < letra.Length; i++) {
                    //Console.WriteLine(palabraArmada);
                    switch (opcion) {
                        case 0:
                            palabraArmada = "";
                            if (letra[i].Equals('\n')) {
                                filaE++;
                                columnaE = 1;
                            } else if (char.IsWhiteSpace(letra[i])) {

                            } else if (char.IsDigit(letra[i])) {
                                i--;
                                opcion = 1;
                            } else if (char.IsLetter(letra[i])) {
                                i--;
                                opcion = 2;
                            } else {
                                i--;
                                opcion = 3;
                            }
                            break;
                        case 0.1:
                            if (char.IsWhiteSpace(letra[i])) {
                                lexicoMalas.Add(new Tokens_Error(palabraArmada, filaE, columnaE, "Error Lexico"));
                                opcion = 0;

                            } else {
                                palabraArmada += letra[i];
                            }
                            break;
                        case 1:
                            if (char.IsDigit(letra[i])) {
                                palabraArmada += letra[i];
                                columnaE++;

                            } else if (letra[i].Equals('.')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 1.1;
                            } else if (char.IsWhiteSpace(letra[i])) {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[0] + "_entero"));
                                columnaE++;
                                opcion = 0;
                            } else if (char.IsLetter(letra[i])) {
                                i--;
                                opcion = 0.1;
                            } else {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[0] + "_entero"));
                                opcion = 0;
                                i--;
                            }
                            break;
                        case 1.1:
                            if (char.IsDigit(letra[i])) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 1.2;

                            } else if (letra[i].Equals("f")) {
                                columnaE++;
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[0] + "_decimal"));
                                opcion = 0;
                            } else if (char.IsWhiteSpace(letra[i])) {
                                palabraArmada += "0";
                                columnaE++;
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[0] + "_decimal"));
                                opcion = 0;
                            } else {
                                i--;
                                opcion = 0.1;
                            }
                            break;
                        case 1.2:
                            if (char.IsDigit(letra[i])) {
                                palabraArmada += letra[i];
                                columnaE++;

                            } else if (char.IsWhiteSpace(letra[i]) | letra[i].Equals('f')) {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[0] + "_decimal"));
                                opcion = 0;
                            } else if (char.IsLetter(letra[i])) {
                                i--;
                                opcion = 0.1;
                            } else {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[0] + "_decimal"));
                                opcion = 0;
                                i--;
                            }
                            break;
                        case 2:
                            if (char.IsLetter(letra[i])) {
                                palabraArmada += letra[i];
                                columnaE++;

                            } else if (char.IsDigit(letra[i])) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 2.1;
                            } else {
                                for (int j = 0; j < palabrasReservadas.Length; j++) {

                                    if (palabraArmada.ToLower().Equals("true") | palabraArmada.ToLower().Equals("false")) {
                                        lexicoBuenas.Add(new Tokens(palabraArmada, "true_false"));
                                        armoPalabra = true;
                                        break;

                                    } else if (palabraArmada.ToLower().Equals(palabrasReservadas[j])) {
                                        lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[1] + "_" + palabraArmada.ToLower()));
                                        armoPalabra = true;
                                        break;
                                    }
                                }

                                if (armoPalabra == false) {
                                    lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[2]));
                                }
                                armoPalabra = false;
                                opcion = 0;
                                i--;
                            }
                            break;
                        case 2.1:
                            if (char.IsLetter(letra[i]) | char.IsDigit(letra[i])) {
                                palabraArmada += letra[i];
                                columnaE++;

                            } else if (char.IsWhiteSpace(letra[i]) | letra[i].Equals(';')) {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[2]));
                                opcion = 0;

                            } else {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[2]));
                                i--;
                                opcion = 0;
                            }
                            break;
                        case 3:
                            if (letra[i].Equals('"')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 3.1;
                            } else if (letra[i].Equals('\'')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 3.11;
                            } else if (letra[i].Equals('/')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 3.2;
                            } else {
                                palabraArmada += letra[i];
                                revisarSimbolo(letra[i]);
                                opcion = 0;
                            }
                            break;
                        case 3.11:
                            if (letra[i].Equals('\'')) {
                                palabraArmada += letra[i];
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[6]));
                                opcion = 0;
                            } else {
                                palabraArmada += letra[i];
                            }
                            break;
                        case 3.1:
                            if (letra[i].Equals('"')) {
                                palabraArmada += letra[i];
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[3]));
                                opcion = 0;
                            } else {
                                palabraArmada += letra[i];
                            }
                            break;
                        case 3.2:
                            if (letra[i].Equals('/')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 3.21;
                            } else if (letra[i].Equals('*')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 3.22;
                            } else if (char.IsWhiteSpace(letra[i])) {
                                revisarSimbolo(letra[i]);
                                opcion = 0;

                            } else {
                                lexicoBuenas.Add(new Tokens(palabraArmada, "diagonal"));
                                i--;
                                opcion = 0;
                            }
                            break;
                        case 3.21:
                            if (letra[i].Equals('\n')) {
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[4]));
                                opcion = 0;
                            } else {
                                palabraArmada += letra[i];
                                columnaE++;
                            }
                            break;
                        case 3.22:
                            if (letra[i].Equals('*')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                opcion = 3.23;
                            } else {
                                palabraArmada += letra[i];
                                columnaE++;
                            }
                            break;
                        case 3.23:
                            if (letra[i].Equals('/')) {
                                palabraArmada += letra[i];
                                columnaE++;
                                lexicoBuenas.Add(new Tokens(palabraArmada, tipoPalabra[4]));
                                opcion = 0;
                            } else {
                                i--;
                                opcion = 0.1;
                            }
                            break;

                        default:
                            break;
                    }

                    Console.WriteLine(palabraArmada);

                }

                Console.WriteLine("--------------");
                if (lexicoBuenas.Count != 0) {
                    Console.WriteLine("Lexemas Buenos >>>> \n");
                    for (int i = 0; i < lexicoBuenas.Count; i++) {
                        Console.WriteLine("palabra: " + lexicoBuenas[i].palabra + " tipo: " + lexicoBuenas[i].tipoPalabra);
                    }
                }

                if (lexicoMalas.Count != 0) {
                    Console.WriteLine("\n Lexemas Malos >>>> \n");
                    for (int i = 0; i < lexicoMalas.Count; i++) {
                        Console.WriteLine(lexicoMalas[i].palabra + " " + lexicoMalas[i].tipoError);
                    }
                }

                void revisarSimbolo(char signo) {
                    string simbolo = "";
                    bool entra = true;
                    switch (signo) {
                        case '{':
                            simbolo = "llave_abrir";
                            break;
                        case '}':
                            simbolo = "llave_cerrar";
                            break;
                        case '(':
                            simbolo = "parentesis_abrir";
                            break;
                        case ')':
                            simbolo = "parentesis_cerrar";
                            break;
                        case '[':
                            simbolo = "corchete_abrir";
                            break;
                        case ']':
                            simbolo = "corchete_cerrar";
                            break;
                        case '<':
                            simbolo = "menor_que";
                            break;
                        case '>':
                            simbolo = "mayor_que";
                            break;
                        case '=':
                            simbolo = "igual";
                            break;
                        case '+':
                            simbolo = "mas";
                            break;
                        case '-':
                            simbolo = "menos";
                            break;
                        case '*':
                            simbolo = "asterisco";
                            break;
                        case '/':
                            simbolo = "diagonal";
                            break;
                        case ',':
                            simbolo = "coma";
                            break;
                        case ';':
                            simbolo = "punto_coma";
                            break;
                        case ':':
                            simbolo = "dos_puntos";
                            break;
                        case '\'':
                            simbolo = "apostrofe";
                            break;
                        case '.':
                            simbolo = "punto";
                            break;
                        case '!':
                            simbolo = "exclamacion";
                            break;
                        case '&':
                            simbolo = "y";
                            break;
                        case '|':
                            simbolo = "or";
                            break;

                        default:
                            entra = false;
                            break;
                    }

                    if (entra) {
                        lexicoBuenas.Add(new Tokens(palabraArmada, simbolo));
                    } else {
                        lexicoMalas.Add(new Tokens_Error(palabraArmada, filaE, columnaE, "Inexistente"));
                    }
                }

                Sintactico(python);
            }


        }

        public void Sintactico(RichTextBox caja) {
            Console.WriteLine("\n Analisis Sintactico >>>>>>>>>>> \n");
            ilex = 0;
            string tipoVariable, valorVariable = "", signoOperador = "", signoCondicional = "";
            Boolean bueno_malo = true;

            Pila.Clear();
            Pila.Add("#");
            Pila.Add("INICIO");
            while (ilex != lexicoBuenas.Count) {
                switch (Pila.Last()) {
                    case "INICIO":
                        inicio();
                        break;
                    case "MAIN":
                        main();
                        break;
                    case "ARGUMENTOS":
                        argumentos();
                        break;
                    case "CODIGO":
                        pre_traduccion.Clear();
                        pre_erroresSintacticos.Clear();
                        bueno_malo = true;
                        if (paraDeclaracion(lexicoBuenas[ilex].tipoPalabra) == true) {
                            Console.WriteLine("*** Comienza Declaracion de Variables");
                            eliminarUltimo();
                            Pila.Add("CODIGO");
                            Pila.Add("DECLARACION");
                            runCodigo();
                            for (int i = 0; i < pre_traduccion.Count; i++) {
                                traduccionFinal.Add(pre_traduccion[i]);
                            }
                            if (bueno_malo == true) {
                                pre_erroresSintacticos.Clear();
                            } else {
                                for (int i = 0; i < pre_erroresSintacticos.Count; i++) {
                                    erroresSintacticos.Add(pre_erroresSintacticos[i]);
                                }
                            }

                        } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                            Console.WriteLine("*** Comienza Asignacion");
                            buscarTipo(lexicoBuenas[ilex].palabra);
                            eliminarUltimo();
                            Pila.Add("CODIGO");
                            Pila.Add("ASIGNACION");
                            runCodigo();
                            for (int i = 0; i < pre_traduccion.Count; i++) {
                                traduccionFinal.Add(pre_traduccion[i]);
                            }
                            if (bueno_malo == true) {
                                pre_erroresSintacticos.Clear();
                            } else {
                                for (int i = 0; i < pre_erroresSintacticos.Count; i++) {
                                    erroresSintacticos.Add(pre_erroresSintacticos[i]);
                                }
                            }


                        } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Comentario")) {
                            eliminarUltimo();
                            Pila.Add("CODIGO");
                            Pila.Add("Comentario");
                            traduccionFinal.Add(new traduccion(lexicoBuenas[ilex].palabra, lexicoBuenas[ilex].tipoPalabra, true));
                            Console.WriteLine("se agrego comentario");
                            if (bueno_malo == true) {
                                pre_erroresSintacticos.Clear();
                            } else {
                                for (int i = 0; i < pre_erroresSintacticos.Count; i++) {
                                    erroresSintacticos.Add(pre_erroresSintacticos[i]);
                                }
                            }

                        } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_console")) {
                            eliminarUltimo();
                            Pila.Add("CODIGO");
                            Pila.Add("PRINT");
                            runCodigo();
                            for (int i = 0; i < pre_traduccion.Count; i++) {
                                traduccionFinal.Add(pre_traduccion[i]);
                            }
                            Console.WriteLine("se agrego impresion");
                            if (bueno_malo == true) {
                                pre_erroresSintacticos.Clear();
                            } else {
                                for (int i = 0; i < pre_erroresSintacticos.Count; i++) {
                                    erroresSintacticos.Add(pre_erroresSintacticos[i]);
                                }
                            }

                        } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_if")) {
                            eliminarUltimo();
                            Pila.Add("CODIGO");
                            Pila.Add("MIF");
                            runCodigo();
                            for (int i = 0; i < pre_traduccion.Count; i++) {
                                traduccionFinal.Add(pre_traduccion[i]);
                            }
                            if (bueno_malo == true) {
                                pre_erroresSintacticos.Clear();
                            } else {
                                for (int i = 0; i < pre_erroresSintacticos.Count; i++) {
                                    erroresSintacticos.Add(pre_erroresSintacticos[i]);
                                }
                            }
                        } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_while")) {
                            eliminarUltimo();
                            Pila.Add("CODIGO");
                            Pila.Add("MWHILE");
                            runCodigo();
                            for (int i = 0; i < pre_traduccion.Count; i++) {
                                traduccionFinal.Add(pre_traduccion[i]);
                            }
                            if (bueno_malo == true) {
                                pre_erroresSintacticos.Clear();
                            } else {
                                for (int i = 0; i < pre_erroresSintacticos.Count; i++) {
                                    erroresSintacticos.Add(pre_erroresSintacticos[i]);
                                }
                            }
                        } else {
                            eliminarUltimo();
                        }
                        break;


                    default:
                        if (Pila.Last().Equals(lexicoBuenas[ilex].tipoPalabra)) {
                            eliminarUltimo();
                            ilex++;
                        } else {
                            Console.WriteLine("ERROR SINTACTICO: " + lexicoBuenas[ilex].tipoPalabra + " en " + Pila.Last());
                            erroresSintacticos.Add(new traduccion(lexicoBuenas[ilex].palabra, lexicoBuenas[ilex].tipoPalabra, false));
                            ilex++;
                        }
                        break;
                }
            }

            if (Pila.Last().Equals("#")) {
                eliminarUltimo();
                Console.WriteLine("TERMINO EXITOSO");
                traducir(caja, traduccionFinal);
            }

            void eliminarUltimo() {
                Pila.RemoveAt(Pila.LastIndexOf(Pila.Last()));
            }

            void metodoIf() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_if")) {
                    Pila.Add("MELSE");
                    Pila.Add("llave_cerrar");
                    Pila.Add("INSTRUCCION");
                    Pila.Add("llave_abrir");
                    Pila.Add("parentesis_cerrar");
                    Pila.Add("CONDICIONAl");
                    Pila.Add("parentesis_abrir");
                    Pila.Add("Reservada_if");
                } else {
                    Pila.Add("NO EXISTE IF");
                }
            }

            void condicional() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    if (buscarTipo(lexicoBuenas[ilex].palabra) == true) {
                        Pila.Add("PSIMBOLO");
                        Pila.Add("Identificador");
                    }

                } else if (paraElTipoDeValor(lexicoBuenas[ilex].tipoPalabra) == true) {
                    Pila.Add("PSIMBOLO");
                    Pila.Add(valorVariable);
                } else {
                    Pila.Add("ERROR EN EL CONDICIONAL");
                }
            }

            void psimbolo() {
                eliminarUltimo();
                if (paraCondicionales(lexicoBuenas[ilex].palabra) == true) {
                    Pila.Add("SSIMBOLO");
                    Pila.Add(signoCondicional);
                } else {
                    Pila.Add("ENLACE");
                }
            }

            void ssimbolo() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("igual")) {
                    Pila.Add("VALORC");
                    Pila.Add("igual");
                } else {
                    Pila.Add("VALORC");
                }
            }

            void valorc() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Numero_entero")) {
                    Pila.Add("ENLACE");
                    Pila.Add(lexicoBuenas[ilex].tipoPalabra);
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Numero_decimal")) {
                    Pila.Add("ENLACE");
                    Pila.Add(lexicoBuenas[ilex].tipoPalabra);
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    Pila.Add("ENLACE");
                    Pila.Add("Identificador");
                } else {
                    Pila.Add("NO EXISTE EL VALOR");
                }
            }

            void enlace() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("y")) {
                    Pila.Add("SENLACE");
                    Pila.Add("y");
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("or")) {
                    Pila.Add("CONDICIONAl");
                    Pila.Add("or");
                } else {
                    Pila.Add("SENLACE");
                }
            }

            void senlace() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("y")) {
                    Pila.Add("CONDICIONAl");
                    Pila.Add("y");
                } else {

                }
            }

            void instruccion() {
                eliminarUltimo();
                if (paraDeclaracion(lexicoBuenas[ilex].tipoPalabra) == true) {
                    Pila.Add("INSTRUCCION");
                    Pila.Add("ASIGNACION");
                    Pila.Add(tipoVariable);
                    runCodigo();
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    Pila.Add("INSTRUCCION");
                    buscarTipo(lexicoBuenas[ilex].palabra);
                    Pila.Add("ASIGNACION");
                    runCodigo();
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_if")) {
                    Pila.Add("MIF");
                    runCodigo();
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_while")) {
                    Pila.Add("MWHILE");
                    runCodigo();
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_console")) {
                    Pila.Add("INSTRUCCION");
                    Pila.Add("PRINT");
                    runCodigo();
                } else {

                }
            }

            void melse() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_else")) {
                    Pila.Add("llave_cerrar");
                    Pila.Add("INSTRUCCION");
                    Pila.Add("llave_abrir");
                    Pila.Add("Reservada_else");
                } else {
                    Pila.Add("INSTRUCCION");

                }

            }

            void print() {
                eliminarUltimo();
                Pila.Add("punto_coma");
                Pila.Add("parentesis_cerrar");
                Pila.Add("CPRINT");
                Pila.Add("parentesis_abrir");
                Pila.Add("Reservada_writeline");
                Pila.Add("punto");
                Pila.Add("Reservada_console");
            }

            void cprint() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Cadena")) {
                    Pila.Add("MPRINT");
                    Pila.Add("Cadena");
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    Pila.Add("MPRINT");
                    Pila.Add("Identificador");
                } else {
                    Pila.Add("MPRINT");
                }
            }

            void mprint() {
                eliminarUltimo();
                if (lexicoBuenas[ilex].tipoPalabra.Equals("mas")) {
                    Pila.Add("CPRINT");
                    Pila.Add("mas");
                }
            }

            void inicio() {
                eliminarUltimo();
                Pila.Add("llave_cerrar");
                Pila.Add("MAIN");
                Pila.Add("llave_abrir");
                Pila.Add("Identificador");
                Pila.Add("Reservada_class");

            }

            void main() {
                eliminarUltimo();
                Pila.Add("llave_cerrar");
                Pila.Add("CODIGO");
                Pila.Add("llave_abrir");
                Pila.Add("parentesis_cerrar");
                Pila.Add("ARGUMENTOS");
                Pila.Add("parentesis_abrir");
                Pila.Add("Reservada_main");
                Pila.Add("Reservada_void");
                Pila.Add("Reservada_static");

            }

            void argumentos() {
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Reservada_string")) {
                    eliminarUltimo();
                    Pila.Add("Reservada_args");
                    Pila.Add("corchete_cerrar");
                    Pila.Add("corchete_abrir");
                    Pila.Add("Reservada_string");
                } else {
                    eliminarUltimo();

                }
            }

            void declaracion() {
                eliminarUltimo();
                Pila.Add("ASIGNACION");
                Pila.Add(tipoVariable);

            }

            void asignacion() {

                if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    eliminarUltimo();
                    Pila.Add("punto_coma");
                    Pila.Add("VALOR");
                    Pila.Add("Identificador");

                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("corchete_abrir")) {
                    eliminarUltimo();
                    Pila.Add("punto_coma");
                    Pila.Add("VALOR");
                    Pila.Add("Identificador");
                    Pila.Add("corchete_cerrar");
                    Pila.Add("corchete_abrir");

                }

            }

            void valor() {
                if (lexicoBuenas[ilex].tipoPalabra.Equals("igual")) {
                    eliminarUltimo();
                    Pila.Add("CONJUNTO");
                    Pila.Add("igual");

                } else if (paraSignoOperador(lexicoBuenas[ilex].tipoPalabra) == true) {
                    eliminarUltimo();
                    Pila.Add("INCREMENTO");
                    Pila.Add(signoOperador);

                } else {
                    eliminarUltimo();
                    Pila.Add("MULTIPLE");

                }

            }

            void conjunto() {
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    eliminarUltimo();
                    Pila.Add("corchete_cerrar");
                    Pila.Add("JERARQUIA");
                    Pila.Add("corchete_abrir");
                    Pila.Add("Identificador");

                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("llave_abrir")) {
                    eliminarUltimo();
                    Pila.Add("llave_cerrar");
                    Pila.Add("MULTIPLE");
                    Pila.Add(valorVariable);
                    Pila.Add("llave_abrir");

                } else {
                    eliminarUltimo();
                    Pila.Add("JERARQUIA");
                }
            }

            void operacion() {
                if (paraSignoOperador(lexicoBuenas[ilex].tipoPalabra) == true) {
                    eliminarUltimo();
                    Pila.Add("JERARQUIA");
                    Pila.Add(signoOperador);

                } else {
                    eliminarUltimo();
                    Pila.Add("MULTIPLE");

                }
            }

            void jerarquia() {

                if (lexicoBuenas[ilex].tipoPalabra.Equals(valorVariable)) {
                    eliminarUltimo();
                    Pila.Add("OPERACION");
                    Pila.Add(valorVariable);
                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    eliminarUltimo();
                    Pila.Add("OPERACION");
                    Pila.Add("Identificador");

                } else {
                    eliminarUltimo();
                    Pila.Add("ERROR CON OPERACIONES");
                }

            }

            void incremento() {
                if (lexicoBuenas[ilex].tipoPalabra.Equals("mas")) {
                    eliminarUltimo();
                    Pila.Add("OPERACION");
                    Pila.Add("mas");

                } else if (lexicoBuenas[ilex].tipoPalabra.Equals("menos")) {
                    eliminarUltimo();
                    Pila.Add("OPERACION");
                    Pila.Add("menos");

                } else {
                    eliminarUltimo();
                    Pila.Add("ERROR EN INCREMENTO");

                }

            }

            void multiple() {
                if (lexicoBuenas[ilex].tipoPalabra.Equals("coma")) {
                    eliminarUltimo();
                    Pila.Add("ARREGLO");
                    Pila.Add("coma");

                } else {
                    eliminarUltimo();

                }
            }

            void arreglo() {
                if (lexicoBuenas[ilex].tipoPalabra.Equals("Identificador")) {
                    eliminarUltimo();
                    Pila.Add("VALOR");
                    Pila.Add("Identificador");

                } else if (lexicoBuenas[ilex].tipoPalabra.Equals(valorVariable)) {
                    eliminarUltimo();
                    Pila.Add("VALOR");
                    Pila.Add(valorVariable);

                } else {
                    eliminarUltimo();
                    Pila.Add("ERROR EN LOS VALORES");

                }
            }

            void mwhile() {
                eliminarUltimo();
                Pila.Add("llave_cerrar");
                Pila.Add("INSTRUCCION");
                Pila.Add("llave_abrir");
                Pila.Add("parentesis_cerrar");
                Pila.Add("CONDICIONAl");
                Pila.Add("parentesis_abrir");
                Pila.Add("Reservada_while");
            }

            void runCodigo() {
                while (!Pila.Last().Equals("CODIGO")) {
                    for (int i = 0; i < Pila.Count; i++) {
                        Console.WriteLine(Pila[i]);
                    }
                    Console.WriteLine("** Lleva: " + lexicoBuenas[ilex].palabra + " " + lexicoBuenas[ilex].tipoPalabra);
                    Console.WriteLine("-------CODIGO--------");
                    switch (Pila.Last()) {
                        case "DECLARACION":
                            declaracion();
                            break;
                        case "ASIGNACION":
                            asignacion();
                            break;
                        case "VALOR":
                            valor();
                            break;
                        case "CONJUNTO":
                            conjunto();
                            break;
                        case "OPERACION":
                            operacion();
                            break;
                        case "JERARQUIA":
                            jerarquia();
                            break;
                        case "INCREMENTO":
                            incremento();
                            break;
                        case "MULTIPLE":
                            multiple();
                            break;
                        case "ARREGLO":
                            arreglo();
                            break;
                        case "PRINT":
                            print();
                            break;
                        case "CPRINT":
                            cprint();
                            break;
                        case "MPRINT":
                            mprint();
                            break;
                        case "MIF":
                            metodoIf();
                            break;
                        case "CONDICIONAl":
                            condicional();
                            break;
                        case "PSIMBOLO":
                            psimbolo();
                            break;
                        case "SSIMBOLO":
                            ssimbolo();
                            break;
                        case "VALORC":
                            valorc();
                            break;
                        case "ENLACE":
                            enlace();
                            break;
                        case "SENLACE":
                            senlace();
                            break;
                        case "INSTRUCCION":
                            instruccion();
                            break;
                        case "MELSE":
                            melse();
                            break;
                        case "MWHILE":
                            mwhile();
                            break;


                        default:
                            if (Pila.Last().Equals(lexicoBuenas[ilex].tipoPalabra)) {
                                if (lexicoBuenas[ilex].tipoPalabra.Equals("punto_coma")) {
                                    pre_traduccion.Add(new traduccion(lexicoBuenas[ilex].palabra, lexicoBuenas[ilex].tipoPalabra, true));
                                    pre_erroresSintacticos.Add(new traduccion(lexicoBuenas[ilex].palabra, lexicoBuenas[ilex].tipoPalabra, true));
                                } else if (lexicoBuenas[ilex].tipoPalabra.Equals(tipoVariable)) {

                                } else {
                                    pre_traduccion.Add(new traduccion(lexicoBuenas[ilex].palabra, lexicoBuenas[ilex].tipoPalabra, false));
                                    pre_erroresSintacticos.Add(new traduccion(lexicoBuenas[ilex].palabra, lexicoBuenas[ilex].tipoPalabra, false));
                                }
                                Console.WriteLine("+++++++ saco: " + Pila.Last() + " " + lexicoBuenas[ilex].tipoPalabra);
                                bueno_malo = true;
                                eliminarUltimo();
                                ilex++;
                            } else {
                                Console.WriteLine(">>>>>>>>> Error " + Pila.Last() + " en " + lexicoBuenas[ilex].tipoPalabra + " con " + lexicoBuenas[ilex].palabra + " <<<<<<<<<");
                                pre_traduccion.Clear();
                                bueno_malo = false;
                                while (!Pila.Last().Equals("punto_coma")) {
                                    eliminarUltimo();
                                }

                                while (!lexicoBuenas[ilex].tipoPalabra.Equals("punto_coma")) {
                                    ilex++;
                                }

                            }
                            break;
                    }
                }
            }

            Boolean buscarTipo(string id) {
                Boolean encontro = false;
                for (int i = ilex - 1; i > 0; i--) {
                    if (lexicoBuenas[i].palabra.Equals(id)) {
                        Console.WriteLine("//// Encontro la Variable: " + id);
                        for (int j = i; j > 0; j--) {
                            if (paraDeclaracion(lexicoBuenas[j].tipoPalabra) == true) {
                                Console.WriteLine("//// Encontro el Tipo: " + lexicoBuenas[j].tipoPalabra);
                                if (lexicoBuenas[j].tipoPalabra.Equals("Reservada_int") | lexicoBuenas[j].tipoPalabra.Equals("Reservada_float")) {
                                    encontro = true;
                                    break;
                                }
                            }

                        }
                    }
                }
                if (encontro == false) {
                    Pila.Add("VARIABLE NO ENCONTRADA");
                    return false;
                } else {
                    return true;
                }
            }

            Boolean paraCondicionales(string p) {
                switch (p) {
                    case "<":
                        signoCondicional = "menor_que";
                        return true;
                        break;
                    case ">":
                        signoCondicional = "mayor_que";
                        return true;
                        break;
                    case "=":
                        signoCondicional = "igual";
                        return true;
                        break;
                    case "!":
                        signoCondicional = "exclamacion";
                        return true;
                        break;

                    default:
                        signoCondicional = "ERROR EN CONDICIONAL";
                        return false;
                        break;
                }
            }

            Boolean paraElTipoDeValor(string p) {
                switch (p) {
                    case "Numero_entero":
                        tipoVariable = "Reservada_int";
                        valorVariable = "Numero_entero";
                        return true;
                        break;
                    case "Numero_decimal":
                        tipoVariable = "Reservada_float";
                        valorVariable = "Numero_decimal";
                        return true;
                        break;

                    default:
                        tipoVariable = "ERROR TIPO DE VARIABLE";
                        return false;
                        break;
                }
            }

            Boolean paraDeclaracion(string p) {
                switch (p) {
                    case "Reservada_int":
                        tipoVariable = "Reservada_int";
                        valorVariable = "Numero_entero";
                        return true;
                        break;
                    case "Reservada_float":
                        tipoVariable = "Reservada_float";
                        valorVariable = "Numero_decimal";
                        return true;
                        break;
                    case "Reservada_char":
                        tipoVariable = "Reservada_char";
                        valorVariable = "Caracter";
                        return true;
                        break;
                    case "Reservada_string":
                        tipoVariable = "Reservada_string";
                        valorVariable = "Cadena";
                        return true;
                        break;
                    case "Reservada_bool":
                        tipoVariable = "Reservada_bool";
                        valorVariable = "true_false";
                        return true;
                        break;

                    default:
                        tipoVariable = "ERROR TIPO DE VARIABLE";
                        return false;
                        break;
                }
            }

            Boolean paraSignoOperador(string p) {
                switch (p) {
                    case "mas":
                        signoOperador = "mas";
                        return true;
                        break;
                    case "menos":
                        signoOperador = "menos";
                        return true;
                        break;
                    case "asterisco":
                        signoOperador = "asterisco";
                        return true;
                        break;
                    case "diagonal":
                        signoOperador = "diagonal";
                        return true;
                        break;

                    default:
                        signoOperador = "ERROR SIGNO OPERADOR";
                        return false;
                        break;
                }
            }


        }

        public void borrarHistorial() {
            if (File.Exists(Path.Combine(Application.StartupPath, "Proyecto2_Tokens.html"))) {
                File.Delete(Path.Combine(Application.StartupPath, "Proyecto2_Tokens.html"));
            }
            if (File.Exists(Path.Combine(Application.StartupPath, "Proyecto2_Simbolos.html"))) {
                File.Delete(Path.Combine(Application.StartupPath, "Proyecto2_Simbolos.html"));
            }
            if (File.Exists(Path.Combine(Application.StartupPath, "Proyecto2_Errores.html"))) {
                File.Delete(Path.Combine(Application.StartupPath, "Proyecto2_Errores.html"));
            }

            lexicoMalas.Clear();
            lexicoBuenas.Clear();
            traduccionFinal.Clear();
            pre_traduccion.Clear();
            pre_erroresSintacticos.Clear();
            erroresSintacticos.Clear();

        }

        public void htmlTokens() {

            string webB = Path.Combine(Application.StartupPath, "Proyecto2_Tokens.html");
            string inicio = "<html>" +
                "<head>" +
                "</head>" +
                "<body style='background-color:#34495E'>";
            string fin =
                "</table>" +
                "<body>" +
                "</html>";

            string fecha = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:tt");
            string medioB = "<h1 align=center><font color ='white'> Lista de Tokens de: " + fecha + "</h1>" + "<br>" +
                "<h1 align=center><font color ='white'> Archivo de Entrada: " + nombreDelArchivoEntrada + "</h1>" +
                "<h1 align=center><font color ='white'> Archivo de Salida: python.py </h1>" +
                "<table border=11 , align='center', bordercolor='orange'>" +
                "<tr align=center> <th><font color ='white'> Numero </th> <th><font color ='white'> Palabra </th> <th><font color ='white'> Tipo </th></tr>";
            if (lexicoBuenas.Count != 0) {
                for (int i = 0; i < lexicoBuenas.Count; i++) {
                    if (lexicoBuenas[i].tipoPalabra.Equals("Reservada_" + lexicoBuenas[i].palabra.ToLower()) | lexicoBuenas[i].tipoPalabra.Equals("Numero_entero") | lexicoBuenas[i].tipoPalabra.Equals("Numero_decimal") | lexicoBuenas[i].tipoPalabra.Equals("Comentario")) {
                        medioB += "\n<tr align=center> <td><font color ='white'> " + (i + 1) + " </td> <td><font color ='white'> " + lexicoBuenas[i].palabra + " </td> <td><font color ='white'> " + lexicoBuenas[i].tipoPalabra + " </td> </tr>";

                    }
                }
            }


            string contenidoB = inicio + medioB + fin;

            File.WriteAllText(webB, contenidoB);

            Process start = new Process();
            start.StartInfo.FileName = webB;
            start.Start();

            //string webM = Path.Combine(Application.StartupPath, "Proyecto2_Malas.html");

            //string medioM = "<h1 align=center><font color ='white'> Archivo de Errores </h1>" +
            //    "<table border=11 , align='center', bordercolor='orange'>" +
            //    "<tr align=center> <th><font color ='white'> Numero </th> <th><font color ='white'> Fila </th> <th><font color ='white'> Columna </th> <th><font color ='white'> Palabra </th> <th><font color ='white'> Error </th></tr>";

            //for (int i = 0; i < lexicoMalas.Count; i++) {
            //    medioM += "\n<tr align=center> <td><font color ='white'> " + (i + 1) + " </td> <td><font color ='white'> " + lexicoMalas[i].fila + " </td> <td><font color ='white'> " + lexicoMalas[i].columna + " </td> <td><font color ='white'> " + lexicoMalas[i].palabra + " </td> <td><font color ='white'> Signo Error </td></tr>";
            //}

            //string contenidoM = inicio + medioM + fin;

            //File.WriteAllText(webM, contenidoM);




        }

        public void htmlSimbolos() {

            string webB = Path.Combine(Application.StartupPath, "Proyecto2_Simbolos.html");
            string inicio = "<html>" +
                "<head>" +
                "</head>" +
                "<body style='background-color:#34495E'>";
            string fin =
                "</table>" +
                "<body>" +
                "</html>";

            string fecha = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:tt");
            string medioB = "<h1 align=center><font color ='white'> Lista de Simbolos de: " + fecha + "</h1>" + "<br>" +
                "<h1 align=center><font color ='white'> Archivo de Entrada: " + nombreDelArchivoEntrada + "</h1>" +
                "<h1 align=center><font color ='white'> Archivo de Salida: python.py </h1>" +
                "<table border=11 , align='center', bordercolor='orange'>" +
                "<tr align=center> <th><font color ='white'> Numero </th> <th><font color ='white'> Palabra </th> <th><font color ='white'> Tipo </th></tr>";

            if (lexicoBuenas.Count != 0) {
                for (int i = 0; i < lexicoBuenas.Count; i++) {
                    if (!lexicoBuenas[i].tipoPalabra.Equals("Reservada_" + lexicoBuenas[i].palabra.ToLower()) | !lexicoBuenas[i].tipoPalabra.Equals("Numero_entero") | !lexicoBuenas[i].tipoPalabra.Equals("Numero_decimal") | !lexicoBuenas[i].tipoPalabra.Equals("Comentario")) {
                        medioB += "\n<tr align=center> <td><font color ='white'> " + (i + 1) + " </td> <td><font color ='white'> " + lexicoBuenas[i].palabra + " </td> <td><font color ='white'> " + lexicoBuenas[i].tipoPalabra + " </td> </tr>";

                    }
                }
            }


            string contenidoB = inicio + medioB + fin;

            File.WriteAllText(webB, contenidoB);

            Process start = new Process();
            start.StartInfo.FileName = webB;
            start.Start();


        }

        public void htmlErrores() {

            string webM = Path.Combine(Application.StartupPath, "Proyecto2_Errores.html");
            string inicio = "<html>" +
                "<head>" +
                "</head>" +
                "<body style='background-color:#34495E'>";
            string fin =
                "</table>" +
                "<body>" +
                "</html>";

            string fecha = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:tt");

            string medioMTokens = "<h1 align=center><font color ='white'> Errores de Tokens de: " + fecha + "</h1>" + "<br>" +
                "<h1 align=center><font color ='white'> Archivo de Entrada: " + nombreDelArchivoEntrada + "</h1>" +
                "<h1 align=center><font color ='white'> Archivo de Salida: python.py </h1>" +
               "<table border=11 , align='center', bordercolor='orange'>" +
               "<tr align=center> <th><font color ='white'> Numero </th> <th><font color ='white'> Fila </th> <th><font color ='white'> Columna </th> <th><font color ='white'> Palabra </th> <th><font color ='white'> Error </th></tr>";

            string medioMSintactico = "<h1 align=center><font color ='white'> Errores Sintacticos de: " + fecha + "</h1>" + "<br>" +
                "<h1 align=center><font color ='white'> Archivo de Entrada: " + nombreDelArchivoEntrada + "</h1>" +
                "<h1 align=center><font color ='white'> Archivo de Salida: python.py </h1>" +
               "<table border=11 , align='center', bordercolor='orange'>" +
               "<tr align=center> <th><font color ='white'> Numero </th> <th><font color ='white'> Fila </th> <th><font color ='white'> Palabra </th> <th><font color ='white'> Tipo </th> </tr>";
            if (lexicoMalas.Count != 0) {
                for (int i = 0; i < lexicoMalas.Count; i++) {
                    medioMTokens += "\n<tr align=center> <td><font color ='white'> " + (i + 1) + " </td> <td><font color ='white'> " + lexicoMalas[i].fila + " </td> <td><font color ='white'> " + lexicoMalas[i].columna + " </td> <td><font color ='white'> " + lexicoMalas[i].palabra + " </td> <td><font color ='white'> Signo Error </td></tr>";
                }
            }

            if (erroresSintacticos.Count != 0) {
                for (int i = 0; i < erroresSintacticos.Count; i++) {
                    medioMTokens += "\n<tr align=center> <td><font color ='white'> " + (i + 1) + " </td> <td><font color ='white'> " + erroresSintacticos[i].palabra + " </td> <td><font color ='white'> " + erroresSintacticos[i].Referencia + "</td></tr>";
                }
            }


            string contenidoM = inicio + medioMTokens + medioMSintactico + fin;

            File.WriteAllText(webM, contenidoM);

            Process start = new Process();
            start.StartInfo.FileName = webM;
            start.Start();


        }

        public void traducir(RichTextBox caja, List<traduccion> listaFinal) {

            int i = 0;
            Boolean print = false;
            Boolean mif = false;
            Boolean ultimaLlaveIf = false;
            Boolean mifelse = false;
            Boolean instruccionesIf = false;
            Boolean mwhile = false;

            while (i < listaFinal.Count) {
                if (listaFinal[i].Referencia.Equals("Comentario")) {
                    if (listaFinal[i].palabra.StartsWith("//")) {
                        if (instruccionesIf == true) {
                            caja.AppendText("\t # " + listaFinal[i].palabra.Substring(2) + "\n");
                        } else {
                            caja.AppendText("# " + listaFinal[i].palabra.Substring(2) + "\n");
                        }
                        i++;
                    } else if (listaFinal[i].palabra.StartsWith("/*")) {
                        caja.AppendText("..." + listaFinal[i].palabra.Substring(2, listaFinal[i].palabra.Length - 4) + "...\n");
                        i++;
                    }
                } else if (listaFinal[i].Referencia.Equals("true_false")) {
                    if (listaFinal[i].palabra.Equals("true", StringComparison.OrdinalIgnoreCase)) {
                        caja.AppendText("True");
                        i++;
                    } else if (listaFinal[i].palabra.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                        caja.AppendText("False");
                        i++;
                    }
                } else if (listaFinal[i].Referencia.Equals("Reservada_console")) {
                    i += 2;
                    if (instruccionesIf == true) {
                        caja.AppendText("\t print");
                    } else {
                        caja.AppendText("print");
                    }
                    print = true;
                    i++;
                } else if (print == true && listaFinal[i].palabra.Equals("+")) {
                    caja.AppendText(" , ");
                    i++;
                } else if (print == true && listaFinal[i].Referencia.Equals("punto_coma")) {
                    caja.AppendText("\n");
                    print = false;
                    i++;
                } else if (print == false && (listaFinal[i].Referencia.Equals("punto_coma") | listaFinal[i].Referencia.Equals("coma"))) {
                    caja.AppendText("\n");
                    i++;

                } else if (listaFinal[i].Referencia.Equals("Reservada_if")) {
                    caja.AppendText("if ");
                    mif = true;
                    i++;
                } else if (listaFinal[i].palabra.Equals("(") && mif == true) {
                    i++;
                } else if (listaFinal[i].palabra.Equals(")") && mif == true) {
                    caja.AppendText(":\n");
                    ultimaLlaveIf = true;
                    mif = false;
                    i++;
                } else if (listaFinal[i].palabra.Equals("{") && ultimaLlaveIf == true) {
                    instruccionesIf = true;
                    i++;
                } else if (listaFinal[i].palabra.Equals("}") && ultimaLlaveIf == true) {
                    ultimaLlaveIf = false;
                    i++;
                } else if (listaFinal[i].Referencia.Equals("Reservada_else")) {
                    caja.AppendText("else: ");
                    mifelse = true;
                    i++;
                } else if (listaFinal[i].palabra.Equals("{") && mifelse == true) {
                    caja.AppendText("\n");
                    i++;
                } else if (listaFinal[i].palabra.Equals("}") && mifelse == true) {
                    mifelse = false;
                    i++;
                } else if (listaFinal[i].Referencia.Equals("Identificador") && ultimaLlaveIf == true) {
                    caja.AppendText("\t " + listaFinal[i].palabra);
                    i++;
                } else if (listaFinal[i].Referencia.Equals("Identificador") && mifelse == true) {
                    caja.AppendText("\t " + listaFinal[i].palabra);
                    i++;
                } else if (listaFinal[i].Referencia.Equals("Reservada_while")) {
                    caja.AppendText("while: ");
                    mwhile = true;
                    i++;
                } else if (listaFinal[i].palabra.Equals("(") && mwhile == true) {
                    i++;
                } else if (listaFinal[i].palabra.Equals("{") && mwhile == true) {
                    caja.AppendText("\n");
                    i++;
                } else if (listaFinal[i].palabra.Equals(")") && mwhile == true) {
                    caja.AppendText(":\n");
                    instruccionesIf = true;
                    mwhile = false;
                    i++;
                } else {
                    caja.AppendText(listaFinal[i].palabra);
                    i++;
                }
            }



        }

    }
}

