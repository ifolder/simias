# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

OpenLdapProvider_CSFILES = $(srcdir)/OpenLdapSync.cs $(srcdir)/User.cs 
OpenLdapProvider_CSFILES_CSC := $(subst /,$(SEP),$(OpenLdapProvider_CSFILES))
OpenLdapProvider_INCLUDES =
OpenLdapProvider_RESOURCES =

OpenLdapProvider_FLAGS = $(CSC_LIBFLAG)

if WINDOWS
OpenLdapProvider_LIBPATH = ../Simias.Server ../Simias.LdapProvider ../../core/SimiasLib.dll ../../core/SimiasClient ../../../external/csharpldap $(DESTDIR)$(webbindir)
else
OpenLdapProvider_LIBPATH = ../Simias.Server ../Simias.LdapProvider ../../core/SimiasLib.dll ../../core/SimiasClient $(DESTDIR)$(webbindir)
endif

OpenLdapProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll Simias.LdapProvider.dll Novell.Directory.Ldap.dll

EXTRA_DIST = $(OpenLdapProvider_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.OpenLdapProvider.dll

Simias.OpenLdapProvider.dll: $(srcdir)/AssemblyInfo.cs $(OpenLdapProvider_CSFILES) $(OpenLdapProvider_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(OpenLdapProvider_FLAGS) $(SIMIAS_CLIENT_LIBS) $(SIMIAS_LIBS) $(OpenLdapProvider_LIBPATH:%=/lib:%) $(OpenLdapProvider_LIBS:%=/r:%) $(OpenLdapProvider_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(OpenLdapProvider_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(SERVER_ASSEMBLIES) 

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.OpenLdapProvider.pdb
endif
endif

install-exec-local: all
if WINDOWS
	$(INSTALL_PROGRAM) ../../../external/csharpldap/Novell.Directory.Ldap.dll $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) ../../../external/csharpldap/Mono.Security.dll $(DEBUG_FILES) $(DESTDIR)$(webdir)
endif	
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
if WINDOWS
	cd $(DESTDIR)$(webdir); rm -f Novell.Directory.dll Mono.Security.dll
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

