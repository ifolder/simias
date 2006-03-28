INCLUDES = -I. -I$(GSOAP_DIR) -DWITH_NONAMESPACES -DWITH_COOKIES -DWITH_OPENSSL

lib_LTLIBRARIES = libiFolderWebAccess.la

libiFolderWebAccess_la_LDFLAGS = -version-info $(LIB_VERSION) -lssl -lcrypto

nodist_libiFolderWebAccess_la_SOURCES = \
	envC.c iFolderWebAccessClientLib.c \
	$(GSOAP_DIR)/stdsoap2.c

EXTRA_DIST = typemap.dat 

CUR_DIR := $(shell pwd)

CLEAN_FILES = iFolderWebAccess*.* env*.* RECV.log TEST.log

all:libiFolderWebAccess.la

iFolderWebAccess.h: $(EXTERNAL_DIR)/ifolder/iFolderWebAccess.wsdl
	$(GSOAP_DIR)/wsdl2h -c -o $@ $<

iFolderWebAccessClientLib.c: iFolderWebAccess.h
	$(GSOAP_DIR)/soapcpp2 -c -w -p iFolderWebAccess $<

env.h:
	> env.h

envC.c: env.h
	$(GSOAP_DIR)/soapcpp2 -c -w -p env $<

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

