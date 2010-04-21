# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(webbindir)

FSBiFolderRestore_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderServer.cs $(srcdir)/xmlDocProcessing.cs $(srcdir)/iFolderAdminProxy.cs $(srcdir)/iFolderWebProxy.cs ../../server/setup/Options.cs 

FSBiFolderRestore_CSFILES_CSC := $(subst /,$(SEP),$(FSBiFolderRestore_CSFILES))
FSBiFolderRestore_INCLUDES = 
FSBiFolderRestore_RESOURCES =  
FSBiFolderRestore_LIBS = System.dll System.Web.dll System.Web.Services.dll
SIMIAS_WEBSERVICE_LIBS = $(SIMIAS_LIBS) /r:SimiasClient.dll /r:Simias.Discovery.Web.dll /r:Simias.Web.dll /r:SimiasLib.dll
FSBiFolderRestore_LIBPATH=../../core/SimiasClient/ ../../server/DiscoveryWS/ ../../core/WebService/ ../../core/SimiasLib.dll/

EXTRA_DIST = FSBiFolderRestore.exe $(FSBiFolderRestore_CSFILES) AssemblyInfo.cs.in FSBiFolderRestore

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: FSBiFolderRestore.exe FSBiFolderRestore

FSBiFolderRestore: FSBiFolderRestore.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@

FSBiFolderRestore.exe: $(FSBiFolderRestore_CSFILES) $(FSBiFolderRestore_RESOURCES) AssemblyInfo.cs
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_WEBSERVICE_LIBS) $(FSBiFolderRestore_LIBS:%=/r:%)  $(FSBiFolderRestore_LIBPATH:%=/lib:%) $(FSBiFolderRestore_RESOURCES:%=$(ICON_FLAG)%) $(FSBiFolderRestore_CSFILES_CSC)
#	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(FSBiFolderRestore_LIBS:%=/r:%) $(FSBiFolderRestore_RESOURCES:%=$(ICON_FLAG)%) $(FSBiFolderRestore_CSFILES_CSC)

install-exec-local: FSBiFolderRestore.exe FSBiFolderRestore
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FSBiFolderRestore.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) FSBiFolderRestore $(DESTDIR)$(bindir)
	
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f FSBiFolderRestore.exe FSBiFolderRestore
    
installcheck-local: install
	
CLEAN_FILES = FSBiFolderRestore.exe FSBiFolderRestore

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

