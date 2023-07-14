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
    public static Brush ListItemBackgroundColorHover { get; } = new SolidColorBrush(Colors.LightGray);

    public static Brush ListItemTextColorSelected { get; } = new SolidColorBrush(Colors.Black);
    public static Brush ListItemBackgroundColorSelected { get; } = new SolidColorBrush(Colors.Gray);
}