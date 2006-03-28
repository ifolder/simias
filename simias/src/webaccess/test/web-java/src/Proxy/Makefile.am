SRC_FILES = \
	$(srcdir)/src/com/novell/ifolder/web/*.java

EXTRA_DIST = \
	$(SRC_FILES)

CLASSES_DIR = ./classes
SRC_DIR = ./src

CP1 = \
	$(EXTERNAL_DIR)/axis/axis.jar \
	$(EXTERNAL_DIR)/axis/jaxrpc.jar \
	$(EXTERNAL_DIR)/axis/wsdl4j.jar	\
	$(EXTERNAL_DIR)/axis/commons-discovery.jar \
	$(EXTERNAL_DIR)/axis/commons-logging.jar \
	$(EXTERNAL_DIR)/axis/saaj.jar \
	$(EXTERNAL_DIR)/xerces/xercesImpl.jar \
	$(EXTERNAL_DIR)/xerces/xml-apis.jar \
	$(CLASSES_DIR)

CP = $(subst $(SPACE),$(PATH_SEP),$(CP1))

CLEAN_FILES = $(CLASSES_DIR) iFolderWebAccess.jar $(SRC_DIR)

.PHONY: SourceFiles

all: iFolderWebAccess.jar

iFolderWebAccess.jar: SourceFiles
	rm -rf $(CLASSES_DIR)
	$(mkinstalldirs) $(CLASSES_DIR)
	$(JAVAC) $(JAVACFLAGS) -d "$(CLASSES_DIR)" -classpath "$(CP)"  $(SRC_FILES)
	$(JAR) cvf $@ -C $(CLASSES_DIR) .  

SourceFiles: $(EXTERNAL_DIR)/ifolder/iFolderWebAccess.wsdl
	rm -rf $(SRC_DIR)
	$(mkinstalldirs) $(SRC_DIR)
	$(JAVA) -classpath "$(CP)" org.apache.axis.wsdl.WSDL2Java -v -o $(SRC_DIR) $< 

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) iFolderWebAccess.jar $(DESTDIR)$(libdir)
	
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f iFolderWebAccess.jar

installcheck-local: install

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

