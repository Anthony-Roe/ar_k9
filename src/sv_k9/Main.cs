namespace sv_k9
{
    using System;
    using CitizenFX.Core;
    using static CitizenFX.Core.Native.API;
    public class Main : BaseScript
    {
        private dynamic ESX;

        public Main()
        {
            Debug.WriteLine("Getting ESX");
            TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => {
                    this.ESX = esx;
                })});
            this.EventHandlers.Add("K9:Spawn", new Action<Player>(this.SpawnK9));
            this.EventHandlers.Add("K9:Follow", new Action<Player>(this.Follow));
            this.EventHandlers.Add("K9:Stay", new Action<Player>(this.Stay));
            this.EventHandlers.Add("K9:EnterVehicle", new Action<Player, int>(this.EnterVehicle));
            this.EventHandlers.Add("K9:ExitVehicle", new Action<Player>(this.ExitVehicle));
            this.EventHandlers.Add("K9:Attack", new Action<Player, string, int>(this.Attack));
            this.EventHandlers.Add("K9:SearchVehicle", new Action<Player, int>(this.SearchVehicle));
        }

        public bool HasPermission(Player source)
        {
            return true;
        }

        private void SpawnK9([FromSource] Player source)
        {
            Debug.WriteLine("Sending Spawn");
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Spawn");
            }
        }

        private void Follow([FromSource] Player source)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Follow");
            }
        }

        private void Stay([FromSource] Player source)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Stay");
            }
        }

        private void EnterVehicle([FromSource] Player source, int vehicle)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:EnterVehicle", vehicle);
            }
        }

        private void ExitVehicle([FromSource] Player source)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:ExitVehicle");
            }
        }

        private void Attack([FromSource] Player source, string type, int ped)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Attack", type, ped);
            }
        }

        private void SearchVehicle([FromSource] Player source, int vehicle)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:SearchVehicle", vehicle);
            }
        }
    }
}
