using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// List-detail à deux panneaux (doc 14 §3). Port XAML du patron Blazor. Layout piloté en code
/// (largeur ≥ 880 px → deux panneaux ; sinon empilement liste/détail avec retour).
/// </summary>
[ContentProperty(Name = nameof(ListContent))]
public sealed partial class ListDetailView : UserControl
{
    private const double TwoPaneThreshold = 880;
    private const double ListPaneWidth = 352;

    public ListDetailView()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => ApplyLayout();
    }

    /// <summary>Levé quand l'utilisateur revient à la liste (format réduit).</summary>
    public event EventHandler? BackRequested;

    public static readonly DependencyProperty ListContentProperty =
        DependencyProperty.Register(nameof(ListContent), typeof(object), typeof(ListDetailView), new PropertyMetadata(null));

    /// <summary>Panneau liste (contenu par défaut du contrôle).</summary>
    public object? ListContent
    {
        get => GetValue(ListContentProperty);
        set => SetValue(ListContentProperty, value);
    }

    public static readonly DependencyProperty DetailContentProperty =
        DependencyProperty.Register(nameof(DetailContent), typeof(object), typeof(ListDetailView), new PropertyMetadata(null));

    /// <summary>Panneau détail (élément sélectionné).</summary>
    public object? DetailContent
    {
        get => GetValue(DetailContentProperty);
        set => SetValue(DetailContentProperty, value);
    }

    public static readonly DependencyProperty HasSelectionProperty =
        DependencyProperty.Register(nameof(HasSelection), typeof(bool), typeof(ListDetailView),
            new PropertyMetadata(false, OnLayoutAffectingChanged));

    /// <summary>Un élément est sélectionné (le détail est affiché).</summary>
    public bool HasSelection
    {
        get => (bool)GetValue(HasSelectionProperty);
        set => SetValue(HasSelectionProperty, value);
    }

    public static readonly DependencyProperty BackLabelProperty =
        DependencyProperty.Register(nameof(BackLabel), typeof(string), typeof(ListDetailView), new PropertyMetadata("← Retour"));

    public string BackLabel
    {
        get => (string)GetValue(BackLabelProperty);
        set => SetValue(BackLabelProperty, value);
    }

    private static void OnLayoutAffectingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((ListDetailView)d).ApplyLayout();

    private void OnRootSizeChanged(object sender, SizeChangedEventArgs e) => ApplyLayout();

    private void OnBack(object sender, RoutedEventArgs e) => BackRequested?.Invoke(this, EventArgs.Empty);

    private void ApplyLayout()
    {
        bool wide = Root.ActualWidth >= TwoPaneThreshold;

        if (wide)
        {
            // Deux panneaux côte à côte : liste fixe + détail fluide.
            Root.ColumnSpacing = 24;
            ListColumn.Width = new GridLength(ListPaneWidth);
            DetailColumn.Width = new GridLength(1, GridUnitType.Star);
            ListHost.Visibility = Visibility.Visible;
            DetailHost.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Collapsed;
            return;
        }

        // Empilement temporel : liste OU détail.
        Root.ColumnSpacing = 0;
        if (HasSelection)
        {
            ListColumn.Width = new GridLength(0);
            DetailColumn.Width = new GridLength(1, GridUnitType.Star);
            ListHost.Visibility = Visibility.Collapsed;
            DetailHost.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
        }
        else
        {
            ListColumn.Width = new GridLength(1, GridUnitType.Star);
            DetailColumn.Width = new GridLength(0);
            ListHost.Visibility = Visibility.Visible;
            DetailHost.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
        }
    }
}
