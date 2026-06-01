using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Coque de navigation : rail latéral des 5 destinations (doc 12 §3 / doc 14 §2) via NavigationView.
/// Le routage réel sera câblé à U4 ; ici la coque héberge le contenu via <see cref="PageContent"/>.
/// </summary>
[ContentProperty(Name = nameof(PageContent))]
public sealed partial class RailShell : UserControl
{
    public RailShell()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty PageContentProperty =
        DependencyProperty.Register(nameof(PageContent), typeof(object), typeof(RailShell), new PropertyMetadata(null));

    /// <summary>Contenu de la zone principale (contenu par défaut du contrôle).</summary>
    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }
}
