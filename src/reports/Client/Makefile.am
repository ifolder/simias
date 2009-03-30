# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderReportsClient_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderReportsClient.cs
iFolderReportsClient_CSFILES_CSC := $(subst /,$(SEP),$(iFolderReportsClient_CSFILES))
iFolderReportsClient_INCLUDES = 
iFolderReportsClient_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderReportsClient_FLAGS = $(CSC_EXEFLAG)
iFolderReportsClient_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Novell.iFolder.Enterprise.Reports.Proxy.dll
iFolderReportsClient_LIBPATH = ../Proxy

EXTRA_DIST = $(srcdir)/AssemblyInfo.cs.in $(iFolderReportsClient_CSFILES) App.ico ifolder-reports-generate.in ifolder-reports-generate.cmd iFolderReportsClient.exe.config

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
if DEBUG
if WINDOWS
DEBUG_FILES = iFolderReportsClient.pdb
endif
endif

CLEAN_FILES = iFolderReportsClient.exe iFolderReportsClient.doc.xml AssemblyInfo.cs $(DEBUG_FILES)

all: iFolderReportsClient.exe

iFolderReportsClient.exe: $(iFolderReportsClient_CSFILES) $(iFolderReportsClient_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderReportsClient_FLAGS) $(SIMIAS_CLIENT_LIBS) $(SIMIAS_LIBS) $(iFolderReportsClient_LIBS:%=/r:%) $(iFolderReportsClient_LIBPATH:%=/lib:%) $(iFolderReportsClient_RESOURCES:%=$(ICON_FLAG)%) $(iFolderReportsClient_CSFILES_CSC) /doc:iFolderReportsClient.doc.xml

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderReportsClient.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) ifolder-reports-generate.cmd iFolderReportsClient.exe.config $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) ifolder-reports-generate $(DESTDIR)$(bindir)
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderReportsClient.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f ifolder-reports-generate ifolder-reports-generate.cmd iFolderReportsClient.exe.config
    
installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

