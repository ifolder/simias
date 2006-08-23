SUBDIRS = Simias.Server Simias.HostService Simias.HttpFile Simias.Rss Simias.LdapProvider Simias.ADLdapProvider Simias.OpenLdapProvider Simias.ClientUpdate setup

DIST_SUBDIRS = $(SUBDIRS) setup

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

EXTRA_DIST = Simias.config.in Simias.Server.conf simiasserver.in simiasserver.cmd.in 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

simiasserver: $(srcdir)/simiasserver.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		

all: simiasserver

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bootstrapdir) $(DESTDIR)$(bootstrapdir)/modules
	$(INSTALL_PROGRAM) Simias.config $(DESTDIR)$(bootstrapdir)
	$(INSTALL_PROGRAM) Simias.Server.conf $(DESTDIR)$(bootstrapdir)/modules
if WINDOWS	
	$(INSTALL_PROGRAM) simiasserver.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simiasserver $(DESTDIR)$(bindir)
endif	

uninstall-local:
	cd $(DESTDIR)$(bootstrapdir); rm -f Simias.config
	cd $(DESTDIR)$(bootstrapdir)/modules; rm -f Simias.Server.conf
if WINDOWS	
	cd $(DESTDIR)$(bindir); rm -f simiasserver.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simiasserver
endif

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf Simias.config simiasserver simiasserver.cmd

