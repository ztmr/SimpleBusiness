
MCS=gmcs
OPTS=-debug -target:library -out:bin/Web.dll # -langversion:linq

REFERENCES=\
  -r:Db4objects.Db4o \
  -r:iTextSharp \
  -r:System.Web \
  -r:System.Configuration \
  -r:System.Core

SOURCES=*.cs

all:
	$(MCS) $(OPTS) $(REFERENCES) $(SOURCES)

