﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events;
using TsRandomizer.Archipelago;
using TsRandomizer.Extensions;
using TsRandomizer.IntermediateObjects;
using TsRandomizer.Randomisation;
using TsRandomizer.Randomisation.ItemPlacers;
using TsRandomizer.Screens;
using TsRandomizer.Settings;

namespace TsRandomizer.LevelObjects
{
	class RoomTrigger
	{
		static readonly LookupDictionary<RoomItemKey, RoomTrigger> RoomTriggers = new LookupDictionary<RoomItemKey, RoomTrigger>(rt => rt.key);

		static readonly Type TransitionWarpEventType = TimeSpinnerType.Get("Timespinner.GameObjects.Events.Doors.TransitionWarpEvent");
		static readonly Type GyreType = TimeSpinnerType.Get("Timespinner.GameObjects.Events.Doors.GyrePortalEvent");
		static readonly Type NelisteNpcType = TimeSpinnerType.Get("Timespinner.GameObjects.NPCs.AstrologerNPC");
		static readonly Type YorneNpcType = TimeSpinnerType.Get("Timespinner.GameObjects.NPCs.YorneNPC");
		static readonly Type GlowingFloorEventType = TimeSpinnerType.Get("Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab.EnvPrefabLabVilete");
		static readonly Type PedestalType = TimeSpinnerType.Get("Timespinner.GameObjects.Events.Treasure.OrbPedestalEvent");
		static readonly Type LakeVacuumLevelEffectType = TimeSpinnerType.Get("Timespinner.GameObjects.Events.LevelEffects.LakeVacuumLevelEffect");

		static RoomTrigger()
		{
			RoomTriggers.Add(new RoomTrigger(0, 3, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (seedOptions.StartWithJewelryBox)
					level.AsDynamic().UnlockRelic(EInventoryRelicType.JewelryBox);

				if (seedOptions.StartWithMeyef)
				{
					level.GameSave.AddItem(level, new ItemIdentifier(EInventoryFamiliarType.Meyef));
					level.GameSave.Inventory.EquippedFamiliar = EInventoryFamiliarType.Meyef;

					var luniasObject = level.MainHero.AsDynamic();
					var familiarManager = ((object)luniasObject._familiarManager).AsDynamic();

					familiarManager.ChangeFamiliar(EInventoryFamiliarType.Meyef);
					familiarManager.AddFamiliarPoofAnimation();
				}

				if (seedOptions.StartWithTalaria)
					level.AsDynamic().UnlockRelic(EInventoryRelicType.Dash);
			}));
			RoomTriggers.Add(new RoomTrigger(1, 0, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.Inverted || level.GameSave.GetSaveBool("TSRandomizerHasTeleportedPlayer")) return;

				level.GameSave.SetValue("TSRandomizerHasTeleportedPlayer", true);

				level.RequestChangeLevel(new LevelChangeRequest { LevelID = 3, RoomID = 6 }); //Refugee Camp

				level.GameSave.SetCutsceneTriggered("LakeDesolation1_Entrance", true); // Fixes music when returning to Lake Desolation later
			}));
			RoomTriggers.Add(new RoomTrigger(1, 5, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp || !level.GameSave.GetSaveBool("IsBossDead_RoboKitty")) return;

				SpawnItemDropPickup(level, itemLocation.ItemInfo, 200, 208);
			}));
			RoomTriggers.Add(new RoomTrigger(5, 5, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp || !level.GameSave.HasCutsceneBeenTriggered("Keep0_Demons0")) return;

				SpawnItemDropPickup(level, itemLocation.ItemInfo, 200, 208);
			}));
			RoomTriggers.Add(new RoomTrigger(11, 1, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp || !level.GameSave.HasRelic(EInventoryRelicType.Dash)) return;

				SpawnItemDropPickup(level, itemLocation.ItemInfo, 280, 191);
			}));
			RoomTriggers.Add(new RoomTrigger(11, 39, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp
					|| !level.GameSave.HasOrb(EInventoryOrbType.Eye)
					|| !level.GameSave.GetSaveBool("11_LabPower")) return;

				SpawnItemDropPickup(level, itemLocation.ItemInfo, 200, 176);
			}));
			RoomTriggers.Add(new RoomTrigger(11, 21, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!itemLocation.IsPickedUp
					&& level.GameSave.HasRelic(EInventoryRelicType.ScienceKeycardA)
					&& level.GameSave.GetSaveBool("IsBossDead_Shapeshift"))
					SpawnItemDropPickup(level, itemLocation.ItemInfo, 200, 208);

				if(!seedOptions.Inverted && level.GameSave.HasCutsceneBeenTriggered("Alt3_Teleport"))
					CreateSimpleOneWayWarp(level, 16, 12);
			}));
			RoomTriggers.Add(new RoomTrigger(7, 5, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.Cantoran)
					return;
				// Set Cantoran quest active when fighting Pink Bird
				if (!level.GameSave.GetSaveBool("IsBossDead_Cantoran"))
				{
					level.GameSave.SetValue("IsCantoranActive", true);
					return;
				}

				// Spawn item if the room has been left without aquiring (only needed if Radiant-element possessed)
				if (!itemLocation.IsPickedUp
					&& level.GameSave.GetSaveBool("IsBossDead_Cantoran")
					&& level.GameSave.HasOrb(EInventoryOrbType.Barrier))
					SpawnItemDropPickup(level, itemLocation.ItemInfo, 170, 194);
			}));
			RoomTriggers.Add(new RoomTrigger(11, 26, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp
					|| !level.GameSave.HasRelic(EInventoryRelicType.TimespinnerGear1)) return;

				SpawnTreasureChest(level, true, 136, 192);
			}));
			RoomTriggers.Add(new RoomTrigger(2, 52, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp
					|| !level.GameSave.HasRelic(EInventoryRelicType.TimespinnerGear2)) return;

				SpawnTreasureChest(level, true, 104, 192);
			}));
			RoomTriggers.Add(new RoomTrigger(9, 13, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp
					|| !level.GameSave.HasRelic(EInventoryRelicType.TimespinnerGear3)) return;

				SpawnTreasureChest(level, false, 296, 176);
			}));
			RoomTriggers.Add(new RoomTrigger(3, 6, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (seedOptions.Inverted || level.GameSave.HasRelic(EInventoryRelicType.PyramidsKey)) return;

				CreateSimpleOneWayWarp(level, 2, 54);
			}));
			RoomTriggers.Add(new RoomTrigger(2, 54, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (seedOptions.Inverted
					|| level.GameSave.HasRelic(EInventoryRelicType.PyramidsKey)
					|| !level.GameSave.DataKeyBools.ContainsKey("HasUsedCityTS")) return;

				CreateSimpleOneWayWarp(level, 3, 6);
			}));
			RoomTriggers.Add(new RoomTrigger(7, 30, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!level.GameSave.HasRelic(EInventoryRelicType.PyramidsKey)) return;

				SpawnTreasureChest(level, false, 296, 176);
			}));
			RoomTriggers.Add(new RoomTrigger(3, 0, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (itemLocation.IsPickedUp
					|| level.GameSave.DataKeyBools.ContainsKey("HasUsedCityTS")
					|| !level.GameSave.HasCutsceneBeenTriggered("Forest3_Haristel")
					|| ((Dictionary<int, NPCBase>)level.AsDynamic()._npcs).Values.Any(npc => npc.GetType() == NelisteNpcType)) return;

				SpawnNeliste(level);
			}));
			// Spawn Gyre portals when applicable
			RoomTriggers.Add(new RoomTrigger(11, 4, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GyreArchives || !level.GameSave.HasFamiliar(EInventoryFamiliarType.MerchantCrow))
					return;

				SpawnGyreWarp(level, 200, 200); // Historical Documents room to Ravenlord
			}));
			RoomTriggers.Add(new RoomTrigger(14, 24, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GyreArchives)
					return;

				level.JukeBox.StopSong();

				level.RequestChangeLevel(new LevelChangeRequest
				{
					LevelID = 11,
					RoomID = 4,
					IsUsingWarp = true,
					IsUsingWhiteFadeOut = true,
					FadeInTime = 0.5f,
					FadeOutTime = 0.25f
				}); // Ravenlord to Historical Documents room

				level.JukeBox.PlaySong(Timespinner.GameAbstractions.EBGM.Level11);
			}));
			RoomTriggers.Add(new RoomTrigger(2, 51, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GyreArchives) return;
				if (level.GameSave.HasFamiliar(EInventoryFamiliarType.Kobo))
				{
					SpawnGyreWarp(level, 200, 200); // Portrait room to Ifrit
					return;
				};

				if (((Dictionary<int, NPCBase>)level.AsDynamic()._npcs).Values.Any(npc => npc.GetType() == YorneNpcType)) return;
				SpawnYorne(level); // Dialog for needing Kobo
			}));
			RoomTriggers.Add(new RoomTrigger(14, 25, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GyreArchives) return;
				level.JukeBox.StopSong();
				level.RequestChangeLevel(new LevelChangeRequest
				{
					LevelID = 2,
					RoomID = 51,
					IsUsingWarp = true,
					IsUsingWhiteFadeOut = true,
					FadeInTime = 0.5f,
					FadeOutTime = 0.25f
				}); // Ifrit to Portrait room
				level.JukeBox.PlaySong(Timespinner.GameAbstractions.EBGM.Library);
			}));
			RoomTriggers.Add(new RoomTrigger(10, 0, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				// Spawn warp after ship crashes
				if (!seedOptions.GyreArchives || !level.GameSave.GetSaveBool("IsPastCleared"))
					return;
				SpawnGyreWarp(level, 340, 180); // Military Hangar crash site to Gyre
			}));
			RoomTriggers.Add(new RoomTrigger(14, 11, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				// Play Gyre music in gyre
				level.JukeBox.PlaySong(Timespinner.GameAbstractions.EBGM.Level14);
				level.AsDynamic().SetLevelSaveInt("GyreDungeonSeed", seedOptions.Flags.GetHashCode()); // Warp to Ravenlord
			}));
			RoomTriggers.Add(new RoomTrigger(14, 23, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				level.JukeBox.StopSong();
				level.RequestChangeLevel(new LevelChangeRequest
				{
					LevelID = 10,
					RoomID = 0,
					IsUsingWarp = true,
					IsUsingWhiteFadeOut = true,
					FadeInTime = 0.5f,
					FadeOutTime = 0.25f
				}); // Military Hangar crash site
				level.JukeBox.PlaySong(Timespinner.GameAbstractions.EBGM.Level10);
			}));
			RoomTriggers.Add(new RoomTrigger(12, 11, (level, itemLocation, seedOptions, gameSettings, screenManager) => // Remove Daddy's pedestal if you havent killed him yet
			{
				if (level.GameSave.DataKeyBools.ContainsKey("IsEndingABCleared")) return;

				((Dictionary<int, GameEvent>)level.AsDynamic()._levelEvents).Values
					.FirstOrDefault(obj => obj.GetType() == PedestalType)
					?.SilentKill();
			}));
			RoomTriggers.Add(new RoomTrigger(8, 6, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GassMaw) return;

				FillRoomWithGass(level);
			}));
			RoomTriggers.Add(new RoomTrigger(8, 7, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GassMaw) return;

				FillRoomWithGass(level);
			}));
			RoomTriggers.Add(new RoomTrigger(8, 13, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GassMaw) return;

				FillRoomWithGass(level);
			}));
			RoomTriggers.Add(new RoomTrigger(8, 21, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (seedOptions.GassMaw) FillRoomWithGass(level);

				var levelReflected = level.AsDynamic();
				IEnumerable<Animate> eventObjects = levelReflected._levelEvents.Values;

				if (!itemLocation.IsPickedUp &&
					!eventObjects.Any(o => o.GetType().ToString() ==
						"Timespinner.GameObjects.Events.EnvironmentPrefabs.EnvPrefabCavesRadiationCrystal"))
					SpawnItemDropPickup(level, itemLocation.ItemInfo, 312, 912);
			}));
			RoomTriggers.Add(new RoomTrigger(8, 33, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				if (!seedOptions.GassMaw) return;

				FillRoomWithGass(level);
			}));
			RoomTriggers.Add(new RoomTrigger(16, 1, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				// Allow the post-Nightmare chest to spawn
				level.GameSave.SetValue("IsGameCleared", true);
				level.GameSave.SetValue("IsEndingCDCleared", true);
			}));
			RoomTriggers.Add(new RoomTrigger(16, 21, (level, itemLocation, seedOptions, gameSettings, screenManager) => {
				// Spawn glowing floor event to give a soft-lock exit warp
				if (((Dictionary<int, NPCBase>)level.AsDynamic()._npcs).Values.Any(npc => npc.GetType() == GlowingFloorEventType)) return;
				SpawnGlowingFloor(level);
			}));
			RoomTriggers.Add(new RoomTrigger(16, 27, (level, itemLocation, seedOptions,  gameSettings, screenManager) =>
			{
				if (!level.GameSave.DataKeyStrings.ContainsKey(ArchipelagoItemLocationRandomizer.GameSaveServerKey)) return;

				var forfeitFlags = Client.ForfeitPermissions;

				if (!forfeitFlags.HasFlag(Permissions.Auto) &&
					(forfeitFlags.HasFlag(Permissions.Enabled) || forfeitFlags.HasFlag(Permissions.Goal)))
				{
					var messageBox = MessageBox.Create(screenManager, "Press OK for forfeit remaining item checks", _ => {
						Client.Say("!forfeit");
					});

					screenManager.AddScreen(messageBox.Screen, null);
				}
			}));
		}

		readonly RoomItemKey key;
		readonly Action<Level, ItemLocation, SeedOptions, SettingCollection, ScreenManager> trigger;

		public RoomTrigger(int levelId, int roomId, Action<Level, ItemLocation, SeedOptions, SettingCollection, ScreenManager> triggerMethod)
		{
			key = new RoomItemKey(levelId, roomId);
			trigger = triggerMethod;
		}

		public static void OnChangeRoom(
			Level level, SeedOptions seedOptions, SettingCollection gameSettings, ItemLocationMap itemLocations, ScreenManager screenManager,
			int levelId, int roomId)
		{
			var roomKey = new RoomItemKey(levelId, roomId);

			if (RoomTriggers.TryGetValue(roomKey, out var trigger))
				trigger.trigger(level, itemLocations[roomKey], seedOptions, gameSettings, screenManager);
		}

		static void SpawnItemDropPickup(Level level, ItemInfo itemInfo, int x, int y)
		{
			var itemDropPickupType = TimeSpinnerType.Get("Timespinner.GameObjects.Items.ItemDropPickup");
			var itemPosition = new Point(x, y);
			var itemDropPickup = Activator.CreateInstance(itemDropPickupType, itemInfo.BestiaryItemDropSpecification, level, itemPosition, -1);

			var item = itemDropPickup.AsDynamic();
			item.Initialize();

			var levelReflected = level.AsDynamic();
			levelReflected.RequestAddObject((Item)itemDropPickup);
		}

		static void SpawnTreasureChest(Level level, bool flipHorizontally, int x, int y)
		{
			var itemPosition = new Point(x, y);
			var specification = new ObjectTileSpecification { IsFlippedHorizontally = flipHorizontally, Layer = ETileLayerType.Objects };
			var treasureChest = new TreasureChestEvent(level, itemPosition, -1, specification);

			var chest = treasureChest.AsDynamic();
			chest.Initialize();

			var levelReflected = level.AsDynamic();
			levelReflected.RequestAddObject(treasureChest);
		}

		static void SpawnOrbPredestal(Level level, int x, int y)
		{
			var orbPedestalEventType = TimeSpinnerType.Get("Timespinner.GameObjects.Events.Treasure.OrbPedestalEvent");
			var itemPosition = new Point(x, y);
			var pedistalSpecification = new TileSpecification
			{
				Argument = (int)EInventoryOrbType.Monske,
				ID = 480, //orb pedistal
				Layer = ETileLayerType.Objects,
			};
			var orbPedestalEvent = Activator.CreateInstance(orbPedestalEventType, level, itemPosition, -1, ObjectTileSpecification.FromTileSpecification(pedistalSpecification));

			var pedestal = orbPedestalEvent.AsDynamic();
			pedestal.DoesSpawnDespiteBeingOwned = true;
			pedestal.Initialize();

			var levelReflected = level.AsDynamic();
			levelReflected.RequestAddObject((GameEvent)orbPedestalEvent);
		}

		static void CreateSimpleOneWayWarp(Level level, int destinationLevelId, int destinationRoomId)
		{
			var dynamicLevel = level.AsDynamic();

			Dictionary<int, GameEvent> events = dynamicLevel._levelEvents;
			var warpTrigger = events.Values.FirstOrDefault(e => e.GetType() == TransitionWarpEventType);
			if (warpTrigger == null)
			{
				var specification = new ObjectTileSpecification
				{
					Category = EObjectTileCategory.Event,
					ID = 468,
					Layer = ETileLayerType.Objects,
					ObjectID = 13,
					X = 12,
					Y = 12
				};
				var point = new Point(specification.X * 16 + 8, specification.Y * 16 + 16);
				warpTrigger = (GameEvent)TransitionWarpEventType.CreateInstance(false, level, point, -1, specification);

				dynamicLevel.RequestAddObject(warpTrigger);
			}

			var dynamicWarpTrigger = warpTrigger.AsDynamic();

			var backToTheFutureWarp =
				new RequestButtonPressTrigger(level, warpTrigger.Position, dynamicWarpTrigger._objectSpec, (Action)delegate
				{
					dynamicWarpTrigger.StartWarpSequence(new LevelChangeRequest
					{
						LevelID = destinationLevelId,
						PreviousLevelID = level.ID,
						RoomID = destinationRoomId,
						IsUsingWarp = true,
						IsUsingWhiteFadeOut = true,
						AdditionalBlackScreenTime = 0.25f,
						FadeOutTime = 0.25f,
						FadeInTime = 1f
					});
				});

			dynamicLevel.RequestAddObject(backToTheFutureWarp);
		}

		static void SpawnNeliste(Level level)
		{
			var position = new Point(720, 368);
			var neliste = (NPCBase)NelisteNpcType.CreateInstance(false, level, position, -1, new ObjectTileSpecification());

			level.AsDynamic().RequestAddObject(neliste);
		}

		static void SpawnGlowingFloor(Level level)
		{
			var position = new Point(100, 195);
			var floor = GlowingFloorEventType.CreateInstance(false, level, position, -1, new ObjectTileSpecification(), EEnvironmentPrefabType.L0_TableCake);

			level.AsDynamic().RequestAddObject(floor);
		}

		static void SpawnYorne(Level level)
		{
			var position = new Point(240, 215);
			var yorne = (NPCBase)YorneNpcType.CreateInstance(false, level, position, -1, new ObjectTileSpecification());

			level.AsDynamic().RequestAddObject(yorne);
		}

		static void SpawnGyreWarp(Level level, int x, int y)
		{
			var position = new Point(x, y);
			var gyrePortal = GyreType.CreateInstance(false, level, position, -1, new ObjectTileSpecification());

			level.AsDynamic().RequestAddObject(gyrePortal);
		}

		static void FillRoomWithGass(Level level)
		{
			var gass = (GameEvent)LakeVacuumLevelEffectType.CreateInstance(false, level, new Point(), -1, new ObjectTileSpecification());

			level.AsDynamic().RequestAddObject(gass);

			var foreground = level.Foregrounds.FirstOrDefault();

			if (foreground == null)
				return;

			foreground.AsDynamic()._baseColor = new Color(8, 16, 2, 12);
			foreground.DrawColor = new Color(8, 16, 2, 12);
		}
	}
}
