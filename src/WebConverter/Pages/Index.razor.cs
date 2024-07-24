using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using PDFtoImage.WebConverter.Models;
using SkiaSharp;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Thinktecture.Blazor.WebShare.Models;

namespace PDFtoImage.WebConverter.Pages
{
    public partial class Index : IDisposable
    {
        private DotNetObjectReference<Index>? _objRef;

        public RenderRequest Model { get; set; } = new();

        public bool IsWebShareSupported { get; private set; } = false;

        public bool IsLoading { get; private set; }

        public Exception? LastException { get; private set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SetupDotNetHelper();
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnInitializedAsync()
        {
            Program.FilesHandled -= OnFilesHandled;
            Program.FilesHandled += OnFilesHandled;

            IsWebShareSupported = await WebShareService.IsSupportedAsync();

            await base.OnInitializedAsync();
        }

        private async Task SetupDotNetHelper()
        {
            _objRef = DotNetObjectReference.Create(this);
            await JS.InvokeAsync<string>("setDotNetHelper", _objRef);
        }

        private async void OnFilesHandled(object? sender, Program.HandledFileEventArgs args)
        {
            if (args.File == null)
                return;

            SetFile(
                new DummyFile(
                    await args.File.GetNameAsync(),
                    await args.File.GetLastModifiedAsync(),
                    (long)await args.File.GetSizeAsync(),
                    await args.File.GetTypeAsync()
                ),
                new MemoryStream(await args.File.ArrayBufferAsync())
            );
        }

        private void SetFile(DummyFile file, Stream stream)
        {
            if (file == null)
                return;

            Logger.LogInformation("Handle file {FileName}.", file.Name);

            Model.File = file;
            Model.Input = stream;

            StateHasChanged();
        }

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            Model.File = e.File;
            Model.Input?.Dispose();
            Model.Input = null;
            Model.Output?.Dispose();
            Model.Output = null;
            StateHasChanged();

            await JS.InvokeVoidAsync("resetImage", "outputImage");
        }

        private async Task SetImage()
        {
            if (Model.Output == null)
            {
                await JS.InvokeVoidAsync("resetImage", "outputImage");
                return;
            }

            Model.Output.Position = 0;
            using var fs = new MemoryStream();
            await Model.Output.CopyToAsync(fs);
            fs.Position = 0;

            using var streamRef = new DotNetStreamReference(fs);
            await JS.InvokeVoidAsync("setImage", "outputImage", RenderRequest.GetMimeType(Model.Format), streamRef);
        }

        private async Task Reset()
        {
            Model.Dispose();
            Model = new();
            LastException = null;
            await JS.InvokeVoidAsync("resetImage", "outputImage");
        }

        private const long MaxAllowedSize = 250 * 1000 * 1000;

        private async Task Submit()
        {
            Logger.LogInformation("Converting {Model}.", Model);
            SKBitmap? bitmap = null;

            try
            {
                IsLoading = true;
                LastException = null;

                Model.Output?.Dispose();
                Model.Output = new MemoryStream();

                if (Model.Input == null)
                {
                    Model.Input = new MemoryStream();
                    await Model.File!.OpenReadStream(MaxAllowedSize).CopyToAsync(Model.Input);
                }

                Model.Input.Position = 0;
                bool encodeSuccess = false;
                PdfAntiAliasing antiAliasing = default;
                var backgroundColor = SKColors.White;

                if (Model.AntiAliasingText)
                    antiAliasing |= PdfAntiAliasing.Text;
                if (Model.AntiAliasingImages)
                    antiAliasing |= PdfAntiAliasing.Images;
                if (Model.AntiAliasingPaths)
                    antiAliasing |= PdfAntiAliasing.Paths;

                if (Model.BackgroundColor != null && Model.BackgroundColor.StartsWith('#') && uint.TryParse(Model.BackgroundColor[1..], NumberStyles.HexNumber, null, out var parsed))
                {
                    parsed += (uint)(Model.Opacity << 24);
                    backgroundColor = (SKColor)parsed;
                }

                await Task.Factory.StartNew(() =>
                {
                    if (!Model.UseBounds)
                    {
                        var pageSize = PDFtoImage.Conversion.GetPageSize(Model.Input, true, Model.Page, !string.IsNullOrEmpty(Model.Password) ? Model.Password : null);
                        Model.BoundsWidth = pageSize.Width;
                        Model.BoundsHeight = pageSize.Height;
                    }

                    bitmap = PDFtoImage.Conversion.ToImage(
                        Model.Input,
                        leaveOpen: true,
                        password: !string.IsNullOrEmpty(Model.Password) ? Model.Password : null,
                        page: Model.Page,
                        new RenderOptions(
                            Dpi: Model.Dpi,
                            Width: Model.Width,
                            Height: Model.Height,
                            WithAnnotations: Model.WithAnnotations,
                            WithFormFill: Model.WithFormFill,
                            WithAspectRatio: Model.WithAspectRatio,
                            Rotation: Model.Rotation,
                            AntiAliasing: antiAliasing,
                            BackgroundColor: backgroundColor,
                            Bounds: Model.UseBounds ? new RectangleF(Model.BoundsX, Model.BoundsY, Model.BoundsWidth, Model.BoundsHeight) : null,
                            UseTiling: Model.UseTiling,
                            DpiRelativeToBounds: Model.DpiRelativeToBounds
                        )
                    );
                    encodeSuccess = bitmap!.Encode(Model.Output, Model.Format, Model.Quality);
                }, TaskCreationOptions.LongRunning);

                if (!encodeSuccess)
                {
                    Model.Output?.Dispose();
                    Model.Output = null;
                }

                await SetImage();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to convert {Model}.", Model);
                LastException = ex;
            }
            finally
            {
                bitmap?.Dispose();
                IsLoading = false;
            }
        }

        private async Task DownloadImage()
        {
            if (Model.Output == null)
                return;

            try
            {
                Model.Output.Position = 0;
                using var fs = new MemoryStream();
                await Model.Output.CopyToAsync(fs);
                fs.Position = 0;

                using var streamRef = new DotNetStreamReference(fs);
                await JS.InvokeVoidAsync("downloadFileFromStream", RenderRequest.GetOutputFileName(Model), RenderRequest.GetMimeType(Model.Format), streamRef);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to download {Model}.", Model);
            }
        }

        private async Task ShareImage()
        {
            if (Model.Output == null)
                return;

            try
            {
                Model.Output.Position = 0;
                using var fs = new MemoryStream();
                await Model.Output.CopyToAsync(fs);
                fs.Position = 0;

                using var streamRef = new DotNetStreamReference(fs);

                var file = await JS.InvokeAsync<IJSObjectReference>("createFileFromStream", RenderRequest.GetOutputFileName(Model), RenderRequest.GetMimeType(Model.Format), streamRef);
                var data = new WebShareDataModel
                {
                    Files = [file]
                };

                if (!await WebShareService.CanShareAsync(data))
                {
                    Logger.LogWarning("Cannot web share {Model}.", Model);
                    return;
                }

                await WebShareService.ShareAsync(data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to web share {Model}.", Model);
            }
        }

        [JSInvokable]
        public async Task ReceiveWebShareTargetAsync(string filesStringyfied)
        {
            try
            {
                var converted = JsonSerializer.Deserialize<FilesStringyfied>(filesStringyfied);
                var file = converted?.Files?.FirstOrDefault();

                if (file == null)
                    return;

                var data = Convert.FromBase64String(file.GetData());
                var ms = new MemoryStream(data.Length);
                await ms.WriteAsync(data);

                SetFile(
                    new DummyFile(
                        file.Name,
                        file.LastModified,
                        file.Size,
                        file.Type
                    ), ms);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to receive web share {FilesStringyfied}.", filesStringyfied);
            }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _objRef?.Dispose();
                    _objRef = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}