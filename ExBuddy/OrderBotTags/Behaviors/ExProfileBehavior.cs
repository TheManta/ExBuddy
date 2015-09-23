﻿namespace ExBuddy.OrderBotTags.Behaviors
{
	using System.Windows.Media;

	using Clio.XmlEngine;

	using ExBuddy.Attributes;
	using ExBuddy.Helpers;
	using ExBuddy.Interfaces;
	using ExBuddy.Logging;

	using ff14bot.Managers;
	using ff14bot.NeoProfiles;
	using ff14bot.Objects;

	public abstract class ExProfileBehavior : ProfileBehavior, ILogColors
	{
		private string statusText;

		protected internal readonly Logger Logger;

		static ExProfileBehavior()
		{
			ReflectionHelper.CustomAttributes<LoggerNameAttribute>.RegisterByAssembly();
		}

		protected ExProfileBehavior()
		{
			Logger = new Logger(this, includeVersion: true);
		}

		[XmlElement("Name")]
		public string Name { get; set; }

		public override string StatusText
		{
			get
			{
				return string.Concat(this.GetType().Name, ": ", statusText);
			}

			set
			{
				statusText = value;
			}
		}

		protected internal static LocalPlayer Me
		{
			get
			{
				return GameObjectManager.LocalPlayer;
			}
		}

		protected virtual Color Error
		{
			get
			{
				return Logger.Colors.Error;
			}
		}

		protected virtual Color Warn
		{
			get
			{
				return Logger.Colors.Warn;
			}
		}

		protected virtual Color Info
		{
			get
			{
				return Logger.Colors.Info;
			}
		}

		Color ILogColors.Error
		{
			get
			{
				return this.Error;
			}
		}

		Color ILogColors.Warn
		{
			get
			{
				return this.Warn;
			}
		}

		Color ILogColors.Info
		{
			get
			{
				return this.Info;
			}
		}
	}
}