using Avalonia;
using Avalonia.Media;

namespace MicroNotes;

public static class ActiveTheme
{
    public static Brush SidebarBackgroundColor { get; } = new SolidColorBrush(Color.Parse("#2f3137"));
    public static  Brush SplitterColor { get; } = new SolidColorBrush(Color.Parse("#2f3137"));
    public static Brush ContentBackgroundColor { get; } = new SolidColorBrush(Color.Parse("#fefffe"));
    
    public static Brush TitleTextColor { get; } = new SolidColorBrush(Color.Parse("#4b4a4a"));
    public static Brush TitleTextSelectionBackgroundColor { get; } = new SolidColorBrush(Color.Parse("#ca7d7f"));
    public static Brush TitleTextSelectionColor { get; } = new SolidColorBrush(Color.Parse("#fefffe"));
    public static Thickness TitlePadding { get; } = new Thickness(28,20,0,8);   
    public static double TitleFontSize { get; } = 28;
    
    public static Brush ContentTextColor { get; } = new SolidColorBrush(Color.Parse("#4b4a4a"));
    public static Brush ContentTextSelectionBackgroundColor { get; } = new SolidColorBrush(Color.Parse("#ca7d7f"));
    public static Brush ContentTextSelectionColor { get; } = new SolidColorBrush(Color.Parse("#fefffe"));
    public static Thickness TextPadding { get; } = new Thickness(28,0,28,0);
    public static double TextFontSize { get; } = 17;
    
    public static Brush ListItemTextColor { get; } = new SolidColorBrush(Color.Parse("#d1d2d4"));
    public static Brush ListItemBackgroundColor { get; } = new SolidColorBrush(Color.Parse("#2f3137"));
    public static Brush ListItemTextColorHover { get; } = new SolidColorBrush(Color.Parse("#e1e1e1"));
    public static Brush ListItemBackgroundColorHover { get; } = new SolidColorBrush(Color.Parse("#464746"));
    public static Brush ListItemTextColorSelected { get; } = new SolidColorBrush(Color.Parse("#e1e1e1"));
    public static Brush ListItemBackgroundColorSelected { get; } = new SolidColorBrush(Color.Parse("#5a5b5a"));
    public static Thickness ListItemMargin { get; } = new Thickness(8,8,8,0);
    public static Thickness ListItemPadding { get; } = new Thickness(8,8);
    public static CornerRadius ListItemRadius { get; } = new CornerRadius(4);
    public static Thickness ListItemBorder { get; } = new Thickness(0);
    public static Brush ListItemBorderColor { get; } = new SolidColorBrush();
    public static Thickness ListItemBorderHover { get; } = new Thickness(0);
    public static Brush ListItemBorderColorHover { get; } = new SolidColorBrush();
    public static Thickness ListItemBorderSelected { get; } = new Thickness(4,0,0,0);
    public static Brush ListItemBorderColorSelected { get; } = new SolidColorBrush(Color.Parse("#d54e50"));
    public static double ListItemFontSize { get; } = 15;
    
    public static Brush MenuButtonIconColor { get; } = new SolidColorBrush(Color.Parse("#d1d2d4"));
    public static Brush MenuButtonBackgroundColor { get; } = new SolidColorBrush(Color.Parse("#2f3137"));
    public static Brush MenuButtonIconColorHover { get; } = new SolidColorBrush(Color.Parse("#e1e1e1"));
    public static Brush MenuButtonBackgroundColorHover { get; } = new SolidColorBrush(Color.Parse("#464746"));
    public static Brush MenuButtonIconColorOpen { get; } = new SolidColorBrush(Color.Parse("#e1e1e1"));
    public static Brush MenuButtonBackgroundColorOpen { get; } = new SolidColorBrush(Color.Parse("#5a5b5a"));
    public static CornerRadius MenuButtonRadius { get; } = new CornerRadius(14);
    public static double MenuButtonHeight { get; } = 28;
    public static double MenuButtonWidth { get; } = 28;
    public static Thickness MenuButtonPadding { get; } = new Thickness(4);
    public static Thickness MenuButtonMargin { get; } = new Thickness(4,4,0,8);
    
    public static double PlaceholderFontSize { get; } = 18;
    public static Brush PlaceholderTextColor { get; } = new SolidColorBrush(Color.Parse("#4b4a4a"));
}