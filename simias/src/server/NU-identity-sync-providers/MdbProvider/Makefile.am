
# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

MdbProvider_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/DomainConfig.cs $(srcdir)/EnumUsers.cs $(srcdir)/Mdb.cs $(srcdir)/MdbUser.cs $(srcdir)/MdbSync.cs $(srcdir)/ThreadService.cs $(srcdir)/User.cs    
MdbProvider_CSFILES_CSC := $(subst /,$(SEP),$(MdbProvider_CSFILES))
MdbProvider_INCLUDES =
MdbProvider_RESOURCES =
MdbProvider_FLAGS = $(CSC_LIBFLAG)
MdbProvider_LIBPATH = $(DESTDIR)$(webbindir) ../../../simias/src/SimiasLib.dll ../../../simias/src/SimiasClient ../
MdbProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll 

EXTRA_DIST = $(MdbProvider_CSFILES) AssemblyInfo.cs.in 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.MdbProvider.dll

Simias.MdbProvider.dll: $(MdbProvider_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(MdbProvider_FLAGS) $(MdbProvider_LIBPATH:%=/lib:%) $(MdbProvider_LIBS:%=/r:%) $(MdbProvider_RESOURCES:%=$(ICON_FLAG)%) $(MdbProvider_CSFILES_CSC) $(DOC_FILE_CMD)

#AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
#	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
#	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.MdbProvider.pdb
endif
endif

install-mdbprovider: Simias.MdbProvider.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.MdbProvider.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.MdbProvider.conf $(DESTDIR)$(webdir)/modules
	
uninstall-mdbprovider:
	cd $(DESTDIR)$(webbindir); rm -f Simias.MdbProvider.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webdir)/modules; rm -f Simias.MdbProvider.conf
	
installcheck-local: install
	
#CLEAN_FILES = Simias.MdbSync.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs
CLEAN_FILES = Simias.MdbProvider.dll Simias.MdbProvider.conf $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

