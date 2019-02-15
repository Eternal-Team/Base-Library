﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace BaseLibrary.Utility
{
	public static partial class Utility
	{
		public static string MouseText;
		public static Color? colorMouseText;

		public static Texture2D TexturePanelBackground = ModLoader.GetTexture("Terraria/UI/PanelBackground");
		public static Texture2D TexturePanelBorder = ModLoader.GetTexture("Terraria/UI/PanelBorder");

		public static void DrawPanel(this SpriteBatch spriteBatch, Rectangle dimensions, Texture2D texture, Color color)
		{
			Point point = new Point(dimensions.X, dimensions.Y);
			Point point2 = new Point(point.X + dimensions.Width - 12, point.Y + dimensions.Height - 12);
			int width = point2.X - point.X - 12;
			int height = point2.Y - point.Y - 12;
			spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, 12, 12), new Rectangle(0, 0, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, 12, 12), new Rectangle(16, 0, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, 12, 12), new Rectangle(0, 16, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, 12, 12), new Rectangle(16, 16, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X + 12, point.Y, width, 12), new Rectangle(12, 0, 4, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X + 12, point2.Y, width, 12), new Rectangle(12, 16, 4, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + 12, 12, height), new Rectangle(0, 12, 12, 4), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + 12, 12, height), new Rectangle(16, 12, 12, 4), color);
			spriteBatch.Draw(texture, new Rectangle(point.X + 12, point.Y + 12, width, height), new Rectangle(12, 12, 4, 4), color);
		}

		public static void DrawPanel(this SpriteBatch spriteBatch, CalculatedStyle dimensions, Color? bgColor = null, Color? borderColor = null) => spriteBatch.DrawPanel(dimensions.ToRectangle(), bgColor, borderColor);

		public static void DrawPanel(this SpriteBatch spriteBatch, Rectangle rectangle, Color? bgColor = null, Color? borderColor = null)
		{
			spriteBatch.DrawPanel(rectangle, TexturePanelBackground, bgColor ?? ColorPanel);
			spriteBatch.DrawPanel(rectangle, TexturePanelBorder, borderColor ?? Color.Black);
		}

		public static void DrawSlot(this SpriteBatch spriteBatch, Rectangle dimensions, Color? color = null, Texture2D texture = null)
		{
			if (texture == null) texture = Main.inventoryBack13Texture;
			if (color == null) color = ColorSlot;

			Point point = new Point(dimensions.X, dimensions.Y);
			Point point2 = new Point(point.X + dimensions.Width - 8, point.Y + dimensions.Height - 8);
			int width = point2.X - point.X - 8;
			int height = point2.Y - point.Y - 8;

			Color value = color.Value;
			spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, 8, 8), new Rectangle(0, 0, 8, 8), value);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, 8, 8), new Rectangle(44, 0, 8, 8), value);
			spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, 8, 8), new Rectangle(0, 44, 8, 8), value);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, 8, 8), new Rectangle(44, 44, 8, 8), value);
			spriteBatch.Draw(texture, new Rectangle(point.X + 8, point.Y, width, 8), new Rectangle(8, 0, 36, 8), value);
			spriteBatch.Draw(texture, new Rectangle(point.X + 8, point2.Y, width, 8), new Rectangle(8, 44, 36, 8), value);
			spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + 8, 8, height), new Rectangle(0, 8, 8, 36), value);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + 8, 8, height), new Rectangle(44, 8, 8, 36), value);
			spriteBatch.Draw(texture, new Rectangle(point.X + 8, point.Y + 8, width, height), new Rectangle(8, 8, 36, 36), value);
		}

		public static void DrawSlot(this SpriteBatch spriteBatch, CalculatedStyle dimensions, Color? color = null, Texture2D texture = null) => spriteBatch.DrawSlot(dimensions.ToRectangle(), color, texture);

		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
		{
			float num = Vector2.Distance(start, end);
			Vector2 vector = (end - start) / num;
			Vector2 value = start;
			float rotation = vector.ToRotation();
			for (float num2 = 0f; num2 <= num; num2 += 1f)
			{
				spriteBatch.Draw(Main.blackTileTexture, value, null, color, rotation, Vector2.Zero, 0.1f, SpriteEffects.None, 0f);
				value = start + num2 * vector;
			}
		}

		public static void DrawOutline(this SpriteBatch spriteBatch, Point16 start, Point16 end, Color color, float lineSize, bool addZero = false)
		{
			float width = Math.Abs(start.X - end.X) * 16 + 16;
			float height = Math.Abs(start.Y - end.Y) * 16 + 16;

			Vector2 position = -Main.screenPosition + start.Min(end).ToVector2() * 16;

			if (addZero)
			{
				Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
				if (Main.drawToScreen) zero = Vector2.Zero;
				position += zero;
			}

			spriteBatch.Draw(Main.magicPixel, position, null, color, 0f, Vector2.Zero, new Vector2(width, lineSize / 1000f), SpriteEffects.None, 0f);
			spriteBatch.Draw(Main.magicPixel, position, null, color, 0f, Vector2.Zero, new Vector2(lineSize, height / 1000f), SpriteEffects.None, 0f);

			spriteBatch.Draw(Main.magicPixel, position + new Vector2(0, height - lineSize), null, color, 0f, Vector2.Zero, new Vector2(width, lineSize / 1000f), SpriteEffects.None, 0f);
			spriteBatch.Draw(Main.magicPixel, position + new Vector2(width - lineSize, 0), null, color, 0f, Vector2.Zero, new Vector2(lineSize, height / 1000f), SpriteEffects.None, 0f);
		}

		public static void DrawMouseText(object text, Color? color = null)
		{
			MouseText = text.ToString();
			colorMouseText = color;
		}

		internal static bool DrawMouseText()
		{
			if (MouseText == null) return true;

			Main.LocalPlayer.showItemIcon = false;
			Main.ItemIconCacheUpdate(0);
			Main.mouseText = true;

			PlayerInput.SetZoom_UI();
			int hackedScreenWidth = Main.screenWidth;
			int hackedScreenHeight = Main.screenHeight;
			int hackedMouseX = Main.mouseX;
			int hackedMouseY = Main.mouseY;
			PlayerInput.SetZoom_UI();
			PlayerInput.SetZoom_Test();

			int posX = Main.mouseX + 10;
			int posY = Main.mouseY + 10;
			if (hackedMouseX != -1 && hackedMouseY != -1)
			{
				posX = hackedMouseX + 10;
				posY = hackedMouseY + 10;
			}

			if (Main.ThickMouse)
			{
				posX += 6;
				posY += 6;
			}

			Vector2 vector = ExtractText(MouseText).Measure();
			if (hackedScreenHeight != -1 && hackedScreenWidth != -1)
			{
				if (posX + vector.X + 4f > hackedScreenWidth) posX = (int)(hackedScreenWidth - vector.X - 4f);
				if (posY + vector.Y + 4f > hackedScreenHeight) posY = (int)(hackedScreenHeight - vector.Y - 4f);
			}
			else
			{
				if (posX + vector.X + 4f > Main.screenWidth) posX = (int)(Main.screenWidth - vector.X - 4f);
				if (posY + vector.Y + 4f > Main.screenHeight) posY = (int)(Main.screenHeight - vector.Y - 4f);
			}

			TextSnippet[] snippets = ChatManager.ParseMessage(MouseText, colorMouseText ?? Color.White).ToArray();
			if (snippets.Length > 1)
			{
				for (int i = 0; i < snippets.Length; i++)
				{
					TextSnippet textSnippet = snippets[i];

					if (textSnippet.Text.EndsWith("\n"))
					{
						if (i + 1 < snippets.Length)
						{
							textSnippet.Text = textSnippet.Text.Replace("\n", "");
							snippets[i + 1].Text = snippets[i + 1].Text.Insert(0, "\n");
						}
					}
				}
			}

			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, snippets, new Vector2(posX, posY), 0f, Vector2.Zero, Vector2.One, out int num);

			MouseText = null;

			return true;
		}

		public static Texture2D CreateGrad(int width, int steps, Channel channel)
		{
			Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, width, 1);
			Color[] data = new Color[width];

			int nextX = 0;
			int step = 0;
			foreach (int i in DistributeInteger(width, steps))
			{
				Color c = Color.White;
				switch (channel)
				{
					case Channel.R:
						c = new Color(255 * step / steps, 0, 0);
						break;
					case Channel.G:
						c = new Color(0, 255 * step / steps, 0);
						break;
					case Channel.B:
						c = new Color(0, 0, 255 * step / steps);
						break;
					case Channel.A:
						c = new Color(0, 0, 0, 255 * step / steps);
						break;
					default:
						c = HSL2RGB(step / (float)steps, 1.0f, 0.5f);
						break;
				}

				for (int x = nextX; x < nextX + i; x++) data[x] = c;

				nextX += i;
				step++;
			}

			texture.SetData(data);
			return texture;
		}

		public static Color HSL2RGB(float h, float sl, float l)
		{
			float r = l;
			float g = l;
			float b = l;

			float v = l <= 0.5f ? l * (1f + sl) : l + sl - l * sl;

			if (v > 0)
			{
				float m = l + l - v;
				float sv = (v - m) / v;
				h *= 6f;
				int sextant = (int)h;
				float fract = h - sextant;
				float vsf = v * sv * fract;
				float mid1 = m + vsf;
				float mid2 = v - vsf;

				if (h < 1f || Math.Abs(h - 6f) < 0.0)
				{
					r = v;
					g = mid1;
					b = m;
				}
				else if (h < 2f)
				{
					r = mid2;
					g = v;
					b = m;
				}
				else if (h < 3f)
				{
					r = m;
					g = v;
					b = mid1;
				}
				else if (h < 4f)
				{
					r = m;
					g = mid2;
					b = v;
				}
				else if (h < 5f)
				{
					r = mid1;
					g = m;
					b = v;
				}
				else if (h < 6f)
				{
					r = v;
					g = m;
					b = mid2;
				}
			}

			return new Color(r, g, b);
		}

		public static void LoadTextures(this Mod mod)
		{
			if (Main.dedServ) return;

			foreach (Type type in mod.Code.GetTypes())
			{
				foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Where(x => x.PropertyType == typeof(Texture2D) || x.PropertyType.GetElementType() == typeof(Texture2D) || x.PropertyType.GenericTypeArguments.Any() && x.PropertyType.GenericTypeArguments[0] == typeof(Texture2D)))
				{
					PathOverrideAttribute overrideAttribute = propertyInfo.GetCustomAttribute<PathOverrideAttribute>();

					if (propertyInfo.PropertyType.IsArray)
					{
						Texture2D[] array = (Texture2D[])propertyInfo.GetValue(null);
						for (int i = 0; i < array.Length; i++) array[i] = ModLoader.GetTexture(overrideAttribute?.path ?? $"{mod.Name}/Textures/{propertyInfo.Name}_{i}");
					}
					else if (propertyInfo.IsEnumerable())
					{
						List<Texture2D> list = (List<Texture2D>)propertyInfo.GetValue(null);
						int i = 0;
						while (File.Exists(overrideAttribute?.path ?? $"{mod.File.path}/Textures/{propertyInfo.Name}_{i}"))
						{
							list.Add(ModLoader.GetTexture(overrideAttribute?.path ?? $"{mod.Name}/Textures/{propertyInfo.Name}_{i}"));
							i++;
						}
					}
					else propertyInfo.SetValue(null, ModLoader.GetTexture(overrideAttribute?.path ?? $"{mod.Name}/Textures/{propertyInfo.Name}"));
				}
			}
		}

		#region SpriteBatch
		public static RasterizerState OverflowHiddenState = new RasterizerState
		{
			CullMode = CullMode.None,
			ScissorTestEnable = true
		};

		public static void DrawOverflowHidden(this SpriteBatch spriteBatch, UIElement uiElement, Action<SpriteBatch> drawAction)
		{
			Rectangle scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
			RasterizerState rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
			SamplerState anisotropicClamp = SamplerState.AnisotropicClamp;

			spriteBatch.End();
			Rectangle clippingRectangle = uiElement.GetClippingRectangle(spriteBatch);
			Rectangle adjustedClippingRectangle = Rectangle.Intersect(clippingRectangle, spriteBatch.GraphicsDevice.ScissorRectangle);
			spriteBatch.GraphicsDevice.ScissorRectangle = adjustedClippingRectangle;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, anisotropicClamp, DepthStencilState.None, OverflowHiddenState, null, Main.UIScaleMatrix);

			drawAction.Invoke(spriteBatch);

			rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, anisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
		}

		public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position) => spriteBatch.Draw(texture, position, Color.White);

		public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Rectangle rectangle) => spriteBatch.Draw(texture, rectangle, Color.White);

		public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, CalculatedStyle dimensions, Color? color = null) => spriteBatch.Draw(texture, dimensions.ToRectangle(), color ?? Color.White);

		public static void DrawImmediate(this SpriteBatch spriteBatch, Action<SpriteBatch> drawAction)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenState, null, Main.UIScaleMatrix);
			drawAction.Invoke(spriteBatch);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenState, null, Main.UIScaleMatrix);
		}

		public static void DrawWithTransformation(this SpriteBatch spriteBatch, Matrix transformation, Action<SpriteBatch> drawAction)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, transformation);

			drawAction(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin();
		}

		public static void DrawScissor(this SpriteBatch spriteBatch, Rectangle rectangle, Action<SpriteBatch> drawAction)
		{
			Rectangle scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
			RasterizerState rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
			SamplerState anisotropicClamp = SamplerState.AnisotropicClamp;

			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = rectangle;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, anisotropicClamp, DepthStencilState.None, OverflowHiddenState, null, Main.UIScaleMatrix);

			drawAction.Invoke(spriteBatch);

			rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, anisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
		}

		public static void DrawItem(this SpriteBatch spriteBatch, Item item, Vector2 position, Vector2 size)
		{
			if (!item.IsAir)
			{
				Texture2D itemTexture = Main.itemTexture[item.type];
				Rectangle rect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Frame();
				Color newColor = Color.White;
				float pulseScale = 1f;
				ItemSlot.GetItemLight(ref newColor, ref pulseScale, item);
				float scale = Math.Min(size.X / rect.Width, size.Y / rect.Height);

				Vector2 origin = rect.Size() * 0.5f * pulseScale;

				if (ItemLoader.PreDrawInInventory(item, spriteBatch, position, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, scale * pulseScale))
				{
					spriteBatch.Draw(itemTexture, position, rect, item.GetAlpha(newColor), 0f, origin, scale * pulseScale, SpriteEffects.None, 0f);
					if (item.color != Color.Transparent) spriteBatch.Draw(itemTexture, position, rect, item.GetColor(Color.White), 0f, origin, scale * pulseScale, SpriteEffects.None, 0f);
				}

				ItemLoader.PostDrawInInventory(item, spriteBatch, position, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, scale * pulseScale);
				if (ItemID.Sets.TrapSigned[item.type]) spriteBatch.Draw(Main.wireTexture, position + new Vector2(40f, 40f), new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
				if (item.stack > 1) ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, item.stack.ToString(), position + new Vector2(10f, 26f) * scale, Color.White, 0f, Vector2.Zero, new Vector2(scale), -1f, scale);
			}
		}

		public static void DrawNPC(this SpriteBatch spriteBatch, NPC npc, Vector2 position, Vector2 size)
		{
			Main.instance.LoadNPC(npc.type);

			Texture2D npcTexture = Main.npcTexture[npc.type];

			Rectangle rectangle = new Rectangle(0, 0, Main.npcTexture[npc.type].Width, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type]);

			Color color = npc.color != Color.Transparent ? new Color(npc.color.R, npc.color.G, npc.color.B, 255f) : new Color(1f, 1f, 1f);

			Main.spriteBatch.Draw(npcTexture, position, rectangle, color, 0, rectangle.Size() * 0.5f, Math.Min(size.X / rectangle.Width, size.Y / rectangle.Height), SpriteEffects.None, 0);
		}

		public static void DrawProjectile(this SpriteBatch spriteBatch, Projectile proj, Vector2 position, Vector2 size)
		{
			Main.instance.LoadProjectile(proj.type);

			Texture2D projTexture = Main.projectileTexture[proj.type];

			Main.spriteBatch.Draw(projTexture, position, null, Color.White, 0, projTexture.Size() * 0.5f, Math.Min(size.X / projTexture.Width, size.Y / projTexture.Height), SpriteEffects.None, 0);
		}
		#endregion
	}

	public class PathOverrideAttribute : Attribute
	{
		public string path;

		public PathOverrideAttribute(string path) => this.path = path;
	}
}