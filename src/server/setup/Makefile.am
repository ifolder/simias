# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
DIST_SUBDIRS = $(SUBDIRS)
SimiasServerSetup_CSFILES = \
	$(srcdir)/SimiasServerSetup.cs \
	$(srcdir)/HostAdmin.cs \
	$(srcdir)/Options.cs \
	$(srcdir)/Prompt.cs \
	$(srcdir)/LdapUtility.cs
	
iFolderWebSetup_CSFILES = \
	$(srcdir)/iFolderWebSetup.cs \
	$(srcdir)/HostAdmin.cs \
	$(srcdir)/Options.cs \
	$(srcdir)/Prompt.cs \
	$(srcdir)/LdapUtility.cs

iFolderAdminSetup_CSFILES = \
	$(srcdir)/iFolderAdminSetup.cs \
	$(srcdir)/HostAdmin.cs \
	$(srcdir)/Options.cs \
	$(srcdir)/Prompt.cs \
	$(srcdir)/LdapUtility.cs

iFolderProxySetup_CSFILES = \
	$(srcdir)/iFolderProxySetup.cs

	
SimiasServerSetup_CSFILES_CSC := $(subst /,$(SEP),$(SimiasServerSetup_CSFILES))
iFolderWebSetup_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebSetup_CSFILES))
iFolderAdminSetup_CSFILES_CSC := $(subst /,$(SEP),$(iFolderAdminSetup_CSFILES))
iFolderProxySetup_CSFILES_CSC := $(subst /,$(SEP),$(iFolderProxySetup_CSFILES))
SimiasServerSetup_INCLUDES =
SimiasServerSetup_RESOURCES = $(srcdir)/App$(ICON_EXT)
SimiasServerSetup_FLAGS = $(CSC_EXEFLAG)
SimiasServerSetup_LIBPATH = $(LOG4NET_DIR) ../../core/SimiasLib.dll ../../core/SimiasClient ../Simias.Server ../../../external/csharpldap ../Simias.LdapProvider
SimiasServerSetup_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	Novell.Directory.Ldap.dll \
	Simias.Server.dll \
	Mono.Security.dll \
	SimiasLib.dll \
	SimiasClient.dll \
	Simias.LdapProvider.dll

if !WINDOWS
	SimiasServerSetup_LIBS := $(SimiasServerSetup_LIBS1) Mono.Posix.dll
else
	SimiasServerSetup_LIBS := $(SimiasServerSetup_LIBS1)
endif

EXTRA_DIST = $(SimiasServerSetup_CSFILES) $(srcdir)/iFolderWebSetup.cs iFolderAdminSetup.cs iFolderProxySetup.cs utility.cs simias-server-setup.in ifolder_mono_setup.in ifolder_cluster_setup ifolder_shutdown ifolder_start simias-server-setup.cmd ifolder-mod-mono-server2.in App.ico SimiasServerSetup.exe.config iFolderWebSetup.exe.config ifolder-web-setup.in iFolderAdminSetup.exe.config ifolder-admin-setup.in AssemblyInfo.cs.in novell-ifolder3.conf.in iFolder_proxy_rights_assign.in iFolder_retrieve_proxy_creds.in iFolder_update_proxy_cred_store.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

ifolder-mod-mono-server2: $(srcdir)/ifolder-mod-mono-server2.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		

novell-ifolder3.conf: $(srcdir)/novell-ifolder3.conf.in
	sed -e 's,@_prefix_@,@prefix@,g' -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_lib64_@,@LIB64@,g' $< > $@        
	chmod +x $@
	
simias-server-setup: $(srcdir)/simias-server-setup.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		

ifolder_mono_setup: $(srcdir)/ifolder_mono_setup.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		

ifolder-web-setup: $(srcdir)/ifolder-web-setup.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		

ifolder-admin-setup: $(srcdir)/ifolder-admin-setup.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		

iFolder_update_proxy_cred_store: $(srcdir)/iFolder_update_proxy_cred_store.in
if !WINDOWS
	sed -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif

iFolder_retrieve_proxy_creds: $(srcdir)/iFolder_retrieve_proxy_creds.in
if !WINDOWS
	sed -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif

iFolder_proxy_rights_assign: $(srcdir)/iFolder_proxy_rights_assign.in
if !WINDOWS
	sed -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif

	
if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SimiasServerSetup.exe iFolderWebSetup.exe iFolderAdminSetup.exe iFolderProxySetup.exe novell-ifolder3.conf 

SimiasServerSetup.exe: $(SimiasServerSetup_CSFILES) $(SimiasServerSetup_RESOURCES) simias-server-setup ifolder-mod-mono-server2 ifolder_mono_setup ifolder_cluster_setup ifolder_shutdown ifolder_start
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServerSetup_FLAGS) $(SIMIAS_LIBS) $(SimiasServerSetup_LIBS:%=/r:%) $(SimiasServerSetup_LIBPATH:%=/lib:%) $(SimiasServerSetup_RESOURCES:%=$(ICON_FLAG)%) $(SimiasServerSetup_CSFILES_CSC) $(DOC_FILE_CMD)
iFolderWebSetup.exe: $(iFolderWebSetup_CSFILES) $(SimiasServerSetup_RESOURCES) ifolder-web-setup
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServerSetup_FLAGS) $(SIMIAS_LIBS) $(SimiasServerSetup_LIBS:%=/r:%) $(SimiasServerSetup_LIBPATH:%=/lib:%) $(SimiasServerSetup_RESOURCES:%=$(ICON_FLAG)%) $(iFolderWebSetup_CSFILES_CSC) $(DOC_FILE_CMD)
iFolderAdminSetup.exe: $(iFolderAdminSetup_CSFILES) $(SimiasServerSetup_RESOURCES) ifolder-admin-setup
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServerSetup_FLAGS) $(SIMIAS_LIBS) $(SimiasServerSetup_LIBS:%=/r:%) $(SimiasServerSetup_LIBPATH:%=/lib:%) $(SimiasServerSetup_RESOURCES:%=$(ICON_FLAG)%) $(iFolderAdminSetup_CSFILES_CSC) $(DOC_FILE_CMD)
iFolderProxySetup.exe: $(iFolderProxySetup_CSFILES) $(SimiasServerSetup_RESOURCES) iFolder_proxy_rights_assign iFolder_retrieve_proxy_creds iFolder_update_proxy_cred_store  
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServerSetup_FLAGS) $(SIMIAS_LIBS) $(SimiasServerSetup_LIBS:%=/r:%) $(SimiasServerSetup_LIBPATH:%=/lib:%) $(SimiasServerSetup_RESOURCES:%=$(ICON_FLAG)%) $(iFolderProxySetup_CSFILES_CSC) $(DOC_FILE_CMD)


if DEBUG
if WINDOWS
DEBUG_FILES = SimiasServerSetup.pdb iFolderWebSetup.pdb iFolderAdminSetup.pdb
endif
endif

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasServerSetup.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasServerSetup.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderWebSetup.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderWebSetup.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderAdminSetup.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderAdminSetup.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderProxySetup.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) simias-server-setup.cmd $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-web-setup.cmd $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-admin-setup.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simias-server-setup $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder_mono_setup $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder_cluster_setup $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder_shutdown $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder_start $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-mod-mono-server2 $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-web-setup $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolder_proxy_rights_assign $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolder_retrieve_proxy_creds $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolder_update_proxy_cred_store $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-admin-setup $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) novell-ifolder3.conf $(DESTDIR)$(sysconfdir)

	
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SimiasServerSetup.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f simias-server-setup simias-server-setup.cmd
	cd $(DESTDIR)$(bindir); rm -f ifolder-mod-mono-server2
	cd $(DESTDIR)$(bindir); rm -f ifolder_mono_setup
	cd $(DESTDIR)$(bindir); rm -f ifolder_cluster_setup
	cd $(DESTDIR)$(bindir); rm -f ifolder_shutdown
	cd $(DESTDIR)$(bindir); rm -f ifolder_start
	cd $(DESTDIR)$(bindir); rm -f SimiasServerSetup.exe.config
	cd $(DESTDIR)$(bindir); rm -f iFolderWebSetup.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f ifolder-web-setup ifolder-web-setup.cmd
	cd $(DESTDIR)$(bindir); rm -f iFolderWebSetup.exe.config
	cd $(DESTDIR)$(bindir); rm -f iFolderAdminSetup.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f ifolder-admin-setup ifolder-admin-setup.cmd
	cd $(DESTDIR)$(bindir); rm -f iFolder_proxy_rights_assign iFolder_retrieve_proxy_creds iFolder_update_proxy_cred_store
	cd $(DESTDIR)$(bindir); rm -f iFolderAdminSetup.exe.config
	cd $(DESTDIR)$(bindir); rm -f iFolderProxySetup.exe
	cd $(DESTDIR)$(sysconfdir); rm -f novell-ifolder3.conf

installcheck-local: install
	
CLEAN_FILES = simias-server-setup ifolder_mono_setup ifolder-mod-mono-server2 ifolder-admin-setup ifolder-web-setup SimiasServerSetup.exe iFolderWebSetup.exe iFolderAdminSetup.exe iFolderProxySetup.exe iFolder_proxy_rights_assign iFolder_retrieve_proxy_creds iFolder_update_proxy_cred_store $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

