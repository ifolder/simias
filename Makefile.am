SUBDIRS = ./src

DIST_SUBDIRS = $(SUBDIRS) package 

# remove all of the CVS directories
dist-hook:
	rm -rf `find $(distdir) -name CVS`
	rm -rf `find $(distdir) -name ".svn"`

EXTRA_DIST = ./iFolder.snk ./dependencies/external/tools/aclocal  ./dependencies/external/tools/gsoap ./dependencies/external/libflaim ./dependencies/external/Mono.WebServer ./dependencies/external/MonoRuntime/gmcs.in ./dependencies/external/MonoRuntime/mod-mono-server2.in ./dependencies/external/MonoRuntime/mod_mono.conf.in

all: $(srcdir)/dependencies/external/MonoRuntime/mod_mono.conf $(srcdir)/dependencies/external/MonoRuntime/mod-mono-server2 $(srcdir)/dependencies/external/MonoRuntime/gmcs

$(srcdir)/dependencies/external/MonoRuntime/mod_mono.conf: $(srcdir)/dependencies/external/MonoRuntime/mod_mono.conf.in
if !SIMIAS_AS_CLIENT
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif

$(srcdir)/dependencies/external/MonoRuntime/gmcs: $(srcdir)/dependencies/external/MonoRuntime/gmcs.in
if !SIMIAS_AS_CLIENT
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif

$(srcdir)/dependencies/external/MonoRuntime/mod-mono-server2: $(srcdir)/dependencies/external/MonoRuntime/mod-mono-server2.in
if !SIMIAS_AS_CLIENT
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif

export PKG_CONFIG_PATH := $(PKG_CONFIG_PATH)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(datadir)/$(PACKAGE)
	$(INSTALL_PROGRAM) $(srcdir)/COPYING $(DESTDIR)$(datadir)/$(PACKAGE)
if !SIMIAS_AS_CLIENT
#	$(mkinstalldirs)  $(DESTDIR)$(bindir)/../lib/Mono
#	$(mkinstalldirs)  $(DESTDIR)$(bindir)/../lib/Mono/bin
#	$(mkinstalldirs)  $(DESTDIR)$(bindir)/../lib/Mono/rpms
#	$(INSTALL_PROGRAM) $(srcdir)/dependencies/external/MonoRuntime/MonoRpms64bit.tar  $(DESTDIR)$(bindir)/../lib/Mono/rpms/
#	$(INSTALL_PROGRAM) $(srcdir)/dependencies/external/MonoRuntime/MonoRpms32bit.tar  $(DESTDIR)$(bindir)/../lib/Mono/rpms/
#	$(INSTALL_PROGRAM) $(srcdir)/dependencies/external/MonoRuntime/gmcs  $(DESTDIR)$(bindir)/../lib/Mono/bin/
#	$(INSTALL_PROGRAM) $(srcdir)/dependencies/external/MonoRuntime/mod-mono-server2  $(DESTDIR)$(bindir)/../lib/Mono/bin/
#	$(INSTALL_PROGRAM) $(srcdir)/dependencies/external/MonoRuntime/mod_mono.conf  $(DESTDIR)$(bindir)/../lib/Mono/bin/mod_mono.conf.in
endif

uninstall-local:
	cd $(DESTDIR)$(datadir)/$(PACKAGE); rm -f COPYING
if !SIMIAS_AS_CLIENT
#	cd $(DESTDIR)$(bindir)/../lib/Mono/rpms/; rm -rf MonoRpms64bit.tar MonoRpms32bit.tar
#	cd $(DESTDIR)$(bindir)/../lib/Mono/bin/; rm -rf gmcs  mod-mono-server2 mod_mono.conf
endif

CLEAN_FILES =
 

clean-local:
	rm -rf $(COMMON_CLEAN_FILES) $(CLEAN_FILES)
if !SIMIAS_AS_CLIENT
	rm -rf  $(srcdir)/dependencies/external/MonoRuntime/mod_mono.conf $(srcdir)/dependencies/external/MonoRuntime/mod-mono-server2 $(srcdir)/dependencies/external/MonoRuntime/gmcs 
endif

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

.PHONY: test
test: install
	$(MAKE) -C ./src/test test
		
test-clean:
	$(MAKE) -C ./src/test clean
		
.PHONY: doc doc-clean
doc:
	$(MAKE) -C ./src/doc all

doc-nodeps:
	$(MAKE) -C ./src/doc all-nodeps

doc-clean:
	$(MAKE) -C ./src/doc clean

.PHONY: package package-sdk package-clean

package:
	$(MAKE) -C ./package all

package-sdk:
	$(MAKE) -C ./package sdk

package-clean:
	$(MAKE) -C ./package clean

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
