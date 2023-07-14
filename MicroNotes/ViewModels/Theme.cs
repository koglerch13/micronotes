using Avalonia;
using Avalonia.Media;

namespace MicroNotes.ViewModels;

public static class Theme
{
    public static Brush SidebarBackgroundColor { get; } = new SolidColorBrush(Colors.White);
    public static  Brush SplitterColor { get; } = new SolidColorBrush(Colors.LightGray);
    public static Brush ContentBackgroundColor { get; } = new SolidColorBrush(Colors.White);

    public static Brush MenuButtonIconColor { get; } = new SolidColorBrush(Colors.Black);
    public static Brush MenuButtonBackgroundColor { get; } = new SolidColorBrush(Colors.White);
    public static Brush MenuButtonIconColorHover { get; } = new SolidColorBrush(Colors.Black);
    public static Brush MenuButtonBackgroundColorHover { get; } = new SolidColorBrush(Colors.LightGray);
    public static Brush MenuButtonIconColorOpen { get; } = new SolidColorBrush(Colors.Black);
    public static Brush MenuButtonBackgroundColorOpen { get; } = new SolidColorBrush(Colors.Gray);
    
    public static Brush TitleTextColor { get; } = new SolidColorBrush(Colors.Black);
    public static Brush TitleTextSelectionBackgroundColor { get; } = new SolidColorBrush(Colors.LightBlue);
    public static Brush TitleTextSelectionColor { get; } = new SolidColorBrush(Colors.Black);
    
    public static Brush ContentTextColor { get; } = new SolidColorBrush(Colors.Black);
    public static Brush ContentTextSelectionBackgroundColor { get; } = new SolidColorBrush(Colors.LightBlue);
    public static Brush ContentTextSelectionColor { get; } = new SolidColorBrush(Colors.Black);
    
    public static Brush ListItemTextColor { get; } = new SolidColorBrush(Colors.Black);
    public static Brush ListItemBackgroundColor { get; } = new SolidColorBrush(Colors.White);
    
    public static Brush ListItemTextColorHover { get; } = new SolidColorBrush(Colors.Black);
    public static Brush ListItemBackgroundColorHover { get; } = new SolidColorBrush(Colors.LightBlue);

    public static Brush ListItemTextColorSelected { get; } = new SolidColorBrush(Colors.Black);
    public static Brush ListItemBackgroundColorSelected { get; } = new SolidColorBrush(Colors.SkyBlue);
    
    public static Thickness ListItemMargin { get; } = new Thickness(0);
    public static Thickness ListItemPadding { get; } = new Thickness(10,10);
    public static CornerRadius ListItemRadius { get; } = new CornerRadius(0);

    public static double ListItemFontSize { get; } = 16;
    public static double TitleFontSize { get; } = 20;
    public static double TextFontSize { get; } = 12;

    public static double PlaceholderFontSize { get; } = 18;
    public static Brush PlaceholderTextColor { get; } = new SolidColorBrush(Colors.Black);

    public static CornerRadius MenuButtonRadius { get; } = new CornerRadius(0);
    public static double MenuButtonHeight { get; } = 24;
    public static double MenuButtonWidth { get; } = 24;
    public static Thickness MenuButtonPadding { get; } = new Thickness(0);
    public static Thickness MenuButtonMargin { get; } = new Thickness(4,4,0,4);

    public static Thickness TitlePadding { get; } = new Thickness(8,4,0,8);
    public static Thickness TextPadding { get; } = new Thickness(8,0,8,0);
}