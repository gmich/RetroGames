﻿using Gem.Gui.Alignment;
using Gem.Gui.Controls;
using Gem.Gui.Core.Controls;
using Gem.Gui.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.Contracts;

namespace Gem.Gui.Fluent
{
    public static class ControlFluentBuilder
    {

        #region AControl Extensions

        public static AControl Text(this AControl control, SpriteFont font, string text, int x = 0, int y = 0, bool relativeToParent = false)
        {
            control.Text = new StandardText(font, control.Region.Position + new Vector2(x, y), text);

            if (relativeToParent)
            {
                control.Text.Alignment.HorizontalAlignment = HorizontalAlignment.RelativeTo(() => control.Region.Position.X + x);
                control.Text.Alignment.VerticalAlignment = VerticalAlignment.RelativeTo(() => control.Region.Position.Y +y);
            }
            return control;
        }

        public static AControl Color(this AControl control, Color color)
        {
            control.RenderParameters.Color = color;
            return control;
        }

        public static AControl Padding(this AControl control, int top = 0, int bottom = 0, int left = 0, int right = 0)
        {
            control.Padding.Top = top;
            control.Padding.Bottom = bottom;
            control.Padding.Left = left;
            control.Padding.Right = right;

            return control;
        }
        
        public static AControl TextColor(this AControl control, Color color)
        {
            Contract.Requires(control.Text != null, "Use Text() before setting the text's color");

            control.Text.RenderParameters.Color = color;
            return control;
        }

        public static AControl Sprite(this AControl control, string tag, Texture2D texture, Rectangle? sourceRectangle = null)
        {
            control.Sprite.Add(tag, texture, sourceRectangle);
            control.Sprite.SwitchSprite(tag);
            return control;
        }

        public static AControl ScreenAlignment(this AControl listView, IHorizontalAlignable horizontalAignment, IVerticalAlignable verticalAignment)
        {
            listView.ScreenAlignment.HorizontalAlignment = horizontalAignment;
            listView.ScreenAlignment.VerticalAlignment = verticalAignment;
            return listView;
        }

        public static AControl TextHorizontalAlignment(this AControl control, IHorizontalAlignable horizontalAignment)
        {
            Contract.Requires(control.Text != null, "Use Text() before setting the text's color");

            control.Text.Alignment.HorizontalAlignment = horizontalAignment;
            return control;
        }

        public static AControl TextVerticalAlignment(this AControl control, IVerticalAlignable verticalAignment)
        {
            Contract.Requires(control.Text != null, "Use Text() before setting the text's color");

            control.Text.Alignment.VerticalAlignment = verticalAignment;
            return control;
        }

        public static AControl OnClick(this AControl control, EventHandler<ControlEventArgs> handler)
        {
            control.Events.Clicked += handler;
            return control;
        }

        #endregion

        #region TextField Extensions

        public static TextField OnTextEntry(this TextField textField, EventHandler<string> handler)
        {
            textField.OnTextEntered += handler;
            return textField;
        }

        #endregion

        #region CheckBox Extensions

        public static CheckBox OnCheckChanged(this CheckBox checkBox, EventHandler<bool> onCheckChangedHandler)
        {
            checkBox.CheckChanged += onCheckChangedHandler;
            return checkBox;
        }

        #endregion

        #region Slider Extensions

        public static Slider ValueChanged(this Slider slider, EventHandler<float> onValueChangedHandler)
        {
            slider.OnValueChanged += onValueChangedHandler;
            return slider;
        }

        public static Slider ValueChanging(this Slider slider, EventHandler<float> onValueChangingHandler)
        {
            slider.OnValueChanging += onValueChangingHandler;
            return slider;
        }

        #endregion

        #region Label Extensions

        public static Label StretchToText(this Label label,bool shouldStretch)
        {
            label.StretchToText = shouldStretch;
            return label;
        }

        #endregion
    }
}
