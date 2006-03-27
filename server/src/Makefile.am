SUBDIRS = Simias.Server iFolder.WebService Simias.HttpFile Simias.Rss

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
	sed -e 's,@_simiasdatadir_@,@simiasdatadir@,g' -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif	

simiasserver.cmd: $(srcdir)/simiasserver.cmd.in Makefile
if WINDOWS
	sed -e 's,@_simiasdatadir_@,$(shell cygpath --mixed $(simiasdatadir)),g' $< > $@
	chmod +x $@
endif
	
Simias.config: $(srcdir)/Simias.config.in
	sed -e 's,@_StoreProviderAssembly_@,@StoreProviderAssembly@,g' -e 's,@_StoreProviderType_@,@StoreProviderType@,g' $< > $@
	chmod +x $@

	
all: Simias.config simiasserver simiasserver.cmd

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(simiasdatadir) $(simiasdatadir)/modules
	$(INSTALL_PROGRAM) Simias.config $(simiasdatadir)
	$(INSTALL_PROGRAM) Simias.Server.conf $(simiasdatadir)/modules
if WINDOWS	
	$(INSTALL_PROGRAM) simiasserver.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simiasserver $(DESTDIR)$(bindir)
endif	

uninstall-local:
	cd $(simiasdatadir); rm -f Simias.config
	cd $(simiasdatadir)/modules; rm -f Simias.Server.conf
if WINDOWS	
	cd $(DESTDIR)$(bindir); rm -f simiasserver.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simiasserver
endif

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(DOC_FILE_CLEAN) Simias.config simiasserver simiasserver.cmd

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

