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

    class Canine : BaseScript
    {
        public int dog;

        private int model;

        public ACTION action;

        public int target;

        public Canine(int model)
        {
            this.model = model;
        }

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
            this.dog = CreatePed(28, (uint)this.model, pPos.X, pPos.Y, groundZ + 1.0f, pHead, true, true);

            GiveWeaponToPed(this.dog, (uint)GetHashKey("WEAPON_ANIMAL"), 200, true, true);

            SetBlockingOfNonTemporaryEvents(this.dog, true);

            SetPedFleeAttributes(this.dog, 0, false);

            SetPedCombatAttributes(this.dog, 3, true);

            SetPedCombatAttributes(this.dog, 5, true);

            SetPedCombatAttributes(this.dog, 46, true);

            SetEntityHealth(this.dog, 400);

            SetPedArmour(this.dog, 200);

            SetPedMoveRateOverride(this.dog, 3.0f);
            await this.Stay();
        }

        public async Task CallCommand(COMMANDS cmd, string str = "", int num = 0)
        {
            switch (cmd)
            {
                case COMMANDS.Enter:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.EnterVehicle(num);
                    break;
                case COMMANDS.Exit:
                    if (this.action != ACTION.Search)
                        await this.ExitVehicle();
                    break;
                case COMMANDS.Follow:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.Follow();
                    break;
                case COMMANDS.Stay:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.Stay();
                    break;
                case COMMANDS.Attack:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    await this.Attack(str, num);
                    break;
                case COMMANDS.SearchVehicle:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.SearchVehicle();
                    break;
                case COMMANDS.SearchPlayer:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    if (this.action != ACTION.Search)
                        await this.SearchPlayer();
                    break;
                case COMMANDS.Spawn:
                    await this.Spawn();
                    break;
                case COMMANDS.Delete:
                    if (this.action == ACTION.inVehicle)
                        await this.ExitVehicle();
                    await this.Delete();
                    break;
            }
        }

        private async Task<int> Delete()
        {
            DeleteEntity(ref this.dog);
            return this.dog;
        }

        private async Task Heal()
        {
            SetEntityHealth(this.dog, 400);
            SetPedArmour(this.dog, 200);
        }

        private async Task Follow()
        {
            if (this.action == ACTION.inVehicle)
                await this.ExitVehicle();
            ClearPedTasks(this.dog);
            Main.ESX.ShowNotification("~y~*HIER*");
            TaskFollowToOffsetOfEntity(this.dog, PlayerPedId(), 0.0f, 0.0f, 0.0f, 10.0f, -1, 0.5f, true);
            this.action = ACTION.Follow;
        }

        private async Task Stay()
        {
            ClearPedTasks(this.dog);
            Main.ESX.ShowNotification("~y~*BLEIBEN*");
            TaskPlayAnim(this.dog, "creatures@rottweiler@amb@world_dog_sitting@base", "base", 8.0f, -4.0f, -1, 1, 0.0f, false, false, false);

            this.action = ACTION.Stay;
        }

        private async Task EnterVehicle(int veh)
        {
            Vector3 vehCoords = GetEntityCoords(veh, true);
            float forwardX = GetEntityForwardX(veh) * 2.0f;
            float forwardY = GetEntityForwardY(veh) * 2.0f;

            Main.ESX.ShowNotification("~y~*GEH REIN*");

            SetVehicleDoorOpen(veh, 6, true, true);
            TaskFollowNavMeshToCoord(this.dog, vehCoords.X - forwardX, vehCoords.Y - forwardY, vehCoords.Z, 4.0f, -1, 1.0f, true, 1);
            await Delay(3000);
            TaskAchieveHeading(this.dog, GetEntityHeading(veh), -1);
            RequestAnimDict("creatures@rottweiler@in_vehicle@van");
            RequestAnimDict("creatures@rottweiler@amb@world_dog_sitting@base");
            while (!HasAnimDictLoaded("creatures@rottweiler@in_vehicle@van") || !HasAnimDictLoaded("creatures@rottweiler@amb@world_dog_sitting@base"))
            {
                await Delay(5);
            }

            TaskPlayAnim(this.dog, "creatures@rottweiler@in_vehicle@van", "get_in", 8.0f, -4.0f, -1, 2, 0.0f, false, false, false);

            await Delay(700);

            ClearPedTasks(this.dog);
            
            AttachEntityToEntity(this.dog, veh, GetEntityBoneIndexByName(veh, "seat_pside_f"), 0.0f, 0.0f, 0.3f, 0.0f, 0.0f, 0.0f, false, false, false, false, 0, true);

            TaskPlayAnim(this.dog, "creatures@rottweiler@amb@world_dog_sitting@base", "base", 8.0f, -4.0f, -1, 1, 0.0f, false, false, false);

            Wait(500);
            SetVehicleDoorShut(veh, 5, false);

            this.action = ACTION.inVehicle;
        }

        private async Task ExitVehicle()
        {
            if (this.action != ACTION.inVehicle) return;

            Vector3 dCoords = GetEntityCoords(this.dog, true);

            // TO DO: Replace with get entity attached
            int veh = GetClosestVehicle(dCoords.X, dCoords.Y, dCoords.Z, 3.0f, 0, 23);

            Vector3 vehCoords = GetEntityCoords(veh, true);

            float forwardX = GetEntityForwardX(veh) * 3.7f;
            
            float forwardY = GetEntityForwardY(veh) * 3.7f;

            Main.ESX.ShowNotification("~y~*VORAUS*");
            ClearPedTasks(this.dog);

            SetVehicleDoorOpen(veh, 5, false, false);

            DetachEntity(this.dog, true, false);

            float groundZ = 0;
            GetGroundZFor_3dCoord(vehCoords.X - forwardX, vehCoords.Y - forwardY, vehCoords.Z, ref groundZ, false);
            SetEntityCoords(this.dog, vehCoords.X - forwardX, vehCoords.Y - forwardY, groundZ + 1.0f, false, false, false, false);

            SetVehicleDoorShut(veh, 5, false);

            this.Stay();
        }

        private async Task SearchVehicle()
        {
            if (this.action == ACTION.Search)
            {
                Main.ESX.ShowNotification("~y~K9: ~w~Already searching something");
                return;
            }

            int veh = 0;
            Vehicle vehicle = await this.GetClosestVehicle2();
            if (vehicle == null)
            {
                Main.ESX.ShowNotification("~y~K9: ~w~No vehicle found!");
                return;
            }

            veh = vehicle.Handle;

            Vector3 vehSideR = GetOffsetFromEntityInWorldCoords(veh, 2.3f, 0.0f, 0.0f);

            Vector3 vehRear = GetOffsetFromEntityInWorldCoords(veh, 0.0f, -3.3f, 0.0f);

            Vector3 vehSideL = GetOffsetFromEntityInWorldCoords(veh, -2.3f, 0.0f, 0.0f);

            float vehHead = GetEntityHeading(veh);


            this.action = ACTION.Search;

            Main.ESX.ShowNotification("~y~*VERLOREN*");

            TaskFollowNavMeshToCoord(this.dog, vehSideL.X, vehSideL.Y, vehSideL.Z, 3.5f, -1, 1.0f, true, 1);

            await Delay(3000);

            TaskAchieveHeading(this.dog, vehHead - 90, -1);

            SetVehicleDoorOpen(veh, 0, false, false);

            SetVehicleDoorOpen(veh, 2, false, false);

            await Delay(3000);

            SetVehicleDoorShut(veh, 0, false);

            SetVehicleDoorShut(veh, 2, false);


            TaskFollowNavMeshToCoord(this.dog, vehRear.X, vehRear.Y, vehRear.Z, 3.0f, -1, 1.0f, true, 1);

            await Delay(3000);

            TaskAchieveHeading(this.dog, vehHead, -1);

            SetVehicleDoorOpen(veh, 5, false, false);

            SetVehicleDoorOpen(veh, 6, false, false);

            SetVehicleDoorOpen(veh, 7, false, false);

            await Delay(3000);

            SetVehicleDoorShut(veh, 5, false);

            SetVehicleDoorShut(veh, 6, false);

            SetVehicleDoorShut(veh, 7, false);


            TaskFollowNavMeshToCoord(this.dog, vehSideR.X, vehSideR.Y, vehSideR.Z, 3.0f, -1, 1.0f, true, 1);

            await Delay(3000);

            TaskAchieveHeading(this.dog, vehHead - 270, -1);

            SetVehicleDoorOpen(veh, 1, false, false);

            SetVehicleDoorOpen(veh, 3, false, false);

            await Delay(3000);

            SetVehicleDoorShut(veh, 1, false);

            SetVehicleDoorShut(veh, 3, false);

            bool foundDrugs = false;

            Main.ESX.TriggerServerCallback("esx_trunk:getInventoryV", new Action<dynamic>(
                (inventory) =>
                    {
                        bool isIllegal = Main.ContainsIllegal(inventory);
                        if (isIllegal)
                            foundDrugs = true;
                    }), GetVehicleNumberPlateText(veh));
            Main.ESX.TriggerServerCallback("esx_glovebox:getInventoryV", new Action<dynamic>(
                (inventory) =>
                    {
                        bool isIllegal = Main.ContainsIllegal(inventory);
                        if (isIllegal)
                            foundDrugs = true;
                    }), GetVehicleNumberPlateText(veh));

            await Delay(1000);

            if (foundDrugs)
            {
                Main.ESX.ShowNotification("~y~K9: ~w~Detects ~r~something...");
            }
            else
            {
                Main.ESX.ShowNotification("~y~K9: ~w~Detects ~y~nothing...");
            }
            this.Stay();
        }

        private async Task SearchPlayer()
        {
            if (this.action == ACTION.Search)
            {
                Main.ESX.ShowNotification("~y~K9: ~w~Already searching something");
                return;
            }

            int ply = 0;
            Ped player = await this.GetClosestPed();
            if (player == null || !player.IsPlayer)
            {
                Main.ESX.ShowNotification("~y~K9: ~w~No player found!");
                return;
            }

            ply = player.Handle;


            this.action = ACTION.Search;

            Main.ESX.ShowNotification("~y~*VERLOREN*");

            Vector3 plySideR = GetOffsetFromEntityInWorldCoords(ply, 1.3f, 0.0f, 0.0f);

            Vector3 plyRear = GetOffsetFromEntityInWorldCoords(ply, 0.0f, -1.3f, 0.0f);

            Vector3 plySideL = GetOffsetFromEntityInWorldCoords(ply, -1.3f, 0.0f, 0.0f);

            float vehHead = GetEntityHeading(ply);

            TaskFollowNavMeshToCoord(this.dog, plySideL.X, plySideL.Y, plySideL.Z, 3.5f, -1, 1.0f, true, 1);

            await Delay(2000);

            TaskFollowNavMeshToCoord(this.dog, plySideR.X, plySideR.Y, plySideR.Z, 3.5f, -1, 1.0f, true, 1);

            await Delay(2000);

            TaskFollowNavMeshToCoord(this.dog, plyRear.X, plyRear.Y, plyRear.Z, 3.5f, -1, 1.0f, true, 1);

            await Delay(2000);

            bool foundDrugs = false;

            Main.ESX.TriggerServerCallback("esx_inventoryhud:getPlayerInventory", new Action<dynamic>(
                (inventory) =>
                    {
                        bool isIllegal = Main.ContainsIllegal(inventory);
                        if (isIllegal)
                            foundDrugs = true;
                    }), GetPlayerServerId(ply));

            await Delay(1000);

            if (foundDrugs)
            {
                Main.ESX.ShowNotification("~y~K9: ~w~Detects ~r~something...");
            }
            else
            {
                Main.ESX.ShowNotification("~y~K9: ~w~Detects ~y~nothing...");
            }
            this.Stay();
        }

        private async Task Attack(string type = "none", int ped = 0)
        {
            if (!DoesEntityExist(ped))
            {
                if (!IsPlayerFreeAiming(PlayerId()) && type == "PANIC")
                {
                    // Attack nearest ped
                    Ped enemy = await this.GetClosestPed();
                    if (enemy != null)
                        await AttackPed(enemy.Handle);
                }
                else
                {
                    int enemy = 0;
                    bool foundEnemy = GetEntityPlayerIsFreeAimingAt(PlayerId(), ref enemy);
                    if (enemy != 0)
                    {
                        await AttackPed(enemy);
                    }
                    else if (type == "PANIC")
                    {
                        // attack nearest player to dog
                        Vector3 area = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), 0.0f, 10.0f, 0.0f);
                        await Delay(4000);

                        int player = GetNearestPlayerToEntity(this.dog);
                        if (GetPlayerPed(player) != PlayerPedId())
                        {
                            await AttackPed(GetPlayerPed(player));
                        }
                    }
                }
            }
            else if (DoesEntityExist(ped))
            {
                int enemyped = GetPlayerPed(GetPlayerFromServerId(ped));
                if (enemyped != 0)
                {
                    await AttackPed(enemyped);
                }
            }
        }

        private async Task AttackPed(int ped)
        {
            ClearPedTasks(this.dog);
            Main.ESX.ShowNotification("~y~*FASSEN*");
            TaskCombatPed(this.dog, ped, 0, 16);
            SetPedKeepTask(this.dog, true);

            target = ped;
            action = ACTION.Attack;

            while (this.action == ACTION.Attack)
            {
                if (IsEntityDead(ped))
                {
                    this.Stay();
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
                if (distance < lastDistance && ped2.Handle != PlayerPedId() && ped2.IsHuman)
                {
                    closestPed = ped2;
                    lastDistance = distance;
                }
            }
            if (closestPed != null)
            {
                if (closestPed.Handle != PlayerPedId())
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
                if (distance < lastDistance && vehicle.Handle != PlayerPedId() && vehicle.IsAlive)
                {
                    closestVehicle = vehicle;
                    lastDistance = distance;
                }
            }
            if (closestVehicle != null)
            {
                if (closestVehicle.Handle != PlayerPedId())
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
