global using Microsoft.Maui.Controls;
global using Microsoft.Maui.ApplicationModel;

// These two compile-only platform stand-ins let the real ViewModels execute on
// plain .NET. They do not render, decode images, simulate a device, or replace
// application logic. Device rendering and camera permissions need device checks.
namespace Microsoft.Maui.Controls
{
    public sealed class ImageSource
    {
        public static ImageSource FromStream(Func<Stream> source) => new();
    }
}

namespace Microsoft.Maui.ApplicationModel
{
    public sealed class PermissionException(string message) : Exception(message);
}
