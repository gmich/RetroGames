﻿using Gem.Gui.Aggregation;
using Gem.Gui.Configuration;
using Gem.Gui.Containers;
using Gem.Gui.Controls;
using Gem.Gui.Factories;
using Gem.Gui.Rendering;
using Gem.Gui.ScreenSystem;
using Gem.Infrastructure.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Gem.Gui.Layout;
using Gem.Gui.Alignment;
using Gem.Gui.Styles;

namespace Gem.Gui
{
    public sealed class GemGui : IDisposable
    {

        #region Fields

        private readonly IConfigurationResolver configuration;
        private readonly IControlFactory controlFactory;
        private readonly AggregationTarget aggregationTarget;

        private readonly AssetContainer<SpriteFont> _fontContainer;
        private readonly AssetContainer<Texture2D> _textureContainer;

        private readonly Settings settings;
        private readonly Dictionary<string, IGuiHost> hosts = new Dictionary<string, IGuiHost>();
        private ScreenManager screenManager;

        public event EventHandler<SpriteBatch> DrawWith;

        #endregion

        #region Construct / Dispose

        public GemGui(Game game,
                      AggregationTarget aggregationTarget = AggregationTarget.All,
                      ControlTarget controlTarget = ControlTarget.Windows,
                      IConfigurationResolver configuration = null)
        {
            this.configuration = configuration ?? new DefaultConfiguration();
            this.aggregationTarget = aggregationTarget;
            this.settings = new Settings(game);
            this.controlFactory = this.configuration.GetControlFactory(controlTarget);
            this.HostTransition = () => TimedTransition.Default;
            screenManager = new ScreenManager(game, settings, DrawTheRest);

            game.Components.Add(screenManager);
            game.Components.Add(new Input.InputManager(game));

            _fontContainer = new AssetContainer<SpriteFont>(game.Content);
            _textureContainer = new AssetContainer<Texture2D>(game.Content);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed = false;
        private void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                screenManager.Dispose();
                isDisposed = true;
            }
        }

        #endregion

        private void DrawTheRest(SpriteBatch batch)
        {
            var handler = DrawWith;
            if (handler != null)
            {
                handler(this, batch);
            }
        }

        #region Control Factory

        private Texture2D CreateDummyTexture()
        {
            var texture = new Texture2D(screenManager.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new[] { Color.White });

            return texture;
        }

        public ARenderStyle GetRenderStyle(Style style)
        {
            switch (style)
            {
                case Style.Transparent:
                    return new TransparentControlStyle();
                case Style.NoStyle:
                    return new NoStyle();
                case Style.Border:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Style not found");
            }
        }

        public Button Button(int x, int y,
                             int sizeX, int sizeY,
                             Style style)
        {
            return controlFactory.CreateButton(new Region(new Vector2(x, y),
                                                           new Vector2(sizeX, sizeY)),
                                                           CreateDummyTexture(), GetRenderStyle(style));
        }

        public ListView ListView(int x, int y,
                                 int sizeX, int sizeY,
                                 Orientation orientation,
                                 IHorizontalAlignable horizontalAlignment,
                                 IVerticalAlignable verticalAlignment,
                                 IAlignmentTransition alignmentTransition,
                                 params AControl[] controls)
        {
            return controlFactory.CreateListView(CreateDummyTexture(),
                                                 new Region(new Vector2(x, y),
                                                            new Vector2(sizeX, sizeY)),
                                                 orientation,
                                                 new AlignmentContext(horizontalAlignment, verticalAlignment, alignmentTransition),
                                                 controls.ToList().AsReadOnly());
        }

        public TextField TextBox(int x, int y,
                         int sizeX, int sizeY,
                         Color textColor,
                         SpriteFont font,
                         Style style,
                         IHorizontalAlignable horizontalAlignment = null,
                         IVerticalAlignable verticalAlignment = null,
                         IAlignmentTransition alignmentTransition = null,
                         TextAppenderHelper appender = null)
        {
            TextField textField = null;

            return textField =
                   controlFactory.CreateTextBox(appender ?? TextAppenderHelper.Default,
                                                font,
                                                CreateDummyTexture(),
                                                new Region(new Vector2(x, y),
                                                            new Vector2(sizeX, sizeY)),
                                                textColor,
                                                GetRenderStyle(style),
                                                horizontalAlignment ?? HorizontalAlignment.Left,
                                                verticalAlignment?? VerticalAlignment.Center, 
                                                alignmentTransition?? AlignmentTransition.Fixed);
        }

        //TODO: add the rest

        #endregion

        #region Properties

        public AssetContainer<SpriteFont> Fonts { get { return _fontContainer; } }

        public AssetContainer<Texture2D> Textures { get { return _textureContainer; } }

        public Func<ITransition> HostTransition;

        #endregion

        #region Public Helper Methods

        public void AddGuiHost(string guiHostId, params AControl[] controls)
        {
            foreach (var control in controls)
            {
                //control.Align(new Region(Vector2.Zero, settings.Resolution));
                settings.OnResolutionChange((sender, args) =>
                                            control.Align(Settings.ViewRegion));
            }
            var entries = controls.Where(control => control.HasAttribute<LayoutAttribute>());
            var controlsEnumerable = controls.AsEnumerable();

            foreach (var entry in entries)
            {
                controlsEnumerable = controlsEnumerable.Concat(entry.Entries());
            }

            var guiHost = new GuiHost(controls.ToList(),
                                   settings.RenderTemplate,
                                   new AggregationContext(configuration.GetAggregators(aggregationTarget), controlsEnumerable),
                                   HostTransition());
            AddGuiHost(guiHostId, guiHost);

        }

        public IGuiHost this[string guiHostId]
        {
            get
            {
                if (hosts.ContainsKey(guiHostId))
                {
                    return hosts[guiHostId];
                }
                else
                {
                    throw new ArgumentException("Gui host was not found");
                }
            }
        }

        public void AddGuiHost(string guiHostId, IGuiHost guiHost)
        {
            Contract.Requires(!hosts.ContainsKey(guiHostId), "A GuiHost with the same id is already registered");
            this.hosts.Add(guiHostId, guiHost);
        }

        public void Disable()
        {
            screenManager.Enabled = false;
        }

        public void Show(string guiHostId)
        {
            Contract.Requires(hosts.ContainsKey(guiHostId), "GuiHost was not found");
            screenManager.Enabled = true;

            screenManager.AddScreen(hosts[guiHostId]);
        }

        public bool IsShowing(string guiHostId)
        {
            return screenManager.IsShowing(hosts[guiHostId]);
        }

        public void Hide(string guiHostId)
        {
            Contract.Requires(hosts.ContainsKey(guiHostId), "GuiHost was not found");
            screenManager.Enabled = true;

            screenManager.RemoveScreen(hosts[guiHostId]);
        }

        public void Swap(string previousHost, string newHost)
        {
            Hide(previousHost);
            Show(newHost);
        }

        #endregion

    }
}