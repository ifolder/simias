# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebSetup_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderWebSetup.cs $(srcdir)/Options.cs $(srcdir)/Prompt.cs
iFolderWebSetup_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebSetup_CSFILES))
iFolderWebSetup_INCLUDES =
iFolderWebSetup_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderWebSetup_FLAGS = $(CSC_EXEFLAG)
iFolderWebSetup_LIBS1 = System.dll System.Data.dll $(SYSTEM_XML)
iFolderWebSetup_LIBPATH = 

if !WINDOWS
	iFolderWebSetup_LIBS := $(iFolderWebSetup_LIBS1) Mono.Posix.dll
else
	iFolderWebSetup_LIBS := $(iFolderWebSetup_LIBS1)
endif

EXTRA_DIST = $(iFolderWebSetup_CSFILES) ifolder-web-setup.in ifolder-web-setup.cmd App.ico iFolderWebSetup.exe.config AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: iFolderWebSetup.exe

iFolderWebSetup.exe: $(iFolderWebSetup_CSFILES) $(iFolderWebSetup_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebSetup_FLAGS) $(iFolderWebSetup_LIBS:%=/r:%) $(iFolderWebSetup_LIBPATH:%=/lib:%) $(iFolderWebSetup_RESOURCES:%=$(ICON_FLAG)%) $(iFolderWebSetup_CSFILES_CSC) /doc:iFolderWebSetup.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = iFolderWebSetup.pdb
endif
endif

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderWebSetup.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderWebSetup.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) ifolder-web-setup.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) ifolder-web-setup $(DESTDIR)$(bindir)
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderWebSetup.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f ifolder-web-setup ifolder-web-setup.cmd
	cd $(DESTDIR)$(bindir); rm -f iFolderWebSetup.exe.config

installcheck-local: install
	
CLEAN_FILES = iFolderWebSetup.exe $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

