using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Atlas.App.Presentation.Controls;

/// <summary>Variantes d'état du corps de section (doc 12 §14).</summary>
public enum SectionState
{
    Loaded,
    Skeleton,
    CoverageEmpty,
    Error,
}

/// <summary>
/// Carte-section repliable de la fiche (doc 12 §13/§14). Port XAML de SectionCard.razor.
/// </summary>
[ContentProperty(Name = nameof(SectionContent))]
public sealed partial class SectionCard : UserControl
{
    private bool _expanded = true;

    public SectionCard()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            _expanded = StartExpanded;
            Refresh();
        };
    }

    /// <summary>Levé quand l'utilisateur demande à recharger une section en erreur.</summary>
    public event EventHandler? RetryRequested;

    /// <summary>Levé quand l'utilisateur (dé)épingle la section.</summary>
    public event EventHandler? PinToggled;

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(SectionCard), new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty IndicatorProperty =
        DependencyProperty.Register(nameof(Indicator), typeof(string), typeof(SectionCard), new PropertyMetadata(null, OnChanged));

    /// <summary>Compteur ou source globale affiché dans l'en-tête (optionnel).</summary>
    public string? Indicator
    {
        get => (string?)GetValue(IndicatorProperty);
        set => SetValue(IndicatorProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(string), typeof(SectionCard), new PropertyMetadata(null, OnChanged));

    /// <summary>Provenance « source · date » rendue avec les données (requise si données sourcées).</summary>
    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(SectionState), typeof(SectionCard),
            new PropertyMetadata(SectionState.Loaded, OnChanged));

    public SectionState State
    {
        get => (SectionState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty EmptyMessageProperty =
        DependencyProperty.Register(nameof(EmptyMessage), typeof(string), typeof(SectionCard), new PropertyMetadata(null, OnChanged));

    /// <summary>Message du vide-de-couverture : nomme la raison, sans appel à l'action (doc 12 §14).</summary>
    public string? EmptyMessage
    {
        get => (string?)GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(SectionCard), new PropertyMetadata(null, OnChanged));

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public static readonly DependencyProperty StartExpandedProperty =
        DependencyProperty.Register(nameof(StartExpanded), typeof(bool), typeof(SectionCard), new PropertyMetadata(true));

    /// <summary>Ouverte par défaut ? (les préférences F-062 piloteront ce défaut plus tard).</summary>
    public bool StartExpanded
    {
        get => (bool)GetValue(StartExpandedProperty);
        set => SetValue(StartExpandedProperty, value);
    }

    public static readonly DependencyProperty PinnedProperty =
        DependencyProperty.Register(nameof(Pinned), typeof(bool), typeof(SectionCard), new PropertyMetadata(false, OnChanged));

    /// <summary>Section épinglée (mise en avant par une barre d'accent, jamais la couleur seule — doc 06).</summary>
    public bool Pinned
    {
        get => (bool)GetValue(PinnedProperty);
        set => SetValue(PinnedProperty, value);
    }

    public static readonly DependencyProperty ShowPinProperty =
        DependencyProperty.Register(nameof(ShowPin), typeof(bool), typeof(SectionCard), new PropertyMetadata(false, OnChanged));

    /// <summary>Affiche le bouton d'épinglage dans l'en-tête (équivalent du delegate OnPinToggle Blazor).</summary>
    public bool ShowPin
    {
        get => (bool)GetValue(ShowPinProperty);
        set => SetValue(ShowPinProperty, value);
    }

    public static readonly DependencyProperty SectionContentProperty =
        DependencyProperty.Register(nameof(SectionContent), typeof(object), typeof(SectionCard), new PropertyMetadata(null));

    /// <summary>Contenu de la section (atomes/champs) — contenu par défaut du contrôle.</summary>
    public object? SectionContent
    {
        get => GetValue(SectionContentProperty);
        set => SetValue(SectionContentProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((SectionCard)d).Refresh();

    private void OnToggle(object sender, RoutedEventArgs e)
    {
        _expanded = !_expanded;
        Refresh();
    }

    private void OnRetry(object sender, RoutedEventArgs e) => RetryRequested?.Invoke(this, EventArgs.Empty);

    private void OnPin(object sender, RoutedEventArgs e) => PinToggled?.Invoke(this, EventArgs.Empty);

    private void Refresh()
    {
        // Repli / déploiement.
        Body.Visibility = _expanded ? Visibility.Visible : Visibility.Collapsed;
        ChevronRotation.Angle = _expanded ? 90 : 0;

        // En-tête : indicateur, épingle.
        bool hasIndicator = !string.IsNullOrWhiteSpace(Indicator);
        IndicatorChip.Visibility = hasIndicator ? Visibility.Visible : Visibility.Collapsed;
        IndicatorText.Text = Indicator ?? string.Empty;
        PinButton.Visibility = ShowPin ? Visibility.Visible : Visibility.Collapsed;
        PinAccent.Visibility = Pinned ? Visibility.Visible : Visibility.Collapsed;

        // Corps : un seul état visible.
        LoadedView.Visibility = State == SectionState.Loaded ? Visibility.Visible : Visibility.Collapsed;
        SkeletonView.Visibility = State == SectionState.Skeleton ? Visibility.Visible : Visibility.Collapsed;
        CoverageView.Visibility = State == SectionState.CoverageEmpty ? Visibility.Visible : Visibility.Collapsed;
        ErrorView.Visibility = State == SectionState.Error ? Visibility.Visible : Visibility.Collapsed;

        bool hasSource = !string.IsNullOrWhiteSpace(Source);
        BodyProvenance.Visibility = hasSource ? Visibility.Visible : Visibility.Collapsed;
        if (hasSource)
        {
            BodyProvenance.Source = Source!;
        }

        CoverageView.Text = EmptyMessage ?? string.Empty;
        ErrorText.Text = ErrorMessage ?? string.Empty;
    }
}
