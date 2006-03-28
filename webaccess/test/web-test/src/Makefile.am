# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWeb_CSFILES = \
	$(srcdir)/Download.ashx.cs \
	$(srcdir)/iFolderJson.ashx.cs \
	$(srcdir)/iFolderWebProxy.cs
	
iFolderWeb_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWeb_CSFILES))

iFolderWeb_INCLUDES =
iFolderWeb_FLAGS = $(CSC_LIBFLAG)

iFolderWeb_LIBS = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Drawing.dll \
	System.Web.dll \
	System.Web.Services.dll \
	Jayrock.dll
	log4net.dll

iFolderWeb_LIBPATH = ../external/jayrock $(LOG4NET_DIR)

iFolderWeb_HTML := \
	$(wildcard *.html) \
	$(wildcard *.ashx) \
	$(wildcard *.js) \
	$(wildcard *.css) \
	Web.config

EXTRA_DIST = $(iFolderWeb_CSFILES) $(iFolderWeb_HTML) images help

dist-hook:
	rm -rf `find $(distdir)/help -name CVS`		

if DEBUG
if WINDOWS
DEBUG_FILES = iFolderWeb.pdb
endif
endif

CLEAN_FILES = iFolderWeb.dll \
	iFolderWeb.doc.xml \
	AssemblyInfo.cs \
	$(DEBUG_FILES) \
	iFolderWebProxy.cs

all: iFolderWeb.dll

iFolderWebProxy.cs: $(EXTERNAL_DIR)/ifolder/iFolderWeb.wsdl
	wsdl /nologo /n:Simias.Ajax.Simias10 /out:$@ $<

iFolderWeb.dll: $(iFolderWeb_CSFILES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWeb_FLAGS) $(iFolderWeb_LIBS:%=/r:%) $(iFolderWeb_LIBPATH:%=/lib:%) $(iFolderWeb_CSFILES_CSC) /doc:iFolderWeb.doc.xml

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(webimgdir)
	$(mkinstalldirs) $(DESTDIR)$(webhelpdir)
	$(INSTALL_PROGRAM) $(iFolderWeb_HTML) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) iFolderWeb.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) ../external/jayrock/Jayrock.* $(DESTDIR)$(webbindir)
	cd $(srcdir)/images; cp -r * $(DESTDIR)$(webimgdir);
	rm -rf `find $(DESTDIR)$(webimgdir) -name CVS`
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f $(iFolderWeb_HTML)
	cd $(DESTDIR)$(webbindir); rm -f iFolderWeb.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Jayrock.*
	cd $(DESTDIR)$(webdir); rm -rf $(DESTDIR)$(webimgdir)
	cd $(DESTDIR)$(webdir); rm -rf $(DESTDIR)$(webhelpdir)

installcheck-local: install
	
clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

