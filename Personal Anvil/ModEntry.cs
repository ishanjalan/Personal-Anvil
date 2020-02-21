using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace PersonalAnvil
{
    public class ModEntry : Mod
    {
        private GameLocation.afterQuestionBehavior _afterQuestion;
        private int _anvilId;

        private IJsonAssetsApi _jsonAssets;

        private string _lastQuestionKey;
        private int _leftClickX;
        private int _leftClickY;
        private readonly List<Response> _responses = new List<Response>();
        private const int SelectedResponse = -1;


        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (_jsonAssets == null) return;
            _anvilId = _jsonAssets.GetBigCraftableId("Anvil");
            

            if (_anvilId == -1) Monitor.Log("Could not get the ID for the Anvil item", LogLevel.Warn);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!e.Button.IsActionButton()) return;
            if (e.Button == SButton.MouseLeft)
            {
                var cursorPos = Helper.Input.GetCursorPosition();
                _leftClickX = (int) cursorPos.ScreenPixels.X;
                _leftClickY = (int) cursorPos.ScreenPixels.Y;
            }

            var tile = Helper.Input.GetCursorPosition().Tile;
            Game1.currentLocation.Objects.TryGetValue(tile, out var obj);
            if (obj == null || !obj.bigCraftable.Value) return;
            if (obj.ParentSheetIndex.Equals(_anvilId)) Game1.activeClickableMenu = new GeodeProcessMenu(Helper.Content);
            Response[] answerChoices;
            if (Game1.player.hasItemInInventory(535, 1) || Game1.player.hasItemInInventory(536, 1) ||
                Game1.player.hasItemInInventory(537, 1) || Game1.player.hasItemInInventory(749, 1) ||
                Game1.player.hasItemInInventory(275, 1))
                answerChoices = new[]
                {
                    new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
                    new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                };
            else
                answerChoices = new[]
                {
                    new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                };

            CreateQuestionDialogue("", answerChoices, "Blacksmith");
        }

        private void CreateQuestionDialogue(string question, IEnumerable<Response> answerChoices, string dialogKey)
        {
            _lastQuestionKey = dialogKey;
            Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
        }

        public virtual bool AnswerDialogue(Response answer)
        {
            var strArray = _lastQuestionKey?.Split(' ');
            var questionParams = strArray;
            var questionAndAnswer =
                questionParams != null ? questionParams[0] + "_" + answer.responseKey : null;
            if (_afterQuestion == null)
                return questionAndAnswer != null && AnswerDialogueAction(questionAndAnswer, questionParams);
            _afterQuestion(Game1.player, answer.responseKey);
            _afterQuestion = null;
            Game1.objectDialoguePortraitPerson = null;
            return true;
        }

        protected virtual bool AnswerDialogueAction(string questionAndAnswer, string[] questionParams)
        {
            switch (questionAndAnswer)
            {
                case "Blacksmith_Process":
                    Game1.activeClickableMenu = new GeodeProcessMenu(Helper.Content);
                    break;
                case "Blacksmith_Upgrade":
                    //Game1.activeClickableMenu = new ToolUpgradeMenu(Utility.getBlacksmithUpgradeStock(Game1.player), 0, "ClintUpgrade");
                    break;
                case null:
                    return false;
                default:
                    return false;
            }

            return true;
        }


        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _jsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            _jsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(4) && Helper.Input.IsDown(SButton.MouseLeft) &&
                Game1.activeClickableMenu is GeodeProcessMenu menu &&
                menu.GeodeSpot.containsPoint(_leftClickX, _leftClickY))
                menu.receiveLeftClick(_leftClickX, _leftClickY);
        }
    }

    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }
}