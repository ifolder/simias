
INCLUDES = -I$(EDIR_INCLUDE)

AM_CFLAGS = -pipe -Wall -W -O2

FOO1 = gcc -c -pipe -Wall -W -O2 $(INCLUDES) -o certcheck.o certcheck.c
FOO2 = gcc -o certcheck certcheck.o -L$(EDIR_LIBDIR) -lldapsdk -lldapssl -lc -lpthread


bin_PROGRAMS = certcheck

certcheck_SOURCES = certcheck.c

certcheck_LDADD = -L$(EDIR_LIBDIR) -lldapsdk -lldapssl -lgssapi -lpthread -ldl 


EXTRA_DIST = get-root-certificate.in get-root-certificate.cmd


install-exec-local:
	$(INSTALL_PROGRAM) $(bin_PROGRAMS) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) get-root-certificate.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) get-root-certificate $(DESTDIR)$(bindir)
endif

CLEAN_FILES = get-root-certificate

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f get-root-certificate*


