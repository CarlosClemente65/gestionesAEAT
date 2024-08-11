using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace gestionesAEAT
{
    public class ListaInfoCertificados
    {
        [JsonProperty("certificados")]
        public List<ElementosCertificado> certificadosInfo { get; set; }

        public ListaInfoCertificados()
        {
            certificadosInfo = new List<ElementosCertificado>();
        }
    }
    public class ElementosCertificado
    {
        //Clase que representa las propiedades de los certificados que necesitamos
        //Se ponen esas propiedades para mantener el mismo sistema de lectura que hacemos desde el basico
        [JsonProperty("A")]
        public string nifCertificado { get; set; }

        [JsonProperty("B")]
        public string titularCertificado { get; set; }

        [JsonProperty("C")]
        public string serieCertificado { get; set; }

        private DateTime _fechaEmision;
        [JsonProperty("D")] 
        public DateTime fechaEmision
        {
            get => _fechaEmision.Date;
            set => _fechaEmision = value.Date;
        }

        private DateTime _fechaValidez; 
        [JsonProperty("E")]
        public DateTime fechaValidez
        {
            get => _fechaValidez.Date;
            set => _fechaValidez = value.Date;
        }

        [JsonProperty("F")]
        public string nifRepresentante { get; set; }

        [JsonProperty("G")]
        public string nombreRepresentante { get; set; }

        [JsonProperty("H")]
        public string datosRepresentante { get; set; }

        [JsonProperty("I")]
        public string passwordCertificado { get; set; }

        [JsonProperty("J")]
        public string huellaCertificado { get; set; }

        public ElementosCertificado()
        {
            nifCertificado = string.Empty;
            titularCertificado = string.Empty;
            serieCertificado = string.Empty;
            fechaEmision = DateTime.MinValue;
            fechaValidez = DateTime.MinValue;
            nifRepresentante = string.Empty;
            nombreRepresentante = string.Empty;
            datosRepresentante = string.Empty;
            passwordCertificado = string.Empty;
            huellaCertificado = string.Empty;
        }
    }


    public class GestionCertificados
    {
        //Clase que engloba la gestion de certificados

        private List<X509Certificate2> certificados; //Lista que contiene los certificados
        private ListaInfoCertificados listaCertificados = new ListaInfoCertificados();

        public GestionCertificados()
        {
            //Al instanciar esta clase, se crea una nueva lista de certificados y se cargan los que estan instalados en la maquina.
            certificados = new List<X509Certificate2>();
            cargarCertificados();
        }

        public void cargarCertificados()
        {
            //Se chequea si ya se ha cargado la lista de certificados para no hacerlo de nuevo
            if (certificados.Count == 0)
            {
                //Metodo para cargar los certificados del almacen de windows
                X509Store store = new X509Store(StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 cert in store.Certificates)
                {
                    certificados.Add(cert);
                }
                store.Close();

                // Graba las propiedades de los certificados en la clase ListaCertificados
                foreach (X509Certificate2 certificado in certificados)
                {
                    if (certificado.Subject.Contains("SERIALNUMBER")) //Deben tener esto para que sean de persona fisica o juridica
                    {
                        //En el Subject estan todos los datos del certificado
                        string datosSubject = certificado.Subject;
                        ElementosCertificado info = new ElementosCertificado
                        {
                            serieCertificado = certificado.SerialNumber,
                            fechaValidez = certificado.NotAfter,
                            fechaEmision = certificado.NotBefore,
                            huellaCertificado = certificado.Thumbprint.ToString()
                        };
                        obtenerDatosSubject(datosSubject, info);
                        listaCertificados.certificadosInfo.Add(info);
                    }
                }
                //Una vez obtenidos los datos, se ordena la lista por el nombre del titular del certificado
                listaCertificados.certificadosInfo = ordenarCertificados("titularCertificado", true);
            }
        }

        public X509Certificate2 buscarCertificado(string serieCertificado)
        {
            //Devuelve el certificado que tiene la serie pasada
            return certificados.Find(cert => cert.SerialNumber == serieCertificado);
        }

        public string buscarSerieCertificado(string textoBusqueda)
        {
            //Devuelve el certificado que contiene el texto a buscar en la serie, el NIF o nombre del titular

            string resultadoBusqueda = string.Empty;
            var buscaCertificado = listaCertificados.certificadosInfo.Find(cert =>
                cert.nifCertificado.Contains(textoBusqueda) ||
                cert.titularCertificado.Contains(textoBusqueda) ||
                cert.serieCertificado == textoBusqueda
                );
            if (buscaCertificado != null)
            {
                resultadoBusqueda = buscaCertificado.serieCertificado;
            }

            return resultadoBusqueda;
        }

        public List<ElementosCertificado> relacionCertificados()
        {
            //Devuelve la lista de certificados (para rellenar la pantalla de seleccion)
            return listaCertificados.certificadosInfo;
        }

        public List<ElementosCertificado> ordenarCertificados(string campoOrdenacion, bool ascendente)
        {
            List<ElementosCertificado> certificados = listaCertificados.certificadosInfo;
            //Devuelve la lista de los certificados ordenados por el campo pasado y en orden ascendente/descedente
            if (certificados == null || certificados.Count == 0)
            {
                //Evita que se produzca una excepcion si no hay certificados cargados en la lista
                return listaCertificados.certificadosInfo;
            }

            switch (campoOrdenacion)
            {
                case "nifCertificado":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.nifCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.nifCertificado).ToList());
                    }
                    break;

                case "titularCertificado":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.titularCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.titularCertificado).ToList());
                    }
                    break;

                case "fechaValidez":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.fechaValidez).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.fechaValidez).ToList());
                    }
                    break;

                case "fechaEmision":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.fechaEmision).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.fechaEmision).ToList());
                    }
                    break;

                case "nifRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.nifRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.nifRepresentante).ToList());
                    }
                    break;

                case "nombreRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.nombreRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.nombreRepresentante).ToList());
                    }
                    break;

                case "datosRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.datosRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.datosRepresentante).ToList());
                    }
                    break;

                case "serieCertificado":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.serieCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.serieCertificado).ToList());
                    }
                    break;

                case "huellaCertificado":
                    if (ascendente)
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderBy(certificado => certificado.huellaCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<ElementosCertificado>(certificados.OrderByDescending(certificado => certificado.huellaCertificado).ToList());
                    }
                    break;

            }
            return certificados;

        }

        public List<ElementosCertificado> filtrarCertificados(string filtro)
        {
            //Devuelve la lista de certificados filtrada por el texto pasado
            List<ElementosCertificado> certificados = listaCertificados.certificadosInfo;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.ToUpper();
                certificados = new List<ElementosCertificado>(certificados.FindAll(certificado => certificado.titularCertificado.ToUpper().Contains(filtro)));
            }
            return certificados;
        }

        public void obtenerDatosSubject(string subject, ElementosCertificado info)
        {
            //Carga los datos del certificado en las propiedades de la clase
            bool juridica = false;
            if (subject.Contains("2.5.4.97")) juridica = true;
            string nombrePF = string.Empty; ;
            string apellidoPF = string.Empty;
            string nombrePJ = string.Empty;
            string nombreRepresentante = string.Empty;
            string apellidoRepresentante = string.Empty;
            string nifCertificado = string.Empty;


            string[] partes = subject.Split(',');
            foreach (string parte in partes)
            {
                string[] elementos = parte.Trim().Split('=');
                string elemento = string.Empty;
                string valor = string.Empty;
                if (elementos.Length == 2)
                {
                    elemento = elementos[0];
                    valor = elementos[1];
                }

                switch (elemento)
                {
                    case "G": //Nombre del titular del certificado o del representante si es juridica
                        if (juridica)
                        {
                            nombreRepresentante = valor;
                        }
                        else
                        {
                            nombrePF = valor;
                        }
                        break;

                    case "SN": //Apellido del titular del certificado o del representante si es juridica
                        if (juridica)
                        {
                            apellidoRepresentante = valor;
                        }
                        else
                        {
                            apellidoPF = valor;
                        }
                        break;

                    case "SERIALNUMBER": //NIF del titular del certificado o del representante si es juridica
                        if (juridica)
                        {
                            info.nifRepresentante = valor.Substring(6);
                        }
                        else
                        {
                            nifCertificado = valor.Substring(6);
                        }
                        break;

                    case "O": //Nombre de la sociedad
                        nombrePJ = valor;
                        break;

                    case "OID.2.5.4.97": //NIF de la sociedad
                        nifCertificado = valor.Substring(6);
                        break;

                    case "CN": //Datos representante
                        if (juridica)
                        {
                            info.datosRepresentante = valor;
                        }
                        break;
                }

                if (string.IsNullOrEmpty(info.nifCertificado)) info.nifCertificado = nifCertificado;
                if (string.IsNullOrEmpty(info.titularCertificado) || string.IsNullOrEmpty(info.nombreRepresentante))
                {
                    if (juridica)
                    {
                        info.titularCertificado = nombrePJ;
                        if (!string.IsNullOrEmpty(nombreRepresentante))
                        {
                            info.nombreRepresentante = apellidoRepresentante + " " + nombreRepresentante;
                        }
                    }
                    else
                    {
                        info.titularCertificado = apellidoPF + " " + nombrePF;
                    }
                }
            }
        }

        public string leerCertificado(string fichero, string password)
        {
            //Permite leer los datos de un certificado que se pase como fichero

            //Como se pasa el certificado como fichero, se borran los certificados que hay en la lista para que solo aparezca el que se ha pasado
            if (certificados.Count > 0)
            {
                certificados.Clear();
                listaCertificados.certificadosInfo.Clear();
            }

            try
            {
                X509Certificate2 certificado = new X509Certificate2(fichero, password);
                certificados.Add(certificado);
                // Graba las propiedades de los certificados en la clase certificadosInfo
                foreach (X509Certificate2 cert in certificados)
                {
                    if (cert.Subject.Contains("SERIALNUMBER")) //Deben tener esto para que sean de persona fisica o juridica
                    {
                        //En el Subject estan todos los datos del certificado
                        string datosSubject = cert.Subject;
                        ElementosCertificado info = new ElementosCertificado
                        {
                            serieCertificado = cert.SerialNumber,
                            fechaValidez = cert.NotAfter,
                            fechaEmision = certificado.NotBefore,
                            passwordCertificado = password,
                            huellaCertificado = cert.Thumbprint.ToString()
                        };
                        obtenerDatosSubject(datosSubject, info);
                        listaCertificados.certificadosInfo.Add(info);

                    }
                }
                return string.Empty;
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void exportarDatosCertificados(string ruta)
        {
            //Permite grabar un fichero con los datos de los certificados
            try
            {
                // Serializar la lista de ficheros a JSON
                var jsonSettings = new JsonSerializerSettings
                {
                    DateFormatString = "dd/MM/yyyy", //Formato de fechas
                    Formatting = Formatting.Indented //Aplica indentacion
                };

                string jsonEnvio = JsonConvert.SerializeObject(listaCertificados, jsonSettings);

                //Guardar el json
                File.WriteAllText(ruta, jsonEnvio);
            }

            catch (Exception ex)
            {
                string mensaje = $"No se ha podido grabar los datos de los certificados. {ex.Message}";
                File.WriteAllText(Program.ficheroErrores, mensaje);
            }

        }

    }
}