namespace cl_k9
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CitizenFX.Core;

    using static CitizenFX.Core.Native.API;

    public enum ACTION
    {
        Stay,
        Follow,
        Search,
        inVehicle,
        Attack
    }

    public enum MODELS
    {
        Rottweiler = -1788665315, // a_c_rottweiler
        Husky = 1318032802, // a_c_husky
        Retriever = 882848737, // a_c_retriever
        Shepherd = 1126154828 // a_c_shepherd
    }

    public enum COMMANDS
    {
        Stay,
        Follow,
        SearchVehicle,
        SearchPlayer,
        Enter,
        Exit,
        Attack,
        Spawn,
        Delete
    }

    public class Canine : BaseScript
    {
        public Ped dog;

        public int model;

        public ACTION action;

        public int target;

        public Blip blip;

        public async Task Spawn()
        {
            while (!HasModelLoaded((uint)this.model))
            {
                RequestModel((uint)this.model);
                await Delay(500);
            }

            int ped = PlayerPedId();

            Vector3 pPos = GetOffsetFromEntityInWorldCoords(ped, 0.0f, 2.0f, 0.0f);
            float pHead = GetEntityHeading(ped);
            float groundZ = 0;
            GetGroundZFor_3dCoord(pPos.X, pPos.Y, pPos.Z, ref groundZ, false);
            pPos.Z = groundZ + 1;
            this.dog = await World.CreatePed(this.model, pPos, pHead);
            
            GiveWeaponToPed(this.dog.Handle, (uint)GetHashKey("WEAPON_ANIMAL"), 200, true, true);

            SetBlockingOfNonTemporaryEvents(this.dog.Handle, true);

            SetPedFleeAttributes(this.dog.Handle, 0, false);

            SetPedCombatAttributes(this.dog.Handle, 3, true);

            SetPedCombatAttributes(this.dog.Handle, 5, true);

            SetPedCombatAttributes(this.dog.Handle, 46, true);

            SetEntityHealth(this.dog.Handle, (int)Main.settings.dog["health"]);

            SetPedArmour(this.dog.Handle, (int)Main.settings.dog["armor"]);

            SetPedMoveRateOverride(this.dog.Handle, (float)Main.settings.dog["speed"]);

            blip = this.dog.AttachBlip();
            blip.Sprite = BlipSprite.PolicePlayer;
            blip.IsShortRange = false;
            blip.Name = "Police K9";

            await this.Stay();
        }

        public async void CallCommand(COMMANDS cmd, string str = "", int num = 0)
        {
            switch (cmd)
            {
                case COMMANDS.Enter:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.EnterVehicle(num);
                    return;
                case COMMANDS.Exit:
                    if (this.action != ACTION.Search)
                        await this.ExitVehicle();
                    return;
                case COMMANDS.Follow:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.Follow();
                    return;
                case COMMANDS.Stay:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.Stay();
                    return;
                case COMMANDS.Attack:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    await this.Attack(str, num);
                    return;
                case COMMANDS.SearchVehicle:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.SearchVehicle();
                    return;
                case COMMANDS.SearchPlayer:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.SearchPlayer();
                    return;
                case COMMANDS.Spawn:
                    this.Spawn();
                    return;
                case COMMANDS.Delete:
                    this.DeleteBlip();
                    this.dog.Delete();
                    return;
            }
        }

        private async Task DeleteBlip()
        {
            this.blip.Delete();
        }

        private async Task Heal()
        {
            this.dog.Health = 400;
            this.dog.Armor = 200;
        }

        private async Task Follow()
        {
            if (this.action == ACTION.inVehicle)
                await this.ExitVehicle();
            this.dog.Task.ClearAll();
            Main.ShowNotification($"~y~*{Main.settings.dict["follow"]}*");
            this.dog.Task.FollowToOffsetFromEntity(LocalPlayer.Character, new Vector3(0.0f, 0.0f, 0.0f), 7.0f, -1, 0.2f, true);
            this.action = ACTION.Follow;
        }

        private async Task Stay()
        {
            this.dog.Task.ClearAll();
            Main.ShowNotification($"~y~*{Main.settings.dict["stay"]}*");
            await this.dog.Task.PlayAnimation("creatures@rottweiler@amb@world_dog_sitting@base", "base", 8.0f, -4.0f, -1, AnimationFlags.Loop, 0.0f);

            this.action = ACTION.Stay;
        }

        private async Task EnterVehicle(int vehicle)
        {
            Vehicle veh = (Vehicle)Entity.FromHandle(vehicle);
            Vector3 vehCoords = veh.Position;
            float forwardX = veh.ForwardVector.X * 2.0f;
            float forwardY = veh.ForwardVector.Y * 2.0f;

            Main.ShowNotification($"~y~*{Main.settings.dict["enterVehicle"]}*");

            veh.Doors[VehicleDoorIndex.Trunk].Open();
            this.dog.Task.RunTo(new Vector3(vehCoords.X - forwardX, vehCoords.Y - forwardY, vehCoords.Z), true);

            await Delay(3000);
            this.dog.Task.AchieveHeading(veh.Heading, -1);
            RequestAnimDict("creatures@rottweiler@in_vehicle@van");
            RequestAnimDict("creatures@rottweiler@amb@world_dog_sitting@base");
            while (!HasAnimDictLoaded("creatures@rottweiler@in_vehicle@van") || !HasAnimDictLoaded("creatures@rottweiler@amb@world_dog_sitting@base"))
            {
                await Delay(5);
            }

            await this.dog.Task.PlayAnimation("creatures@rottweiler@in_vehicle@van", "get_in", 8.0f, -4.0f, -1, AnimationFlags.StayInEndFrame, 0.0f);

            await Delay(700);

            this.dog.Task.ClearAll();

            this.dog.AttachTo(veh.Bones[Main.settings.vehicleBoneToAttachTo], new Vector3(0.0f, 0.0f, 0.25f));

            await this.dog.Task.PlayAnimation("creatures@rottweiler@amb@world_dog_sitting@base", "base", 8.0f, -4.0f, -1, AnimationFlags.StayInEndFrame, 0.0f);

            await Delay(500);

            veh.Doors[VehicleDoorIndex.Trunk].Close();

            this.action = ACTION.inVehicle;
        }

        private async Task ExitVehicle()
        {
            if (this.action != ACTION.inVehicle) return;

            // TO DO: Replace with get entity attached
            Vehicle veh = (Vehicle)this.dog.GetEntityAttachedTo();

            Vector3 vehCoords = veh.Position;

            float forwardX = veh.ForwardVector.X * 3.7f;
            
            float forwardY = veh.ForwardVector.Y * 3.7f;

            Main.ShowNotification($"~y~*{Main.settings.dict["exitVehicle"]}*");
            this.dog.Task.ClearAll();

            veh.Doors[VehicleDoorIndex.Trunk].Open();

            this.dog.Detach();

            this.dog.Position = new Vector3(vehCoords.X - forwardX, vehCoords.Y - forwardY, World.GetGroundHeight(vehCoords));

            veh.Doors[VehicleDoorIndex.Trunk].Close();

            await this.Stay();
        }

        private async Task SearchVehicle()
        {
            if (this.action == ACTION.Search)
            {
                Main.ShowNotification(Main.settings.dict["alreadySearching"]);
                return;
            }

            Vehicle vehicle = await this.GetClosestVehicle2();
            if (vehicle == null)
            {
                Main.ShowNotification(Main.settings.dict["noVehicleFound"]);
                return;
            }

            Vector3 vehSideR = vehicle.GetOffsetPosition(new Vector3(2.3f, 0.0f, 0.0f));

            Vector3 vehRear = vehicle.GetOffsetPosition(new Vector3(0.0f, -3.3f, 0.0f));

            Vector3 vehSideL = vehicle.GetOffsetPosition(new Vector3(-2.3f, 0.0f, 0.0f));

            this.action = ACTION.Search;

            // Get Inventory 
            bool found = false;

            if (Main.settings.standalone == false)
            {
                Main.ESX.TriggerServerCallback("esx_trunk:getInventoryV", new Action<dynamic>(
                    (inventory) =>
                        {
                            bool isIllegal = Main.ContainsIllegal(inventory);
                            if (isIllegal)
                                found = true;
                        }), vehicle.Mods.LicensePlate);
                Main.ESX.TriggerServerCallback("esx_glovebox:getInventoryV", new Action<dynamic>(
                    (inventory) =>
                        {
                            bool isIllegal = Main.ContainsIllegal(inventory);
                            if (isIllegal)
                                found = true;
                        }), vehicle.Mods.LicensePlate);
            }
            

            Main.ShowNotification($"~y~*{Main.settings.dict["search"]}*");

            this.dog.Task.RunTo(vehSideR, true);

            await Delay(3000);

            this.dog.Task.AchieveHeading(vehicle.Heading - 90);

            vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Open();

            vehicle.Doors[VehicleDoorIndex.BackRightDoor].Open();

            await Delay(3000);


            vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Close();

            vehicle.Doors[VehicleDoorIndex.BackRightDoor].Close();

            this.dog.Task.RunTo(vehRear, true);

            await Delay(3000);

            this.dog.Task.AchieveHeading(vehicle.Heading);

            vehicle.Doors[VehicleDoorIndex.Hood].Open();

            vehicle.Doors[VehicleDoorIndex.Trunk].Open();

            await Delay(3000);

            vehicle.Doors[VehicleDoorIndex.Hood].Close();

            vehicle.Doors[VehicleDoorIndex.Trunk].Close();

            this.dog.Task.RunTo(vehSideL, true);

            await Delay(3000);

            this.dog.Task.AchieveHeading(vehicle.Heading - 270);

            vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].Open();

            vehicle.Doors[VehicleDoorIndex.BackLeftDoor].Open();

            await Delay(3000);

            vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].Close();

            vehicle.Doors[VehicleDoorIndex.BackLeftDoor].Close();

            if (Main.settings.standalone == false)
            {
                if (found)
                {
                    Main.ShowNotification(Main.settings.dict["dogDetects"]);
                }
                else
                {
                    Main.ShowNotification(Main.settings.dict["dogDoesntDetect"]);
                }
            }

            // For people that want to use the search function in their own ways
            TriggerEvent("K9:Export:SearchVehicle", vehicle.Mods.LicensePlate);
            TriggerServerEvent("K9:Export:SearchVehicle", vehicle.Mods.LicensePlate);

            await this.Stay();
        }

        private async Task SearchPlayer()
        {
            if (this.action == ACTION.Search)
            {
                Main.ShowNotification(Main.settings.dict["alreadySearching"]);
                return;
            }

            Ped player = await this.GetClosestPed();
            if (player == null || !player.IsPlayer)
            {
                Main.ShowNotification(Main.settings.dict["noPlayerFound"]);
                return;
            }

            this.action = ACTION.Search;

            Main.ShowNotification($"~y~*{Main.settings.dict["search"]}*");

            Vector3 plySideR = player.GetOffsetPosition(new Vector3(1.3f, 0.0f, 0.0f));

            Vector3 plyRear = player.GetOffsetPosition(new Vector3(0.0f, -1.3f, 0.0f));

            Vector3 plySideL = player.GetOffsetPosition(new Vector3(-1.3f, 0.0f, 0.0f));

            // Get Inventory
            bool found = false;
            if (Main.settings.standalone == false)
            {
                Main.ESX.TriggerServerCallback(
                "esx_inventoryhud:getPlayerInventory",
                new Action<dynamic>(
                    (inventory) =>
                        {
                            bool isIllegal = Main.ContainsIllegal(inventory, true);
                            if (isIllegal)
                                found = true;
                        }), GetPlayerServerId(NetworkGetPlayerIndexFromPed(player.Handle)));
            }
            

            this.dog.Task.RunTo(plySideR, true);

            this.dog.Task.AchieveHeading(player.Heading - 90, -1);

            await Delay(2000);

            this.dog.Task.RunTo(plyRear, true);

            this.dog.Task.AchieveHeading(player.Heading, -1);

            await Delay(2000);

            this.dog.Task.RunTo(plySideL, true);

            this.dog.Task.AchieveHeading(player.Heading - 270, -1);

            if (found)
            {
                Main.ShowNotification(Main.settings.dict["dogDetects"]);
            }
            else
            {
                Main.ShowNotification(Main.settings.dict["dogDoesntDetect"]);
            }

            // For people that want to use the search function in their own ways
            TriggerEvent("K9:Export:SearchPlayer", GetPlayerServerId(NetworkGetPlayerIndexFromPed(player.Handle)));
            TriggerServerEvent("K9:Export:SearchPlayer", GetPlayerServerId(NetworkGetPlayerIndexFromPed(player.Handle)));

            await this.Stay();
        }

        private async Task Attack(string type = "none", int pedHandle = 0)
        {
            try
            {
                Debug.WriteLine(pedHandle.ToString());
                if (pedHandle == 0)
                {
                    if (!this.LocalPlayer.IsAiming && type == "PANIC")
                    {
                        // Attack nearest ped
                        Ped enemy = await this.GetClosestPed();
                        if (enemy != null)
                            await this.AttackPed(enemy);
                    }
                    else
                    {
                        Ped enemy = (Ped)this.LocalPlayer.GetTargetedEntity();

                        if (enemy != null)
                        {
                            await this.AttackPed(enemy);
                        }
                    }
                }
                else if (pedHandle != 0)
                {
                    Entity ped = Entity.FromHandle(pedHandle);
                    await this.AttackPed(ped);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.InnerException.ToString());
            }
        }

        private async Task AttackPed(Entity ped)
        {
            this.dog.Task.ClearAll();
            Main.ShowNotification($"~y~*{Main.settings.dict["attack"]}*");
            this.dog.Task.FightAgainst((Ped)ped);

            this.dog.AlwaysKeepTask = true;

            this.action = ACTION.Attack;

            while (this.action == ACTION.Attack)
            {
                if (!ped.IsAlive)
                {
                    await this.Stay();
                    break;
                }

                await Delay(1000);
            }
        }

        private async Task<Ped> GetClosestPed()
        {
            float maxDistance = 50f;
            Ped[] peds = World.GetAllPeds();
            Ped closestPed = null;
            float lastDistance = maxDistance;
            foreach (Ped ped2 in peds)
            {
                float distance = ped2.Position.DistanceToSquared(Game.Player.Character.Position);
                if (distance < lastDistance && ped2.Handle != this.LocalPlayer.Character.Handle && ped2.IsHuman)
                {
                    closestPed = ped2;
                    lastDistance = distance;
                }
            }

            if (closestPed != null)
            {
                if (closestPed.Handle != this.LocalPlayer.Character.Handle)
                {
                    return closestPed;
                }
            }

            return null;
        }

        private async Task<Vehicle> GetClosestVehicle2()
        {
            float maxDistance = 50f;
            Vehicle[] vehicles = World.GetAllVehicles();
            Vehicle closestVehicle = null;
            float lastDistance = maxDistance;
            foreach (Vehicle vehicle in vehicles)
            {
                float distance = vehicle.Position.DistanceToSquared(Game.Player.Character.Position);
                if (distance < lastDistance && vehicle.Handle != this.LocalPlayer.Character.Handle && vehicle.IsAlive)
                {
                    closestVehicle = vehicle;
                    lastDistance = distance;
                }
            }

            if (closestVehicle != null)
            {
                if (closestVehicle.Handle != this.LocalPlayer.Character.Handle)
                {
                    return closestVehicle;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return (this.dog == null).ToString();
        }
    }
}
