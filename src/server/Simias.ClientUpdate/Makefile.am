# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

ClientUpdate_CSFILES = $(srcdir)/ClientUpdate.asmx.cs $(srcdir)/ClientUpdateHandler.ashx.cs
ClientUpdate_CSFILES_CSC := $(subst /,$(SEP),$(ClientUpdate_CSFILES))
ClientUpdate_INCLUDES =
ClientUpdate_RESOURCES =
ClientUpdate_FLAGS = $(CSC_LIBFLAG)
ClientUpdate_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasClient.dll SimiasLib.dll
ClientUpdate_LIBPATH = ../../core/SimiasLib.dll ../../core/SimiasClient

EXTRA_DIST = ClientUpdate.asmx ClientUpdateHandler.ashx $(ClientUpdate_CSFILES) version.config unix-version.config AssemblyInfo.cs.in mac-version.config

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: ClientUpdate.dll

ClientUpdate.dll: $(srcdir)/AssemblyInfo.cs $(ClientUpdate_CSFILES) $(ClientUpdate_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ClientUpdate_FLAGS) $(SIMIAS_LIBS) $(SIMIAS_CLIENT_LIBS) $(ClientUpdate_LIBS:%=/r:%) $(ClientUpdate_LIBPATH:%=/lib:%) $(ClientUpdate_RESOURCES:%=$(ICON_FLAG)%) $(ClientUpdate_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = ClientUpdate.pdb
endif
endif

install-exec-local: ClientUpdate.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update/windows
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update/unix
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update/unix/default
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update/unix/suse-linux-10.0
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update/unix/opensuse-linux-10.3
	$(mkinstalldirs) $(DESTDIR)$(webdir)/update/mac
	$(INSTALL_PROGRAM) ClientUpdate.asmx $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) ClientUpdateHandler.ashx $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) ClientUpdate.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) version.config $(DESTDIR)$(webdir)/update/windows
	$(INSTALL_PROGRAM) unix-version.config $(DESTDIR)$(webdir)/update/unix
	$(INSTALL_PROGRAM) install-ifolder.sh $(DESTDIR)$(webdir)/update/unix/default
	$(INSTALL_PROGRAM) install-ifolder-script.sh $(DESTDIR)$(webdir)/update/unix/default
	$(INSTALL_PROGRAM) install-ifolder.sh $(DESTDIR)$(webdir)/update/unix/suse-linux-10.0
	$(INSTALL_PROGRAM) install-ifolder-script.sh $(DESTDIR)$(webdir)/update/unix/suse-linux-10.0
	$(INSTALL_PROGRAM) install-ifolder.sh $(DESTDIR)$(webdir)/update/unix/opensuse-linux-10.3
	$(INSTALL_PROGRAM) install-ifolder-script.sh $(DESTDIR)$(webdir)/update/unix/opensuse-linux-10.3
	$(INSTALL_PROGRAM) mac-version.config $(DESTDIR)$(webdir)/update/mac
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f ClientUpdate.asmx ClientUpdateHandler.ashx
	cd $(DESTDIR)$(webbindir); rm -f ClientUpdate.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webdir)/update/windows; rm -f version.config unix-version.config mac-version.config

installcheck-local: install
	
CLEAN_FILES = ClientUpdate.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

