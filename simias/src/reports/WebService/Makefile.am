# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderReports_CSFILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Report.cs \
	$(srcdir)/ReportColumn.cs \
	$(srcdir)/iFolderReport.cs \
	$(srcdir)/iFolderReports.asmx.cs
	
iFolderReports_CSFILES_CSC := $(subst /,$(SEP),$(iFolderReports_CSFILES))
iFolderReports_INCLUDES =
iFolderReports_RESOURCES =
iFolderReports_FLAGS = $(CSC_LIBFLAG)
iFolderReports_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll
iFolderReports_LIBPATH =

EXTRA_DIST = $(srcdir)/AssemblyInfo.cs.in iFolderReports.asmx iFolderReports.wsdl $(iFolderReports_CSFILES)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: Novell.iFolder.Enterprise.Reports.dll

Novell.iFolder.Enterprise.Reports.dll: $(iFolderReports_CSFILES) $(iFolderReports_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderReports_FLAGS) $(SIMIAS_CLIENT_LIBS) $(SIMIAS_LIBS) $(IFOLDER_LIBS) $(SIMIAS_ENTERPRISE_LIBS) $(iFolderReports_LIBS:%=/r:%) $(iFolderReports_LIBPATH:%=/lib:%) $(iFolderReports_RESOURCES:%=$(ICON_FLAG)%) $(iFolderReports_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.Enterprise.Reports.pdb
endif
endif

install-exec-local: Novell.iFolder.Enterprise.Reports.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) iFolderReports.asmx $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Novell.iFolder.Enterprise.Reports.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f iFolderEnterprise.asmx iFolderWebAccess.asmx Download.ashx Upload.ashx
	cd $(DESTDIR)$(webbindir); rm -f Novell.iFolder.Enterprise.Reports.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = Novell.iFolder.Enterprise.Reports.dll AssemblyInfo.cs $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

