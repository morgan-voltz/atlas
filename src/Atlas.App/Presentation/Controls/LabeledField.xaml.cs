using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Atome champ étiqueté (doc 12 §11). Port XAML de LabeledField.razor. Usage :
/// <c>&lt;c:LabeledField Label="Forme juridique"&gt;SA&lt;/c:LabeledField&gt;</c>.
/// </summary>
[ContentProperty(Name = nameof(FieldContent))]
public sealed partial class LabeledField : UserControl
{
    public LabeledField()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledField), new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty FieldContentProperty =
        DependencyProperty.Register(nameof(FieldContent), typeof(object), typeof(LabeledField), new PropertyMetadata(null));

    /// <summary>Valeur du champ (contenu par défaut du contrôle).</summary>
    public object? FieldContent
    {
        get => GetValue(FieldContentProperty);
        set => SetValue(FieldContentProperty, value);
    }
}
