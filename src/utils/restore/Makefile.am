# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

ifrecovery_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderServer.cs $(srcdir)/xmlDocProcessing.cs $(srcdir)/iFolderAdminProxy.cs $(srcdir)/iFolderWebProxy.cs ../../server/setup/Options.cs 

ifrecovery_CSFILES_CSC := $(subst /,$(SEP),$(ifrecovery_CSFILES))
ifrecovery_INCLUDES = 
ifrecovery_RESOURCES =  
ifrecovery_LIBS = System.dll System.Web.dll System.Web.Services.dll
SIMIAS_WEBSERVICE_LIBS = $(SIMIAS_LIBS) /r:SimiasClient.dll /r:Simias.Discovery.Web.dll /r:Simias.Web.dll /r:SimiasLib.dll 

ifrecovery_LIBPATH=../../core/SimiasClient/ ../../server/DiscoveryWS/ ../../core/WebService/ ../../core/SimiasLib.dll/ ../../webservices

dist_man_MANS=ifolder-data-recovery.8

EXTRA_DIST = ifrecovery.exe $(ifrecovery_CSFILES) AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

all: ifrecovery.exe ifolder-data-recovery

ifolder-data-recovery: ifolder-data-recovery.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@

ifrecovery.exe: $(ifrecovery_CSFILES) $(ifrecovery_RESOURCES) AssemblyInfo.cs
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_WEBSERVICE_LIBS) $(ifrecovery_LIBS:%=/r:%)  $(ifrecovery_LIBPATH:%=/lib:%) $(ifrecovery_RESOURCES:%=$(ICON_FLAG)%) $(ifrecovery_CSFILES_CSC)

install-exec-local: ifrecovery.exe ifolder-data-recovery 
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifrecovery.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-data-recovery $(DESTDIR)$(bindir)
	
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ifrecovery.exe ifolder-data-recovery
    
installcheck-local: install
	
CLEAN_FILES = ifrecovery.exe ifolder-data-recovery

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

