﻿namespace ExBuddy.RemoteWindows
{
	using System.Threading.Tasks;

	using Buddy.Coroutines;

	using ExBuddy.Enums;
	using ExBuddy.Helpers;

	using ff14bot.RemoteWindows;

	public sealed class ShopExchangeCurrency : Window<ShopExchangeCurrency>
	{
		public ShopExchangeCurrency()
			: base("ShopExchangeCurrency") {}

		public SendActionResult PurchaseItem(uint index)
		{
			return TrySendAction(2, 0, 0, 1, index);
		}

		public async Task<bool> PurchaseItem(uint index, byte attempts, ushort interval = 200)
		{
			var result = SendActionResult.None;
			var purchaseAttempts = 0;
			while (result != SendActionResult.Success && !SelectYesno.IsOpen && purchaseAttempts++ < attempts
					&& Behaviors.ShouldContinue)
			{
				result = PurchaseItem(index);
				if (interval <= 33)
				{
					await Coroutine.Yield();
				}
				else
				{
					await Coroutine.Wait(interval, () => SelectYesno.IsOpen);
				}
			}

			if (result != SendActionResult.Success || purchaseAttempts > attempts)
			{
				return false;
			}

			// Wait an extra second in case interval is really short.
			await Coroutine.Wait(1000, () => SelectYesno.IsOpen);

			purchaseAttempts = 0;
			while (SelectYesno.IsOpen && purchaseAttempts++ < attempts && Behaviors.ShouldContinue)
			{
				SelectYesno.ClickYes();

				if (interval <= 33)
				{
					await Coroutine.Yield();
				}
				else
				{
					await Coroutine.Wait(interval, () => !SelectYesno.IsOpen);
				}
			}

			return !SelectYesno.IsOpen;
		}
	}
}