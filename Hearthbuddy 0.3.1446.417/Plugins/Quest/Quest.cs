using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using log4net;
using Triton.Bot;
using Triton.Bot.Logic.Bots.DefaultBot;
using Triton.Common;
using Triton.Game;
using Logger = Triton.Common.LogUtilities.Logger;

// TODO: With the new card specific quests, this logic needs to be revamped to support checking deck contents to find a better match.
// However, there's a new quest plugin in works that should be able to do that, as this one is old, and a basic example.

namespace Quest
{
	/// <summary>
	/// NOTE: Due to the old design of HSBs dev tools, this GUI implementation is very ugly.
	/// </summary>
	public class Quest : IPlugin
	{
		private static readonly ILog Log = Logger.GetLoggerInstanceForType();

		private bool _enabled;

		private bool _findNewQuest = true;
		private string _lastDeck;
		private GameRule _lastRule;

		#region Implementation of IPlugin

		/// <summary> The name of the plugin. </summary>
		public string Name
		{
			get
			{
				return "Quest";
			}
		}

		/// <summary> The description of the plugin. </summary>
		public string Description
		{
			get
			{
				return "A plugin that provides basic quest functionality for Hearthbuddy.";
			}
		}

		/// <summary>The author of the plugin.</summary>
		public string Author
		{
			get
			{
				return "Bossland GmbH";
			}
		}

		/// <summary>The version of the plugin.</summary>
		public string Version
		{
			get
			{
				return "0.0.1.1";
			}
		}

		/// <summary>Initializes this object. This is called when the object is loaded into the bot.</summary>
		public void Initialize()
		{
			Log.DebugFormat("[Quest] Initialize");
		}

		/// <summary> The plugin start callback. Do any initialization here. </summary>
		public void Start()
		{
			Log.DebugFormat("[Quest] Start");

			GameEventManager.StartingNewGame += GameEventManagerOnStartingNewGame;
			GameEventManager.QuestUpdate += GameEventManagerOnQuestUpdate;
			GameEventManager.NewGame += GameEventManagerOnNewGame;
			GameEventManager.GameOver += GameEventManagerOnGameOver;

			if (!(BotManager.CurrentBot is DefaultBot))
			{
				Log.ErrorFormat(
					"[Quest] This plugin is not compatible with this bot. Please disable the plugin before starting again or change the bot being used.");
				BotManager.Stop();
				return;
			}

			// This plugin is for construted mode only, as routines must implement quest logic for arena drafts, and most non-basic quests
			// require Play mode or Arena to be completed.
			if (DefaultBotSettings.Instance.GameMode != GameMode.Constructed)
			{
				Log.ErrorFormat(
					"[Quest] This plugin only works when the \"GameMode\" in DefaultBot is set to \"Constructed\". Please disable the plugin or change the DefaultBot settings.");
				BotManager.Stop();
				return;
			}

			_lastDeck = DefaultBotSettings.Instance.ConstructedCustomDeck;
			_lastRule = DefaultBotSettings.Instance.ConstructedGameRule;

			DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Auto;
		}

		/// <summary> The plugin tick callback. Do any update logic here. </summary>
		public void Tick()
		{
		}

		/// <summary> The plugin stop callback. Do any pre-dispose cleanup here. </summary>
		public void Stop()
		{
			Log.DebugFormat("[Quest] Stop");

			GameEventManager.StartingNewGame -= GameEventManagerOnStartingNewGame;
			GameEventManager.QuestUpdate -= GameEventManagerOnQuestUpdate;
			GameEventManager.NewGame -= GameEventManagerOnNewGame;
			GameEventManager.GameOver -= GameEventManagerOnGameOver;

			DefaultBotSettings.Instance.ConstructedCustomDeck = _lastDeck;
			DefaultBotSettings.Instance.ConstructedGameRule = _lastRule;

			_findNewQuest = true;
		}

		private void GameEventManagerOnNewGame(object sender, NewGameEventArgs newGameEventArgs)
		{
			foreach (var quest in TritonHs.CurrentQuests)
			{
				Log.InfoFormat("[Quest::GameEventManagerOnNewGame] {0}: {1} ({2} / {3}) [{5}x {4}]", quest.Name, quest.Description,
					quest.CurProgress,
					quest.MaxProgress, quest.RewardData[0].Type, quest.RewardData[0].Count);
			}
		}

		public JsonSettings Settings
		{
			get
			{
				return QuestSettings.Instance;
			}
		}

		private UserControl _control;

		/// <summary> The plugin's settings control. This will be added to the Hearthbuddy Settings tab.</summary>
		public UserControl Control
		{
			get
			{
				if (_control != null)
				{
					return _control;
				}

				using (var fs = new FileStream(@"Plugins\Quest\SettingsGui.xaml", FileMode.Open))
				{
					var root = (UserControl) XamlReader.Load(fs);

					// Your settings binding here.

					// StopAfterAllQuestsAreDone
					if (
						!Wpf.SetupCheckBoxBinding(root, "StopAfterAllQuestsAreDoneCheckBox",
							"StopAfterAllQuestsAreDone",
							BindingMode.TwoWay, QuestSettings.Instance))
					{
						Log.DebugFormat(
							"[SettingsControl] SetupCheckBoxBinding failed for 'StopAfterXGamesCheckBox'.");
						throw new Exception("The SettingsControl could not be created.");
					}

					for (var i = 1; i <= 17; i++)
					{
						var name = string.Format("IgnoreDeck{0}", i);
						var ctrlName = string.Format("{0}TextBox", name);
						if (!Wpf.SetupTextBoxBinding(root, ctrlName, name, BindingMode.TwoWay, QuestSettings.Instance))
						{
							Log.DebugFormat(
								"[SettingsControl] SetupCheckBoxBinding failed for '{0}'.", ctrlName);
							throw new Exception("The SettingsControl could not be created.");
						}
					}

					// Your settings event handlers here.

					_control = root;
				}

				return _control;
			}
		}

		/// <summary>Is this plugin currently enabled?</summary>
		public bool IsEnabled
		{
			get
			{
				return _enabled;
			}
		}

		/// <summary> The plugin is being enabled.</summary>
		public void Enable()
		{
			Log.DebugFormat("[Quest] Enable");
			_enabled = true;
		}

		/// <summary> The plugin is being disabled.</summary>
		public void Disable()
		{
			Log.DebugFormat("[Quest] Disable");
			_enabled = false;
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
		public void Deinitialize()
		{
		}

		#endregion

		#region Override of Object

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name + ": " + Description;
		}

		#endregion

		private void GameEventManagerOnStartingNewGame(object sender, StartingNewGameEventArgs startingNewGameEventArgs)
		{
			var usableDecks = QuestSettings.Instance.UsableDecks;

			if (_findNewQuest)
			{
				var quests = TritonHs.CurrentQuests;

				// Remove Tavern Brawl quest and spectate.
				quests = quests.Where(q => q.Id != 222 && q.Id != 214).ToList();

				// No quests to do, not much else for this plugin to mess with.
				if (!quests.Any())
				{
					// Stop the bot if the user wants it.
					if (QuestSettings.Instance.StopAfterAllQuestsAreDone)
					{
						Log.ErrorFormat(
							"[Quest] Now stopping the bot, because there are no quests to complete and [StopAfterAllQuestsAreDone] is enabled.");
						BotManager.Stop();
						return;
					}

					_findNewQuest = false;

					if (usableDecks.Any())
					{
						// Choose a random deck. We can add more logic for selection later...
						var rngDeck =
							usableDecks[
								Client.Random.Next(0, usableDecks.Count)];

						Log.InfoFormat("[Quest] Now choosing a random custom deck to play with since all quests are done.");

						DefaultBotSettings.Instance.ConstructedCustomDeck = rngDeck.Name;
						if (rngDeck.IsWild)
						{
							DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Wild;
						}
						else
						{
							DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Standard;
						}
					}
					return;
				}

				var foundQuest = false;

				// First, look for a quest to complete with a custom deck. We want to use custom decks to complete quests first,
				// before trying to use a basic deck (which is why this logic is two parts).
				foreach (var quest in quests)
				{
					// Loop through for each each class.
					foreach (var @class in TritonHs.BasicHeroTagClasses)
					{
						// If this is a class specific quest, find a suitable deck. Otherwise,
						// just use a random custom deck.
						if (TritonHs.IsQuestForSpecificClass(quest.Id))
						{
							// If this quest is a win quest for this class.
							if (TritonHs.IsQuestForClass(quest.Id, @class))
							{
								// Check to see if we have a custom deck to complete it with.
								var cardId = TritonHs.GetBasicHeroCardIdFromClass(@class);
								var decks =
									usableDecks.Where(d => d.HeroCardId == cardId).ToList();
								if (decks.Any())
								{
									// Choose a random deck. We can add more logic for selection later...
									var deck = decks[Client.Random.Next(0, decks.Count)];

									Log.InfoFormat(
										"[Quest] Now choosing a random compatible deck to complete the class specific quest [{0}] with.",
										quest.Name);

									DefaultBotSettings.Instance.ConstructedCustomDeck = deck.Name;
									if (deck.IsWild)
									{
										DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Wild;
									}
									else
									{
										DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Standard;
									}

									foundQuest = true;

									break;
								}
							}
						}
					}

					// If we found a quest and changed bot settings, we're done.
					if (foundQuest)
						break;

					// Make sure we have a non-class quest to choose a random deck for.
					// Also make sure we have custom decks in the first place.

					var filterDecks = usableDecks;
					if (quest.Id == 341 || quest.Id == 340)
					{
						filterDecks = filterDecks.Where(d => !d.IsWild).ToList();
					}

					if (!TritonHs.IsQuestForSpecificClass(quest.Id) && filterDecks.Any())
					{
						// Choose a random deck. We can add more logic for selection later...
						var rngDeck = filterDecks[Client.Random.Next(0, filterDecks.Count)];

						Log.InfoFormat(
							"[Quest] Now choosing a random compatible deck to complete the non-class specific quest [{0}] with.",
							quest.Name);

						DefaultBotSettings.Instance.ConstructedCustomDeck = rngDeck.Name;
						if (rngDeck.IsWild)
						{
							DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Wild;
						}
						else
						{
							DefaultBotSettings.Instance.ConstructedGameRule = GameRule.Standard;
						}

						foundQuest = true;

						break;
					}
				}

				// All done? For now, yes.
				if (foundQuest)
				{
					_findNewQuest = false;
					return;
				}

				Log.ErrorFormat(
					"[Quest] Now stopping the bot, because there are no quests we can complete with the current custom decks.");
				BotManager.Stop();
			}
		}

		private void GameEventManagerOnQuestUpdate(object sender, QuestUpdateEventArgs questUpdateEventArgs)
		{
			_findNewQuest = true;
		}

		private void GameEventManagerOnGameOver(object sender, GameOverEventArgs gameOverEventArgs)
		{
			// Find the next quest to do ater a game over, regardless of the outcome.
			_findNewQuest = true;
		}
	}
}