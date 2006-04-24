# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderAdmin_CSFILES = \
	$(srcdir)/AccountEnabled.ascx.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/CreateUser.aspx.cs \
	$(srcdir)/Default.aspx.cs \
	$(srcdir)/DiskSpaceQuota.ascx.cs \
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
	$(srcdir)/iFolders.aspx.cs \
	$(srcdir)/iFolderSearch.ascx.cs \
	$(srcdir)/iFolderWebLogger.cs \
	$(srcdir)/ListFooter.ascx.cs \
	$(srcdir)/Login.aspx.cs \
	$(srcdir)/MemberSearch.ascx.cs \
	$(srcdir)/MemberSelect.aspx.cs \
	$(srcdir)/Policy.ascx.cs \
	$(srcdir)/Servers.aspx.cs \
	$(srcdir)/SyncInterval.ascx.cs \
	$(srcdir)/SystemInfo.aspx.cs \
	$(srcdir)/TopNavigation.ascx.cs \
	$(srcdir)/UserDetails.aspx.cs \
	$(srcdir)/Users.aspx.cs \
	$(srcdir)/Utils.cs
	
iFolderAdmin_CSFILES_CSC := $(subst /,$(SEP),$(iFolderAdmin_CSFILES))

iFolderAdmin_INCLUDES =

iFolderAdmin_RESX = \
	$(srcdir)/iFolderAdmin.resx
#	$(shell ls $(srcdir)/iFolderAdmin.*.resx)

iFolderAdmin_RESOURCES := $(iFolderAdmin_RESX:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderWeb.Admin.%.resources)
	
iFolderAdmin_FLAGS = $(CSC_LIBFLAG)

iFolderAdmin_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll

if !LINUX
	iFolderAdmin_LIBS := $(iFolderAdmin_LIBS) log4net.dll
endif

iFolderAdmin_LIBPATH = $(LOG4NET_DIR)


iFolderAdmin_FORMS = \
	AccountEnabled.ascx \
	Default.aspx \
	DiskSpaceQuota.ascx \
	CreateUser.aspx \
	Error.aspx \
	FileSizeFilter.ascx \
	FileTypeFilter.ascx \
	Footer.ascx \
	Global.asax \
	Header.ascx \
	iFolderAdminPage.aspx \
	iFolderDetailsPage.aspx \
	iFolderEnabled.ascx \
	iFolders.aspx \
	iFolderSearch.ascx \
	ListFooter.ascx \
	Log4Net.config \
	Login.aspx \
	MemberSearch.ascx \
	MemberSelect.aspx \
	Policy.ascx \
	Servers.aspx \
	SyncInterval.ascx \
	SystemInfo.aspx \
	TopNavigation.ascx \
	UserDetails.aspx \
	Users.aspx \
	Web.config

EXTRA_DIST = AssemblyInfo.cs.in $(iFolderAdmin_CSFILES) $(iFolderAdmin_FORMS) css images help $(iFolderAdmin_RESX)

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
	Novell.iFolderAdmin.doc.xml \
	$(iFolderAdmin_RESOURCES) \
	$(DEBUG_FILES)

all: Novell.iFolderAdmin.dll

iFolderAdminWebProxy.cs: ../webservices/iFolderAdmin.wsdl
	wsdl /nologo /out:$@ $<

Novell.iFolderAdmin.dll: iFolderAdminWebProxy.cs $(iFolderAdmin_CSFILES) $(iFolderAdmin_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderAdmin_FLAGS) $(iFolderAdmin_LIBS:%=/r:%) $(iFolderAdmin_RESOURCES:%=/res:%) $(iFolderAdmin_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderAdmin_CSFILES_CSC) /doc:iFolderAdmin.doc.xml

Novell.iFolderWeb.Admin.%.resources : %.resx
	$(RESGEN) $< $@

install-exec-local: all
	rm -rf $(DESTDIR)$(admindir)
	$(mkinstalldirs) $(DESTDIR)$(admindir)
	$(mkinstalldirs) $(DESTDIR)$(admindir)/bin
	$(mkinstalldirs) $(DESTDIR)$(admindir)/images
	$(mkinstalldirs) $(DESTDIR)$(admindir)/css
	$(mkinstalldirs) $(DESTDIR)$(admindir)/help
	$(INSTALL_PROGRAM) $(iFolderAdmin_FORMS) $(DESTDIR)$(admindir)
	$(INSTALL_PROGRAM) Novell.iFolderAdmin.dll $(DEBUG_FILES) $(DESTDIR)$(admindir)/bin
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(admindir)/help
	rm -rf `find $(DESTDIR)$(admindir)/help -name .svn`
	cd $(srcdir)/images; cp -r * $(DESTDIR)$(admindir)/images
	rm -rf `find $(DESTDIR)$(admindir)/images -name .svn`
	cd $(srcdir)/css; cp -r * $(DESTDIR)$(admindir)/css
	rm -rf `find $(DESTDIR)$(admindir)/css -name .svn`
if !LINUX
	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(admindir)/bin
endif

uninstall-local:
	rm -rf $(DESTDIR)$(admindir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

