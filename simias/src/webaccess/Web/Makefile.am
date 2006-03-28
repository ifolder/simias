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

iFolderWeb_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll \
	log4net.dll

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

EXTRA_DIST = AssemblyInfo.cs.in $(iFolderWeb_CSFILES) $(iFolderWeb_FORMS) $(iFolderWeb_IMAGES) help $(iFolderWeb_RESX)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
dist-hook:
	rm -rf `find $(distdir)/help -name CVS`		

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolderWeb.pdb
endif
endif

CLEAN_FILES = Novell.iFolderWeb.dll \
	Novell.iFolderWeb.doc.xml \
	$(iFolderWeb_RESOURCES) \
	AssemblyInfo.cs \
	$(srcdir)/iFolderWebProxy.cs \
	$(DEBUG_FILES)

all: Novell.iFolderWeb.dll

iFolderWebProxy.cs: ../../iFolder.WebService/iFolderWeb.wsdl
	wsdl /nologo /out:$@ $<

Novell.iFolderWeb.dll: $(iFolderWeb_CSFILES) $(iFolderWeb_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWeb_FLAGS) $(iFolderWeb_LIBS:%=/r:%) $(iFolderWeb_RESOURCES:%=/res:%) $(iFolderWeb_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWeb_CSFILES_CSC) /doc:iFolderWeb.doc.xml

Novell.iFolderApp.Web.%.resources : %.resx
	$(RESGEN) $< $@

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(webimgdir)
	$(mkinstalldirs) $(DESTDIR)$(webhelpdir)
	$(INSTALL_PROGRAM) $(iFolderWeb_FORMS) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Novell.iFolderWeb.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(iFolderWeb_IMAGES) $(DESTDIR)$(webimgdir)
	cd $(srcdir)/help; cp -r * $(DESTDIR)$(webhelpdir);
	rm -rf `find $(DESTDIR)$(webhelpdir) -name CVS`
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f $(iFolderWeb_FORMS)
	cd $(DESTDIR)$(webbindir); rm -f Novell.iFolderWeb.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f log4net.dll
	cd $(DESTDIR)$(webdir); rm -f $(iFolderWeb_IMAGES)
	cd $(DESTDIR)$(webdir); rm -rf $(DESTDIR)$(webhelpdir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

