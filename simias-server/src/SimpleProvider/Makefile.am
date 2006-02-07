
# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimpleSync_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/SimpleSync.cs $(srcdir)/ThreadService.cs $(srcdir)/User.cs     
SimpleSync_CSFILES_CSC := $(subst /,$(SEP),$(SimpleSync_CSFILES))
SimpleSync_INCLUDES =
SimpleSync_RESOURCES =
SimpleSync_FLAGS = $(CSC_LIBFLAG)
SimpleSync_LIBPATH = $(DESTDIR)$(webbindir) ../ ../../../simias/src/SimiasLib.dll ../../../simias/src/SimiasClient
SimpleSync_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll 

EXTRA_DIST = $(SimpleSync_CSFILES) AssemblyInfo.cs.in 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.SimpleSync.dll

Simias.SimpleSync.dll: $(SimpleSync_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(SimpleSync_FLAGS) $(SimpleSync_LIBPATH:%=/lib:%) $(SimpleSync_LIBS:%=/r:%) $(SimpleSync_RESOURCES:%=$(ICON_FLAG)%) $(SimpleSync_CSFILES_CSC) $(DOC_FILE_CMD)

#AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
#	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
#	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.SimpleSync.pdb
endif
endif

install-exec-local: Simias.SimpleSync.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir) $(simiasdatadir) $(simiasdatadir)/modules
	$(INSTALL_PROGRAM) Simias.SimpleSync.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.SimpleSync.conf $(simiasdatadir)/modules
	$(INSTALL_PROGRAM) SimpleServer.xml $(DESTDIR)$(sysconfdir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.SimpleSync.dll $(DEBUG_FILES)
	cd $(simiasdatadir)/modules; rm -f Simias.SimpleSync.conf
	cd $(DESTDIR)$(sysconfdir); rm -f SimpleServer.xml
	
	
installcheck-local: install
	
#CLEAN_FILES = Simias.SimpleSync.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs
CLEAN_FILES = Simias.SimpleSync.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

