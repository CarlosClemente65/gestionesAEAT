
# gestionesAEAT v1.1
## Programa para la validación, envío y consulta de modelos de Hacienda, ratificar domicilio y descarga de datos fiscales de renta, y obtener relación de certificados instalados en el equipo

### Desarrollado por Carlos Clemente (07-2024)

### Control de versiones
- Version 1.1.0 - Primera version funcional

Instrucciones:
- Mediante un fichero que contiene los datos de los modelos, permite hacer una validacion y envio a los servidores de Hacienda en presentacion directa.
- Tambien permite la descarga de modelos presentados utilizando el NIF del presentador o de la empresa. Se pueden obtener todos los que correspondan al mismo modelo y ejercicio
- En las gestiones de la campaña de renta, se puede hacer la consulta de la ratificacion del domicilio asi como la descarga de datos fiscales.
- En el caso de ser necesario el uso de un certificado digital, si no se pasa como parametro, se solicita la seleccion de uno de los instalados en el equipo.
- Tambien se puede obtener una relacion de los certificados instalados en el equipo con sus propiedades (de uso en el certbase o la presentacion directa).

Parametros de ejecucion:
* dsclave: unicamente sirve como medida de seguridad de ejecucion y debe pasarse siempre ds123456
* tipo: permite indicar el tipo de proceso segun los siguientes:
	1 = envio modelos
	2 = validacion de modelos (no necesita certificado)
	3 = consulta de modelos presentados
	4 = ratificacion domicilio renta
	5 = Descarga de datos fiscales de la renta
	6 = obtener relacion de certificados y sus datos.
			
* entrada: nombre del fichero que contiene los datos a enviar en txt
* salida: nombre del fichero en el que se dejara la respuesta
* SI/NO: indica si el proceso necesita usar certificado. Si no se indica, los procesos que lo necesiten pediran seleccionar un certificado
* numeroserie: numero de serie del certificado de los instalados en la maquina a utilizar en el proceso
* certificado: nombre del fichero.pfx que contiene el certificado digital.
* password: contraseña del certificado que se pasa por fichero
* NIF: para la descarga de datos fiscales es necesario pasra el NIF del contribuyente
* refRenta: codigo de 5 caracteres de la referencia de renta para la descarga de datos fiscales
* datosPersonales: en la descarga de datos fiscales, indica si se quieren tambien los datos personales (opciones 'S' o 'N')
* urlDescarga: direccion a la que hacer la peticion de descarga de datos fiscales (cambia cada año)

Ejemplos de uso:

    Envio modelos: gestionesAEAT dsclave 1 entrada.txt salida.txt SI (numeroserie | (certificado password)
    
    Validar modelos: gestionesAEAT dsclave 2 entrada.txt salida.txt NO
    
    Consulta modelos: gestionesAEAT dsclave 3 entrada.txt salida.txt SI (numeroserie | (certificado password)
    
    Ratificar domicilio renta: gestionesAEAT dsclave 4 entrada.txt salida.txt SI (numeroserie | (certificado password)
    