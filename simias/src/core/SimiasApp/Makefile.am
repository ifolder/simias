# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

if WINDOWS
SYSTEM_INSTALLER_LIB = System.Configuration.Install.dll
else
SYSTEM_INSTALLER_LIB =
endif

MONO_WEBSERVER_PATH = ../../../external/Mono.WebServer

Simias_CSFILES = $(srcdir)/AssemblyInfo.cs \
		$(srcdir)/Simias.cs \
		$(srcdir)/SimiasAppInstallerClass.cs
				
Simias_CSFILES_CSC := $(subst /,$(SEP),$(Simias_CSFILES))
Simias_INCLUDES =
Simias_RESOURCES = $(srcdir)/App$(ICON_EXT)
Simias_FLAGS = $(CSC_EXEFLAG)
Simias_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll $(SYSTEM_INSTALLER_LIB) SimiasClient.dll Mono.WebServer.dll
Simias_LIBPATH = ../SimiasClient $(MONO_WEBSERVER_PATH)

EXTRA_DIST = App.ico $(Simias_CSFILES) AssemblyInfo.cs.in $(srcdir)/simias.cmd $(srcdir)/simias.in $(srcdir)/SimiasDirectoryMapping.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

if WINDOWS
SIMIAS_PATH = $(shell cygpath --mixed $(webbindir))/Simias.exe
else
SIMIAS_PATH = @webbindir@/Simias.exe
endif

all: Simias.exe

Simias.exe: AssemblyInfo $(Simias_CSFILES) $(Simias_RESOURCES) simias DirectoryMapping
	$(CSC) /out:$@ $(CSCFLAGS) $(Simias_FLAGS) $(SIMIAS_LIBS) $(Simias_LIBS:%=/r:%) $(Simias_LIBPATH:%=/lib:%) $(Simias_RESOURCES:%=$(ICON_FLAG)%) $(Simias_CSFILES_CSC) $(DOC_FILE_CMD)

AssemblyInfo: $(srcdir)/AssemblyInfo.cs.in 
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > AssemblyInfo.cs

simias: $(srcdir)/simias.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif	

DirectoryMapping: $(srcdir)/SimiasDirectoryMapping.in
	sed -e 's,@_mappingpath_@,$(SIMIAS_PATH),g' $< > SimiasDirectoryMapping
	chmod +x SimiasDirectoryMapping

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.pdb
endif
endif

install-exec-local: Simias.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.exe $(DEBUG_FILES) $(DESTDIR)$(webbindir)
if WINDOWS
	$(INSTALL_PROGRAM) $(MONO_WEBSERVER_PATH)/Mono.Security.dll $(DESTDIR)$(webbindir)
endif

if SIMIAS_AS_CLIENT
	$(INSTALL_PROGRAM) SimiasDirectoryMapping $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) simias.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif
endif

if !GACD_MONO_WEBSERVER
	$(INSTALL_PROGRAM) $(MONO_WEBSERVER_PATH)/Mono.WebServer.dll $(DESTDIR)$(webbindir)
endif

	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f SimiasDirectoryMapping
if WINDOWS
	cd $(DESTDIR)$(webbindir); rm -f Mono.Security.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f simias.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simias
endif

if !GACD_MONO_WEBSERVER
	cd $(DESTDIR)$(webbindir); rm -f Mono.WebServer.dll $(DEBUG_FILES)
endif


installcheck-local: install
	
CLEAN_FILES = Simias.exe Mono.WebServer.dll Mono.Security.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs simias simias.cmd SimiasDirectoryMapping

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

