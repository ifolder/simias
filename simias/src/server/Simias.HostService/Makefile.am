# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasHostLocation_CSFILES = $(srcdir)/HostService.cs 
	
SimiasHostLocation_CSFILES_CSC := $(subst /,$(SEP),$(SimiasHostLocation_CSFILES))
SimiasHostLocation_INCLUDES =
SimiasHostLocation_RESOURCES =
SimiasHostLocation_FLAGS = $(CSC_LIBFLAG)
SimiasHostLocation_LIBPATH = ../../core/SimiasLib.dll ../../core/SimiasClient ../Simias.Server $(DESTDIR)$(webbindir)
SimiasHostLocation_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll

EXTRA_DIST = HostLocation.asmx HostAdmin.asmx $(SimiasHostLocation_CSFILES) 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

HOSTSERVICE_ASSEMBLIES := Simias.HostService.dll

Simias.HostService.dll: $(SimiasHostLocation_CSFILES) $(SimiasHostLocation_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasHostLocation_FLAGS) $(SimiasHostLocation_LIBPATH:%=/lib:%) $(SimiasHostLocation_LIBS:%=/r:%) $(SimiasHostLocation_RESOURCES:%=$(ICON_FLAG)%) $(SimiasHostLocation_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(HOSTSERVICE_ASSEMBLIES)

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.HostService.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) 
	$(INSTALL_PROGRAM) HostAdmin.asmx HostLocation.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(HOSTSERVICE_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f HostAdmin.asmx HostLocation.asmx
	cd $(DESTDIR)$(webbindir); rm -f $(HOSTSERVICE_ASSEMBLIES) $(DEBUG_CHECK_FILES)

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(HOSTSERVICE_ASSEMBLIES) $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

