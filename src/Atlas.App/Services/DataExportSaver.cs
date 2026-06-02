using System.Threading.Tasks;

namespace Atlas.App.Services;

/// <summary>
/// Remet l'archive d'export RGPD (art. 20) à l'utilisateur, par tête : téléchargement navigateur sur la
/// tête WebAssembly (data-URL base64, l'archive ne quitte pas le navigateur — parité avec le client
/// Blazor) ; sélecteur d'enregistrement natif sur les têtes desktop. Renvoie <c>true</c> si l'archive a
/// été remise/enregistrée, <c>false</c> si l'utilisateur a annulé (ou en cas d'échec).
/// </summary>
internal static class DataExportSaver
{
    private const string FileName = "atlas-mes-donnees.json";

#if __WASM__
    public static Task<bool> SaveAsync(string json)
    {
        // base64 sûr à interpoler dans un littéral JS (alphabet [A-Za-z0-9+/=], aucune injection).
        string base64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        string script =
            "(function(){var a=document.createElement('a');" +
            $"a.href='data:application/json;base64,{base64}';" +
            $"a.download='{FileName}';" +
            "document.body.appendChild(a);a.click();a.remove();})();";
        Uno.Foundation.WebAssemblyRuntime.InvokeJS(script);
        return Task.FromResult(true);
    }
#else
    public static async Task<bool> SaveAsync(string json)
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads,
            SuggestedFileName = "atlas-mes-donnees",
        };
        picker.FileTypeChoices.Add("Fichier JSON", new System.Collections.Generic.List<string> { ".json" });

        // Têtes natives : le sélecteur doit être rattaché à la fenêtre courante.
        if (App.WindowInstance is { } window)
        {
            nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }

        Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return false;
        }

        await Windows.Storage.FileIO.WriteTextAsync(file, json);
        return true;
    }
#endif
}
