# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

if WINDOWS
SYSTEM_INSTALLER_LIB = System.Configuration.Install.dll
else
SYSTEM_INSTALLER_LIB =
endif

MONO_WEBSERVER_PATH = ../../../dependencies/external/Mono.WebServer

Simias_CSFILES = $(srcdir)/Simias.cs \
		$(srcdir)/SimiasAppInstallerClass.cs

Simias_CSFILES_CSC := $(subst /,$(SEP),$(Simias_CSFILES))
Simias_INCLUDES =
Simias_RESOURCES = $(srcdir)/App$(ICON_EXT)
Simias_FLAGS = $(CSC_EXEFLAG)
Simias_LIBS = System.dll System.Web.Services.dll System.Data.dll $(SYSTEM_XML) System.Web.dll $(SYSTEM_INSTALLER_LIB) SimiasClient.dll $(MONO_WEBSERVER_DLL) 
Simias_LIBPATH = ../SimiasClient $(MONO_WEBSERVER_PATH)

EXTRA_DIST = App.ico $(Simias_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

if WINDOWS
SIMIAS_PATH = $(shell cygpath --mixed $(webbindir))/Simias.exe
else
SIMIAS_PATH = @webbindir@/Simias.exe
endif

all: Simias.exe

Simias.exe: $(srcdir)/AssemblyInfo.cs $(Simias_CSFILES) $(Simias_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Simias_FLAGS) $(SIMIAS_LIBS) $(Simias_LIBS:%=/r:%) $(Simias_LIBPATH:%=/lib:%) $(Simias_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(Simias_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.pdb
endif
endif

install-exec-local: Simias.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.exe $(DEBUG_FILES) $(DESTDIR)$(webbindir)

	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.exe $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Simias.exe $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

