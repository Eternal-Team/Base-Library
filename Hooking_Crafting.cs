﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader.Container;

namespace BaseLibrary
{
	public interface ICraftingStorage : IItemHandler
	{
		ItemHandler GetItemHandlerForCrafting();
	}

	public static class Hooking
	{
		internal static void Initialize()
		{
			IL.Terraria.Recipe.FindRecipes += Recipe_FindRecipes;
			IL.Terraria.Recipe.Create += Recipe_Create;
			// IL.Terraria.Player.AdjTiles += Player_AdjTiles;

			// if (ModLoader.GetMod("BaseLibrary") != null) ItemSlot.OverrideHover += ItemSlot_OverrideHover;
		}

		private static void Recipe_FindRecipes(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(0), i => i.MatchStloc(31)))
			{
				cursor.Emit(OpCodes.Ldloc, 12);

				cursor.EmitDelegate<Func<Dictionary<int, int>, Dictionary<int, int>>>(availableItems =>
				{
					foreach (Item pItem in Main.LocalPlayer.inventory)
					{
						if (!(pItem.modItem is ICraftingStorage storage)) continue;
						ItemHandler handler = storage.GetItemHandlerForCrafting();

						for (int i = 0; i < handler.Slots; i++)
						{
							Item item = handler.GetItemInSlot(i);
							if (item.stack > 0)
							{
								if (availableItems.ContainsKey(item.netID)) availableItems[item.netID] += item.stack;
								else availableItems[item.netID] = item.stack;
							}
						}
					}

					return availableItems;
				});

				cursor.Emit(OpCodes.Stloc, 12);
			}
		}

		private static void Recipe_Create(ILContext il)
		{
			bool AcceptedByItemGroups(Recipe recipe, int invType, int reqType)
			{
				return recipe.acceptedGroups.Any(num =>
				{
					var group = RecipeGroup.recipeGroups[num];
					return group.ContainsItem(invType) && group.ContainsItem(reqType);
				});
			}

			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdarg(0), i => i.MatchCall<AchievementsHelper>("NotifyItemCraft")))
			{
				cursor.Emit(OpCodes.Ldarg, 0);
				cursor.Emit(OpCodes.Ldloc, 2);
				cursor.Emit(OpCodes.Ldloc, 4);

				cursor.EmitDelegate<Func<Recipe, Item, int, int>>((self, ingredient, amount) =>
				{
					foreach (Item pItem in Main.LocalPlayer.inventory)
					{
						if (!(pItem.modItem is ICraftingStorage storage)) continue;
						ItemHandler handler = storage.GetItemHandlerForCrafting();

						for (int i = 0; i < handler.Slots; i++)
						{
							if (amount <= 0) return amount;
							Item item = handler.GetItemInSlot(i);

							if (!item.IsTheSameAs(ingredient) && !AcceptedByItemGroups(self, item.type, ingredient.type)) continue;

							int count = Math.Min(amount, item.stack);
							amount -= count;
							handler.ExtractItem(i, out _, count);
						}
					}

					return amount;
				});

				cursor.Emit(OpCodes.Stloc, 4);
			}
		}

		// private static void ItemSlot_OverrideHover(ILContext il)
		// {
		// 	ILCursor cursor = new ILCursor(il);
		// 	ILLabel label = cursor.DefineLabel();
		// 	ILLabel caseStart = cursor.DefineLabel();
		//
		// 	ILLabel[] targets = null;
		// 	if (cursor.TryGotoNext(i => i.MatchSwitch(out targets))) targets[0] = caseStart;
		//
		// 	if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main).GetField("npcShop", Utility.defaultFlags))))
		// 	{
		// 		cursor.MarkLabel(caseStart);
		//
		// 		cursor.Emit(OpCodes.Ldloc, 0);
		// 		cursor.EmitDelegate<Func<Item, bool>>(item =>
		// 		{
		// 			BaseElement BaseElement = PanelUI.Instance.Children.FirstOrDefault(element => (element as IItemHandlerUI)?.Handler?.HasSpace(item) ?? false);
		// 			string texture = (BaseElement as IItemHandlerUI)?.GetTexture(item);
		//
		// 			if (!string.IsNullOrWhiteSpace(texture))
		// 			{
		// 				BaseLibrary.Hooking.SetCursor(texture);
		// 				return true;
		// 			}
		//
		// 			return false;
		// 		});
		// 		cursor.Emit(OpCodes.Brtrue, label);
		// 	}
		//
		// 	if (cursor.TryGotoNext(i => i.MatchLdsflda(typeof(Main).GetField("keyState", Utility.defaultFlags)))) cursor.MarkLabel(label);
		// }
	}
}