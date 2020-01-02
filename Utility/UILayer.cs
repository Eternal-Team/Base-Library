﻿using BaseLibrary.Input;
using BaseLibrary.UI;
using BaseLibrary.UI.New;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace BaseLibrary
{
	public interface IHasUI
	{
		Guid UUID { get; set; }

		BaseUIPanel UI { get; set; }

		LegacySoundStyle CloseSound { get; }

		LegacySoundStyle OpenSound { get; }
	}

	public class BaseUIPanel : UIDraggablePanel
	{
		public IHasUI Container;
	}

	public class BaseUIPanel<T> : BaseUIPanel where T : IHasUI
	{
		public new T Container => (T)base.Container;
	}

	public class PanelUI : BaseElement
	{
		private static Dictionary<Type, Type> UICache;
		public static PanelUI Instance;

		public PanelUI()
		{
			Width.Percent = 100;
			Height.Percent = 100;

			UICache = new Dictionary<Type, Type>();
			Instance = this;

			foreach (Type type in ModLoader.Mods.Where(mod => mod.Name != "ModLoader").SelectMany(mod => mod.Code?.GetTypes()))
			{
				if (type.IsSubclassOfRawGeneric(typeof(BaseUIPanel<>)) && type.BaseType != null && type.BaseType.GenericTypeArguments.Length > 0) UICache[type.BaseType.GenericTypeArguments[0]] = type;
			}
		}

		public void HandleUI(IHasUI entity)
		{
			if (Main.netMode == NetmodeID.Server || entity == null) return;

			if (entity.UI != null) CloseUI(entity);
			else
			{
				/*if (!BaseLibrary.ClosedUICache.Contains(entity))*/
				OpenUI(entity);

				if (!Main.playerInventory) Main.playerInventory = true;
			}
		}

		public void CloseUI(IHasUI entity)
		{
			if (Main.netMode == NetmodeID.Server || entity == null) return;

			BaseElement element = entity.UI;
			if (element == null) return;

			Main.LocalPlayer.GetModPlayer<BLPlayer>().UIPositions[entity.UUID] = new Vector2(element.X.Percent, element.Y.Percent);
			//element.Deactivate();
			Remove(element);
			entity.UI = null;

			Main.PlaySound(entity.CloseSound);
		}

		public void OpenUI(IHasUI entity)
		{
			if (Main.netMode == NetmodeID.Server || entity == null) return;

			Type entityType = UICache.ContainsKey(entity.GetType()) ? entity.GetType() : entity.GetType().BaseType;

			entity.UI = (BaseUIPanel)Activator.CreateInstance(UICache[entityType]);
			entity.UI.Container = entity;

			//entity.UI.Activate();

			if (Main.LocalPlayer.GetModPlayer<BLPlayer>().UIPositions.TryGetValue(entity.UUID, out Vector2 position))
			{
				entity.UI.X.Percent = (int)position.X;
				entity.UI.Y.Percent = (int)position.Y;
			}

			entity.UI.OnMouseDown += args =>
			{
				if (entity.UI == null) return;

				Remove(entity.UI);
				Add(entity.UI);
			};

			Add(entity.UI);

			Main.PlaySound(entity.OpenSound);
		}

		public void CloseAllUIs()
		{
			for (int i = 0; i < Children.Count; i++)
			{
				BaseElement element = Children[i];
				if (element is BaseUIPanel panel)
				{
					Main.LocalPlayer.GetModPlayer<BLPlayer>().UIPositions[panel.Container.UUID] = new Vector2(element.X.Percent, element.Y.Percent);
					//element.Deactivate();
					panel.Container.UI = null;
				}
			}

			Clear();
		}

		public override BaseElement GetElementAt(Vector2Int point)
		{
			BaseElement element = Children.FirstOrDefault(current => current.ContainsPoint(point) && current.Display != Display.None);

			return element?.GetElementAt(point);
		}
	}

	public class UILayer : Layer
	{
		public override bool Enabled => !Main.ingameOptionsWindow && !Main.gameMenu;

		private List<BaseElement> Elements;
		private BaseElement Current;

		internal UILayer()
		{
			TestUI ui = new TestUI();
			ui.Recalculate();

			PanelUI panelUI = new PanelUI();
			panelUI.Recalculate();

			Elements = new List<BaseElement> { ui, panelUI };
		}

		private BaseElement MouseDownElement;

		public override void OnDraw(SpriteBatch spriteBatch)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None))
			{
				element.InternalDraw(spriteBatch);
			}
		}

		public override void OnUpdate(GameTime gameTime)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None))
			{
				element.InternalUpdate(gameTime);
			}
		}

		public override void OnMouseDown(MouseButtonEventArgs args)
		{
			var elements = Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)).ToList();
			foreach (BaseElement element in elements)
			{
				MouseDownElement = element.InternalMouseDown(args);
				if (args.Handled) break;
			}
		}

		public override void OnMouseUp(MouseButtonEventArgs args)
		{
			if (MouseDownElement != null)
			{
				MouseDownElement.InternalMouseUp(args);

				MouseDownElement = null;

				return;
			}

			var elements = Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)).ToList();
			foreach (BaseElement element in elements)
			{
				element.InternalMouseUp(args);
				if (args.Handled) break;
			}
		}

		public override void OnMouseMove(MouseMoveEventArgs args)
		{
			var elements = Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)).ToList();
			foreach (BaseElement element in elements)
			{
				element.InternalMouseMove(args);
				if (args.Handled) break;
			}

			BaseElement at = Elements.Select(baseElement => baseElement.GetElementAt(args.Position)).FirstOrDefault(baseElement => baseElement != null);
			if (Current != at)
			{
				Current?.InternalMouseLeave(args);
				at?.InternalMouseEnter(args);
				Current = at;

				foreach (string key in PlayerInput.MouseKeys)
				{
					foreach (var pair in PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus)
					{
						if (pair.Value.Contains(key))
						{
							PlayerInput.Triggers.Current.KeyStatus[pair.Key] = false;
						}
					}
				}

				PlayerInput.MouseKeys.Clear();
			}
		}

		public override void OnMouseScroll(MouseScrollEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)))
			{
				element.InternalMouseScroll(args);
				if (args.Handled) break;
			}
		}

		public override void OnClick(MouseButtonEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)))
			{
				element.InternalMouseClick(args);
				if (args.Handled) break;
			}
		}

		public override void OnDoubleClick(MouseButtonEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)))
			{
				element.InternalDoubleClick(args);
				if (args.Handled) break;
			}
		}

		public override void OnTripleClick(MouseButtonEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None && element.ContainsPoint(args.Position)))
			{
				element.InternalTripleClick(args);
				if (args.Handled) break;
			}
		}

		public override void OnKeyPressed(KeyboardEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None))
			{
				element.InternalKeyPressed(args);
				if (args.Handled) break;
			}
		}

		public override void OnKeyReleased(KeyboardEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None))
			{
				element.InternalKeyReleased(args);
				if (args.Handled) break;
			}
		}

		public override void OnKeyTyped(KeyboardEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None))
			{
				element.InternalKeyTyped(args);
				if (args.Handled) break;
			}
		}

		public override void OnWindowResize(WindowResizedEventArgs args)
		{
			foreach (BaseElement element in Elements.Where(element => element.Display != Display.None))
			{
				element.Recalculate();
			}
		}
	}
}