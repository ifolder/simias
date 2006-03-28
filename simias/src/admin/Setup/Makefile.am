# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderAdminSetup_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderAdminSetup.cs $(srcdir)/Options.cs $(srcdir)/Prompt.cs
iFolderAdminSetup_CSFILES_CSC := $(subst /,$(SEP),$(iFolderAdminSetup_CSFILES))
iFolderAdminSetup_INCLUDES =
iFolderAdminSetup_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderAdminSetup_FLAGS = $(CSC_EXEFLAG)
iFolderAdminSetup_LIBS1 = System.dll System.Data.dll $(SYSTEM_XML)
iFolderAdminSetup_LIBPATH = 

if !WINDOWS
	iFolderAdminSetup_LIBS := $(iFolderAdminSetup_LIBS1) Mono.Posix.dll
else
	iFolderAdminSetup_LIBS := $(iFolderAdminSetup_LIBS1)
endif

EXTRA_DIST = $(iFolderAdminSetup_CSFILES) ifolder-admin-setup.in ifolder-admin-setup.cmd App.ico iFolderAdminSetup.exe.config AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: iFolderAdminSetup.exe

iFolderAdminSetup.exe: $(iFolderAdminSetup_CSFILES) $(iFolderAdminSetup_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderAdminSetup_FLAGS) $(iFolderAdminSetup_LIBS:%=/r:%) $(iFolderAdminSetup_LIBPATH:%=/lib:%) $(iFolderAdminSetup_RESOURCES:%=$(ICON_FLAG)%) $(iFolderAdminSetup_CSFILES_CSC) /doc:iFolderAdminSetup.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = iFolderAdminSetup.pdb
endif
endif

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderAdminSetup.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderAdminSetup.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) ifolder-admin-setup.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) ifolder-admin-setup $(DESTDIR)$(bindir)
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderAdminSetup.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f ifolder-admin-setup ifolder-admin-setup.cmd
	cd $(DESTDIR)$(bindir); rm -f iFolderAdminSetup.exe.config

installcheck-local: install
	
CLEAN_FILES = iFolderAdminSetup.exe $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

