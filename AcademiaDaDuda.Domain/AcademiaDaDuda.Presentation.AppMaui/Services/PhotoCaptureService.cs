using Microsoft.Maui.Storage;

namespace AcademiaDaDuda.Presentation.AppMaui.Services;

internal static class PhotoCaptureService
{
    public static async Task<FileResult?> CapturePhotoAsync()
    {
#if WINDOWS
        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        if (mauiWindow?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
            throw new InvalidOperationException("Não foi possível localizar a janela ativa para acessar a câmera.");

        var cameras = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
            Windows.Devices.Enumeration.DeviceClass.VideoCapture);
        if (cameras.Count == 0)
            throw new InvalidOperationException("O Windows não encontrou uma câmera conectada.");

        using var capture = new Windows.Media.Capture.MediaCapture();
        try
        {
            await capture.InitializeAsync(new Windows.Media.Capture.MediaCaptureInitializationSettings
            {
                VideoDeviceId = cameras[0].Id,
                StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Video
            });
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new InvalidOperationException(
                "O acesso à câmera está bloqueado. Habilite o acesso para aplicativos da área de trabalho em Configurações do Windows > Privacidade e segurança > Câmera.",
                exception);
        }

        var frameSource = capture.FrameSources.Values.FirstOrDefault(source =>
                source.Info.MediaStreamType == Windows.Media.Capture.MediaStreamType.VideoPreview &&
                source.Info.SourceKind == Windows.Media.Capture.Frames.MediaFrameSourceKind.Color)
            ?? capture.FrameSources.Values.FirstOrDefault(source =>
                source.Info.MediaStreamType == Windows.Media.Capture.MediaStreamType.VideoRecord &&
                source.Info.SourceKind == Windows.Media.Capture.Frames.MediaFrameSourceKind.Color);
        if (frameSource is null)
            throw new InvalidOperationException("A câmera conectou, mas não disponibilizou uma prévia de vídeo.");

        var xamlRoot = nativeWindow.Content?.XamlRoot;
        if (xamlRoot is null)
            throw new InvalidOperationException("Não foi possível abrir a prévia da câmera nesta janela.");

        var preview = new Microsoft.UI.Xaml.Controls.MediaPlayerElement
        {
            AutoPlay = false,
            AreTransportControlsEnabled = false,
            Height = 320
        };
        var previewSource = Windows.Media.Core.MediaSource.CreateFromMediaFrameSource(frameSource);

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Tirar foto",
            Content = preview,
            PrimaryButtonText = "Capturar",
            CloseButtonText = "Cancelar",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };
        dialog.Opened += (_, _) =>
        {
            preview.Source = previewSource;
            preview.MediaPlayer.RealTimePlayback = true;
            preview.MediaPlayer.Play();
        };

        try
        {
            if (await dialog.ShowAsync() != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
                return null;

            var cacheFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(FileSystem.CacheDirectory);
            var photo = await cacheFolder.CreateFileAsync(
                $"foto-{Guid.NewGuid():N}.jpg",
                Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            try
            {
                await capture.CapturePhotoToStorageFileAsync(
                    Windows.Media.MediaProperties.ImageEncodingProperties.CreateJpeg(), photo);
            }
            catch
            {
                await photo.DeleteAsync();
                throw;
            }

            return new FileResult(photo.Path);
        }
        finally
        {
            preview.MediaPlayer?.Pause();
            preview.Source = null;
            previewSource.Dispose();
        }
#else
        return await MediaPicker.Default.CapturePhotoAsync();
#endif
    }
}
