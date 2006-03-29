# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWeb_CSFILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Global.asax.cs \
	$(srcdir)/Default.aspx.cs \
	$(srcdir)/Error.aspx.cs \
	$(srcdir)/Header.ascx.cs \
	$(srcdir)/Message.ascx.cs \
	$(srcdir)/Pagging.ascx.cs \
	$(srcdir)/iFolder.aspx.cs \
	$(srcdir)/iFolders.aspx.cs \
	$(srcdir)/Entries.aspx.cs \
	$(srcdir)/Share.aspx.cs \
	$(srcdir)/Login.aspx.cs \
	$(srcdir)/Download.ashx.cs \
	$(srcdir)/UploadModule.cs \
	$(srcdir)/UploadStream.cs \
	$(srcdir)/WebLogger.cs \
	$(srcdir)/WebUtility.cs \
	$(srcdir)/iFolderWebProxy.cs
	
iFolderWeb_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWeb_CSFILES))

iFolderWeb_INCLUDES =

iFolderWeb_RESX = \
	$(srcdir)/iFolderWeb.resx \
	$(shell ls $(srcdir)/iFolderWeb.*.resx)

iFolderWeb_RESOURCES := $(iFolderWeb_RESX:$(srcdir)/%.resx=$(srcdir)/Novell.iFolderApp.Web.%.resources)
	
iFolderWeb_FLAGS = $(CSC_LIBFLAG)

if LINUX
iFolderWeb_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll
else
iFolderWeb_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll \
	log4net.dll
endif

iFolderWeb_LIBPATH = $(LOG4NET_DIR)

iFolderWeb_FORMS = \
	Web.config \
	Log4Net.config \
	iFolderWeb.css \
	Global.asax \
	Default.aspx \
	Error.aspx \
	Header.ascx \
	Message.ascx \
	Pagging.ascx \
	iFolder.aspx \
	iFolders.aspx \
	Entries.aspx \
	Login.aspx \
	Download.ashx

iFolderWeb_IMAGES = images/*.png images/*.ico

EXTRA_DIST = AssemblyInfo.cs.in $(iFolderWeb_CSFILES) $(iFolderWeb_FORMS) $(iFolderWeb_IMAGES) elp $(iFolderWeb_RESX)

dist-hook:
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
	$(mkinstalldirs) $(DESTDIR)$(webaccessdir)
	$(mkinstalldirs) $(DESTDIR)$(webaccessbindir)
	$(mkinstalldirs) $(DESTDIR)$(webaccessimgdir)
	$(mkinstalldirs) $(DESTDIR)$(webaccesshelpdir)
	$(INSTALL_PROGRAM) $(iFolderWeb_FORMS) $(DESTDIR)$(webaccessdir)
	$(INSTALL_PROGRAM) Novell.iFolderWeb.dll $(DEBUG_FILES) $(DESTDIR)$(webaccessbindir)
if !LINUX
	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webaccessbindir)
endif
	$(INSTALL_PROGRAM) $(iFolderWeb_IMAGES) $(DESTDIR)$(webaccessimgdir)
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(webaccesshelpdir);
	rm -rf `find $(DESTDIR)$(webaccesshelpdir) -name .svn`
	
uninstall-local:
	cd $(DESTDIR)$(webaccessdir); rm -f $(iFolderWeb_FORMS)
	cd $(DESTDIR)$(webaccessbindir); rm -f Novell.iFolderWeb.dll $(DEBUG_FILES)
if !LINUX
	cd $(DESTDIR)$(webaccessbindir); rm -f log4net.dll
endif
	cd $(DESTDIR)$(webaccessimgdir); rm -f $(iFolderWeb_IMAGES)
	cd $(DESTDIR)$(webaccessdir); rm -rf $(DESTDIR)$(webaccesshelpdir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

