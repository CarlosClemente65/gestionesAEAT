﻿using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace gestionesAEAT
{
    public class Utiles
    {
        public string url { get; set; } //Variable que almacena la url a la que enviar los datos a la AEAT

        ArrayList cabecera = new ArrayList(); //Lista con las lineas que vienen en el guion como cabecera
        ArrayList body = new ArrayList(); //Bloque de datos identificados como body en la entrada
        ArrayList respuesta = new ArrayList(); //Bloque de datos identificados como respuesta en la entrada

        public string quitaRaros(string cadena)
        {
            List<(string, string)> caracteresReemplazo = new List<(string, string)>
            {
                ("á", "a"),
                ("é", "e"),
                ("í", "i"),
                ("ó", "o"),
                ("ú", "u"),
                ("º", "."),
                ("ª", "."),
                ("ñ", "¤"),
                ("Á", "A"),
                ("É", "E"),
                ("Í", "I"),
                ("Ó", "O"),
                ("Ú", "U"),
                ("Ñ", "¤")
            };

            foreach (var tupla in caracteresReemplazo)
            {
                cadena = cadena.Replace(tupla.Item1, tupla.Item2);
            }
            return cadena;

        }

        public string codificacionFicheroEntrada(ArrayList ficheroEntrada)
        {
            //Permite obtener la codificacion UTF-8 o ISO8859-1 (ascii extendido 256 bits o ansi)
            string cadena, valor;
            int bloque = 0;

            valor = "";

            for (int x = 0; x < ficheroEntrada.Count; x++)
            {
                cadena = ficheroEntrada[x].ToString().Trim();
                if (cadena != "")
                {
                    switch (cadena)
                    {
                        case "[cabecera]":
                            bloque = 2;
                            continue;
                    }

                    if (bloque == 2)
                    {
                        string[] parte = cadena.ToString().Split('=');
                        if (parte[0] == "CODIFICACION")
                        {
                            if (parte[1].Length > 1) valor = parte[1];
                            break;
                        }
                    }
                }
            }

            if (valor == "") valor = "utf-8";
            return valor.ToUpper();
        }

        public void borrarFicheros(string fichero)
        {
            if (File.Exists(fichero)) File.Delete(fichero);
        }

        public string procesarGuionJson(string guion)
        {
            string textoJson = string.Empty;
            //Desarrollar este metodo para montar el texto que debe pasarse a Hacienda en formato JSON

            return textoJson;
        }

        public string procesarGuionHtml(string guion)
        {
            List<string> textoEntrada = new List<string>();
            string textoAEAT = string.Empty;

            //Monta un array con el fichero de entrada para procesarlo
            using (StreamReader sr = new StreamReader(guion))
            {
                string linea;
                while ((linea = sr.ReadLine()) != null)
                {
                    textoEntrada.Add(linea);
                }
            }

            cargaDatosGuion(textoEntrada);

            for (int linea = 0; linea < cabecera.Count; linea++)
            {
                if (string.IsNullOrEmpty(textoAEAT))
                {
                    textoAEAT = cabecera[linea].ToString();
                }
                else
                {
                    textoAEAT += "&" + cabecera[linea].ToString();
                }
            }
            return textoAEAT;
        }

        private void cargaDatosGuion(List<string> textoEntrada)
        {
            //Lee el fichero de entrada y monta una lista con todas las lineas segun si son de la cabecera, body o respuesta
            string cadena;
            int bloque = 0; //Controla el tipo de dato a grabar en el fichero

            for (int x = 0; x < textoEntrada.Count; x++)
            {
                cadena = textoEntrada[x].ToString().Trim();
                if (cadena != "")
                {
                    //Control para saber que parte del fichero se va a procesar
                    switch (cadena)
                    {
                        case "[url]":
                            bloque = 1;
                            continue;

                        case "[cabecera]":
                            bloque = 2;
                            continue;

                        case "[body]":
                            bloque = 3;
                            continue;

                        case "[respuesta]":
                            bloque = 4;
                            continue;
                    }
                    switch (bloque)
                    {
                        //Las lineas que van despues de cada parte se asignan a cada una de ellas
                        case 1:
                            url = cadena;
                            break;

                        case 2:
                            cabecera.Add(cadena);
                            break;

                        case 3:
                            body.Add(cadena);
                            break;

                        case 4:
                            respuesta.Add(cadena);
                            break;
                    }
                }
            }
        }


    }
}
