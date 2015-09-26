﻿namespace ExBuddy.RemoteWindows
{
	using System;
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using ExBuddy.Enums;
	using ExBuddy.Helpers;
	using ExBuddy.Logging;

	using ff14bot;
	using ff14bot.Behavior;
	using ff14bot.Managers;

	public abstract class Window<T>
		where T : Window<T>, new()
	{
		// ReSharper disable once StaticMemberInGenericType
		private static Action updateWindows;

		static Window()
		{
			updateWindows = RaptureAtkUnitManager.Update;
			TreeRoot.OnStart += TreeRootOnStart;
			TreeRoot.OnStop += TreeRootOnStop;
		}

		private static void TreeRootOnStop(ff14bot.AClasses.BotBase bot)
		{
			updateWindows = RaptureAtkUnitManager.Update;
		}

		private static void TreeRootOnStart(ff14bot.AClasses.BotBase bot)
		{
			if (bot.PulseFlags.HasFlag(PulseFlags.Windows))
			{
				updateWindows = () => { };
			}
		}

		private AtkAddonControl control;

		protected Window(string name)
		{
			Name = name;
			control = RaptureAtkUnitManager.GetWindowByName(name);
		}

		public static bool IsOpen
		{
			get
			{
				return new T().Control != null;
			}
		}

		public static AtkAddonControl AtkAddonControl
		{
			get
			{
				return new T().Control;
			}
		}

		public static void Close()
		{
			new T().Control.TrySendAction(1, 3, uint.MaxValue);
		}

		public static async Task<bool> CloseGently(byte maxTicks = 10, ushort interval = 200)
		{
			return await new T().CloseInstanceGently(maxTicks, interval);
		}

		public bool IsValid
		{
			get
			{
				return Control != null && Control.IsValid;
			}
		}

		public string Name { get; private set; }

		public virtual AtkAddonControl Control
		{
			get
			{
				return control ?? Refresh().control;
			}
		}

		public virtual async Task<SendActionResult> CloseInstance(ushort interval = 200)
		{
			await Sleep(interval / 2);

			Logger.Instance.Verbose("Attempting to close the [{0}] window", Name);

			var result = TrySendAction(1, 3, uint.MaxValue);

			await Refresh(interval / 2, false);

			if (result == SendActionResult.Success)
			{
				if (!IsValid)
				{
					Logger.Instance.Verbose("The [{0}] window has been closed.", Name);
					return result;
				}

				Logger.Instance.Verbose("Unexpected result while closing [{0}], we may be trying to close too early.", Name);
				return SendActionResult.UnexpectedResult;
			}

			if (result == SendActionResult.InvalidWindow)
			{
				Logger.Instance.Verbose("The [{0}] window was not valid, it was either not open or closed on its own.", Name);
			}

			return result;
		}

		public virtual async Task<bool> CloseInstanceGently(byte maxTicks = 10, ushort interval = 200)
		{
			if (!IsValid)
			{
				return true;
			}

			if (await CloseInstance(interval) == SendActionResult.Success)
			{
				if (!IsValid)
				{
					return true;
				}
			}

			await Sleep(interval);

			var result = SendActionResult.None;
			var ticks = 0;
			while (result != SendActionResult.Success && ticks++ < maxTicks && IsValid && Behaviors.ShouldContinue)
			{
				if (result == SendActionResult.InvalidWindow)
				{
					return true;
				}

				result = await CloseInstance(interval);
			}

			return result > SendActionResult.UnexpectedResult && !IsValid;
		}

		public virtual SendActionResult TrySendAction(int pairCount, params uint[] param)
		{
			return Control.TrySendAction(pairCount, param);
		}

		public T Refresh()
		{
			updateWindows();
			control = RaptureAtkUnitManager.GetWindowByName(Name);
			return (T)this;
		}

		public async Task<bool> Refresh(int timeoutMs, bool valid = true)
		{
			return await Coroutine.Wait(timeoutMs, () => Refresh().IsValid == valid);
		}

		protected async Task Sleep(int interval)
		{
			if (interval <= 33)
			{
				await Coroutine.Yield();
			}
			else
			{
				await Coroutine.Sleep(interval);
			}
		}

		protected async Task Wait(int interval, Func<bool> condition)
		{
			if (interval <= 33)
			{
				await Coroutine.Yield();
			}
			else
			{
				await Coroutine.Wait(interval, condition);
			}
		}
	}
}