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
CMFLAGS=
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
all:		mtom-stream-test
mtom-stream-test:	mtom-stream-test.h mtom-stream-test.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c -I../../import mtom-stream-test.h
		$(CC) $(CFLAGS) -o mtom-stream-test mtom-stream-test.c soapC.c soapClient.c soapServer.c $(SOAPC) $(LIBS) -lpthread
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h mtom-stream-test
