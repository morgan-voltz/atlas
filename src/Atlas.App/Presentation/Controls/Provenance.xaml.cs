using System;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Atome provenance (doc 12 §4/§13). Port XAML de Provenance.razor : « Source : {source} · {date} ».
/// </summary>
public sealed partial class Provenance : UserControl
{
    private static readonly CultureInfo Culture = new("fr-FR");

    public Provenance()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => Refresh();
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(string), typeof(Provenance),
            new PropertyMetadata(string.Empty, OnChanged));

    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty DateProperty =
        DependencyProperty.Register(nameof(Date), typeof(DateTimeOffset?), typeof(Provenance),
            new PropertyMetadata(null, OnChanged));

    /// <summary>Date de fraîcheur optionnelle, rendue « · {date} » après la source.</summary>
    public DateTimeOffset? Date
    {
        get => (DateTimeOffset?)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public static readonly DependencyProperty SeparatedProperty =
        DependencyProperty.Register(nameof(Separated), typeof(bool), typeof(Provenance),
            new PropertyMetadata(false, OnChanged));

    /// <summary>Sépare la provenance du bloc au-dessus (filet + marge) ; pour un en-tête de fiche.</summary>
    public bool Separated
    {
        get => (bool)GetValue(SeparatedProperty);
        set => SetValue(SeparatedProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Provenance)d).Refresh();

    private void Refresh()
    {
        string suffix = Date is { } d ? $" · {d.ToString("d MMM yyyy", Culture)}" : string.Empty;
        Text.Text = $"Source : {Source}{suffix}";
        Separator.Visibility = Separated ? Visibility.Visible : Visibility.Collapsed;
    }
}
