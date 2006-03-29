# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderAdmin_CSFILES = \
	$(srcdir)/AccountEnabled.ascx.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Default.aspx.cs \
	$(srcdir)/DiskSpaceQuota.ascx.cs \
	$(srcdir)/Error.aspx.cs \
	$(srcdir)/FileSizeFilter.ascx.cs \
	$(srcdir)/FileTypeFilter.ascx.cs \
	$(srcdir)/Global.asax.cs \
	$(srcdir)/Header.ascx.cs \
	$(srcdir)/iFolderAdmin.aspx.cs \
	$(srcdir)/iFolderAdminWebProxy.cs \
	$(srcdir)/iFolderDetails.aspx.cs \
	$(srcdir)/iFolderEnabled.ascx.cs \
	$(srcdir)/iFolders.aspx.cs \
	$(srcdir)/iFolderWebLogger.cs \
	$(srcdir)/Login.aspx.cs \
	$(srcdir)/MemberSearch.ascx.cs \
	$(srcdir)/MemberSelect.aspx.cs \
	$(srcdir)/PageFooter.ascx.cs \
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
	System.Web.Services.dll \
	log4net.dll

iFolderAdmin_LIBPATH = $(LOG4NET_DIR)

iFolderAdmin_FORMS = \
	AccountEnabled.ascx \
	Default.aspx \
	DiskSpaceQuota.ascx \
	Error.aspx \
	FileSizeFilter.ascx \
	FileTypeFilter.ascx \
	Global.asax \
	Header.ascx \
	iFolderAdmin.aspx \
	iFolderAdmin.css \
	iFolderDetails.aspx \
	iFolderDetails.css \
	iFolderEnabled.ascx \
	iFolders.aspx \
	iFolders.css \
	Log4Net.config \
	Login.aspx \
	Login.css \
	MemberSearch.ascx \
	MemberSelect.aspx \
	MemberSelect.css \
	PageFooter.ascx \
	Policy.ascx \
	Servers.aspx \
	Servers.css \
	SyncInterval.ascx \
	SystemInfo.aspx \
	SystemInfo.css \
	TopNavigation.ascx \
	UserDetails.aspx \
	UserDetails.css \
	Users.aspx \
	Users.css \
	Web.config

iFolderAdmin_IMAGES = images/*.gif images/*.ico

EXTRA_DIST = AssemblyInfo.cs.in $(iFolderAdmin_CSFILES) $(iFolderAdmin_FORMS) $(iFolderAdmin_IMAGES) help $(iFolderAdmin_RESX)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
dist-hook:
	rm -rf `find $(distdir)/help -name CVS`		

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolderAdmin.pdb
endif
endif

CLEAN_FILES = Novell.iFolderAdmin.dll \
	Novell.iFolderAdmin.doc.xml \
	$(iFolderAdmin_RESOURCES) \
	AssemblyInfo.cs \
	$(DEBUG_FILES)

all: Novell.iFolderAdmin.dll

iFolderAdminWebProxy.cs: ../iFolder.WebService/iFolderAdmin.wsdl
	wsdl /nologo /out:$@ $<

Novell.iFolderAdmin.dll: iFolderAdminWebProxy.cs $(iFolderAdmin_CSFILES) $(iFolderAdmin_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderAdmin_FLAGS) $(iFolderAdmin_LIBS:%=/r:%) $(iFolderAdmin_RESOURCES:%=/res:%) $(iFolderAdmin_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderAdmin_CSFILES_CSC) /doc:iFolderAdmin.doc.xml

Novell.iFolderWeb.Admin.%.resources : %.resx
	$(RESGEN) $< $@

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(webimgdir)
	$(mkinstalldirs) $(DESTDIR)$(webhelpdir)
	$(INSTALL_PROGRAM) $(iFolderAdmin_FORMS) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Novell.iFolderAdmin.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(iFolderAdmin_IMAGES) $(DESTDIR)$(webimgdir)
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(webhelpdir);
	rm -rf `find $(DESTDIR)$(webhelpdir) -name CVS`
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f $(iFolderAdmin_FORMS)
	cd $(DESTDIR)$(webbindir); rm -f Novell.iFolderAdmin.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f log4net.dll
	cd $(DESTDIR)$(webdir); rm -f $(iFolderAdmin_IMAGES)
	cd $(DESTDIR)$(webdir); rm -rf $(DESTDIR)$(webhelpdir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

