# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SIMIAS_CSFILES     := $(shell cat $(srcdir)/SimiasLib.dll.sources)
SIMIAS_CSFILES_SRC := $(SIMIAS_CSFILES:%=$(srcdir)/%)
SIMIAS_CSFILES_CSC := $(subst /,$(SEP),$(SIMIAS_CSFILES_SRC))
SIMIAS_INCLUDES =
SIMIAS_RESOURCES =
SIMIAS_FLAGS = $(CSC_LIBFLAG) /unsafe
if LINUX
SIMIAS_LIBS1 = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Runtime.Remoting.dll System.Web.dll System.Web.Services.dll SimiasClient.dll
else
SIMIAS_LIBS1 = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Runtime.Remoting.dll System.Web.dll System.Web.Services.dll SimiasClient.dll log4net.dll 
endif
SIMIAS_LIBPATH = $(LOG4NET_DIR) ../SimiasClient

if !WINDOWS
	SIMIAS_LIBS = $(SIMIAS_LIBS1) Mono.Posix.dll
else
	SIMIAS_LIBS = $(SIMIAS_LIBS1)
endif

EXTRA_DIST = $(srcdir)/SimiasLib.dll.sources $(SIMIAS_CSFILES) ../Common/Simias.log4net ../Common/Simias.config.in ../Common/MyDns.cs.in ../Login/Login.ashx SimiasLib.dll.config.in AssemblyInfo.cs.in SimiasLib.conf simias.pc.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: SimiasLib.dll ../Common/Simias.config

SimiasLib.dll: AssemblyInfo.cs $(SIMIAS_CSFILES_SRC) ../Common/MyDns.cs $(SIMIAS_RESOURCES) $(srcdir)/SimiasLib.dll.sources
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_FLAGS) $(SIMIAS_LIBS:%=/r:%) $(SIMIAS_LIBPATH:%=/lib:%) $(LOG4NET_LIBS) $(SIMIAS_CSFILES_CSC) $(subst /,$(SEP),../Common/MyDns.cs) $(DOC_FILE_CMD)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasLib.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(modulesdir) $(DESTDIR)$(sysconfdir) $(DESTDIR)$(datadir)/simias
	$(INSTALL_PROGRAM) SimiasLib.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SimiasLib.conf $(DESTDIR)$(modulesdir)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig
	$(INSTALL_PROGRAM) simias.pc $(DESTDIR)$(libdir)/pkgconfig
if !LINUX
	$(INSTALL_PROGRAM) $(srcdir)/$(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(webbindir)
	cp -f $(srcdir)/$(LOG4NET_DIR)/../../../../LICENSE.txt $(DESTDIR)$(datadir)/simias/log4net.LICENSE.txt
endif	
	$(INSTALL_PROGRAM) ../Common/Simias.log4net $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) ../Common/Simias.config $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) ../Login/Login.ashx $(DESTDIR)$(webdir)
if LINUX
	$(INSTALL_PROGRAM) SimiasLib.dll.config $(DESTDIR)$(webbindir)
endif

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -rf $(modulesdir)
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f simias.pc
if !LINUX
	cd $(DESTDIR)$(webbindir); rm -f log4net.dll
	cd $(DESTDIR)$(datadir)/simias; rm -f log4net.LICENSE.txt
endif
	cd $(DESTDIR)$(webbindir); rm -f SimiasLib.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(sysconfdir); rm -f Simias.log4net Simias.config
if LINUX
	cd $(DESTDIR)$(webbindir); rm -f SimiasLib.dll.config
endif

CLEAN_FILES = SimiasLib.dll $(DOC_FILE_CLEAN) $(DEBUG_FILES) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf simias.pc

