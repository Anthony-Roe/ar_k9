namespace cl_k9
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CitizenFX.Core;
    using static CitizenFX.Core.Native.API;

    public class Main : BaseScript
    {
        private static Canine k9 = null;

        public static dynamic ESX = null;
        public Main()
        {
            this.EventHandlers["onClientResourceStart"] += new Action<string>(this.Start);
            this.Tick += this.OnTask;
        }

        private void Start(string name)
        {
            if (GetCurrentResourceName() != name) return;

            TriggerEvent("esx:getSharedObject", new Action<dynamic>(
                (dynamic obj) => { ESX = obj; }));


            k9 = new Canine();
            RegisterCommand("k9", new Action<int, List<object>, string>((source, args, rawCommand) =>
                {
                if (args.Count >= 1)
                {
                    if (args[0].ToString().ToLower().Contains("spawn"))
                    {
                        Debug.WriteLine("Sending Spawn");
                        TriggerServerEvent("K9:Spawn");
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
                        TriggerServerEvent("K9:EnterVehicle", GetVehiclePedIsIn(PlayerPedId(), false));
                    }
                    else if (args[0].ToString().ToLower().Contains("exit"))
                    {
                        TriggerServerEvent("K9:ExitVehicle");
                    }
                    else if (args[0].ToString().ToLower().Contains("attack"))
                    {
                        TriggerServerEvent("K9:Attack", "PANIC", ESX.Game.GetClosestPed(GetEntityCoords(PlayerPedId(), true), new List<object> { PlayerPedId() }));
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
                        ESX.TriggerServerCallback("esx_trunk:getInventoryV", new Action<object>(
                            (inventory) =>
                                {
                                    var dynamicDictionary = inventory as IDictionary<string, object>;

                                    foreach (KeyValuePair<string, object> property in dynamicDictionary)
                                    {
                                        Debug.WriteLine("{0}: {1}", property.Key, property.Value.ToString());
                                    }
                                    Debug.WriteLine();
                                }), GetVehicleNumberPlateText(veh));

                    }
                }
                }), false /*This command is also not restricted, anyone can use it.*/ );

            this.EventHandlers.Add("K9:Spawn", new Action(this.SpawnK9));
            this.EventHandlers.Add("K9:Follow", new Action(this.Follow));
            this.EventHandlers.Add("K9:Stay", new Action(this.Stay));
            this.EventHandlers.Add("K9:EnterVehicle", new Action<int>(this.EnterVehicle));
            this.EventHandlers.Add("K9:ExitVehicle", new Action(this.ExitVehicle));
            this.EventHandlers.Add("K9:Attack", new Action<string, int>(this.Attack));
            this.EventHandlers.Add("K9:SearchVehicle", new Action<int>(this.SearchVehicle));
        }

        private async Task OnTask()
        {
            if (IsDisabledControlJustPressed(0, 36))
            {
                TriggerServerEvent("K9:Attack", "PANIC", 0);
            }

            await Delay(1);
        }

        private void SpawnK9()
        {
            k9.Spawn();
        }

        private void Follow()
        {
            k9.Follow();
        }

        private void Stay()
        {
            k9.Stay();
        }

        private void EnterVehicle(int vehicle)
        {
            k9.EnterVehicle(vehicle);
        }

        private void ExitVehicle()
        {
            k9.ExitVehicle();
        }

        private void Attack(string type, int ped)
        {
            k9.Attack(type, ped);
        }

        private void SearchVehicle(int vehicle)
        {
            k9.SearchVehicle(vehicle);
        }
    }
}
