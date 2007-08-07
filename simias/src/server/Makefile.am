SUBDIRS = Simias.Server Simias.HostService Simias.HttpFile Simias.Rss Simias.LdapProvider Simias.ADLdapProvider Simias.OpenLdapProvider Simias.ClientUpdate setup DiscoveryWS

DIST_SUBDIRS = $(SUBDIRS) setup

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

EXTRA_DIST = Simias.config.in Simias.Server.conf simias-server.in simias-server.cmd.in 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

simias-server.cmd: $(srcdir)/simias-server.cmd.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< >$@

simias-server: $(srcdir)/simias-server.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< >$@

if !WINDOWS
	chmod +x $@
endif

all: simias-server.cmd simias-server

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bootstrapdir) $(DESTDIR)$(bootstrapdir)/modules
	$(INSTALL_PROGRAM) Simias.config $(DESTDIR)$(bootstrapdir)
	$(INSTALL_PROGRAM) Simias.Server.conf $(DESTDIR)$(bootstrapdir)/modules
if WINDOWS
	$(INSTALL_PROGRAM) ./simias-server.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) ./simias-server $(DESTDIR)$(bindir)
endif	

uninstall-local:
	cd $(DESTDIR)$(bootstrapdir); rm -f Simias.config
	cd $(DESTDIR)$(bootstrapdir)/modules; rm -f Simias.Server.conf
if WINDOWS	
	cd $(DESTDIR)$(bindir); rm -f simias-server.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simias-server
endif

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	rm -rf simias-server simias-server.cmd

distclean: distclean-recursive

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf simias-server simias-server.cmd

