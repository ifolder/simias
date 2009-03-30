# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasServer_CSFILES = \
	$(srcdir)/Service.cs \
	$(srcdir)/Authentication.cs \
	$(srcdir)/Catalog.cs \
	$(srcdir)/Domain.cs \
	$(srcdir)/DomainInfo.cs \
	$(srcdir)/Exceptions.cs \
	$(srcdir)/ProvisionInfo.cs \
	$(srcdir)/DomainService.asmx.cs \
	$(srcdir)/HostLocationService.cs \
	$(srcdir)/HostProvider.cs \
	$(srcdir)/IdentitySyncService.cs \
	$(srcdir)/IIdentitySyncProvider.cs \
	$(srcdir)/IdentitySyncManager.asmx.cs \
	$(srcdir)/IProvisionUser.cs \
	$(srcdir)/Registration.asmx.cs \
	$(srcdir)/Report.cs \
	$(srcdir)/ReportLogHandler.ashx.cs \
	$(srcdir)/IUserProvider.cs \
	$(srcdir)/User.cs \
	$(srcdir)/JournalService.cs \
	$(srcdir)/UserMove.cs \
	$(srcdir)/Utils.cs
	
SimiasServer_CSFILES_CSC := $(subst /,$(SEP),$(SimiasServer_CSFILES))
SimiasServer_INCLUDES =

SimiasServer_RESX = \
	$(srcdir)/Report.resx

SimiasServer_RESOURCES := $(SimiasServer_RESX:$(srcdir)/%.resx=$(srcdir)/Simias.Server.%.resources)

SimiasServer_FLAGS = $(CSC_LIBFLAG)
SimiasServer_LIBPATH = ../../core/SimiasLib.dll ../../core/SimiasClient $(DESTDIR)$(webbindir)
SimiasServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll

EXTRA_DIST =  AssemblyInfo.cs.in DomainService.asmx Registration.asmx IdentitySyncManager.asmx $(SimiasServer_CSFILES) $(SimiasServer_RESX) ReportLogHandler.ashx

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.Server.dll

all: $(SERVER_ASSEMBLIES)

Simias.Server.dll: $(srcdir)/AssemblyInfo.cs $(SimiasServer_CSFILES) $(SimiasServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServer_FLAGS) $(SimiasServer_LIBPATH:%=/lib:%) $(SimiasServer_LIBS:%=/r:%) $(SimiasServer_RESOURCES:%=/res:%) $(srcdir)/AssemblyInfo.cs $(SimiasServer_CSFILES_CSC) $(DOC_FILE_CMD)
	
Simias.Server.%.resources : %.resx
	$(RESGEN) $< $@

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.Server.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) 
	$(INSTALL_PROGRAM) DomainService.asmx IdentitySyncManager.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Registration.asmx ReportLogHandler.ashx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f DomainService.asmx IdentitySyncManager.asmx
	cd $(DESTDIR)$(webdir); rm -f Regististration.asmx ReportLogHandler.ashx
	cd $(DESTDIR)$(webbindir); rm -f $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES)

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(SERVER_ASSEMBLIES) $(DOC_FILE_CLEAN) $(SimiasServer_RESOURCES) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile	

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

