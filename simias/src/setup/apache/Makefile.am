SUBDIRS = default example.com

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = ifolder_apache.conf.in

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(simiasconfdir)/apache
	$(INSTALL_PROGRAM) ifolder_apache.conf $(DESTDIR)$(simiasconfdir)/apache

uninstall-local:
	cd $(DESTDIR)$(simiasconfdir)/apache; rm -f ifolder_apache.conf

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:
	rm -f ifolder_apache.conf

