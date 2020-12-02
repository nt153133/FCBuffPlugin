using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;

namespace LlamaLibrary
{
    public class FCActionPlugin : BotPlugin
    {
        private static readonly string name = "FC Buffs";

        private static Func<int, int, GrandCompany, Task> _activate;
        private static Action<string, Func<Task>> _addHook;
        private static Action<string> _removeHook;
        private static Func<List<string>> _getHookList;
        private static bool FoundLisbeth = false;
        private static bool FoundLL = false;
        private static readonly string HookName = "FCBuffsCraft";
        private static readonly string HookName1 = "FCBuffsReg";
        private static Action<string, Func<Task>> _addHook1;
        private static Action<string> _removeHook1;
        public override string Author { get; } = "Kayla";
        public static BuffSettings Settings = BuffSettings.Instance;
        private BuffSetttingsFrm settings;

        public override Version Version => new Version(2, 6);

        public override string Name { get; } = name;

        public override bool WantButton => true;

        public override void OnButtonPress()
        {
            if (settings == null || settings.IsDisposed)
                settings = new BuffSetttingsFrm();
            try
            {
                settings.Show();
                settings.Activate();
            }
            catch (ArgumentOutOfRangeException ee)
            {
            }
        }

        public override void OnInitialize()
        {
            FindLL();
        }

        public override void OnEnabled()
        {
            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
            Log($"{name} Enabled");
        }

        public override void OnDisabled()
        {
            TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;
            Log($"{name} Disabled");
        }

        private void OnBotStop(BotBase bot)
        {
            if (bot.Name == "Lisbeth")
            {
                if (!FoundLisbeth) FindLisbeth();
                if (FoundLisbeth && FoundLL)
                    RemoveHooks();
            }
        }

        private void OnBotStart(BotBase bot)
        {
            if (bot.Name == "Lisbeth")
            {
                if (!FoundLisbeth) FindLisbeth();
                if (FoundLisbeth && FoundLL)
                    AddHooks();
            }
        }

        private void AddHooks()
        {
            var hooks = _getHookList.Invoke();
            Log($"Adding {HookName} Hook");
            if (!hooks.Contains(HookName))
            {
                _addHook.Invoke(HookName, BuffTask);
            }
            Log($"Adding {HookName1} Hook");
            if (!hooks.Contains(HookName1))
            {
                _addHook1.Invoke(HookName1, BuffTask);
            }
        }

        private void RemoveHooks()
        {
            var hooks = _getHookList.Invoke();
            Log($"Removing {HookName} Hook");
            if (hooks.Contains(HookName))
            {
                _removeHook.Invoke(HookName);
            }
            Log($"Removing {HookName1} Hook");
            if (!hooks.Contains(HookName1))
            {
                _removeHook1.Invoke(HookName1);
            }
        }

        public static async Task BuffTask()
        {
            uint[] FCAuras = new uint[] {353, 354, 355, 356, 357, 360, 361, 362, 363, 364, 365, 366, 367, 368, 413, 414, 902};
            var buffs = Core.Me.Auras.Where(i => FCAuras.Contains(i.Id)).ToList();
            if (buffs.Count() < 2)
            {
                Log($"Found {buffs.Count} Buffs Active");
                Log($"Calling Activate");
                await _activate((int) BuffSettings.Instance.Buff1, (int) BuffSettings.Instance.Buff2, BuffSettings.Instance.GrandCompany);
            }
        }

        private static void FindLL()
        {
            var loader = BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Retainers");

            if (loader == null) return;

            var q = from t in loader.GetType().Assembly.GetTypes()
                    where t.Namespace == "LlamaLibrary.Helpers" && t.Name.Equals("FreeCompanyActions")
                    select t;

            if (q.Any())
            {
                var helper = q.First();
                var fcAction = helper.GetMethod("ActivateBuffs");
                _activate = (Func<int, int, GrandCompany, Task>) fcAction?.CreateDelegate(typeof(Func<int, int, GrandCompany, Task>));
                Log($"Found {helper.GetMethod("ActivateBuffs")?.Name}");
            }

            FoundLL = true;
        }

        private static void FindLisbeth()
        {
            var loader = BotManager.Bots
                .FirstOrDefault(c => c.Name == "Lisbeth");

            if (loader == null) return;

            var lisbethObjectProperty = loader.GetType().GetProperty("Lisbeth");
            var lisbeth = lisbethObjectProperty?.GetValue(loader);
            if (lisbeth == null) return;
            var apiObject = lisbeth.GetType().GetProperty("Api")?.GetValue(lisbeth);
            if (apiObject != null)
            {
                var m = apiObject.GetType().GetMethod("GetCurrentAreaName");
                if (m != null)
                {
                    _addHook = (Action<string, Func<Task>>) Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddCraftCycleHook");
                    _removeHook = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveCraftCycleHook");
                    _getHookList = (Func<List<string>>) Delegate.CreateDelegate(typeof(Func<List<string>>), apiObject, "GetHookList");
                    _addHook1 = (Action<string, Func<Task>>) Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddHook");
                    _removeHook1 = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveHook");
                    FoundLisbeth = true;
                }
            }

            Logging.Write("Lisbeth found.");
        }

        private static void Log(string text)
        {
            var msg = string.Format($"[{name}] " + text);
            Logging.Write(Colors.Aquamarine, msg);
        }
    }
}