﻿using System;
using System.ComponentModel.Composition;
using Gemini.Framework;
using Gem.IDE.Modules.SpriteSheets.Views;
using Gem.DrawingSystem.Animations;
using System.ComponentModel;
using Gemini.Modules.Inspector;
using Caliburn.Micro;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Gem.IDE.Infrastructure;

namespace Gem.IDE.Modules.SpriteSheets.ViewModels
{
    [Export(typeof(AnimationStripViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AnimationStripViewModel : PersistedEditor
    {

        #region Fields

        private ISceneView sceneView;
        private readonly Func<string, IAnimationRepository> repository;
        private AnimationStripSettings settings;

        #endregion

        #region Persistence


        protected override Task DoSave(string filePath)
        {
            DisplayName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            RedrawAndSave(filePath);
            return TaskUtility.Completed;
        }

        public override void CanClose(System.Action<bool> callback)
        {
            if (IsDirty)
            {
                ShowSavePrompt(FilePath);
            }
            callback(true);
        }

        private void ShowSavePrompt(string filePath)
        {
            string sMessageBoxText = "Save changes to the following item?";
            string sCaption = Name;
            var rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (rsltMessageBox == MessageBoxResult.Yes)
            {
                RedrawAndSave(filePath);
            }
        }


        private void RedrawAndSave(string filePath)
        {
            sceneView?.Invalidate(Settings, settings => Save(settings, filePath));
        }
        #endregion

        #region Ctor

        public AnimationStripViewModel(string path, AnimationStripSettings settings, Func<string, IAnimationRepository> repository)
        {
            this.repository = repository;
            this.settings = settings;
            Path = path;

            frameWidth = settings.FrameWidth;
            frameHeight = settings.FrameHeight;
            frameDelay = settings.FrameDelay * 1000;
            lastFrame = settings.LastFrame;
            firstFrame = settings.StartFrame;
            name = settings.Name;
            DisplayName = $"{name}{Extensions.Animation}";
            FilePath = Path;
            FileName = DisplayName;
            SetupInspector();
        }

        #endregion

        #region Helpers

        private void SetupInspector()
        {
            var inspectorTool = IoC.Get<IInspectorTool>();
            inspectorTool.DisplayName = "Sprite-Sheet Animation Inspector";
            IoC.Get<IShell>().ShowTool<IInspectorTool>();
        }

        private void Save(AnimationStripSettings asettings, string path)
        {
            repository(path).Save(asettings);
        }


        [Browsable(false)]
        public override bool ShouldReopenOnStart
        {
            get { return true; }
        }

        private AnimationStripSettings Settings =>
           settings = new AnimationStripSettings(
                FrameWidth,
                FrameHeight,
                TileSheetWidth,
                TileSheetHeight,
                Name,
                FrameDelay / 1000,
                true,
                settings.Image,
                FirstFrame,
                LastFrame);

        private AnimationViewOptions options =>
            new AnimationViewOptions(ShowNumbers, ShowGrid, Animate, ShowTilesheet);

        #endregion

        #region Frame

        private string name;
        [Category("Animation")]
        public string Name
        {
            get { return name; }
            set
            {
                //repository.Delete(name);
                name = value.Trim();
                //DisplayName = name + ".animation";
                //DisplayName = (IsDirty) ? DisplayName + "*" : DisplayName;
                NotifyOfPropertyChange(() => Name);
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
            }
        }

        private int frameWidth;
        [DisplayName("Frame Width"), Category("Animation")]
        public int FrameWidth
        {
            get { return frameWidth; }
            set
            {
                frameWidth = MathHelper.Clamp(value, 1, TileSheetWidth);
                NotifyOfPropertyChange(() => FrameWidth);
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
            }
        }

        private int frameHeight;
        [DisplayName("Frame Height"), Category("Animation")]
        public int FrameHeight
        {
            get { return frameHeight; }
            set
            {
                frameHeight = MathHelper.Clamp(value, 1, TileSheetHeight);
                NotifyOfPropertyChange(() => FrameHeight);
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
            }
        }


        private int firstFrame;
        [DisplayName("First Frame"), Category("Animation")]
        public int FirstFrame
        {
            get { return firstFrame; }
            set
            {
                firstFrame = (value < 0) ? 0 : value;
                firstFrame = (firstFrame > lastFrame) ? lastFrame : firstFrame;
                NotifyOfPropertyChange(() => FirstFrame);
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
            }
        }

        private int lastFrame;
        [DisplayName("Last Frame"), Category("Animation")]
        public int LastFrame
        {
            get { return lastFrame; }
            set
            {
                lastFrame = (value < firstFrame) ? firstFrame : value;
                NotifyOfPropertyChange(() => LastFrame);
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
            }
        }

        private double frameDelay;
        [DisplayName("Frame Delay (ms)"), Category("Animation")]
        public double FrameDelay
        {
            get { return frameDelay; }
            set
            {
                frameDelay = (value < 0) ? 0 : value;
                NotifyOfPropertyChange(() => FrameDelay);
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
            }
        }
        #endregion

        #region SpriteSheet

        [Category("SpriteSheet")]
        public string Path { get; }

        private int tilesheetWidth;
        [DisplayName("Width"), Category("SpriteSheet")]
        public int TileSheetWidth
        {
            get
            {
                return tilesheetWidth;
            }
            private set
            {
                tilesheetWidth = value;
                NotifyOfPropertyChange(() => TileSheetWidth);
            }
        }

        private int tileSheetHeight;
        [DisplayName("Height"), Category("SpriteSheet")]
        public int TileSheetHeight
        {
            get
            {
                return tileSheetHeight;
            }
            private set
            {
                tileSheetHeight = value;
                NotifyOfPropertyChange(() => TileSheetHeight);
            }
        }

        #endregion

        #region Presentation

        private bool animate;
        [Category("Presentation")]
        public bool Animate
        {
            get { return animate; }
            set
            {
                animate = value;
                ShowTilesheet = !animate;
                NotifyOfPropertyChange(() => Animate);
                sceneView?.SetOptions(options);
            }
        }

        private bool showNumbers;
        [DisplayName("Show Numbers"), Category("Presentation")]
        public bool ShowNumbers
        {
            get { return showNumbers; }
            set
            {
                showNumbers = value;
                NotifyOfPropertyChange(() => ShowNumbers);
                sceneView?.SetOptions(options);
            }
        }

        private bool showGrid = true;
        [DisplayName("Show Grid"), Category("Presentation")]
        public bool ShowGrid
        {
            get { return showGrid; }
            set
            {
                showGrid = value;
                NotifyOfPropertyChange(() => ShowGrid);
                sceneView?.SetOptions(options);
            }
        }

        private bool showTilesheet = true;
        [DisplayName("Show Tilesheet"), Category("Presentation")]
        public bool ShowTilesheet
        {
            get { return showTilesheet; }
            set
            {
                showTilesheet = value;
                NotifyOfPropertyChange(() => ShowTilesheet);
                sceneView?.SetOptions(options);
            }
        }

        private double zoom = 1.0d;
        [DisplayName("Scale"), Range(0.2, 2.5), Category("Presentation")]
        public double Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                NotifyOfPropertyChange(() => Zoom);
                sceneView.Scale = zoom;
            }
        }

        private Color backgroundColor;
        [DisplayName("Background color"), Category("Presentation")]
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                NotifyOfPropertyChange(() => BackgroundColor);
                sceneView.BackgroundColor = value;
            }
        }
        #endregion

        #region Document Members

        protected override void OnViewLoaded(object view)
        {
            sceneView = view as ISceneView;
            sceneView.OnGraphicsDeviceLoaded += (sender, args) =>
            {
                if (settings.Image == null)
                {
                    var result = sceneView.LoadTexture(Path);
                    TileSheetWidth = result.Item1;
                    TileSheetHeight = result.Item2;
                    settings.Image = result.Item3;
                }
                else
                {
                    TileSheetWidth = settings.TileSheetWidth;
                    TileSheetHeight = settings.TileSheetHeight;
                    sceneView.SetColorData(settings.Image, settings.TileSheetWidth, settings.TileSheetHeight);
                }
                IsDirty = true;
                sceneView?.Invalidate(Settings, null);
                sceneView.SetOptions(options);
            };
            sceneView.onScaleChange += (sender, newScale) => Zoom = newScale;
            base.OnViewLoaded(view);
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                var view = GetView() as IDisposable;
                view?.Dispose();
            }

            base.OnDeactivate(close);
        }

        #endregion
    }
}