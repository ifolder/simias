# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderWebService_CSFILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/iFolderAdmin.asmx.cs \
	$(srcdir)/iFolderWeb.asmx.cs \
	$(srcdir)/iFolder.cs \
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
	$(srcdir)/NodeUtility.cs \
	$(srcdir)/IdentityPolicy.cs
	
iFolderWebService_WEBFILES = \
	$(srcdir)/iFolderAdmin.asmx \
	$(srcdir)/iFolderWeb.asmx \
	$(srcdir)/Download.ashx \
	$(srcdir)/Upload.ashx
	
iFolderWebService_CSFILES_CSC := $(subst /,$(SEP),$(iFolderWebService_CSFILES))
iFolderWebService_INCLUDES =
iFolderWebService_RESOURCES =
iFolderWebService_FLAGS = $(CSC_LIBFLAG)
iFolderWebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.Server.dll
iFolderWebService_LIBPATH = ../core/SimiasClient ../core/SimiasLib.dll ../server/Simias.Server

EXTRA_DIST =  AssemblyInfo.cs.in $(iFolderWebService_WEBFILES) $(iFolderWebService_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: iFolder.WebService.dll

iFolder.WebService.dll: $(iFolderWebService_CSFILES) $(iFolderWebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderWebService_FLAGS) $(iFolderWebService_LIBS:%=/r:%) $(iFolderWebService_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(iFolderWebService_RESOURCES:%=$(ICON_FLAG)%) $(iFolderWebService_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = iFolder.WebService.pdb
endif
endif

install-exec-local: iFolder.WebService.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) $(iFolderWebService_WEBFILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) iFolder.WebService.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f $(iFolderWebService_WEBFILES)
	cd $(DESTDIR)$(webbindir); rm -f iFolder.WebService.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = iFolder.WebService.dll $(DOC_FILE_CLEAN) 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs

