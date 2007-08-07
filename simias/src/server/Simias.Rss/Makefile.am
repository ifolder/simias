# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasRss_CSFILES = $(srcdir)/Channel.cs $(srcdir)/Item.cs $(srcdir)/RssFeed.cs $(srcdir)/PublicFeedHandler.cs $(srcdir)/RssHeaders.cs  $(srcdir)/Util.cs 
SimiasRss_CSFILES_CSC := $(subst /,$(SEP),$(SimiasRss_CSFILES))
SimiasRss_INCLUDES =
SimiasRss_RESOURCES =
SimiasRss_FLAGS = $(CSC_LIBFLAG)
SimiasRss_LIBPATH = ../Simias.HttpFile ../../core/SimiasLib.dll ../../core/SimiasClient $(DESTDIR)$(webbindir)
SimiasRss_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll Simias.HttpFile.dll

EXTRA_DIST = rss.ashx pubrss.ashx $(SimiasRss_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.Rss.dll

Simias.Rss.dll: $(srcdir)/AssemblyInfo.cs $(SimiasRss_CSFILES) $(SimiasRss_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasRss_FLAGS) $(SIMIAS_CLIENT_LIBS) $(SIMIAS_LIBS) $(SimiasRss_LIBPATH:%=/lib:%) $(SimiasRss_LIBS:%=/r:%) $(SimiasRss_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(SimiasRss_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(SERVER_ASSEMBLIES) 

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.Rss.pdb
endif
endif

install-exec-local: all
	$(INSTALL_PROGRAM) rss.ashx pubrss.ashx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f rss.ashx pubrss.ashx
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

