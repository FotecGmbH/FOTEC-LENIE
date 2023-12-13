// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.IO;
using System.Threading.Tasks;
using BaseApp.ViewModel;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Syncfusion.SfImageEditor.XForms;
using Xamarin.Forms;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewImageEditor
    {
        public ViewImageEditor() : this(null)
        {
        }

        public ViewImageEditor(object? args = null) : base(args)
        {
            InitializeComponent();

            ViewModel.SaveImage += ViewModelOnSaveImage;
            ViewModel.ToolbarRequest += ViewModelOnToolbarRequest;
        }

        private void ViewModelOnToolbarRequest(object sender, ImageToolbarRequest e)
        {
            try
            {
                if (e.ResetAll)
                {
                    Editor.Reset();
                    ViewModel.IsCropping = false;
                }
                else if (e.StartCrop)
                {
                    if (ViewModel.EditRequest.IsIdeaPicture)
                    {
                        Editor.ToggleCropping(2, 3);
                    }
                    else if (ViewModel.EditRequest.IsProfilePicture)
                    {
                        Editor.ToggleCropping(1, 1);
                    }
                    else
                    {
                        Editor.ToggleCropping(float.NaN, float.NaN);
                    }

                    ViewModel.IsCropping = true;
                }
                else if (e.RotateRight)
                {
                    Editor.Rotate();
                    ViewModel.IsCropping = false;
                }
                else if (e.RotateLeft)
                {
                    Editor.Rotate();
                    Editor.Rotate();
                    Editor.Rotate();
                    ViewModel.IsCropping = false;
                }
                else if (e.CancelEdit)
                {
                    if (ViewModel.IsCropping)
                    {
                        Editor.ToggleCropping();
                        ViewModel.IsCropping = false;
                    }
                }
                else if (e.FinishEdit)
                {
                    if (ViewModel.IsCropping)
                    {
                        Editor.Crop();
                        ViewModel.IsCropping = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log.LogError($"[{nameof(ViewImageEditor)}]({nameof(ViewModelOnToolbarRequest)}): {ex}");
            }
        }

        private async void ViewModelOnSaveImage(object sender, ImageSaveRequest e)
        {
            try
            {
                if (e.ForceCropping)
                {
                    if (ViewModel.EditRequest.IsIdeaPicture)
                    {
                        Editor.ToggleCropping(2, 3);
                    }
                    else if (ViewModel.EditRequest.IsProfilePicture)
                    {
                        Editor.ToggleCropping(1, 1);
                    }
                    else
                    {
                        Editor.ToggleCropping(float.NaN, float.NaN);
                    }

                    await Task.Delay(50).ConfigureAwait(true);

                    Editor.Crop();

                    await Task.Delay(50).ConfigureAwait(true);
                }

                if (e.Height.HasValue && e.Width.HasValue)
                {
                    Editor.Save(e.FileType, new Size(e.Width.Value, e.Height.Value));
                }
                else if (e.Height.HasValue)
                {
                    Editor.Save(e.FileType, new Size(e.Height.Value, e.Height.Value));
                }
                else if (e.Width.HasValue)
                {
                    Editor.Save(e.FileType, new Size(e.Width.Value, e.Width.Value));
                }
                else
                {
                    Editor.Save(e.FileType);
                }
            }
            catch (Exception ex)
            {
                Logging.Log.LogError($"[{nameof(ViewImageEditor)}]({nameof(ViewModelOnSaveImage)}): {ex}");
            }
        }

        private void SfImageEditor_OnImageSaving(object sender, ImageSavingEventArgs args)
        {
            try
            {
                // Bild wird gespeichert
                var stream = args.Stream;

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);

                    ViewModel.ImageSource = ms.ToArray();
                    ViewModel.FinishSaving(true);

                    args.Cancel = true;
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
                ViewModel.FinishSaving(false);
            }
        }

        private void Editor_OnImageEdited(object sender, ImageEditedEventArgs e)
        {
            // Bild wurde bearbeitet
            ViewModel.ImageEdited = e.IsImageEdited;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member