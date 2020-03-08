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
        private static Canine k9;

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
                                else if (args[0].ToString().ToLower().Contains("searchvehicle"))
                                {
                                    int veh = 0;
                                    GetEntityPlayerIsFreeAimingAt(PlayerId(), ref veh);
                                    TriggerServerEvent("K9:SearchVehicle", veh);
                                }
                                else if (args[0].ToString().ToLower().Contains("test"))
                                {
                                    int veh = 0;
                                    GetEntityPlayerIsFreeAimingAt(PlayerId(), ref veh);
                                    ESX.TriggerServerCallback(
                                        "esx_trunk:getInventoryV",
                                        new Action<dynamic>(
                                            (inventory) =>
                                                {
                                                    List<string> illegalItems = ContainsIllegal(inventory);
                                                    if (illegalItems.Count > 0)
                                                    {
                                                        string joined = string.Join(", ", illegalItems);
                                                        ESX.ShowNotification("Vehicle Contains: " + joined);
                                                    }
                                                }),
                                        GetVehicleNumberPlateText(veh));

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
            this.EventHandlers.Add("K9:SearchVehicle", new Action<int>(this.SearchVehicle));
        }

        private async Task OnTask()
        {
            if (IsControlJustPressed(0, 36) || IsDisabledControlJustPressed(0, 36) && IsPlayerFreeAiming(PlayerId()))
            {
                TriggerServerEvent("K9:Attack", "PANIC");
            }

            await Delay(1);
        }

        private void Spawn(int dog)
        {
            this.Delete();
            k9 = new Canine(dog);
            k9.CallCommand(COMMANDS.Spawn);
        }

        private void Delete()
        {
            if (k9 != null)
            {
                k9.CallCommand(COMMANDS.Delete);
                k9 = null;
                ESX.Notification("~y~K9: ~w~Deleted!");
            }
                
        }

        private void Follow()
        {
            k9.CallCommand(COMMANDS.Follow);
        }

        private void Stay()
        {
            k9.CallCommand(COMMANDS.Stay);
        }

        private void EnterVehicle(int vehicle)
        {
            k9.CallCommand(COMMANDS.Enter, null, vehicle);
        }

        private void ExitVehicle()
        {
            k9.CallCommand(COMMANDS.Exit);
        }

        private void Attack(string type, int ped)
        {
            k9.CallCommand(COMMANDS.Attack, type, ped);
        }

        private void SearchVehicle(int vehicle)
        {
            k9.CallCommand(COMMANDS.Search, null, vehicle);
        }

        public static bool ContainsIllegal(IDictionary<string, Object> inventory)
        {
            bool isIllegal = false;
            dynamic weapons = ((IDictionary<string, Object>)inventory)["weapons"];
            dynamic items = ((IDictionary<string, Object>)inventory)["items"];

            foreach (ExpandoObject item in items)
            {
                dynamic name = ((IDictionary<string, Object>)item)["name"];
                if (illegalItems.Contains(name.ToString()))
                    isIllegal = true;
            }

            return isIllegal;
        }
    }
}
