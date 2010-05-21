# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebServiceLocal_CSFILES = \
	$(srcdir)/iFolderCommonLocal.cs \
	$(srcdir)/iFolderAdminLocal.asmx.cs \
	$(srcdir)/iFolderWebLocal.asmx.cs \
	$(srcdir)/iFolder.cs \
	$(srcdir)/iFolderDetails.cs \
	$(srcdir)/iFolderType.cs \
	$(srcdir)/iFolderUser.cs \
	$(srcdir)/iFolderUserDetails.cs \
	$(srcdir)/iFolderServer.cs \
	$(srcdir)/iFolderSystem.cs \
	$(srcdir)/SystemPolicy.cs \
	$(srcdir)/iFolderPolicy.cs \
	$(srcdir)/UserPolicy.cs \
	$(srcdir)/Exceptions.cs \
	$(srcdir)/SmartException.cs \
	$(srcdir)/FileHandler.cs \
	$(srcdir)/Upload.ashx.cs \
	$(srcdir)/Download.ashx.cs \
	$(srcdir)/SearchOperation.cs \
	$(srcdir)/SearchProperty.cs \
	$(srcdir)/iFolderEntry.cs \
	$(srcdir)/iFolderEntryType.cs \
	$(srcdir)/iFolderFile.cs \
	$(srcdir)/MemberRole.cs \
	$(srcdir)/ChangeEntry.cs \
	$(srcdir)/Settings.cs \
	$(srcdir)/NodeUtility.cs \
	$(srcdir)/Rights.cs \
	$(srcdir)/IdentityPolicy.cs \
	$(srcdir)/SampleData.cs \
	$(srcdir)/LastSyncInfo.cs \
	$(srcdir)/SyncServiceInfo.cs

iFolderWebServiceLocal_WEBFILES = \
	$(srcdir)/iFolderAdminLocal.asmx \
	$(srcdir)/iFolderWebLocal.asmx \
	$(srcdir)/Download.ashx \
	$(srcdir)/Upload.ashx
	
iFolderWebService_CSFILES = \
	$(srcdir)/iFolderAdmin.asmx.cs \
	$(srcdir)/iFolderWeb.asmx.cs \
	$(srcdir)/iFolderAdminLocalProxy.cs \
	$(srcdir)/iFolderWebLocalProxy.cs

iFolderWebService_WEBFILES = \
	$(srcdir)/iFolderAdmin.asmx \
	$(srcdir)/iFolderWeb.asmx
	
iFolderWebServiceLocal_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebServiceLocal_CSFILES))
iFolderWebServiceLocal_INCLUDES =
iFolderWebServiceLocal_RESOURCES =
iFolderWebServiceLocal_FLAGS = $(CSC_LIBFLAG)
if LINUX
iFolderWebServiceLocal_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll Simias.LdapProvider.dll 
else
iFolderWebServiceLocal_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll Simias.LdapProvider.dll 
endif
iFolderWebServiceLocal_LIBPATH = $(LOG4NET_DIR) ../core/SimiasClient ../core/SimiasLib.dll ../server/Simias.Server ../server/Simias.LdapProvider/

GenerateWsdl_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll

iFolderWebService_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebService_CSFILES))
iFolderWebService_INCLUDES =
iFolderWebService_RESOURCES =
iFolderWebService_FLAGS = $(CSC_LIBFLAG)
if LINUX
iFolderWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll iFolder.WebService.Local.dll 
else
iFolderWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll iFolder.WebService.Local.dll 
endif
iFolderWebService_LIBPATH = $(LOG4NET_DIR) ../core/SimiasClient ../core/SimiasLib.dll ../server/Simias.Server 

EXTRA_DIST =  AssemblyInfo.cs.in $(iFolderWebServiceLocal_WEBFILES) $(iFolderWebServiceLocal_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: iFolder.WebService.dll GenerateWsdl.exe iFolderAdmin.wsdl iFolderWeb.wsdl

iFolder.WebService.Local.dll: $(srcdir)/AssemblyInfo.cs copy $(iFolderWebServiceLocal_CSFILES) $(iFolderWebServiceLocal_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebServiceLocal_FLAGS) $(iFolderWebServiceLocal_LIBS:%=/r:%) $(iFolderWebServiceLocal_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWebServiceLocal_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(iFolderWebServiceLocal_CSFILES_CSC) /r:Novell.Directory.Ldap

iFolder.WebService.dll: $(srcdir)/AssemblyInfo.cs iFolder.WebService.Local.dll $(iFolderWebService_CSFILES) $(iFolderWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebService_FLAGS) $(iFolderWebService_LIBS:%=/r:%) $(iFolderWebService_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWebService_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(iFolderWebService_CSFILES_CSC) /r:Novell.Directory.Ldap

GenerateWsdl.exe: GenerateWsdl.cs
	$(CSC) /out:$@ $(CSCFLAGS) $(CSC_EXEFLAG) $(GenerateWsdl_LIBS:%=/r:%) $<

copy:
	cp ../core/SimiasClient/SimiasClient.dll .
	cp ../core/SimiasLib.dll/SimiasLib.dll .
	cp ../server/Simias.Server/Simias.Server.dll .
	cp ../server/Simias.LdapProvider/Simias.LdapProvider.dll .

iFolderWebLocalProxy.cs: iFolderWebLocal.wsdl
	$(WSDL) /nologo /namespace:iFolder.WebService.Web /out:$@ $<

iFolderAdminLocalProxy.cs: iFolderAdminLocal.wsdl
	$(WSDL) /nologo /namespace:iFolder.WebService.Admin /out:$@ $<

iFolderWebLocal.wsdl: copy iFolder.WebService.Local.dll GenerateWsdl.exe
	$(MONO) GenerateWsdl.exe iFolder.WebService.Local.dll iFolder.WebService.iFolderWebLocal http://127.0.0.1:8086/simias10/iFolderWebLocal.asmx $@

iFolderAdminLocal.wsdl: copy iFolder.WebService.Local.dll GenerateWsdl.exe
	$(MONO) GenerateWsdl.exe iFolder.WebService.Local.dll iFolder.WebService.iFolderAdminLocal http://127.0.0.1:8086/simias10/iFolderAdminLocal.asmx $@

iFolderWeb.wsdl: copy iFolder.WebService.dll GenerateWsdl.exe
	$(MONO) GenerateWsdl.exe iFolder.WebService.dll iFolder.WebService.iFolderWeb http://127.0.0.1:8086/simias10/iFolderWeb.asmx $@

iFolderAdmin.wsdl: copy iFolder.WebService.dll GenerateWsdl.exe
	$(MONO) GenerateWsdl.exe iFolder.WebService.dll iFolder.WebService.iFolderAdmin http://127.0.0.1:8086/simias10/iFolderAdmin.asmx $@

if DEBUG
if WINDOWS
DEBUG_FILES = iFolder.WebService.pdb iFolder.WebService.Local.pdb
endif
endif

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(etcfdir)
	$(INSTALL_PROGRAM) $(iFolderWebServiceLocal_WEBFILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(iFolderWebService_WEBFILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) iFolder.WebService.dll iFolder.WebService.Local.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f $(iFolderWebServiceLocal_WEBFILES)
	cd $(DESTDIR)$(webdir); rm -f $(iFolderWebService_WEBFILES)
	cd $(DESTDIR)$(webbindir); rm -f iFolder.WebService.dll iFolder.WebService.Local.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = iFolder.WebService.dll iFolder.WebService.Local.dll GenerateWsdl.exe *.wsdl $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES) SimiasClient.dll SimiasLib.dll SimiasServer.dll

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

