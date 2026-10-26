// IL2CPP resolves [DllImport("pdfium")] and [DllImport("libSkiaSharp")] by probing for dylib files.
// iOS apps must embed native libraries as frameworks instead, so redirect both names to the embedded frameworks.
#include <string.h>

typedef const char* (*Il2CppSetFindPlugInCallback)(const char*);
extern "C" void il2cpp_set_find_plugin_callback(Il2CppSetFindPlugInCallback method);

static const char* PDFtoImage_FindPlugin(const char* libraryName)
{
    if (libraryName == NULL)
        return libraryName;

    if (strcmp(libraryName, "pdfium") == 0)
        return "@rpath/pdfium.framework/pdfium";

    if (strcmp(libraryName, "libSkiaSharp") == 0)
        return "@rpath/libSkiaSharp.framework/libSkiaSharp";

    return libraryName;
}

__attribute__((constructor))
static void PDFtoImage_RegisterFindPlugin()
{
    il2cpp_set_find_plugin_callback(PDFtoImage_FindPlugin);
}
