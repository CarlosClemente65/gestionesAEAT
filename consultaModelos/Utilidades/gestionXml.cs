using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace consultaModelos
{
    public class gestionXml
    {
        //Esta clase sirve para serializar los datos del XML recidibo con la respuesta y poder tratarlos para generar el fichero de salida.
        public gestionXml(string xmlString, string salida)
        {
            // Ejemplo de XML recibido de la API. Habra que sustituirlo por la respuesta real.
            xmlString = @"<servicioConsultasDirectas>
                                <respuestaCorrecta>
                                    <ejercicio>2023</ejercicio>
		                            <modelo>303</modelo>
                            		<periodo>1T</periodo>
                            		<nif>21523660F</nif>
                            		<csv>HQHH4VAUADX5USW5</csv>
		                            <expediente>202330323660010D </expediente>
		                            <justificante>3034297995951</justificante>
		                            <fechaYHoraPresentacion>2023-04-02T21:34:33</fechaYHoraPresentacion>
                                </respuestaCorrecta>
                                <respuestaCorrecta>
                                    <ejercicio>2023</ejercicio>
		                            <modelo>303</modelo>
		                            <periodo>2T</periodo>
		                            <nif>21523660F</nif>
		                            <csv>BEJG38NFWTDCJ9YD</csv>
		                            <expediente>202330323660380P </expediente>
		                            <justificante>3034288442455</justificante>
		                            <fechaYHoraPresentacion>2023-07-02T23:27:18</fechaYHoraPresentacion>
                                </respuestaCorrecta>
                            </servicioConsultasDirectas>";

            // Deserializar el XML en objetos
            ServicioConsultasDirectas servicio = DeserializeFromXml<ServicioConsultasDirectas>(xmlString);

            // Acceder a los valores de las respuestas correctas
            string textoSalida = string.Empty;
            int r = 1;
            foreach (var respuesta in servicio.RespuestasCorrectas)
            {
                textoSalida += $"Resultado {r}\n";
                textoSalida += $"NIF: {respuesta.nif}\n";
                textoSalida += $"Modelo: {respuesta.modelo}\n";
                textoSalida += $"Ejercicio: {respuesta.ejercicio}\n";
                textoSalida += $"Periodo: {respuesta.periodo}\n";
                textoSalida += $"CSV: {respuesta.Csv}\n";
                textoSalida += $"Justificante: {respuesta.Justificante}\n";
                textoSalida += $"Expediente: {respuesta.Expediente}\n";
                textoSalida += $"Fecha presentacion: {respuesta.fechaYHoraPresentacion}\n\n";
                r++;
            }

            File.WriteAllText(salida, textoSalida);
        }

        public T DeserializeFromXml<T>(string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
