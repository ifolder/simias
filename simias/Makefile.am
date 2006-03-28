SUBDIRS = src

DIST_SUBDIRS = $(SUBDIRS) doc package test

PROJECT_FILES = $(srcdir)/SimiasLib.sln $(srcdir)/SimiasLib.csproj

# remove all of the CVS directories
dist-hook:
	rm -rf `find $(distdir) -name CVS`
	rm -rf `find $(distdir) -name ".svn"`

EXTRA_DIST = tools/aclocal tools/gsoap external/libflaim external/log4net $(srcdir)/$(PACKAGE)*.pc.in $(PROJECT_FILES)

all:

export PKG_CONFIG_PATH := $(PKG_CONFIG_PATH)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)/pkgconfig $(DESTDIR)$(datadir)/$(PACKAGE)
	$(INSTALL_PROGRAM) $(srcdir)/COPYING $(DESTDIR)$(datadir)/$(PACKAGE)
	$(INSTALL_PROGRAM) $(PACKAGE)*.pc $(DESTDIR)$(libdir)/pkgconfig
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(libdir)
endif
endif

uninstall-local:
	cd $(DESTDIR)$(libdir)/pkgconfig; rm -f $(PACKAGE)*.pc
	cd $(DESTDIR)$(datadir)/$(PACKAGE); rm -f COPYING
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(libdir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif

CLEAN_FILES = 

clean-local:
	rm -rf $(COMMON_CLEAN_FILES) $(CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -f configure $(PACKAGE)-*.tar.gz config.cache
	rm -f aclocal.m4 conf*.sh configure.lineno config.log
	rm -f config.guess config.sub config.status
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache
	rm -rf simias.pc simias-client.pc simias-client-c.pc

.PHONY: test
test: install
	$(MAKE) -C test test
		
test-clean:
	$(MAKE) -C test clean
		
.PHONY: doc doc-clean
doc:
	$(MAKE) -C doc all

doc-nodeps:
	$(MAKE) -C doc all-nodeps

doc-clean:
	$(MAKE) -C doc clean

.PHONY: package package-sdk package-clean

package:
	$(MAKE) -C package all

package-sdk:
	$(MAKE) -C package sdk

package-clean:
	$(MAKE) -C package clean

if WINDOWS
#
# On Windows, overwrite dist targets to alter how gzipped-tarballs are created
#
dist-gzip: distdir
#	$(AMTAR) chof - $(distdir) | GZIP=$(GZIP_ENV) gzip -c >$(distdir).tar.gz
	GZIP=$(GZIP_ENV) $(AMTAR) zchof $(distdir).tar.gz $(distdir)
	$(am__remove_distdir)

dist dist-all: distdir
#	$(AMTAR) chof - $(distdir) | GZIP=$(GZIP_ENV) gzip -c >$(distdir).tar.gz
	GZIP=$(GZIP_ENV) $(AMTAR) zchof $(distdir).tar.gz $(distdir)
	$(am__remove_distdir)

endif # WINDOWS
