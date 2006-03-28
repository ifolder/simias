# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebClient_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderWebClient.cs
iFolderWebClient_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebClient_CSFILES))
iFolderWebClient_INCLUDES = 
iFolderWebClient_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderWebClient_FLAGS = $(CSC_EXEFLAG)
iFolderWebClient_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Novell.iFolderWeb.Proxy.dll
iFolderWebClient_LIBPATH = ../Proxy

EXTRA_DIST = $(iFolderWebClient_CSFILES) App.ico iFolderWebClient.in iFolderWebClient.cmd iFolderWebClient.exe.config AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
if DEBUG
if WINDOWS
DEBUG_FILES = iFolderWebClient.pdb
endif
endif

CLEAN_FILES = iFolderWebClient.exe iFolderWebClient.doc.xml $(DEBUG_FILES) AssemblyInfo.cs

all: iFolderWebClient.exe

iFolderWebClient.exe: $(iFolderWebClient_CSFILES) $(iFolderWebClient_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebClient_FLAGS) $(iFolderWebClient_LIBS:%=/r:%) $(iFolderWebClient_LIBPATH:%=/lib:%) $(iFolderWebClient_RESOURCES:%=$(ICON_FLAG)%) $(iFolderWebClient_CSFILES_CSC) /doc:iFolderWebClient.doc.xml

install-exec-local: all $(DATA_FILES)
if DEBUG	
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderWebClient.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) iFolderWebClient.cmd iFolderWebClient.exe.config $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) iFolderWebClient $(DESTDIR)$(bindir)
endif
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderWebClient.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f iFolderWebClient iFolderWebClient.cmd iFolderWebClient.exe.config
    
installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

