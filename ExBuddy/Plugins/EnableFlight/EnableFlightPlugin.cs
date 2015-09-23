﻿
#pragma warning disable 1998

namespace ExBuddy.Plugins.EnableFlight
{
	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Buddy.Coroutines;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Navigation;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Enums;
	using ff14bot.Helpers;
	using ff14bot.Navigation;
	using ff14bot.RemoteWindows;

	using TreeSharp;

	[LoggerName("EnableFlight")]
	public class EnableFlightPlugin : ExBotPlugin<EnableFlightPlugin>
	{
		private Composite startCoroutine;

		private Composite deathCoroutine;

		private FlightEnabledNavigator navigator;

		private BotEvent cleanup;

		public override string Name
		{
			get
			{
				return "EnableFlight";
			}
		}

		protected override Color Info
		{
			get
			{
				return Colors.LightSteelBlue;
			}
		}

		private async Task<bool> HandleDeath()
		{
			if (Poi.Current.Type != PoiType.Death)
			{
				// We are not dead, continue
				return false;
			}

			var returnStrategy = Behaviors.GetReturnStrategy();

			await Coroutine.Wait(3000, () => ClientGameUiRevive.ReviveState == ReviveState.Dead);

			var ticks = 0;
			while (ClientGameUiRevive.ReviveState == ReviveState.Dead && ticks++ < 5)
			{
				ClientGameUiRevive.Revive();
				await Coroutine.Wait(5000, () => ClientGameUiRevive.ReviveState != ReviveState.Dead);
			}

			await Coroutine.Wait(15000, () => ClientGameUiRevive.ReviveState == ReviveState.None);

			Poi.Clear("We live!, now to get back to where we were. Using return strategy -> " + returnStrategy);

			await returnStrategy.ReturnToZone();

			if (EnableFlightSettings.Instance.ReturnToLocationOnDeath)
			{
				await returnStrategy.ReturnToLocation();
			}

			return false;
		}

		private async Task<bool> Start()
		{
			if (navigator == null)
			{
				var settings = EnableFlightSettings.Instance;
				navigator = new FlightEnabledNavigator(
					Navigator.NavigationProvider,
					new FlightEnabledSlideMover(Navigator.PlayerMover, new FlightMovementArgs { MountId = settings.MountId }),
					new FlightNavigationArgs
						{
							ForcedAltitude = settings.ForcedAltitude,
							InverseParabolicMagnitude = settings.InverseParabolicMagnitude,
							Radius = settings.Radius,
							Smoothing = settings.Smoothing
						});

				cleanup = bot =>
					{
						DoCleanup();
						DisposeNav();
						TreeRoot.OnStop -= cleanup;
					};

				TreeRoot.OnStop += cleanup;

				Logger.Info("Started Flight Navigator.");
			}

			return false;
		}

		private void DoCleanup()
		{
			if (navigator != null)
			{
				Logger.Info("Stopped Flight Navigator.");
				navigator.Dispose();
				navigator = null;
			}
		}

		private void DisposeNav()
		{
			var nav = Navigator.NavigationProvider as GaiaNavigator;
			if (nav != null)
			{
				Logger.Info("Disposing the GaiaNavigator");
				try
				{
					nav.Dispose();
				}
				catch (NullReferenceException) {}
				finally
				{
					Navigator.NavigationProvider = null;
				}
			}
		}

		public override void OnShutdown()
		{
			TreeRoot.OnStop -= cleanup;
			DoCleanup();
		}

		public override void OnInitialize()
		{
			startCoroutine = new ActionRunCoroutine(ctx => Start());
			deathCoroutine = new ActionRunCoroutine(ctx => HandleDeath());
		}

		public override void OnDisabled()
		{
			TreeHooks.Instance.OnHooksCleared -= OnHooksCleared;
			TreeHooks.Instance.RemoveHook("TreeStart", startCoroutine);
			TreeHooks.Instance.RemoveHook("PoiAction", deathCoroutine);
			TreeRoot.OnStop -= cleanup;
			DoCleanup();
		}

		public override void OnEnabled()
		{
			TreeHooks.Instance.AddHook("TreeStart", startCoroutine);
			TreeHooks.Instance.AddHook("PoiAction", deathCoroutine);
			TreeHooks.Instance.OnHooksCleared += OnHooksCleared;
		}

		private void OnHooksCleared(object sender, EventArgs args)
		{
			TreeHooks.Instance.AddHook("TreeStart", startCoroutine);
			TreeHooks.Instance.AddHook("PoiAction", deathCoroutine);
		}
	}

	public class EnableFlightSettings : JsonSettings
	{
		private static EnableFlightSettings instance;

		public static EnableFlightSettings Instance
		{
			get
			{
				return instance ?? (instance = new EnableFlightSettings("EnableFlightSettings"));
			}
		}

		// ReSharper disable once UnusedParameter.Local
		public EnableFlightSettings(string path)
			: base(Path.Combine(SettingsPath, "EnableFlight.json")) {}

		[DefaultValue(3.0f)]
		public float Radius { get; set; }

		[DefaultValue(5)]
		public int InverseParabolicMagnitude { get; set; }

		[DefaultValue(0.1f)]
		public float Smoothing { get; set; }

		[DefaultValue(6.0f)]
		public float ForcedAltitude { get; set; }

		[DefaultValue(false)]
		public bool ReturnToLocationOnDeath { get; set; }

		[DefaultValue(0)]
		public int MountId { get; set; }
	}
}