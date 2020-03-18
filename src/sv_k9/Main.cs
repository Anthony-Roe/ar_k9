namespace sv_k9
{
    using System;
    using CitizenFX.Core;

    using Newtonsoft.Json;

    using sh_k9;

    using static CitizenFX.Core.Native.API;
    public class Main : BaseScript
    {
        private dynamic ESX;

        public static Settings settings;

        public Main()
        {
            LoadConfig();

            while (settings == null)
            {
                Delay(100);
            }

            if (settings.standalone == false)
                TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => {
                    this.ESX = esx;
                })});
            this.EventHandlers.Add("K9:Spawn", new Action<Player, int>(this.Spawn));
            this.EventHandlers.Add("K9:Delete", new Action<Player>(this.Delete));
            this.EventHandlers.Add("K9:Follow", new Action<Player>(this.Follow));
            this.EventHandlers.Add("K9:Stay", new Action<Player>(this.Stay));
            this.EventHandlers.Add("K9:EnterVehicle", new Action<Player, int>(this.EnterVehicle));
            this.EventHandlers.Add("K9:ExitVehicle", new Action<Player>(this.ExitVehicle));
            this.EventHandlers.Add("K9:Attack", new Action<Player, string, int>(this.Attack));
            this.EventHandlers.Add("K9:SearchVehicle", new Action<Player>(this.SearchVehicle));
            this.EventHandlers.Add("K9:SearchPlayer", new Action<Player>(this.SearchPlayer));
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

        public bool HasPermission(Player source)
        {
            if (settings.standalone == false)
            {
                dynamic player = this.ESX.GetPlayerFromId(source.Handle);
                dynamic job = player != null ? player.getJob() : null;

                if (player == null || job == null)
                    return false;

                if (job.grade_name == settings.allowedJobGrade)
                    return true;

                return false;
            }

            return IsPlayerAceAllowed(source.Handle, "k9");
        }

        private void Spawn([FromSource] Player source, int model)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Spawn", model);
            }
        }

        private void Delete([FromSource] Player source)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Delete");
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

        private void Attack([FromSource] Player source, string type, int ped = 0)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:Attack", type, ped);
            }
        }
        
        private void SearchVehicle([FromSource] Player source)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:SearchVehicle");
            }
        }

        private void SearchPlayer([FromSource] Player source)
        {
            if (this.HasPermission(source))
            {
                TriggerClientEvent(source, "K9:SearchPlayer");
            }
        }
    }
}
