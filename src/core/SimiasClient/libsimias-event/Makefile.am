lib_LTLIBRARIES = libsimias-event.la

libsimias_event_la_SOURCES = \
	simias-event-client.c simias-event-client.h

pkginclude_HEADERS = \
	simias-event-client.h

if DARWIN
libsimias_event_la_CFLAGS = \
	$(XML2_CFLAGS) -DDARWIN
else
libsimias_event_la_CFLAGS = \
	$(XML2_CFLAGS) $(CFLAGS_OPTIMIZE)
endif

if !WINDOWS
install-exec-hook:
	rm -f $(DESTDIR)$(libdir)/libsimias-event.a
	rm -f $(DESTDIR)$(libdir)/libsimias-event.la
endif

libsimias_event_la_LDFLAGS = \
	$(XML2_LIBS)

