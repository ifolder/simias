# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWeb_CSFILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Default.aspx.cs \
	$(srcdir)/Download.ashx.cs \
	$(srcdir)/Entries.aspx.cs \
	$(srcdir)/Entry.aspx.cs \
	$(srcdir)/Error.aspx.cs \
	$(srcdir)/Global.asax.cs \
	$(srcdir)/Header.ascx.cs \
	$(srcdir)/iFolder.aspx.cs \
	$(srcdir)/iFolders.aspx.cs \
	$(srcdir)/iFolderWebProxy.cs \
	$(srcdir)/Login.aspx.cs \
	$(srcdir)/Message.ascx.cs \
	$(srcdir)/Pagging.ascx.cs \
	$(srcdir)/Share.aspx.cs \
	$(srcdir)/UploadModule.cs \
	$(srcdir)/UploadStream.cs \
	$(srcdir)/WebLogger.cs \
	$(srcdir)/WebUtility.cs
	
iFolderWeb_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWeb_CSFILES))

iFolderWeb_INCLUDES =

iFolderWeb_RESX = \
	$(srcdir)/iFolderWeb.resx \
	$(shell ls $(srcdir)/iFolderWeb.*.resx)

iFolderWeb_RESOURCES := $(iFolderWeb_RESX:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderApp.Web.%.resources)
	
iFolderWeb_FLAGS = $(CSC_LIBFLAG)

iFolderWeb_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll

if !LINUX
	iFolderWeb_LIBS := $(iFolderWeb_LIBS) log4net.dll
endif

iFolderWeb_LIBPATH = $(LOG4NET_DIR)

iFolderWeb_FORMS = \
	Global.asax \
    Default.aspx \
	Entries.aspx \
	Entry.aspx \
	Error.aspx \
	iFolder.aspx \
	iFolders.aspx \
	Login.aspx \
	Share.aspx \
	Header.ascx \
	Message.ascx \
	Pagging.ascx \
	Download.ashx \
	Log4Net.config \
	Web.config

EXTRA_DIST = AssemblyInfo.cs.in $(iFolderWeb_CSFILES) $(iFolderWeb_FORMS) images css js help $(iFolderWeb_RESX)

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
	$(iFolderWeb_RESOURCES) \
	$(srcdir)/iFolderWebProxy.cs \
	$(DEBUG_FILES)

all: Novell.iFolderWeb.dll

iFolderWebProxy.cs: ../webservices/iFolderWeb.wsdl
	wsdl /nologo /out:$@ $<

Novell.iFolderWeb.dll: $(iFolderWeb_CSFILES) $(iFolderWeb_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWeb_FLAGS) $(iFolderWeb_LIBS:%=/r:%) $(iFolderWeb_RESOURCES:%=/res:%) $(iFolderWeb_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWeb_CSFILES_CSC) /doc:iFolderWeb.doc.xml

Novell.iFolderApp.Web.%.resources : %.resx
	$(RESGEN) $< $@

install-exec-local: all
	rm -rf $(DESTDIR)$(webaccessdir)
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/bin
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/images
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/help
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/css
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)/js
	$(INSTALL_PROGRAM) $(iFolderWeb_FORMS) $(DESTDIR)$(webaccessdir)
	$(INSTALL_PROGRAM) Novell.iFolderWeb.dll $(DEBUG_FILES) $(DESTDIR)$(webaccessdir)/bin
	$(INSTALL_PROGRAM) $(srcdir)/css/*.css $(DESTDIR)$(webaccessdir)/css
	$(INSTALL_PROGRAM) $(srcdir)/js/*.js $(DESTDIR)$(webaccessdir)/js
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(webaccessdir)/help
	rm -rf `find $(DESTDIR)$(webaccessdir)/help -name .svn`
	cd $(srcdir)/images; cp -r * $(DESTDIR)$(webaccessdir)/images
	rm -rf `find $(DESTDIR)$(webaccessdir)/images -name .svn`
if !LINUX
	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webaccessdir)/bin
endif
	
uninstall-local:
	rm -rf $(DESTDIR)$(webaccessdir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

