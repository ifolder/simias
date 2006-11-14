# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

DiscoveryWS_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/DiscoveryService.asmx.cs
DiscoveryWS_CSFILES_CSC := $(subst /,$(SEP),$(DiscoveryWS_CSFILES))
DiscoveryWS_INCLUDES =
DiscoveryWS_RESOURCES =
DiscoveryWS_FLAGS = $(CSC_LIBFLAG)
DiscoveryWS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll SimiasLib.dll Simias.Server.dll
DiscoveryWS_LIBPATH = ../SimiasClient ../SimiasLib.dll ../../webservices/ ../../server/Simias.Server/

EXTRA_DIST = DiscoveryService.asmx $(DiscoveryWS_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.Discovery.Web.dll

Simias.Discovery.Web.dll: $(DiscoveryWS_CSFILES) $(DiscoveryWS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(DiscoveryWS_FLAGS) $(SIMIAS_LIBS) $(DiscoveryWS_LIBS:%=/r:%) $(DiscoveryWS_LIBPATH:%=/lib:%) $(DiscoveryWS_RESOURCES:%=$(ICON_FLAG)%) $(DiscoveryWS_CSFILES_CSC) $(DOC_FILE_CMD)
	
if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Discovery.Web.pdb
endif
endif

install-exec-local: Simias.Discovery.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) DiscoveryService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Simias.Discovery.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f  DiscoveryService.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Simias.Discovery.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.Discovery.Web.dll $(DOC_FILE_CLEAN) 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES) AssemblyInfo.cs

