﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace BaseLibrary.UI.Elements
{
	public class UIGradient : BaseElement
	{
		private Texture2D texture;
		private int steps;
		private Channel channel;
		public Action<Color> OnChangeColor;

		public UIGradient(int steps, Channel channel = Channel.All)
		{
			this.steps = steps;
			this.channel = channel;
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);

			CalculatedStyle dimensions = GetDimensions();

			Color[] data = new Color[texture.Width];
			texture.GetData(data);

			OnChangeColor?.Invoke(data[(int)(evt.MousePosition.X - dimensions.X)]);
		}

		public override void Recalculate()
		{
			base.Recalculate();
			texture = null;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (texture == null) texture = Utility.CreateGrad((int)GetDimensions().Width, steps, channel);
			spriteBatch.Draw(texture, GetDimensions());
		}
	}
}