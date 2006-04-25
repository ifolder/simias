SUBDIRS =

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = ifolder_admin.conf.in ifolder_webaccess.conf.in simias_server.conf.in

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(simiasconfdir)/apache/example.com
	$(INSTALL_PROGRAM) ifolder_admin.conf ifolder_webaccess.conf simias_server.conf $(DESTDIR)$(simiasconfdir)/apache/example.com

uninstall-local:
	cd $(DESTDIR)$(simiasconfdir)/apache/example.com; rm -f ifolder_admin.conf ifolder_webaccess.conf simias_server.conf 

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:
	rm -f ifolder_admin.conf ifolder_webaccess.conf simias_server.conf 

