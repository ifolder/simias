# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

ADLdapProvider_CSFILES = $(srcdir)/ADLdapSync.cs $(srcdir)/User.cs 
ADLdapProvider_CSFILES_CSC := $(subst /,$(SEP),$(ADLdapProvider_CSFILES))
ADLdapProvider_INCLUDES =
ADLdapProvider_RESOURCES =

ADLdapProvider_FLAGS = $(CSC_LIBFLAG)

if WINDOWS
ADLdapProvider_LIBPATH = ../Simias.Server ../Simias.LdapProvider ../../core/SimiasLib.dll ../../core/SimiasClient ../../../external/csharpldap $(DESTDIR)$(webbindir)
else
ADLdapProvider_LIBPATH = ../Simias.Server ../Simias.LdapProvider ../../core/SimiasLib.dll ../../core/SimiasClient $(DESTDIR)$(webbindir)
endif

ADLdapProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll Simias.LdapProvider.dll Novell.Directory.Ldap.dll

EXTRA_DIST = $(ADLdapProvider_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.ADLdapProvider.dll

Simias.ADLdapProvider.dll: $(srcdir)/AssemblyInfo.cs $(ADLdapProvider_CSFILES) $(ADLdapProvider_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ADLdapProvider_FLAGS) $(SIMIAS_CLIENT_LIBS) $(SIMIAS_LIBS) $(ADLdapProvider_LIBPATH:%=/lib:%) $(ADLdapProvider_LIBS:%=/r:%) $(ADLdapProvider_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(ADLdapProvider_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(SERVER_ASSEMBLIES) 

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.ADLdapProvider.pdb
endif
endif

install-exec-local: all
if WINDOWS
	$(INSTALL_PROGRAM) ../../../dependencies/external/csharpldap/Novell.Directory.Ldap.dll $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) ../../../dependencies/external/csharpldap/Mono.Security.dll $(DEBUG_FILES) $(DESTDIR)$(webdir)
endif	
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
if WINDOWS
	cd $(DESTDIR)$(webdir); rm -f Novell.Directory.Ldap.dll Mono.Security.dll
endif
	cd $(DESTDIR)$(webbindir); rm -f $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES)

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(SERVER_ASSEMBLIES) $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

