# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWeb_CSFILES = \
	$(srcdir)/Default.aspx.cs \
	$(srcdir)/Download.ashx.cs \
	$(srcdir)/Browse.aspx.cs \
	$(srcdir)/Search.aspx.cs \
	$(srcdir)/NewFolder.aspx.cs \
	$(srcdir)/Upload.aspx.cs \
	$(srcdir)/ItemHistory.aspx.cs \
	$(srcdir)/Error.aspx.cs \
	$(srcdir)/Quota.ascx.cs \
	$(srcdir)/Global.asax.cs \
	$(srcdir)/Header.ascx.cs \
	$(srcdir)/Details.aspx.cs \
	$(srcdir)/History.aspx.cs \
	$(srcdir)/Members.aspx.cs \
	$(srcdir)/iFolders.aspx.cs \
	$(srcdir)/iFolderWebProxy.cs \
	$(srcdir)/Login.aspx.cs \
	$(srcdir)/ICLogout.aspx.cs \
	$(srcdir)/Message.ascx.cs \
	$(srcdir)/Pagging.ascx.cs \
	$(srcdir)/HomeContext.ascx.cs \
	$(srcdir)/iFolderActions.ascx.cs \
	$(srcdir)/iFolderContext.ascx.cs \
	$(srcdir)/TabControl.ascx.cs \
	$(srcdir)/Share.aspx.cs \
	$(srcdir)/iFolderNew.aspx.cs \
	$(srcdir)/iFolderEdit.aspx.cs \
	$(srcdir)/Settings.aspx.cs \
	$(srcdir)/UploadModule.cs \
	$(srcdir)/UploadStream.cs \
	$(srcdir)/WebLogger.cs \
	$(srcdir)/iFolderCertificate.aspx.cs \
	$(srcdir)/WebSettings.cs \
	$(srcdir)/WebUtility.cs 
#	$(srcdir)/../core/Encryption/Blowfish.cs

	
iFolderWeb_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWeb_CSFILES))

iFolderWeb_INCLUDES =

iFolderWeb_RESX = \
	$(srcdir)/iFolderWeb.resx

iFolderWeb_RESXES = \
	$(shell ls $(srcdir)/iFolderWeb.*.resx)

iFolderWeb_RESOURCE := $(iFolderWeb_RESX:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderApp.Web.%.resources)
iFolderWeb_RESOURCES := $(iFolderWeb_RESXES:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderApp.Web.%.resources)
iFolderWeb__RESOURCES_DLLS := $(iFolderWeb_RESOURCES:$(srcdir)/Novell.iFolderApp.Web.iFolderWeb.%.resources=$(srcdir)/%/Novell.iFolderWeb.resources.dll)
ASSEMBLYLINKER = al
	
iFolderWeb_FLAGS = $(CSC_LIBFLAG)

iFolderWeb_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll \
	$(srcdir)/../core/SimiasLib.dll/SimiasLib.dll
#if !LINUX
#	iFolderWeb_LIBS := $(iFolderWeb_LIBS) log4net.dll
#endif

iFolderWeb_LIBPATH = $(LOG4NET_DIR)

iFolderWeb_FORMS = \
	Global.asax \
	Default.aspx \
	Browse.aspx \
	Search.aspx \
	NewFolder.aspx \
	Upload.aspx \
	ItemHistory.aspx \
	Error.aspx \
	Details.aspx \
	History.aspx \
	Members.aspx \
	iFolders.aspx \
	Login.aspx \
	ICLogout.aspx \
	iFolderNew.aspx \
	iFolderEdit.aspx \
	Share.aspx \
	Settings.aspx \
	iFolderCertificate.aspx \
	Quota.ascx \
	Header.ascx \
	Message.ascx \
	Pagging.ascx \
	HomeContext.ascx \
	iFolderContext.ascx \
	iFolderActions.ascx \
	TabControl.ascx \
	Download.ashx \
	Log4Net.config \
	Web.config

EXTRA_DIST = AssemblyInfo.cs.in Log4Net.config.in $(iFolderWeb_CSFILES) $(iFolderWeb_FORMS) images css js help $(iFolderWeb_RESX)

dist-hook:
	rm -rf `find $(distdir)/images -name .svn`		
	rm -rf `find $(distdir)/css -name .svn`		
	rm -rf `find $(distdir)/js -name .svn`		
	rm -rf `find $(distdir)/help -name .svn`		

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolderWeb.pdb
endif
endif

CLEAN_FILES = Novell.iFolderWeb.dll \
	Novell.iFolderWeb.doc.xml \
	$(iFolderWeb_RESOURCE) \
	$(iFolderWeb_RESOURCES) \
	$(srcdir)/iFolderWebProxy.cs \
	$(DEBUG_FILES)	\
	AssemblyInfo.cs

if MONO24BUILD
all: Novell.iFolderWeb.dll $(iFolderWeb__RESOURCES_DLLS)
else
all: Novell.iFolderWeb.dll
endif

iFolderWebProxy.cs: ../webservices/iFolderWeb.wsdl
	$(WSDL) /nologo /out:$@ $<

if MONO24BUILD
Novell.iFolderWeb.dll: $(srcdir)/AssemblyInfo.cs $(iFolderWeb_CSFILES) $(iFolderWeb_RESOURCE)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWeb_FLAGS) $(iFolderWeb_LIBS:%=/r:%) $(iFolderWeb_RESOURCE:%=/res:%) $(iFolderWeb_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWeb_CSFILES_CSC) /doc:iFolderWeb.doc.xml
else
Novell.iFolderWeb.dll: $(srcdir)/AssemblyInfo.cs $(iFolderWeb_CSFILES) $(iFolderWeb_RESOURCE) $(iFolderWeb_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWeb_FLAGS) $(iFolderWeb_LIBS:%=/r:%) $(iFolderWeb_RESOURCE:%=/res:%) $(iFolderWeb_RESOURCES:%=/res:%) $(iFolderWeb_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWeb_CSFILES_CSC) /doc:iFolderWeb.doc.xml
endif

Novell.iFolderApp.Web.iFolderWeb.resources : iFolderWeb.resx
	$(RESGEN) $< $@

Novell.iFolderApp.Web.%.resources : %.resx
	$(RESGEN) $< $@

%/Novell.iFolderWeb.resources.dll: Novell.iFolderApp.Web.iFolderWeb.%.resources
	ResLang="`dirname $@`"; \
	$(mkinstalldirs) $$ResLang; \
	$(ASSEMBLYLINKER) -c:$$ResLang  -embed:$< -t:lib -out:$@ -keyf:../../iFolder.snk -template:Novell.iFolderWeb.dll

install-exec-local: all
	rm -rf $(DESTDIR)$(webaccessdir)
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/bin
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/images
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/help
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/css
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/js
if MONO24BUILD
	ResLangDirs="$(shell ls $(srcdir)/*/Novell.iFolderWeb.resources.dll)";\
	for subdir in $$ResLangDirs; do \
	LDName="`dirname $$subdir`";\
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/bin/$$LDName/;\
	$(INSTALL_PROGRAM) $$LDName/Novell.iFolderWeb.resources.dll $(DESTDIR)$(webaccessdir)/bin/$$LDName/;\
	done;
endif
	$(INSTALL_PROGRAM) $(iFolderWeb_FORMS) $(DESTDIR)$(webaccessdir)
	$(INSTALL_PROGRAM) Novell.iFolderWeb.dll $(DEBUG_FILES) $(DESTDIR)$(webaccessdir)/bin
	$(INSTALL_PROGRAM) $(srcdir)/css/*.css $(DESTDIR)$(webaccessdir)/css
	$(INSTALL_PROGRAM) $(srcdir)/js/*.js $(DESTDIR)$(webaccessdir)/js
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(webaccessdir)/help
	rm -rf `find $(DESTDIR)$(webaccessdir)/help -name .svn`
	cd $(srcdir)/images; cp -r * $(DESTDIR)$(webaccessdir)/images
	rm -rf `find $(DESTDIR)$(webaccessdir)/images -name .svn`
	$(INSTALL_PROGRAM) $(DESTDIR)$(webbindir)/SimiasLib.dll $(DESTDIR)$(webaccessdir)/bin
#if !LINUX
#	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webaccessdir)/bin
#endif
	
uninstall-local:
	rm -rf $(DESTDIR)$(webaccessdir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
if MONO24BUILD
	ResLangDirs="$(shell ls $(srcdir)/*/Novell.iFolderWeb.resources.dll)";\
	for subdir in $$ResLangDirs; do \
	LDName="`dirname $$subdir`";\
	rm -rf $$LDName;\
	done;
endif
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf Makefile
	rm -rf AssemblyInfo.cs

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Log4Net.config

