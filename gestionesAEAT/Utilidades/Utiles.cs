using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionesAEAT
{
    public class Utiles
    {
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
    }
}
