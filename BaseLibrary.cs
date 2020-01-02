﻿using BaseLibrary.Tiles.TileEntites;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;

namespace BaseLibrary
{
	// note: update checker via github releases

	public class BaseLibrary : Mod
	{
		internal static List<UI.IHasUI> ClosedUICache = new List<UI.IHasUI>();

		public static Effect ColorSelectionShader { get; private set; }
		public static Effect DesaturateShader { get; private set; }
		public static Effect RoundedRectShader { get; private set; }

		public static Texture2D texturePanelBackground;
		public static Texture2D texturePanelBorder;

		public static GUI<UI.PanelUI> PanelGUI { get; private set; }

		private LegacyGameInterfaceLayer MouseInterface;
		internal static ModHotKey hotkey;

		public override void Load()
		{
			TagSerializer.AddSerializer(new GUIDSerializer());

			Dispatcher.Load();
			Hooking.Load();

			if (!Main.dedServ)
			{
				texturePanelBackground = ModContent.GetTexture("Terraria/UI/PanelBackground");
				texturePanelBorder = ModContent.GetTexture("Terraria/UI/PanelBorder");

				hotkey = RegisterHotKey("Test hotkey", Keys.OemOpenBrackets.ToString());

				Utility.Font = GetFont("Fonts/Mouse_Text");
				typeof(DynamicSpriteFont).SetValue("_characterSpacing", 1f, Utility.Font);

				ColorSelectionShader = GetEffect("Effects/ColorSelectionShader");
				DesaturateShader = GetEffect("Effects/DesaturateShader");
				RoundedRectShader = GetEffect("Effects/BorderRadius");

				PanelGUI = Utility.SetupGUI<UI.PanelUI>();

				MouseInterface = new LegacyGameInterfaceLayer("BaseLibrary: MouseText", Utility.DrawMouseText, InterfaceScaleType.UI);
			}
		}

		public override void Unload()
		{
			Dispatcher.Unload();

			this.UnloadNullableTypes();
		}

		public override void PostSetupContent()
		{
			Input.Input.Load();
			Utility.Cache.Load();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

			if (MouseTextIndex != -1)
			{
				layers.Insert(MouseTextIndex + 1, MouseInterface);
				layers.Insert(MouseTextIndex, PanelGUI.InterfaceLayer);
				layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer("aa", delegate
				{
					foreach (Layer layer in Input.Input.Layers)
					{
						layer.OnDraw(Main.spriteBatch);
					}

					return true;
				}, InterfaceScaleType.UI));
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			var gui = PanelGUI;

			for (int i = 0; i < gui.Elements.Count; i++)
			{
				UIElement element = gui.Elements[i];
				if (element is UI.BaseUIPanel panel && panel.Container is BaseTE tileEntity)
				{
					TileObjectData data = TileObjectData.GetTileData(tileEntity.mod.GetTile(tileEntity.TileType.Name).Type, 0);
					Vector2 offset = data != null ? new Vector2(data.Width * 8f, data.Height * 8f) : Vector2.Zero;

					if (Vector2.DistanceSquared(tileEntity.Position.ToWorldCoordinates(offset), Main.LocalPlayer.Center) > 160 * 160) gui.UI.CloseUI(panel.Container);
				}
			}

			if (!Main.playerInventory)
			{
				List<UI.BaseUIPanel> bagPanels = gui.Elements.Cast<UI.BaseUIPanel>().ToList();
				foreach (UI.BaseUIPanel ui in bagPanels)
				{
					ClosedUICache.Add(ui.Container);
					gui.UI.CloseUI(ui.Container);
				}
			}
			else
			{
				foreach (UI.IHasUI ui in ClosedUICache) gui.UI.OpenUI(ui);

				ClosedUICache.Clear();
			}

			gui?.Update(gameTime);

			foreach (Layer layer in Input.Input.Layers) layer.OnUpdate(gameTime);
		}

		public override void PreSaveAndQuit()
		{
			ClosedUICache.Clear();
			PanelGUI.UI.CloseAllUIs();
		}
	}
}