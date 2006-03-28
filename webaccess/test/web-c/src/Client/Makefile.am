INCLUDES = -I. -I../Proxy -I$(GSOAP_DIR) -DWITH_COOKIES -DWITH_OPENSSL

bin_PROGRAMS = iFolderWebClientC

iFolderWebClientC_SOURCES = \
	./iFolderWebClient.c

iFolderWebClientC_LDADD = -L../Proxy -liFolderWebAccess

CLEAN_FILES = 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

