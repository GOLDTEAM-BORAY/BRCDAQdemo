using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;

namespace BRCDAQdemo.Avalonia.Views;

public static class ErrorDialog
{
    public static Task ShowAsync(Window owner, string title, string message)
    {
        Window dialog = new()
        {
            Title = title,
            Width = 420,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new global::Avalonia.Thickness(18),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                    new Button
                    {
                        Content = "确定",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        MinWidth = 88
                    }
                }
            }
        };

        ((Button)((StackPanel)dialog.Content!).Children[1]).Click += (_, _) => dialog.Close();
        return dialog.ShowDialog(owner);
    }
}
