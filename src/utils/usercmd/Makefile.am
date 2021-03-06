# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UserCmd_CSFILES = \
	$(srcdir)/Main.cs \
	$(srcdir)/AdminService.cs
	
UserCmd_CSFILES_CSC := $(subst /,$(SEP),$(UserCmd_CSFILES))
UserCmd_INCLUDES =
UserCmd_RESOURCES = 
UserCmd_FLAGS = $(CSC_EXEFLAG)
UserCmd_LIBPATH = $(LOG4NET_DIR)  
UserCmd_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll 

if !WINDOWS
	UserCmd_LIBS := $(UserCmd_LIBS1)
else
	UserCmd_LIBS := $(UserCmd_LIBS1)
endif

EXTRA_DIST = $(UserCmd_CSFILES) AssemblyInfo.cs.in simias-create-user.in simias-delete-user.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: UserCmd.exe

AdminService.cs: ../../webservices/iFolderAdmin.wsdl
	$(WSDL) /nologo /out:$@ $<

UserCmd.exe: $(srcdir)/AssemblyInfo.cs $(UserCmd_CSFILES) $(UserCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UserCmd_FLAGS) $(SIMIAS_LIBS) $(UserCmd_LIBS:%=/r:%) $(UserCmd_LIBPATH:%=/lib:%) $(UserCmd_RESOURCES:%=$(ICON_FLAG)%) $(UserCmd_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = UserCmd.pdb
endif
endif

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libexecdir)/bin
if WINDOWS	
	$(INSTALL_PROGRAM) UserCmd.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) simias-user.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) UserCmd.exe $(DEBUG_FILES) $(DESTDIR)$(libexecdir)/bin
	$(INSTALL_PROGRAM) simias-create-user $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) simias-delete-user $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) simias-user $(DESTDIR)$(bindir)
endif

uninstall-local:
if WINDOWS	
	cd $(DESTDIR)$(bindir); rm -f UserCmd.exe simias-user.cmd $(DEBUG_FILES)
else
	cd $(DESTDIR)$(libexecdir)/bin; rm -f UserCmd.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f simias-create-user simias-delete-user simias-user
endif

installcheck-local: install
	
CLEAN_FILES = UserCmd.exe $(DOC_FILE_CLEAN)	$(srcdir)/AdminService.cs $(srcdir)/AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

