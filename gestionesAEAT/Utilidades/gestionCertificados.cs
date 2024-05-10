using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gestionesAEAT
{
    public class certificadoInfo
    {
        public string nifCertificado { get; set; }
        public string titularCertificado { get; set; }
        public DateTime fechaCertificado { get; set; }
        public string nifRepresentante { get; set; }
        public string nombreRepresentante { get; set; }
        public string serieCertificado { get; set; }
    }

    public class gestionCertificados
    {
        private List<X509Certificate2> certificados;
        private List<certificadoInfo> certificadosInfo = new List<certificadoInfo>();
        private static gestionCertificados instancia;

        private gestionCertificados()
        {
            certificados = new List<X509Certificate2>();
            cargarCertificados();
        }

        // Método estático para obtener la instancia única de la clase
        public static gestionCertificados ObtenerInstancia()
        {
            if (instancia == null)
            {
                instancia = new gestionCertificados();
            }
            return instancia;
        }

        public void cargarCertificados()
        {
            //Metodo para cargar los certificados del almacen de windows
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                certificados.Add(cert);
            }
            store.Close();

            // Graba las propiedades de los certificados en la clase certificadosInfo
            foreach (X509Certificate2 cert in this.certificados)
            {
                if (cert.Subject.Contains("SERIALNUMBER")) //Deben tener esto para que sean de persona fisica o juridica
                {
                    //En el Subject estan todos los datos del certificado
                    string datosSubject = cert.Subject;
                    certificadoInfo info = new certificadoInfo
                    {
                        serieCertificado = cert.SerialNumber,
                        fechaCertificado = cert.NotAfter
                    };
                    obtenerDatosSubject(datosSubject, info);
                    certificadosInfo.Add(info);
                }
            }
            //Una vez obtenidos los datos, se ordena la lista por el nombre del titular del certificado
            certificadosInfo = ordenarCertificados(certificadosInfo, "titularCertificado", true);
        }

        public X509Certificate2 buscarSerieCertificado(string serieCertificado)
        {
            //Devuelve el certificado que tiene la serie pasada
            return certificados.Find(cert => cert.SerialNumber == serieCertificado);
        }

        public List<certificadoInfo> listaCertificados()
        {
            //Devuelve la lista de certificados (para rellenar el dgv)
            return certificadosInfo;
        }

        public List<certificadoInfo> ordenarCertificados(List<certificadoInfo> certificados, string campoOrdenacion, bool ascendente)
        {
            //Devuelve la lista de los certificados ordenados por el campo pasado y en orden ascendente/descedente
            if (certificados == null || certificados.Count == 0)
            {
                //Evita que se produzca una excepcion si no se pasa una lista de certificados
                return certificados;
            }

            switch (campoOrdenacion)
            {
                case "nifCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.nifCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.nifCertificado).ToList());
                    }
                    break;

                case "titularCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.titularCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.titularCertificado).ToList());
                    }
                    break;

                case "fechaCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.fechaCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.fechaCertificado).ToList());
                    }
                    break;


                case "nifRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.nifRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.nifRepresentante).ToList());
                    }
                    break;

                case "nombreRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.nombreRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.nombreRepresentante).ToList());
                    }
                    break;

                case "serieCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.serieCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.serieCertificado).ToList());
                    }
                    break;

            }
            return certificados;

        }

        public List<certificadoInfo> filtrarCertificados(string filtro)
        {
            //Devuelve la lista de certificados filtrada por el filtro pasado
            List<certificadoInfo> certificados = this.certificadosInfo;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.ToUpper();
                certificados = new List<certificadoInfo>(certificados.FindAll(certificado => certificado.titularCertificado.ToUpper().Contains(filtro)));
            }
            return certificados;
        }

        public void obtenerDatosSubject(string subject, certificadoInfo info)
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
                }

                info.nifCertificado = nifCertificado;
                if (juridica)
                {
                    info.titularCertificado = nombrePJ;
                    info.nombreRepresentante = apellidoRepresentante + " " + nombreRepresentante;
                }
                else
                {
                    info.titularCertificado = apellidoPF + " " + nombrePF;
                }
            }
        }
    }
}