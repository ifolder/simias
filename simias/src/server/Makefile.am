# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasServer_CSFILES = \
	$(srcdir)/Service.cs \
	$(srcdir)/Authentication.cs \
	$(srcdir)/Domain.cs \
	$(srcdir)/DomainInfo.cs \
	$(srcdir)/ProvisionInfo.cs \
	$(srcdir)/DomainService.asmx.cs \
	$(srcdir)/IdentitySyncService.cs \
	$(srcdir)/IIdentitySyncProvider.cs \
	$(srcdir)/IdentitySyncManager.asmx.cs \
	$(srcdir)/Registration.asmx.cs \
	$(srcdir)/IUserProvider.cs \
	$(srcdir)/User.cs \
	$(srcdir)/JournalService.cs
	
SimiasServer_CSFILES_CSC := $(subst /,$(SEP),$(SimiasServer_CSFILES))
SimiasServer_INCLUDES =
SimiasServer_RESOURCES =
SimiasServer_FLAGS = $(CSC_LIBFLAG)
SimiasServer_LIBPATH = 
SimiasServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll
#SimiasServer_LIBPATH = ../../src/SimiasLib.dll ../../src/SimiasClient $(DESTDIR)$(webbindir)

EXTRA_DIST = DomainService.asmx Registration.asmx IdentitySyncManager.asmx $(SimiasServer_CSFILES) 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.Server.dll

Simias.Server.dll: $(SimiasServer_CSFILES) $(SimiasServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServer_FLAGS) $(SIMIAS_CLIENT_LIBS) $(SIMIAS_LIBS) $(SimiasServer_LIBPATH:%=/lib:%) $(SimiasServer_LIBS:%=/r:%) $(SimiasServer_RESOURCES:%=$(ICON_FLAG)%) $(SimiasServer_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(SERVER_ASSEMBLIES)

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.Server.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(simiasdatadir) $(simiasdatadir)/modules
	$(INSTALL_PROGRAM) DomainService.asmx IdentitySyncManager.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Registration.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f DomainService.asmx IdentitySyncManager.asmx
	cd $(DESTDIR)$(webdir); rm -f Regististration.asmx
	cd $(DESTDIR)$(webbindir); rm -f $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES)

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(SERVER_ASSEMBLIES) $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

