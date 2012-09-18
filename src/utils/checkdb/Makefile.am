export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
checkdb_CSFILES = \
	$(srcdir)/CheckSimiasDB.cs 
	
dist_man_MANS=ifolder-database-check.8

checkdb_CSFILES_CSC := $(subst /,$(SEP),$(checkdb_CSFILES))
oldserveragent_CSFILES_CSC := $(subst /,$(SEP),$(oldserveragent_CSFILES))
#checkdb_FLAGS = $(CSC_EXEFLAG)
checkdb_FLAGS =  /optimize+ /d:LINUX /d:MONO /warn:4 /d:TRACE /d:MONONATIVE 
checkdb_LIBS = \
			/r:System.dll \
			/r:System.Data.dll \
			/r:System.Security.dll \
			/r:System.Runtime.Remoting.dll \
			/r:System.Web.dll \
			/r:Mono.Posix.dll
checkdb_LIBPATH = \
			../../core/SimiasClient/ \
			../../core/SimiasLib.dll/ \
			../../server/Simias.Server/
checkdb_LIBS = \
			/r:SimiasClient.dll \
			/r:SimiasLib.dll \
			/r:Simias.Server.dll

EXTRA_DIST = $(checkdb_CSFILES) ifolder-database-check 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $<  > $@
	chmod +x $@

all: CheckSimiasDB.exe ifolder-database-check 

CheckSimiasDB.exe: $(checkdb_CSFILES) AssemblyInfo.cs
	gmcs /out:CheckSimiasDB.exe  $(checkdb_FLAGS) /target:exe $(checkdb_LIBS) $(checkdb_LIBPATH:%=/lib:%) $(checkdb_LIBS) $(checkdb_CSFILES_CSC) AssemblyInfo.cs

ifolder-database-check: $(srcdir)/ifolder-database-check.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@

install-exec-local: CheckSimiasDB.exe ifolder-database-check 
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CheckSimiasDB.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder-database-check $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CheckSimiasDB.exe ifolder-database-check

installcheck-local: install
	
CLEAN_FILES = CheckSimiasDB.exe AssemblyInfo.cs ifolder-database-check

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
