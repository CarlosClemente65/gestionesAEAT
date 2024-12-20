# gestionesAEAT v1.6.4.0
## Programa para la gestion de varios metodos de interaccion con los servidores web de Hacienda en la presentacion de declaraciones tributarias

### Desarrollado por Carlos Clemente (10-2024)

### Control de versiones
- Version 1.0.0.0 - Primera version funcional
- Version 1.3.0.0 - Incorporado envio facturas al SII
- Version 1.4.0.0 - Incorporado envio de declaraciones informativas
- Version 1.5.0.0 - Incorporado alta y consulta de pagos de modelos con NRC
- Version 1.6.2.0 - Incorporado descarga de documentos PDF mediante CSV
- Version 1.6.3.0 - Incorporado la generacion de un borrador en declaraciones informativas
- Version 1.6.4.0 - Incorporado la validacion de NIFs
<br>

### Funcionalidades:
- Validacion y presentacion directa de modelos tributarios periodicos
- Descarga de modelos presentados en un ejercicio de un contribuyente (individualmente por modelo)
- Consulta del estado de la ratificacion del domicilio para la renta.
- Descarga de datos fiscales de la renta.
- Envio de facturas al SII
- Presentacion de declaraciones informativas mediantes TGVI Online
- Pago de declaraciones mediante el cargo en cuenta con NRC
- Consulta de NRCs enviados a la AEAT por las entidades financieras
- Descarga de documentos PDF mediante codigo CSV que se envia a una url para su cotejo
- Validacion de NIFs en los servidores de la AEAT
<br>

### Parametros de ejecucion:
* dsclave: clave de seguridad para la ejecucion
* opciones.txt: fichero que contiene las opciones que admite la aplicacion que son las siguientes:
	* tipo: permite indicar el tipo de proceso segun los siguientes:
		- 1 = envio modelos
		- 2 = validacion de modelos (no necesita certificado)
		- 3 = consulta de modelos presentados
		- 4 = ratificacion domicilio renta
		- 5 = Descarga de datos fiscales de la renta
		- 6 = Presentacion de facturas al SII
		- 7 = Presentacion declaraciones informativas
		- 8 = Alta y consulta de pagos de modelos mediante cargo en cuenta con NRC
		- 9 = Descarga de documentos PDF a traves del codigo CSV
		- 10 = Validacion de NIFs
	* ENTRADA= Nombre del fichero que contiene los datos a enviar en txt
	* SALIDA= Nombre del fichero en el que se dejara la respuesta
	* URLSII= Url a la que hacer el envio de facturas al SII.
	* RESPUESTA= Etiquetas del xml de respuesta en el envio al SII de las que se quiere obtener los resultados
	* OBLIGADO= Indica si el proceso necesita usar certificado (valores SI/NO). Si no se indica, los procesos que lo necesiten pediran seleccionar un certificado
	* BUSQUEDA= Cadena a buscar en los certificados instalados en la maquina a utilizar en el proceso (puede ser el numero de serie, el NIF o el nombre del titular o del representante del certificado)
	* CERTIFICADO= Nombre del fichero.pfx que contiene el certificado digital.
	* PASSWORD= Contraseña del certificado que se pasa por fichero
	* NIFRENTA= Para la descarga de datos fiscales es necesario pasar el NIF del contribuyente
	* REFRENTA= Codigo de 5 caracteres de la referencia de renta para la descarga de datos fiscales
	* DPRENTA= En la descarga de datos fiscales, indica si se quieren tambien los datos personales (opciones 'S' o 'N')
	* URLRENTA= Direccion a la que hacer la peticion de descarga de datos fiscales (cambia cada año)
	* URLCSV= Url de descarga del documento mediante codigo CSV
<br>

<b>Ejemplos de uso: </b>

- gestionesAEAT dsclave guion.txt

<u>Envio de modelos con numero de serie</u>
```
CLIENTE=00001
TIPO=1
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=SI
BUSQUEDA=numeroserie
```

<u>Validar modelos (no necesita certificado)</u>
```
CLIENTE=00001
TIPO=2
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=NO
	
Nota: si en el guion viene el parametro VALIDAR=NO se genera el PDF sin validar
```
<u>Consulta modelos con fichero y password del certificado</u>
```
CLIENTE=00001
TIPO=3
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=SI
CERTIFICADO=certificado.pfx
PASSWORD=contraseña
```
<u>Ratificar domicilio renta con solicitud del certificado por pantalla</u>
```
CLIENTE=00001
TIPO=4
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=SI
```
<u>Descarga datos fiscales</u>
```
CLIENTE = 00065
TIPO=5
SALIDA=salida.txt
NIFRENTA=NIF
REFRENTA=refRenta
DPRENTA=S
URLRENTA=https://www9.agenciatributaria.gob.es/wlpl/DFPA-D182/SvDesDF23Pei
```
<u>Envio facturas al SII con numero de serie del certificado</u>
```
CLIENTE = 00065
TIPO=6
ENTRADA=facturaEmitida.xml
SALIDA=respuesta.xml
URLSII=https://prewww1.aeat.es/wlpl/SSII-FACT/ws/fe/SiiFactFEV1SOAP
OBLIGADO=SI
BUSQUEDA=numeroSerie
RESPUESTA=[siiR:CSV,sii:TimestampPresentacion,sii:NIF,sii:IDOtro,sii:ID,sii:NumSerieFacturaEmisor ... siiR:DescripcionErrorRegistro,faultstring]
```

<u>Presentacion declaraciones informativas con numero de serie del certificado</u>
```
CLIENTE=00065
TIPO=7
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=SI
BUSQUEDA=SerieCertificado
```

<u>Alta / consulta de pagos mediante NRC con el numero de serie del certificado</u>
```
CLIENTE=00065
TIPO=8
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=SI
BUSQUEDA=SerieCertificado
```

<u>Descarga de documentos PDF mediante CSV</u>
```
CLIENTE=00065
TIPO=9
URLCSV=https://prewww2.aeat.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc?COMPLETA=SI&ORIGEN=E&NIF=&CSV=SDMCFEZXM8QKF9ZU
SALIDA=salida.pdf	

Nota: no es necesario el fichero de entrada, se pone la url en el propio guion
```

<u>Validar NIFs</u>
```
TIPO=10
ENTRADA=entrada.txt
SALIDA=salida.txt
OBLIGADO=SI
BUSQUEDA=numeroserie
```