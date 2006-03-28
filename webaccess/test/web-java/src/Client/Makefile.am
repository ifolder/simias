SRC_FILES = \
	$(srcdir)/iFolderWebClient.java \
	$(srcdir)/iFolderAuthenticator.java

EXTRA_DIST = \
	$(SRC_FILES) iFolderWebClientJava.in iFolderWebClientJava.cmd

CLASSES_DIR = ./classes

CP1 = \
	../Proxy/iFolderWebAccess.jar \
	$(EXTERNAL_DIR)/axis/axis.jar \
	$(EXTERNAL_DIR)/axis/jaxrpc.jar \
	$(CLASSES_DIR)

CP = $(subst $(SPACE),$(PATH_SEP),$(CP1))

CLEAN_FILES = $(CLASSES_DIR) iFolderWebClient.jar

all: iFolderWebClient.jar

iFolderWebClient.jar: $(SRC_FILES)
	rm -rf $(CLASSES_DIR)
	$(mkinstalldirs) $(CLASSES_DIR)
	$(JAVAC) $(JAVACFLAGS) -d "$(CLASSES_DIR)" -classpath "$(CP)" $(SRC_FILES)
	$(JAR) cvf $@ -C $(CLASSES_DIR) .

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
if WINDOWS
	$(INSTALL_PROGRAM) iFolderWebClientJava.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) iFolderWebClientJava $(DESTDIR)$(bindir)
endif
	$(INSTALL_PROGRAM) iFolderWebClient.jar $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(EXTERNAL_DIR)/axis/axis.jar \
		$(EXTERNAL_DIR)/axis/jaxrpc.jar \
		$(EXTERNAL_DIR)/axis/saaj.jar \
		$(EXTERNAL_DIR)/axis/commons-discovery.jar \
		$(EXTERNAL_DIR)/axis/commons-logging.jar \
		$(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderWebClientJava iFolderWebClientJava.cmd
	cd $(DESTDIR)$(libdir); rm -f iFolderWebClient.jar
	cd $(DESTDIR)$(libdir); rm -f axis.jar jaxrpc.jar saaj.jar \
		commons-logging.jar commons-discovery.jar
    
installcheck-local: install

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

