namespace cl_k9
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Threading.Tasks;
    using CitizenFX.Core;

    using Newtonsoft.Json;

    using sh_k9;

    using static CitizenFX.Core.Native.API;

    public class Main : BaseScript
    {
        public static Canine k9;

        public static dynamic ESX;

        public static Settings settings;

        public Main()
        {
            LoadConfig();

            while (settings == null)
            {
                Delay(100);
            }

            this.EventHandlers["onClientResourceStart"] += new Action<string>(this.Start);
            this.Tick += this.OnTask;
        }

        private void LoadConfig()
        {
            string content = null;

            try
            {
                content = LoadResourceFile(GetCurrentResourceName(), "config.json");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"An error occurred while loading the config file, error description: {e.Message}.");
            }

            settings = JsonConvert.DeserializeObject<Settings>(content);
        }

        private void Start(string name)
        {
            if (GetCurrentResourceName() != name) return;

            if (settings.standalone == false)
                TriggerEvent("esx:getSharedObject", new Action<dynamic>((dynamic obj) => { ESX = obj; }));

            RegisterCommand(
                "k9",
                new Action<int, List<object>, string>(
                    (source, args, rawCommand) =>
                        {
                            if (args.Count >= 1)
                            {
                                if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["spawn"].ToString()))
                                {
                                    if (args.Count >= 2)
                                    {
                                        if (args[1].ToString().ToLower().Contains("rott"))
                                            TriggerServerEvent("K9:Spawn", (int)MODELS.Rottweiler);
                                        else if (args[1].ToString().ToLower().Contains("husky"))
                                            TriggerServerEvent("K9:Spawn", (int)MODELS.Husky);
                                        else if (args[1].ToString().ToLower().Contains("retriever"))
                                            TriggerServerEvent("K9:Spawn", (int)MODELS.Retriever);
                                        else if (args[1].ToString().ToLower().Contains("shepherd"))
                                            TriggerServerEvent("K9:Spawn", (int)MODELS.Shepherd);
                                        else
                                            ShowNotification(settings.dict["dogNameNotFound"]);

                                        ShowNotification("Loading...");
                                    }
                                    else
                                        ShowNotification(settings.dict["invalidDogName"]);
                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["delete"].ToString()))
                                {
                                    TriggerServerEvent("K9:Delete");
                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["follow"].ToString()))
                                {
                                    TriggerServerEvent("K9:Follow");
                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["stay"].ToString()))
                                {
                                    TriggerServerEvent("K9:Stay");
                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["enter"].ToString()))
                                {
                                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                                    {
                                        int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                                        TriggerServerEvent("K9:EnterVehicle", veh);
                                    }
                                    else
                                        ShowNotification(settings.dict["commandEnterHelp"]);

                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["exit"].ToString()))
                                {
                                    TriggerServerEvent("K9:ExitVehicle");
                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["attack"].ToString()))
                                {
                                    TriggerServerEvent("K9:Attack", "PANIC");
                                }
                                else if (args[0].ToString().ToLower().Contains(settings.dict["commands"]["search"].ToString()))
                                {
                                    if (args.Count >= 2)
                                    {
                                        if (args[1].ToString().ToLower().Contains(settings.dict["commands"]["searchVehicle"].ToString()))
                                            TriggerServerEvent("K9:SearchVehicle");
                                        else if (args[1].ToString().ToLower().Contains(settings.dict["commands"]["searchPlayer"].ToString()))
                                            TriggerServerEvent("K9:SearchPlayer");
                                        else
                                            ShowNotification(settings.dict["commandSearchHelp"]);
                                    }
                                    else
                                        ShowNotification(settings.dict["commandSearchHelp"]);
                                }
                            }
                        }),
                false /*This command is also not restricted, anyone can use it.*/);

            this.EventHandlers.Add("K9:Spawn", new Action<int>(this.Spawn));
            this.EventHandlers.Add("K9:Delete", new Action(this.Delete));
            this.EventHandlers.Add("K9:Follow", new Action(this.Follow));
            this.EventHandlers.Add("K9:Stay", new Action(this.Stay));
            this.EventHandlers.Add("K9:EnterVehicle", new Action<int>(this.EnterVehicle));
            this.EventHandlers.Add("K9:ExitVehicle", new Action(this.ExitVehicle));
            this.EventHandlers.Add("K9:Attack", new Action<string, int>(this.Attack));
            this.EventHandlers.Add("K9:SearchVehicle", new Action(this.SearchVehicle));
            this.EventHandlers.Add("K9:SearchPlayer", new Action(this.SearchPlayer));
        }

        private async Task OnTask()
        {
            if (k9 == null) return;
            if (IsPlayerFreeAiming(PlayerId()) && IsControlJustPressed((int)settings.dict["attackKeyModifier"][0], (int)settings.dict["attackKeyModifier"][1]) || IsDisabledControlJustPressed((int)settings.dict["attackKeyModifier"][0], (int)settings.dict["attackKeyModifier"][1]))
            {
                TriggerServerEvent("K9:Attack", "PANIC");
            }
            if (!k9.dog.Exists())

            await Delay(1);
        }

        private void Spawn(int dog)
        {
            if (k9 != null)
                if (k9.dog.Exists())
                    this.Delete();
            k9 = new Canine { model = dog };
            k9.CallCommand(COMMANDS.Spawn);
        }
        
        private void Delete()
        {
            if (k9 != null)
            {
                if (k9.dog.Exists())
                    k9.CallCommand(COMMANDS.Delete);
                k9 = null;
            }
                
                    
        }

        private void Follow()
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.Follow);
        }

        private void Stay()
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.Stay);
        }

        private void EnterVehicle(int vehicle)
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.Enter, null, vehicle);
        }

        private void ExitVehicle()
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.Exit);
        }

        private void Attack(string type, int ped)
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.Attack, type, ped);
        }

        private void SearchVehicle()
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.SearchVehicle);
        }

        private void SearchPlayer()
        {
            if (k9 != null)
                k9.CallCommand(COMMANDS.SearchPlayer);
        }

        public static void ShowNotification(string msg)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentSubstringPlayerName(msg);
            DrawNotification(true, true);
        }

        public static bool ContainsIllegal(IDictionary<string, object> inventory, bool player = false)
        {
            bool isIllegal = false;
            if (player == false)
            {
                dynamic items = inventory["items"];
                dynamic weapons = inventory["weapons"];

                foreach (IDictionary<string, object> item in items)
                {
                    if (settings.illegalItems.Contains(item["name"].ToString()))
                        isIllegal = true;
                }
                foreach (IDictionary<string, object> weapon in weapons)
                {
                    if (settings.illegalWeapons.Contains(weapon["name"].ToString()))
                        isIllegal = true;
                }
            }
            else
            {
                dynamic items = inventory["inventory"];
                dynamic weapons = inventory["weapons"];
                

                foreach (IDictionary<string, object> item in items)
                {
                    if (settings.illegalItems.Contains(item["name"].ToString()) && Convert.ToInt32(item["count"]) > 0)
                        isIllegal = true;
                }

                foreach (IDictionary<string, object> weapon in weapons)
                {
                    if (settings.illegalWeapons.Contains(weapon["name"].ToString()))
                        isIllegal = true;
                }
            }

            

            return isIllegal;
        }
    }
}
