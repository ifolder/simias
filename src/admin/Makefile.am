# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
iFolderAdmin_CSFILES = \
	$(srcdir)/AccountEnabled.ascx.cs \
	$(srcdir)/AddDataPath.aspx.cs \
	$(srcdir)/AdminRights.aspx.cs \
	$(srcdir)/CreateiFolder.aspx.cs \
	$(srcdir)/CreateUser.aspx.cs \
	$(srcdir)/Default.aspx.cs \
	$(srcdir)/Sharing.ascx.cs \
	$(srcdir)/DiskSpaceQuota.ascx.cs \
	$(srcdir)/SecurityState.ascx.cs \
	$(srcdir)/Error.aspx.cs \
	$(srcdir)/FileSizeFilter.ascx.cs \
	$(srcdir)/FileTypeFilter.ascx.cs \
	$(srcdir)/Footer.ascx.cs \
	$(srcdir)/Global.asax.cs \
	$(srcdir)/Header.ascx.cs \
	$(srcdir)/iFolderAdminPage.aspx.cs \
	$(srcdir)/iFolderAdminWebProxy.cs \
	$(srcdir)/iFolderDetailsPage.aspx.cs \
	$(srcdir)/iFolderEnabled.ascx.cs \
	$(srcdir)/iFolderLimit.ascx.cs \
	$(srcdir)/iFolders.aspx.cs \
	$(srcdir)/iFolderSearch.ascx.cs \
	$(srcdir)/iFolderWebLogger.cs \
	$(srcdir)/ListFooter.ascx.cs \
	$(srcdir)/Login.aspx.cs \
	$(srcdir)/ICLogout.aspx.cs \
	$(srcdir)/LdapAdminAuth.aspx.cs \
	$(srcdir)/LogTailHandler.ashx.cs \
	$(srcdir)/MemberSearch.ascx.cs \
	$(srcdir)/MemberSelect.aspx.cs \
	$(srcdir)/OwnerSelect.aspx.cs \
	$(srcdir)/Policy.ascx.cs \
	$(srcdir)/ProvisionUsers.aspx.cs \
	$(srcdir)/ReportLogHandler.ashx.cs \
	$(srcdir)/Reports.aspx.cs \
	$(srcdir)/ServerDetails.aspx.cs \
	$(srcdir)/Servers.aspx.cs \
	$(srcdir)/StatusPage.aspx.cs \
	$(srcdir)/SyncInterval.ascx.cs \
	$(srcdir)/SystemInfo.aspx.cs \
	$(srcdir)/TopNavigation.ascx.cs \
	$(srcdir)/UserDetails.aspx.cs \
	$(srcdir)/Users.aspx.cs \
	$(srcdir)/userMove.aspx.cs \
	$(srcdir)/userMoveDetails.aspx.cs \
	$(srcdir)/Utils.cs \
	$(srcdir)/PreferencesEnum.cs
	
iFolderAdmin_CSFILES_CSC := $(subst /,$(SEP),$(iFolderAdmin_CSFILES))

iFolderAdmin_INCLUDES =

iFolderAdmin_RESX = \
	$(srcdir)/iFolderAdmin.resx

iFolderAdmin_RESXES = \
	$(shell ls $(srcdir)/iFolderAdmin.*.resx)

iFolderAdmin_RESOURCE := $(iFolderAdmin_RESX:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderWeb.Admin.%.resources)
iFolderAdmin_RESOURCES := $(iFolderAdmin_RESXES:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderWeb.Admin.%.resources)
iFolderAdmin_RESOURCES_DLLS := $(iFolderAdmin_RESOURCES:$(srcdir)/Novell.iFolderWeb.Admin.iFolderAdmin.%.resources=$(srcdir)/%/Novell.iFolderAdmin.resources.dll)
ASSEMBLYLINKER = al
	
iFolderAdmin_FLAGS = $(CSC_LIBFLAG)

iFolderAdmin_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll \
	$(srcdir)/../core/SimiasLib.dll/SimiasLib.dll \
	$(srcdir)/../core/SimiasClient/SimiasClient.dll

#if !LINUX
#	iFolderAdmin_LIBS := $(iFolderAdmin_LIBS) log4net.dll
#endif

#iFolderAdmin_LIBPATH = $(LOG4NET_DIR)


iFolderAdmin_FORMS = \
	AccountEnabled.ascx \
	AddDataPath.aspx \
	AdminRights.aspx \
	CreateiFolder.aspx \
	CreateUser.aspx \
	Default.aspx \
	Sharing.ascx \
	DiskSpaceQuota.ascx \
	SecurityState.ascx \
	Error.aspx \
	FileSizeFilter.ascx \
	FileTypeFilter.ascx \
	Footer.ascx \
	Global.asax \
	Header.ascx \
	iFolderAdminPage.aspx \
	iFolderDetailsPage.aspx \
	iFolderEnabled.ascx \
	iFolderLimit.ascx \
	iFolders.aspx \
	iFolderSearch.ascx \
	ListFooter.ascx \
	Log4Net.config \
	Login.aspx \
	ICLogout.aspx \
	LdapAdminAuth.aspx \
	LogTailHandler.ashx \
	MemberSearch.ascx \
	MemberSelect.aspx \
	OwnerSelect.aspx \
	Policy.ascx \
	ProvisionUsers.aspx \
	ReportLogHandler.ashx \
	Reports.aspx \
	ServerDetails.aspx \
	Servers.aspx \
	StatusPage.aspx \
	SyncInterval.ascx \
	SystemInfo.aspx \
	TopNavigation.ascx \
	UserDetails.aspx \
	userMoveDetails.aspx \
	userMove.aspx \
	Users.aspx \
	Web.config

EXTRA_DIST = AssemblyInfo.cs.in Log4Net.config.in $(iFolderAdmin_CSFILES) $(iFolderAdmin_FORMS) css images help $(iFolderAdmin_RESX) tail.txt

dist-hook:
	rm -rf `find $(distdir)/help -name .svn`		
	rm -rf `find $(distdir)/images -name .svn`		
	rm -rf `find $(distdir)/css -name .svn`		

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolderAdmin.pdb
endif
endif

CLEAN_FILES = Novell.iFolderAdmin.dll \
	$(iFolderAdmin_RESOURCES) \
	$(DEBUG_FILES) \
	AssemblyInfo.cs

if MONO24BUILD
all: Novell.iFolderAdmin.dll $(iFolderAdmin_RESOURCES_DLLS)
else
all: Novell.iFolderAdmin.dll
endif

iFolderAdminWebProxy.cs: ../webservices/iFolderAdmin.wsdl
	$(WSDL) /nologo /out:$@ $<

if MONO24BUILD
Novell.iFolderAdmin.dll: $(srcdir)/AssemblyInfo.cs iFolderAdminWebProxy.cs $(iFolderAdmin_CSFILES) $(iFolderAdmin_RESOURCE)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderAdmin_FLAGS) $(iFolderAdmin_LIBS:%=/r:%) $(iFolderAdmin_RESOURCE:%=/res:%) $(iFolderAdmin_LIBPATH:%=/lib:%) $(srcdir)/AssemblyInfo.cs $(iFolderAdmin_CSFILES_CSC) $(LOG4NET_LIBS)
else
Novell.iFolderAdmin.dll: $(srcdir)/AssemblyInfo.cs iFolderAdminWebProxy.cs $(iFolderAdmin_CSFILES) $(iFolderAdmin_RESOURCE) $(iFolderAdmin_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderAdmin_FLAGS) $(iFolderAdmin_LIBS:%=/r:%) $(iFolderAdmin_RESOURCE:%=/res:%) $(iFolderAdmin_RESOURCES:%=/res:%) $(iFolderAdmin_LIBPATH:%=/lib:%) $(srcdir)/AssemblyInfo.cs $(iFolderAdmin_CSFILES_CSC) $(LOG4NET_LIBS)
endif

Novell.iFolderWeb.Admin.iFolderAdmin.resources : iFolderAdmin.resx
	$(RESGEN) $< $@

Novell.iFolderWeb.Admin.%.resources : %.resx
	$(RESGEN) $< $@

%/Novell.iFolderAdmin.resources.dll: Novell.iFolderWeb.Admin.iFolderAdmin.%.resources Novell.iFolderAdmin.dll
	ResLang="`dirname $@`"; \
	$(mkinstalldirs) $$ResLang; \
	$(ASSEMBLYLINKER) -c:$$ResLang  -embed:$< -t:lib -out:$@ -keyf:../../iFolder.snk -template:Novell.iFolderAdmin.dll

install-exec-local: all
	rm -rf $(DESTDIR)$(admindir)
	$(mkinstalldirs) $(DESTDIR)$(admindir)
	$(mkinstalldirs) $(DESTDIR)$(admindir)/bin
	$(mkinstalldirs) $(DESTDIR)$(admindir)/images
	$(mkinstalldirs) $(DESTDIR)$(admindir)/css
	$(mkinstalldirs) $(DESTDIR)$(admindir)/help
	$(mkinstalldirs) $(DESTDIR)$(admindir)/tail
if MONO24BUILD
	ResLangDirs="$(shell ls $(srcdir)/*/Novell.iFolderAdmin.resources.dll)";\
	for subdir in $$ResLangDirs; do \
	LDName="`dirname $$subdir`";\
	$(mkinstalldirs) $(DESTDIR)$(admindir)/bin/$$LDName/;\
	$(INSTALL_PROGRAM) $$LDName/Novell.iFolderAdmin.resources.dll $(DESTDIR)$(admindir)/bin/$$LDName/;\
	done;
endif
	$(INSTALL_PROGRAM) $(iFolderAdmin_FORMS) $(DESTDIR)$(admindir)
	$(INSTALL_PROGRAM) Novell.iFolderAdmin.dll $(DEBUG_FILES) $(DESTDIR)$(admindir)/bin
	$(INSTALL_PROGRAM) tail.txt $(DESTDIR)$(admindir)/tail
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(admindir)/help  
	rm -rf `find $(DESTDIR)$(admindir)/help -name .svn`
	cd $(srcdir)/images; cp -r * $(DESTDIR)$(admindir)/images
	rm -rf `find $(DESTDIR)$(admindir)/images -name .svn`
	cd $(srcdir)/css; cp -r * $(DESTDIR)$(admindir)/css
	rm -rf `find $(DESTDIR)$(admindir)/css -name .svn`
	$(INSTALL_PROGRAM) $(DESTDIR)$(webbindir)/SimiasLib.dll $(DESTDIR)$(admindir)/bin/
	$(INSTALL_PROGRAM) $(DESTDIR)$(webbindir)/SimiasClient.dll $(DESTDIR)$(admindir)/bin/
#if !LINUX
#	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(admindir)/bin
#endif

uninstall-local:
	rm -rf $(DESTDIR)$(admindir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
if MONO24BUILD
	ResLangDirs="$(shell ls $(srcdir)/*/Novell.iFolderAdmin.resources.dll)";\
	for subdir in $$ResLangDirs; do \
	LDName="`dirname $$subdir`";\
	rm -rf $$LDName;\
	done;
endif

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Log4Net.config

