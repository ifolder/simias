GSOAP=../../bin/soapcpp2
SOAPH=../../stdsoap2.h
SOAPC=../../stdsoap2.c
SOAPCPP=../../stdsoap2.cpp
CC=gcc
CPP=g++
LIBS=
COFLAGS=-O2
CWFLAGS=-Wall
CIFLAGS=-I../..
CMFLAGS=-DWITH_UDP -DWITH_GZIP
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
all:		udpclient udpserver
udpclient:	udp.h udpclient.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c -I../../import udp.h
		$(CC) $(CFLAGS) -o udpclient udpclient.c soapC.c soapClient.c $(SOAPC) $(LIBS) -lz
udpserver:	udp.h udpserver.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c -I../../import udp.h
		$(CC) $(CFLAGS) -o udpserver udpserver.c soapC.c soapServer.c $(SOAPC) $(LIBS) -lz
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h soap*Object.h
distclean:
		rm -f *.o *.wsdl *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h soap*Object.h udpclient udpserver
