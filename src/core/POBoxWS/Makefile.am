# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

POBoxWS_CSFILES = $(srcdir)/POBoxService.asmx.cs
POBoxWS_CSFILES_CSC := $(subst /,$(SEP),$(POBoxWS_CSFILES))
POBoxWS_INCLUDES =
POBoxWS_RESOURCES =
POBoxWS_FLAGS = $(CSC_LIBFLAG)
POBoxWS_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll SimiasLib.dll
POBoxWS_LIBPATH = ../SimiasClient ../SimiasLib.dll

EXTRA_DIST = POService.asmx $(POBoxWS_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.POBox.Web.dll

Simias.POBox.Web.dll: $(srcdir)/AssemblyInfo.cs $(POBoxWS_CSFILES) $(POBoxWS_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(POBoxWS_FLAGS) $(SIMIAS_LIBS) $(POBoxWS_LIBS:%=/r:%) $(POBoxWS_LIBPATH:%=/lib:%) $(POBoxWS_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(POBoxWS_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.POBox.Web.pdb
endif
endif

install-exec-local: Simias.POBox.Web.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) POService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Simias.POBox.Web.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f  POService.asmx $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Simias.POBox.Web.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.POBox.Web.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES) AssemblyInfo.cs
	rm -rf AssemblyInfo.cs

