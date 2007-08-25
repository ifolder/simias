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
all:		xml-rpc-currentTime xml-rpc-weblogs
xml-rpc-currentTime:	xml-rpc.h xml-rpc-currentTime.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c xml-rpc.h
		$(CC) $(CFLAGS) -o xml-rpc-currentTime xml-rpc-currentTime.c soapC.c $(SOAPC) $(LIBS)
xml-rpc-weblogs:	xml-rpc.h xml-rpc-io.h xml-rpc-iters.h xml-rpc.cpp
xml-rpc-io.cpp xml-rpc-weblogs.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) xml-rpc.h
		$(CPP) $(CFLAGS) -o xml-rpc-weblogs xml-rpc-weblogs.cpp xml-rpc.cpp xml-rpc-io.cpp $(SOAPCPP) $(LIBS)
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h xml-rpc-currentTime xml-rpc-weblogs
