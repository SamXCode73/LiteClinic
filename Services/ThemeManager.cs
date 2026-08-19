using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;


namespace LiteClinic.Services
{
    internal partial class ThemeManager

    {
        public static event Action<string>? ThemeChanged;

        private static string? currentTheme = "Light";
        public static string CurrentTheme
        {
            get => currentTheme!;
            set
            {
                if (currentTheme != value)
                {
                    currentTheme = value;
                    ApplyTheme(currentTheme); // Apply the theme here
                    ThemeChanged?.Invoke(currentTheme);
                }
            }
        }

        public static void ApplyTheme(string themeName)
        {
            // Example: switch theme resources
            switch (themeName)
            {
                // Light Theme with gradients
                case "Light":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var lightTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 28, 42, 58), Offset = 0.0 }, // Darker start
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 47, 62, 77), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 70, 85, 100), Offset = 1.0 } // Lighter end
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = lightTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var lightSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 47, 62, 77), Offset = 0.0 }, // Darker top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 63, 78, 93), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 95, 110, 125), Offset = 1.0 } // Lighter bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = lightSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    break;
                // Dark Theme with gradients
                case "Dark":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var darkTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 18, 18, 18), Offset = 0.0 }, // Darker start
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 30, 30, 30), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 42, 42, 42), Offset = 1.0 }  // Lighter end
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = darkTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var darkSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 46, 46, 46), Offset = 0.0 }, // Slightly lighter top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 60, 60, 60), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 74, 74, 74), Offset = 1.0 }  // Lighter bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = darkSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 224, 224, 224));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 204, 204, 204));
                    break;
                case "Pink":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var pinkTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 204, 90, 158), Offset = 0.0 }, // Slightly darker pink
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 105, 180), Offset = 0.5 }, // HotPink mid
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 133, 193), Offset = 1.0 }  // Softer light pink
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = pinkTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var pinkSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 133, 193), Offset = 0.0 }, // LightPink top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 182, 193), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 198, 204), Offset = 1.0 }  // Very soft pink bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = pinkSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "RoyalBlue":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var royalBlueTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 53, 89, 230), Offset = 0.0 }, // Darker royal blue
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 65, 105, 255), Offset = 0.5 }, // Base RoyalBlue
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 90, 127, 255), Offset = 1.0 }  // Softer lighter blue
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = royalBlueTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var royalBlueSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 100, 149, 237), Offset = 0.0 }, // CornflowerBlue top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 115, 159, 239), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 136, 174, 242), Offset = 1.0 }  // Lighter bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = royalBlueSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "Teal":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var tealTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 0, 102, 102), Offset = 0.0 }, // Darker teal
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 0, 128, 128), Offset = 0.5 }, // Base teal
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 51, 153, 153), Offset = 1.0 }  // Softer lighter teal
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = tealTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var tealSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 74, 127, 127), Offset = 0.0 }, // Light teal top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 95, 154, 154), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 117, 173, 173), Offset = 1.0 }  // Very soft teal bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = tealSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "Violet":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var violetTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 122, 0, 179), Offset = 0.0 }, // Darker violet
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 148, 0, 211), Offset = 0.5 }, // Base violet
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 166, 77, 218), Offset = 1.0 }  // Softer lighter violet
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = violetTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var violetSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 175, 160, 198), Offset = 0.0 }, // Light lavender top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 198, 191, 224), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 210, 204, 230), Offset = 1.0 }  // Very soft violet bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = violetSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "MintGreen":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var mintTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 112, 204, 112), Offset = 0.0 }, // Darker mint
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 152, 255, 152), Offset = 0.5 }, // Base mint green
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 178, 255, 178), Offset = 1.0 }  // Softer lighter mint
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = mintTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var mintSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 158, 204, 158), Offset = 0.0 }, // Light mint top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 185, 220, 185), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 194, 220, 194), Offset = 1.0 }  // Very soft mint bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = mintSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "Coral":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var coralTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 229, 83, 58), Offset = 0.0 }, // Darker coral
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 99, 71), Offset = 0.5 }, // Base coral
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 127, 102), Offset = 1.0 } // Softer lighter coral
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = coralTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var coralSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 179, 140), Offset = 0.0 }, // Light coral/peach top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 194, 163), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 255, 209, 186), Offset = 1.0 }  // Very soft peach bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = coralSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;

                case "Lavender":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var lavenderTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 126, 102, 160), Offset = 0.0 }, // Darker lavender
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 150, 123, 182), Offset = 0.5 }, // Base lavender
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 178, 159, 208), Offset = 1.0 }  // Softer lighter lavender
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = lavenderTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var lavenderSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 184, 184, 200), Offset = 0.0 }, // Lavender top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 201, 201, 213), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 209, 209, 233), Offset = 1.0 }  // Very soft lavender bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = lavenderSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "Sandstone":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var sandstoneTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 168, 144, 96), Offset = 0.0 }, // Darker sandstone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 194, 178, 128), Offset = 0.5 }, // Base sandstone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 214, 200, 154), Offset = 1.0 }  // Softer lighter sandstone
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = sandstoneTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var sandstoneSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 222, 202, 160), Offset = 0.0 }, // Light sandy beige top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 230, 213, 179), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 238, 224, 198), Offset = 1.0 }  // Very soft sandstone bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = sandstoneSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
                case "Monochrome":
                    // Top bar gradient (diagonal: top-left → bottom-right)
                    var monoTopBarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(1, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 48, 48, 48), Offset = 0.0 }, // Darker gray
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 64, 64, 64), Offset = 0.5 }, // Base gray
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 80, 80, 80), Offset = 1.0 }  // Lighter gray
                                }
                    };
                    Application.Current.Resources["TopBarBackgroundBrush"] = monoTopBarGradient;

                    // Sidebar gradient (vertical: top → bottom)
                    var monoSidebarGradient = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops =
                                {
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 200, 200, 200), Offset = 0.0 }, // Light gray top
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 216, 216, 216), Offset = 0.5 }, // Mid tone
                                    new GradientStop { Color = ColorHelper.FromArgb(255, 232, 232, 232), Offset = 1.0 }  // Very soft gray bottom
                                }
                    };
                    Application.Current.Resources["SideBarBackgroundBrush"] = monoSidebarGradient;

                    Application.Current.Resources["TopBarForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["MenuLabelForegroundBrush"] =
                        new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 51));
                    break;
            }
        }


        public static string GetThemedImagePath(string imageName)
        {
            // Themes that use the "light" folder
            var lightThemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Light", "Dark", "RoyalBlue" // Add any others that should use light images
                };

            // Default to "dark" unless explicitly listed as light
            var folder = lightThemes.Contains(CurrentTheme) ? "light" : "dark";

            return $"ms-appx:///Assets/images/{folder}/{imageName}";
        }
    }
}
