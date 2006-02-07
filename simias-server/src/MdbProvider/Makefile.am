
# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

MdbSync_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/DomainConfig.cs $(srcdir)/EnumUsers.cs $(srcdir)/Mdb.cs $(srcdir)/MdbUser.cs $(srcdir)/MdbSync.cs $(srcdir)/ThreadService.cs     
MdbSync_CSFILES_CSC := $(subst /,$(SEP),$(MdbSync_CSFILES))
MdbSync_INCLUDES =
MdbSync_RESOURCES =
MdbSync_FLAGS = $(CSC_LIBFLAG)
MdbSync_LIBPATH = $(DESTDIR)$(webbindir) ../../../simias/src/SimiasLib.dll ../../../simias/src/SimiasClient
MdbSync_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll  

EXTRA_DIST = $(MdbSync_CSFILES) AssemblyInfo.cs.in 

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Simias.MdbSync.dll

Simias.MdbSync.dll: $(MdbSync_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(MdbSync_FLAGS) $(MdbSync_LIBPATH:%=/lib:%) $(MdbSync_LIBS:%=/r:%) $(MdbSync_RESOURCES:%=$(ICON_FLAG)%) $(MdbSync_CSFILES_CSC) $(DOC_FILE_CMD)

#Simias.MdbSync.dll: AssemblyInfo.cs $(srcdir)/SimiasSetup.cs $(MdbSync_CSFILES) $(MdbSync_RESOURCES)
#	$(CSC) /out:$@ $(CSCFLAGS) $(STRONG_NAME) $(MdbSync_FLAGS) $(MdbSync_LIBS:%=/r:%) $(MdbSync_LIBPATH:%=/lib:%) $(MdbSync_RESOURCES:%=$(ICON_FLAG)%) $(MdbSync_CSFILES_CSC) $(subst /,$(SEP),$(srcdir)/SimiasSetup.cs) $(DOC_FILE_CMD)

#AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
#	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
#	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.MdbSync.pdb
endif
endif

install-exec-local: Simias.MdbSync.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.MdbSync.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.MdbSync.conf $(DESTDIR)$(webdir)/modules
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.MdbSync.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webdir)/modules; rm -f Simias.MdbSync.conf
	
installcheck-local: install
	
#CLEAN_FILES = Simias.MdbSync.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs
CLEAN_FILES = Simias.MdbSync.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

