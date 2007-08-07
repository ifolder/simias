# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasHttpFile_CSFILES = $(srcdir)/SimiasFile.cs 
SimiasHttpFile_CSFILES_CSC := $(subst /,$(SEP),$(SimiasHttpFile_CSFILES))
SimiasHttpFile_INCLUDES =
SimiasHttpFile_RESOURCES =
SimiasHttpFile_FLAGS = $(CSC_LIBFLAG)
SimiasHttpFile_LIBPATH = ../../core/SimiasLib.dll ../../core/SimiasClient $(DESTDIR)$(webbindir)
SimiasHttpFile_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll

EXTRA_DIST = sfile.ashx pubsfile.ashx $(SimiasHttpFile_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.HttpFile.dll


Simias.HttpFile.dll: $(srcdir)/AssemblyInfo.cs $(SimiasHttpFile_CSFILES) $(SimiasHttpFile_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasHttpFile_FLAGS) $(SimiasHttpFile_LIBPATH:%=/lib:%) $(SimiasHttpFile_LIBS:%=/r:%) $(SimiasHttpFile_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(SimiasHttpFile_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(SERVER_ASSEMBLIES) 

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.HttpFile.pdb
endif
endif

install-exec-local: all
	$(INSTALL_PROGRAM) sfile.ashx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) pubsfile.ashx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f sfile.ashx
	cd $(DESTDIR)$(webdir); rm -f pubsfile.ashx
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

