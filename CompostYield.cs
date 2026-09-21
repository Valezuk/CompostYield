using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace CompostYield;

[BepInPlugin(ModGuid, ModName, ModVersion)]
[BepInDependency("Raelaziel.OdinArchitect", BepInDependency.DependencyFlags.HardDependency)]
public class CompostYieldPlugin : BaseUnityPlugin
{
	public const string ModName = "CompostYield";
	public const string ModVersion = "1.0.0";
	public const string ModGuid = "ezuku.mods.compostyield";

	private static readonly ConfigSync configSync = new(ModGuid)
	{
		DisplayName = ModName,
		CurrentVersion = ModVersion,
		MinimumRequiredVersion = ModVersion,
		ModRequired = false
	};

	private static ConfigEntry<bool> serverConfigLocked = null!;
	private static ConfigEntry<int> yieldMultiplier = null!;
	private static ConfigEntry<string> stationPrefabs = null!;

	private static readonly HashSet<string> stations = new(StringComparer.OrdinalIgnoreCase);

	private void Awake()
	{
		serverConfigLocked = config("1 - General", "Lock Configuration", true,
			"If on, the configuration is locked and can be changed by server admins only.");
		configSync.AddLockingConfigEntry(serverConfigLocked);

		yieldMultiplier = config("1 - General", "Yield multiplier", 5,
			new ConfigDescription("Number of items the station produces per conversion, instead of one.",
				new AcceptableValueRange<int>(1, 20)));

		stationPrefabs = config("1 - General", "Stations", "rae_compost",
			"Comma separated prefab names of the stations the multiplier applies to.");
		stationPrefabs.SettingChanged += (_, _) => RefreshStations();
		RefreshStations();

		new Harmony(ModGuid).PatchAll(Assembly.GetExecutingAssembly());

		Logger.LogInfo($"{ModName} {ModVersion} loaded, multiplier {yieldMultiplier.Value} for [{stationPrefabs.Value}]");
	}

	private static void RefreshStations()
	{
		stations.Clear();
		foreach (string station in stationPrefabs.Value.Split(','))
		{
			string trimmed = station.Trim();
			if (trimmed.Length > 0)
			{
				stations.Add(trimmed);
			}
		}
	}

	private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
	{
		ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
		configSync.AddConfigEntry(configEntry);
		return configEntry;
	}

	private ConfigEntry<T> config<T>(string group, string name, T value, string description) =>
		config(group, name, value, new ConfigDescription(description));

	[HarmonyPatch(typeof(Smelter), "Spawn")]
	private static class SmelterSpawnPatch
	{
		private static readonly MethodInfo spawn =
			AccessTools.DeclaredMethod(typeof(Smelter), "Spawn", new[] { typeof(string), typeof(int) });

		// Set by the prefix when the multiplied stack does not fit into a single item drop.
		private static int overflow;
		private static bool spawningOverflow;

		private static void Prefix(Smelter __instance, string ore, ref int stack)
		{
			overflow = 0;
			if (spawningOverflow || stack <= 0 || !stations.Contains(Utils.GetPrefabName(__instance.gameObject)))
			{
				return;
			}

			int multiplier = yieldMultiplier.Value;
			if (multiplier <= 1)
			{
				return;
			}

			long total = (long)stack * multiplier;
			int maxStack = MaxStackSize(__instance, ore);
			if (total > maxStack)
			{
				stack = maxStack;
				overflow = (int)Math.Min(total - maxStack, int.MaxValue);
			}
			else
			{
				stack = (int)total;
			}
		}

		private static void Postfix(Smelter __instance, string ore)
		{
			if (spawningOverflow || overflow <= 0)
			{
				return;
			}

			int remaining = overflow;
			overflow = 0;
			int maxStack = MaxStackSize(__instance, ore);
			spawningOverflow = true;
			try
			{
				while (remaining > 0)
				{
					int chunk = Math.Min(remaining, maxStack);
					spawn.Invoke(__instance, new object[] { ore, chunk });
					remaining -= chunk;
				}
			}
			finally
			{
				spawningOverflow = false;
			}
		}

		private static int MaxStackSize(Smelter smelter, string ore)
		{
			foreach (Smelter.ItemConversion conversion in smelter.m_conversion)
			{
				if (conversion?.m_from == null || conversion.m_to == null || conversion.m_from.gameObject.name != ore)
				{
					continue;
				}

				int maxStackSize = conversion.m_to.m_itemData.m_shared.m_maxStackSize;
				return maxStackSize > 0 ? maxStackSize : int.MaxValue;
			}

			return int.MaxValue;
		}
	}
}
