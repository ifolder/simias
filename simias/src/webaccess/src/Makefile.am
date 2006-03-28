SUBDIRS = Proxy Client Web Setup 

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = 

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-summary:
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

