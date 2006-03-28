# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderReportsProxy_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderReportsProxy.cs
iFolderReportsProxy_CSFILES_CSC := $(subst /,$(SEP),$(iFolderReportsProxy_CSFILES))
iFolderReportsProxy_INCLUDES =
iFolderReportsProxy_RESOURCES =
iFolderReportsProxy_FLAGS = $(CSC_LIBFLAG)
iFolderReportsProxy_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll
iFolderReportsProxy_LIBPATH = 

EXTRA_DIST = AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.Enterprise.Reports.Proxy.pdb
endif
endif

CLEAN_FILES = iFolderReportsProxy.cs Novell.iFolder.Enterprise.Reports.Proxy.dll \
	AssemblyInfo.cs Novell.iFolder.Enterprise.Reports.Proxy.doc.xml \
	$(DEBUG_FILES)

all: Novell.iFolder.Enterprise.Reports.Proxy.dll

iFolderReportsProxy.cs: ../WebService/iFolderReports.wsdl
	wsdl /nologo /out:$@ $<

Novell.iFolder.Enterprise.Reports.Proxy.dll: iFolderReportsProxy.cs $(iFolderReportsProxy_CSFILES) $(iFolderReportsProxy_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderReportsProxy_FLAGS) $(iFolderReportsProxy_LIBS:%=/r:%) $(iFolderReportsProxy_LIBPATH:%=/lib:%) $(iFolderReportsProxy_CSFILES_CSC) /doc:iFolderReportsProxy.doc.xml

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Novell.iFolder.Enterprise.Reports.Proxy.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Novell.iFolder.Enterprise.Reports.Proxy.dll $(DEBUG_FILES)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

