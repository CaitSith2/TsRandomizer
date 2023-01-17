﻿using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using TsRandomizer.Extensions;
using TsRandomizer.IntermediateObjects;
using TsRandomizer.Randomisation;
using TsRandomizer.Screens;

namespace TsRandomizer.LevelObjects.ItemManipulators
{
	[TimeSpinnerType("Timespinner.GameObjects.Events.Cutscene.CutscenePrologue4")]
	// ReSharper disable once UnusedMember.Global
	class CutscenePrologue4 : ItemManipulator
	{
		bool hasAwardedMeleeOrb;

		public CutscenePrologue4(Mobile typedObject, ItemLocation itemLocation) : base(typedObject, itemLocation)
		{
		}

		protected override void Initialize(Seed seed)
		{
			if (ItemInfo == null)
				return;

			Scripts.UpdateRelicOrbGetToastToItem(Level, ItemInfo);
		}

		protected override void OnUpdate(GameplayScreen gameplayScreen)
		{
			if (ItemInfo == null)
				return;

			var orbCollection = Level.GameSave.Inventory.OrbInventory.Inventory;

			if (hasAwardedMeleeOrb || !orbCollection.ContainsKey((int)EInventoryOrbType.Blue))
				return;

			AwardContainedItem();
			hasAwardedMeleeOrb = true;
		}
	}
}
