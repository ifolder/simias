SUBDIRS = libsimias-event libsimias-manager

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

if !SIMIAS_AS_CLIENT
SimiasClient_CSFILES = $(srcdir)/CertPolicy.cs $(srcdir)/CollectionSyncEventArgs.cs $(srcdir)/FileSyncEventArgs.cs $(srcdir)/DomainService.cs $(srcdir)/IProcEventClient.cs $(srcdir)/IProcEventData.cs $(srcdir)/IProcEventListener.cs $(srcdir)/IProcEventRegistration.cs $(srcdir)/IProcEventTypes.cs $(srcdir)/LocalService.cs $(srcdir)/Manager.cs $(srcdir)/MyEnvironment.cs $(srcdir)/MyPath.cs $(srcdir)/MyPlatformID.cs $(srcdir)/MyRuntimeID.cs $(srcdir)/NeedCredentialsEventArgs.cs $(srcdir)/NodeEventArgs.cs $(srcdir)/NodeTypes.cs $(srcdir)/NotifyEventArgs.cs $(srcdir)/SimiasEventArgs.cs $(srcdir)/Authentication.cs $(srcdir)/SimiasService2.cs $(srcdir)/SyncStatus.cs
else
SimiasClient_CSFILES = $(srcdir)/CertPolicy.cs $(srcdir)/CollectionSyncEventArgs.cs $(srcdir)/FileSyncEventArgs.cs $(srcdir)/DomainService.cs $(srcdir)/IProcEventClient.cs $(srcdir)/IProcEventData.cs $(srcdir)/IProcEventListener.cs $(srcdir)/IProcEventRegistration.cs $(srcdir)/IProcEventTypes.cs $(srcdir)/LocalService.cs $(srcdir)/Manager.cs $(srcdir)/MyEnvironment.cs $(srcdir)/MyPath.cs $(srcdir)/MyPlatformID.cs $(srcdir)/MyRuntimeID.cs $(srcdir)/NeedCredentialsEventArgs.cs $(srcdir)/NodeEventArgs.cs $(srcdir)/NodeTypes.cs $(srcdir)/NotifyEventArgs.cs $(srcdir)/SimiasEventArgs.cs $(srcdir)/Authentication.cs $(srcdir)/SimiasService.cs $(srcdir)/SyncStatus.cs
endif

SimiasClient_CSFILES_CSC := $(subst /,$(SEP),$(SimiasClient_CSFILES))
SimiasClient_INCLUDES =
SimiasClient_RESOURCES =
SimiasClient_FLAGS = $(CSC_LIBFLAG)
SimiasClient_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Web.dll System.Web.Services.dll 
SimiasClient_LIBPATH = 

EXTRA_DIST = $(SimiasClient_CSFILES) $(srcdir)/SimiasSetup.cs.in AssemblyInfo.cs.in $(SIMIASCLIENT_KEYFILE) simias-client.pc.in simias-client-c.pc.in

# This is defined by including a PPK in a file named "simiasclient.snk" in the SimiasClient source directory.
# Note: that autogen must be run in order for this to take effect.
if HAS_SIMIASCLIENT_KEYFILE
STRONG_NAME = /d:STRONGNAME
else
STRONG_NAME=
endif

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SimiasClient.dll

$(srcdir)/SimiasService.cs: $(srcdir)/../WebService/Simias.wsdl

if WINDOWS
	wsdl /out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif
if LINUX
	-$(WSDL) -nologo -out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif
if DARWIN
	wsdl -nologo -out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif

$(srcdir)/SimiasService2.cs: $(srcdir)/../WebService/Simias.wsdl
if LINUX
	wsdl -nologo -out:SimiasService2.cs $(srcdir)/../WebService/Simias.wsdl
	wsdl1 -nologo -out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif

SimiasClient.dll: $(srcdir)/AssemblyInfo.cs $(SimiasClient_CSFILES) $(SimiasClient_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(STRONG_NAME) $(SimiasClient_FLAGS) $(SimiasClient_LIBS:%=/r:%) $(SimiasClient_LIBPATH:%=/lib:%) $(SimiasClient_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(SimiasClient_CSFILES_CSC) $(subst /,$(SEP),$(srcdir)/SimiasSetup.cs) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasClient.pdb
endif
endif

install-exec-local: SimiasClient.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasClient.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig
	$(INSTALL_PROGRAM) simias-client.pc $(DESTDIR)$(libdir)/pkgconfig
	$(INSTALL_PROGRAM) simias-client-c.pc $(DESTDIR)$(libdir)/pkgconfig
if HAS_SIMIASCLIENT_KEYFILE
	$(GACINSTALL) $(shell cygpath --mixed $(webbindir))/SimiasClient.dll
endif	
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f SimiasClient.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f simias-client.pc simias-client-c.pc
	
installcheck-local: install
	
CLEAN_FILES = SimiasClient.dll $(DOC_FILE_CLEAN) $(srcdir)/SimiasService.cs $(srcdir)/SimiasService2.cs AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)
	rm -rf AssemblyInfo.cs
	rm -rf Makefile

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf simias-client.pc simias-client-c.pc
	rm -rf SimiasSetup.cs AssemblyInfo.cs

