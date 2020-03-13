namespace cl_k9
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Threading.Tasks;
    using CitizenFX.Core;

    using static CitizenFX.Core.Native.API;

    public class Main : BaseScript
    {
        public static Canine k9;

        public static dynamic ESX;

        public static List<string> illegalItems = new List<string>
                                                      {
                                                          "weed20g",
                                                          "weed4g",
                                                          "weedbrick",
                                                          "weed_packaged",
                                                          "weed_untrimmed",
                                                          "bagofdope",
                                                          "meth",
                                                          "meth10g",
                                                          "meth1g",
                                                          "methbrick",
                                                          "meth_packaged",
                                                          "meth_raw",
                                                          "cocaine_cut",
                                                          "cocaine_packaged",
                                                          "cocaine_uncut",
                                                          "coke10g",
                                                          "coke1g",
                                                          "cokebrick"
                                                      };

        public Main()
        {
            this.EventHandlers["onClientResourceStart"] += new Action<string>(this.Start);
            this.Tick += this.OnTask;
        }

        private void Start(string name)
        {
            if (GetCurrentResourceName() != name) return;

            TriggerEvent("esx:getSharedObject", new Action<dynamic>((dynamic obj) => { ESX = obj; }));

            RegisterCommand(
                "k9",
                new Action<int, List<object>, string>(
                    (source, args, rawCommand) =>
                        {
                            if (args.Count >= 1)
                            {
                                if (args[0].ToString().ToLower().Contains("spawn"))
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
                                            ESX.ShowNotification("Dog name not found");
                                    }
                                    else
                                        ESX.ShowNotification("You have not entered a valid dog name try: [~y~rott~w~,~y~husky~w~,~y~retriever~w~,~y~shepherd~w~]");
                                }
                                else if (args[0].ToString().ToLower().Contains("delete"))
                                {
                                    TriggerServerEvent("K9:Delete");
                                }
                                else if (args[0].ToString().ToLower().Contains("follow"))
                                {
                                    TriggerServerEvent("K9:Follow");
                                }
                                else if (args[0].ToString().ToLower().Contains("stay"))
                                {
                                    TriggerServerEvent("K9:Stay");
                                }
                                else if (args[0].ToString().ToLower().Contains("enter"))
                                {
                                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                                    {
                                        int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                                        TriggerServerEvent("K9:EnterVehicle", veh);
                                    }
                                    else
                                        ESX.ShowNotification("~y~K9: ~w~You must be in a vehicle.");

                                }
                                else if (args[0].ToString().ToLower().Contains("exit"))
                                {
                                    TriggerServerEvent("K9:ExitVehicle");
                                }
                                else if (args[0].ToString().ToLower().Contains("attack"))
                                {
                                    TriggerServerEvent("K9:Attack", "PANIC");
                                }
                                else if (args[0].ToString().ToLower().Contains("search"))
                                {
                                    if (args.Count >= 2)
                                    {
                                        if (args[1].ToString().ToLower().Contains("vehicle"))
                                            TriggerServerEvent("K9:SearchVehicle");
                                        else if (args[1].ToString().ToLower().Contains("player"))
                                            TriggerServerEvent("K9:SearchPlayer");
                                        else
                                            ESX.ShowNotification("Dog name not found");
                                    }
                                    else
                                        ESX.ShowNotification("~y~Specify~w~: ~y~player~w~/~y~vehicle");
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
            if (IsControlJustPressed(0, 36) || IsDisabledControlJustPressed(0, 36) && IsPlayerFreeAiming(PlayerId()))
            {
                if (k9 != null)
                    TriggerServerEvent("K9:Attack", "PANIC");
            }

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

        public static bool ContainsIllegal(IDictionary<string, object> inventory, bool player = false)
        {
            bool isIllegal = false;
            if (!player)
            {
                dynamic items = inventory["items"];

                foreach (ExpandoObject item in items)
                {
                    dynamic name = ((IDictionary<string, object>)item)["name"];
                    if (illegalItems.Contains(name.ToString()))
                        isIllegal = true;
                }
            }
            else
            {
                dynamic itemsPly = inventory["inventory"];

                foreach (ExpandoObject item in itemsPly)
                {
                    dynamic name = ((IDictionary<string, object>)item)["name"];
                    if (illegalItems.Contains(name.ToString()))
                        isIllegal = true;
                }
            }

            

            return isIllegal;
        }
    }
}
