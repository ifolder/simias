# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebProxy_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderWebProxy.cs
iFolderWebProxy_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebProxy_CSFILES))
iFolderWebProxy_INCLUDES =
iFolderWebProxy_RESOURCES =
iFolderWebProxy_FLAGS = $(CSC_LIBFLAG)
iFolderWebProxy_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll
iFolderWebProxy_LIBPATH = 

EXTRA_DIST = AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolderWeb.Proxy.pdb
endif
endif

CLEAN_FILES = iFolderWebProxy.cs Novell.iFolderWeb.Proxy.dll \
	Novell.iFolderWeb.Proxy.doc.xml \
	$(DEBUG_FILES) \
	AssemblyInfo.cs

all: Novell.iFolderWeb.Proxy.dll

iFolderWebProxy.cs: $(EXTERNAL_DIR)/ifolder/iFolderWeb.wsdl
	wsdl /nologo /out:$@ $<

Novell.iFolderWeb.Proxy.dll: iFolderWebProxy.cs $(iFolderWebProxy_CSFILES) $(iFolderWebProxy_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebProxy_FLAGS) $(iFolderWebProxy_LIBS:%=/r:%) $(iFolderWebProxy_LIBPATH:%=/lib:%) $(iFolderWebProxy_CSFILES_CSC) /doc:iFolderWebProxy.doc.xml

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Novell.iFolderWeb.Proxy.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Novell.iFolderWeb.Proxy.dll $(DEBUG_FILES)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

