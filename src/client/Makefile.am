# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

#Simias_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/Simias.cs $(srcdir)/server.cs $(srcdir)/ApplicationServer.cs $(srcdir)/IApplicationHost.cs $(srcdir)/IWebSource.cs $(srcdir)/Tracing.cs $(srcdir)/XSPApplicationHost.cs $(srcdir)/BaseApplicationHost.cs $(srcdir)/BaseRequestBroker.cs $(srcdir)/MonoWorkerRequest.cs $(srcdir)/XSPWorkerRequest.cs $(srcdir)/LingeringNetworkStream.cs $(srcdir)/InitialWorkerRequest.cs $(srcdir)/SimiasAppInstallerClass.cs
#Simias_CSFILES_CSC := $(subst /,$(SEP),$(Simias_CSFILES))
#Simias_INCLUDES =
#Simias_RESOURCES = $(srcdir)/App$(ICON_EXT)
#Simias_FLAGS = $(CSC_EXEFLAG)
#Simias_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll $(SYSTEM_INSTALLER_LIB) SimiasClient.dll
#Simias_LIBPATH = ../SimiasClient

EXTRA_DIST = $(srcdir)/SimiasDirectoryMapping.in $(srcdir)/simias.cmd $(srcdir)/simias.in

if WINDOWS
SIMIAS_PATH = $(shell cygpath --mixed $(webbindir))/Simias.exe
else
SIMIAS_PATH = @webbindir@/Simias.exe
endif

all:

#Simias.exe: AssemblyInfo.cs $(Simias_CSFILES) $(Simias_RESOURCES) SimiasDirectoryMapping simias
#	$(CSC) /out:$@ $(CSCFLAGS) $(Simias_FLAGS) $(SIMIAS_LIBS) $(Simias_LIBS:%=/r:%) $(Simias_LIBPATH:%=/lib:%) $(Simias_RESOURCES:%=$(ICON_FLAG)%) $(Simias_CSFILES_CSC) $(DOC_FILE_CMD)

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasDirectoryMapping $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) simias.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif

	
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SimiasDirectoryMapping
if WINDOWS
	cd $(DESTDIR)$(bindir); rm -f simias.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simias
endif

installcheck-local: install
	
CLEAN_FILES = $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf SimiasDirectoryMapping simias

