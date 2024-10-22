# gestionesAEAT v1.5.0.0
## Programa para la gestion de varios metodos de interaccion con los servidores web de Hacienda en la presentacion de declaraciones tributarias

### Desarrollado por Carlos Clemente (10-2024)

### Control de versiones
- Version 1.0.0.0 - Primera version funcional
- Version 1.3.0.0 - Incorporado envio facturas al SII
- Version 1.4.0.0 - Incorporado envio de declaraciones informativas
- Version 1.5.0.0 - Incorporado alta y consulta de pagos de modelos con NRC
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
	* ENTRADA= Nombre del fichero que contiene los datos a enviar en txt
	* SALIDA= Nombre del fichero en el que se dejara la respuesta
	* URLSII= Url a la que hacer el envio de facturas al SII.
	* OBLIGADO= Indica si el proceso necesita usar certificado (valores SI/NO). Si no se indica, los procesos que lo necesiten pediran seleccionar un certificado
	* BUSQUEDA= Cadena a buscar en los certificados instalados en la maquina a utilizar en el proceso (puede ser el numero de serie, el NIF o el nombre del titular o del representante del certificado)
	* CERTIFICADO= Nombre del fichero.pfx que contiene el certificado digital.
	* PASSWORD= Contraseña del certificado que se pasa por fichero
	* NIFRENTA= Para la descarga de datos fiscales es necesario pasar el NIF del contribuyente
	* REFRENTA= Codigo de 5 caracteres de la referencia de renta para la descarga de datos fiscales
	* DPRENTA= En la descarga de datos fiscales, indica si se quieren tambien los datos personales (opciones 'S' o 'N')
	* URLRENTA= Direccion a la que hacer la peticion de descarga de datos fiscales (cambia cada año)
	* RESPUESTA= Etiquetas del xml de respuesta en el envio al SII de las que se quiere obtener los resultados
<br>

<b>Ejemplos de uso: </b>

- gestionesAEAT dsclave opciones.txt

<u>Envio de modelos</u>
```
Fichero de opciones con numero de serie
	CLIENTE=00001
	TIPO=1
	ENTRADA=entrada.txt
	SALIDA=salida.txt
	OBLIGADO=SI
	BUSQUEDA=numeroserie
```

<u>Validar modelos</u>
```
Fichero de opciones (no necesita certificado)
	CLIENTE=00001
	TIPO=2
	ENTRADA=entrada.txt
	SALIDA=salida.txt
	OBLIGADO=NO
	
	Nota: si en el guion viene el parametro VALIDAR=NO se genera el PDF sin validar
```
<u>Consulta modelos</u>
```
Fichero de opciones con fichero y password del certificado
	CLIENTE=00001
	TIPO=3
	ENTRADA=entrada.txt
	SALIDA=salida.txt
	OBLIGADO=SI
	CERTIFICADO=certificado.pfx
	PASSWORD=contraseña
```
<u>Ratificar domicilio renta</u>
```
Fichero de opciones con solicitud del certificado por pantalla
	CLIENTE=00001
	TIPO=4
	ENTRADA=entrada.txt
	SALIDA=salida.txt
	OBLIGADO=SI
```
<u>Descarga datos fiscales</u>
```
Fichero de opciones
	CLIENTE = 00065
	TIPO=5
	SALIDA=salida.txt
	NIFRENTA=NIF
	REFRENTA=refRenta
	DPRENTA=S
	URLRENTA=https://www9.agenciatributaria.gob.es/wlpl/DFPA-D182/SvDesDF23Pei
```
<u>Envio facturas al SII</u>
```
Fichero de opciones con numero de serie del certificado
	CLIENTE = 00065
	TIPO=6
	ENTRADA=facturaEmitida.xml
	SALIDA=respuesta.xml
	URLSII=https://prewww1.aeat.es/wlpl/SSII-FACT/ws/fe/SiiFactFEV1SOAP
	OBLIGADO=SI
	BUSQUEDA=numeroSerie
	RESPUESTA=[siiR:CSV,sii:TimestampPresentacion,sii:NIF,sii:IDOtro,sii:ID,sii:NumSerieFacturaEmisor ... siiR:DescripcionErrorRegistro,faultstring]
```

<u>Presentacion declaraciones informativas</u>
```
	CLIENTE=00065
	TIPO=7
	ENTRADA=entrada.txt
	SALIDA=salida.txt
	OBLIGADO=SI
	BUSQUEDA=SerieCertificado
```

<u>Alta / consulta de pagos mediante NRC</u>
```
	CLIENTE=00065
	TIPO=8
	ENTRADA=entrada.txt
	SALIDA=salida.txt
	OBLIGADO=SI
	BUSQUEDA=SerieCertificado
```