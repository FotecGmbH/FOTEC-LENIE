// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Interfaces;
using Biss.Apps.Model;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Interfaces;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Plugin.Media.Abstractions;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Bild speichern Request</para>
    ///     Klasse ImageSaveRequest. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ImageSaveRequest : EventArgs, IBissModel
    {
        #region Properties

        /// <summary>
        ///     Dateityp - ".png", ".jpg" oder ".bmp"
        /// </summary>
        public string FileType { get; set; } = ".png";

        /// <summary>
        ///     Höhe
        /// </summary>
        public double? Height { get; set; }

        /// <summary>
        ///     Breite
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        ///     Croppen forcen, damit die Dimensionen sicher passen
        /// </summary>
        public bool ForceCropping { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }

    /// <summary>
    ///     <para>Bild bearbeiten Request</para>
    ///     Klasse ImageEditRequest. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ImageEditRequest : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Datei zum bearbeiten
        /// </summary>
        public ExFile File { get; set; } = null!;

        /// <summary>
        ///     Für Profilbild
        /// </summary>
        public bool IsProfilePicture { get; set; }

        /// <summary>
        ///     Bild für Idee
        /// </summary>
        public bool IsIdeaPicture { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }

    /// <summary>
    ///     <para>Bild bearbeiten per Toolbar Request</para>
    ///     Klasse ImageToolbarRequest. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ImageToolbarRequest : EventArgs, IBissModel
    {
        #region Properties

        /// <summary>
        ///     Croppen starten
        /// </summary>
        public bool StartCrop { get; set; }

        /// <summary>
        ///     Aktuelles bearbeiten beenden
        /// </summary>
        public bool CancelEdit { get; set; }

        /// <summary>
        ///     Aktuelles bearbeiten finalisieren
        /// </summary>
        public bool FinishEdit { get; set; }

        /// <summary>
        ///     nach rechts drehen
        /// </summary>
        public bool RotateRight { get; set; }

        /// <summary>
        ///     nach links drehen
        /// </summary>
        public bool RotateLeft { get; set; }

        /// <summary>
        ///     Alles zurücksetzen
        /// </summary>
        public bool ResetAll { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }

    /// <summary>
    ///     <para>Comparer für Image</para>
    ///     Klasse ImageCompareJson. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ImageCompareJson : IBissSerialize
    {
        /// <summary>
        ///     Image Compare Json Class.
        /// </summary>
        public ImageCompareJson()
        {
        }

        /// <summary>
        ///     Image Compare Json Class.
        /// </summary>
        /// <param name="img"></param>
        /// <param name="edited"></param>
        public ImageCompareJson(byte[] img, bool edited)
        {
            Image = img;
            Edited = edited;
        }

        #region Properties

        /// <summary>
        ///     Bild
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Image { get; set; } = Array.Empty<byte>();
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        ///     Editiert
        /// </summary>
        public bool Edited { get; set; }

        #endregion
    }

    /// <summary>
    ///     <para>Bild bearbeiten</para>
    ///     Klasse VmImageEditor. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewImageEditor")]
    public class VmImageEditor : VmProjectBase
    {
        /// <summary>
        ///     Max  Filesize - 10 MB
        /// </summary>
        private const int MaxImageSize = 10 * 1024 * 1024;

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmImageEditor.DesignInstance}"
        /// </summary>
        public static VmImageEditor DesignInstance = new VmImageEditor();

        /// <summary>
        ///     VmImageEditor
        /// </summary>
        public VmImageEditor() : base("Bild bearbeiten")
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Request
        /// </summary>
        public ImageEditRequest EditRequest { get; set; } = null!;

        /// <summary>
        ///     Imagesource für Syncfusion
        /// </summary>
#pragma warning disable CA1819
        public byte[] ImageSource { get; set; } = null!;
#pragma warning restore CA1819

        /// <summary>
        ///     Bild wurde im UI bearbeitet -> SaveCheck
        /// </summary>
        public bool ImageEdited { get; set; }

        /// <summary>
        ///     Ist gerade beim zuschneiden
        /// </summary>
        public bool IsCropping { get; set; }

        /// <summary>
        ///     Hat Editor im UI
        /// </summary>
        public bool HasCustomEditor => DeviceInfo.Plattform == EnumPlattform.XamarinAndroid || DeviceInfo.Plattform == EnumPlattform.XamarinIos;

        #endregion

        /// <summary>
        ///     Ereignis für Bild speichern
        /// </summary>
        public event EventHandler<ImageSaveRequest>? SaveImage;

        /// <summary>
        ///     Ereignis für Action von Toolbar
        /// </summary>
        public event EventHandler<ImageToolbarRequest>? ToolbarRequest;

        /// <summary>
        ///     Speichern finalisieren
        /// </summary>
        /// <param name="success"></param>
        public void FinishSaving(bool success)
        {
            if (success)
            {
                var tmp = Files.GetTempFileName(".png");
                Files.WriteAllBytes(tmp, ImageSource);

                var fi = new FileInfo(tmp);
                ViewResult = new ExFile
                             {
                                 Id = -2,
                                 DownloadLink = tmp,
                                 Bytes = ImageSource,
                                 Name = fi.Name,
                                 Type = fi.Extension,
                             };
                CheckSaveBehavior = null;
                Nav.Back();
            }

            View.BusyClear();
        }

        /// <summary>
        ///     Methode von Ereignis für Action von Toolbar
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void OnToolbarRequest(ImageToolbarRequest eventData)
        {
            var handler = ToolbarRequest;
            handler?.Invoke(this, eventData);
        }

        /// <summary>
        ///     Methode von Ereignis für Bild speichern
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void OnSaveImage(ImageSaveRequest eventData)
        {
            var handler = SaveImage;
            handler?.Invoke(this, eventData);
        }

        private async Task<byte[]?> GetBytes(string? downloadLink)
        {
            if (string.IsNullOrWhiteSpace(downloadLink))
            {
                return null;
            }

            try
            {
                var http = new HttpClient();
                var res = await http.GetByteArrayAsync(downloadLink).ConfigureAwait(true);
                http.Dispose();
                return res;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                return null;
            }
        }

        private async Task GetImageSource()
        {
            if (EditRequest.File.Bytes != null! && EditRequest.File.Bytes.Any())
            {
                ImageSource = EditRequest.File.Bytes;
            }
            else if (!string.IsNullOrWhiteSpace(EditRequest.File.DownloadLink))
            {
                if (EditRequest.File.DownloadLink.StartsWith("http"))
                {
                    var image = await GetBytes(EditRequest.File.DownloadLink).ConfigureAwait(true);
                    if (image != null)
                    {
                        ImageSource = image;
                    }
                }
                else
                {
                    ImageSource = Files.ReadAllBytes(EditRequest.File.DownloadLink)!;
                }
            }

            if (ImageSource == null!)
            {
                ImageSource = Array.Empty<byte>();
            }

            var csb = new CheckSaveJsonBehavior();
            csb.SetCompareData(new ImageCompareJson(ImageSource, false).ToJson());
            csb.CheckSaveComparer += (_, args) =>
            {
                var curr = new ImageCompareJson(ImageSource, ImageEdited);
                args.JsonToCompare = curr.ToJson();
            };

            CheckSaveBehavior = csb;
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            MenuGestureEnabled = false;
            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (args is ImageEditRequest request)
            {
                EditRequest = request;
            }

            await GetImageSource().ConfigureAwait(true);

            await base.OnActivated(args).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            MenuGestureEnabled = true;
            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdPickPicture = new VmCommand("Gallerie", async () =>
            {
                var options = new PickMediaOptions
                              {
                                  PhotoSize = PhotoSize.MaxWidthHeight,
                                  MaxWidthHeight = 1024,
                                  RotateImage = false,
                                  SaveMetaData = true,
                              };

                DcDoNotAutoDisconnect = true;

                await PermissionCheck!.CheckFilePermissions().ConfigureAwait(true);

                var img = await Files.PickPhotoAsync(options).ConfigureAwait(true);

                DcDoNotAutoDisconnect = false;

                if (img != null)
                {
                    if (img.Bytes.Length > MaxImageSize)
                    {
                        await MsgBox.Show("Bild zu groß, bitte wählen Sie ein anderes Bild aus!").ConfigureAwait(true);
                        return;
                    }

                    ImageSource = img.Bytes;
                }
            }, glyph: Glyphs.Picture_polaroid_landscape);

            CmdTakePicture = new VmCommand("Kamera", async () =>
            {
                var o = new StoreCameraMediaOptions
                        {
                            PhotoSize = PhotoSize.MaxWidthHeight,
                            MaxWidthHeight = 1024,
                            AllowCropping = true,
                            RotateImage = false,
                            SaveToAlbum = true,
                        };

                var options = new PickMediaOptions
                              {
                                  PhotoSize = PhotoSize.MaxWidthHeight,
                                  MaxWidthHeight = 1024,
                                  RotateImage = false,
                                  SaveMetaData = true,
                              };

                DcDoNotAutoDisconnect = true;

                if (PermissionCheck == null)
                {
                    Logging.Log.LogError($"[{GetType().Name}]({nameof(InitializeCommands)}): Berechtigungen können nicht abgefragt werden!");
                    return;
                }

                await PermissionCheck.CheckCameraPermissions().ConfigureAwait(true);
                await PermissionCheck.CheckFilePermissions().ConfigureAwait(true);

                var img = await Files.TakePhotoAsync(options: options, cameraOptions: o).ConfigureAwait(true);

                DcDoNotAutoDisconnect = false;

                if (img != null)
                {
                    if (img.Bytes.Length > MaxImageSize)
                    {
                        await MsgBox.Show("Bild zu groß, bitte wählen Sie ein anderes Bild aus!").ConfigureAwait(true);
                        return;
                    }

                    ImageSource = img.Bytes;
                }
            }, () => HasCustomEditor, glyph: Glyphs.Camera_1);

            View.CmdSaveHeader = new VmCommand("Speichern", () =>
            {
                View.BusySet("Speichern");

                if (HasCustomEditor)
                {
                    OnSaveImage(new ImageSaveRequest
                                {
                                    // TODO ggf nur für neue Bilder machen?
                                    ForceCropping = true,
                                });
                }
                else
                {
                    FinishSaving(true);
                }
            }, glyph: Glyphs.Floppy_disk);

            CmdReset = new VmCommand("Rückgängig", async () =>
            {
                if (HasCustomEditor)
                {
                    OnToolbarRequest(new ImageToolbarRequest {ResetAll = true});
                }
                else
                {
                    await GetImageSource().ConfigureAwait(true);
                }
            }, () => CheckSaveBehavior != null && CheckSaveBehavior.Check(), glyph: Glyphs.Navigation_left);

            CmdToggleCrop = new VmCommand("Croppen", () =>
            {
                if (HasCustomEditor)
                {
                    OnToolbarRequest(new ImageToolbarRequest
                                     {
                                         CancelEdit = IsCropping,
                                         StartCrop = !IsCropping,
                                     });
                }
            }, () => HasCustomEditor, glyph: Glyphs.Artboard_image);

            CmdExecuteCrop = new VmCommand("Durchführen", () =>
            {
                if (HasCustomEditor)
                {
                    OnToolbarRequest(new ImageToolbarRequest {FinishEdit = true});
                }
            }, () => HasCustomEditor && IsCropping, glyph: Glyphs.Check);

            CmdRotateRight = new VmCommand("rechts drehen", () =>
            {
                if (HasCustomEditor)
                {
                    OnToolbarRequest(new ImageToolbarRequest {RotateRight = true});
                }
            }, () => HasCustomEditor, glyph: Glyphs.Redo);

            CmdRotateLeft = new VmCommand("links drehen", () =>
            {
                if (HasCustomEditor)
                {
                    OnToolbarRequest(new ImageToolbarRequest {RotateLeft = true});
                }
            }, () => HasCustomEditor, glyph: Glyphs.Undo);
        }

        /// <summary>
        ///     Bild von Kamera
        /// </summary>
        public VmCommand CmdTakePicture { get; set; } = null!;

        /// <summary>
        ///     Bild von Gallerie
        /// </summary>
        public VmCommand CmdPickPicture { get; set; } = null!;

        /// <summary>
        ///     Croppen starten/beenden
        /// </summary>
        public VmCommand CmdToggleCrop { get; set; } = null!;

        /// <summary>
        ///     Croppen
        /// </summary>
        public VmCommand CmdExecuteCrop { get; set; } = null!;

        /// <summary>
        ///     rechts drehen
        /// </summary>
        public VmCommand CmdRotateRight { get; set; } = null!;

        /// <summary>
        ///     links drehen
        /// </summary>
        public VmCommand CmdRotateLeft { get; set; } = null!;

        /// <summary>
        ///     Bearbeiten rückgängig machen
        /// </summary>
        public VmCommand CmdReset { get; set; } = null!;

        #endregion
    }
}