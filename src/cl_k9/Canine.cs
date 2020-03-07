namespace cl_k9
{
    using System.Threading.Tasks;
    using CitizenFX.Core;
    using static CitizenFX.Core.Native.API;

    public enum ACTION
    {
        Follow,
        Search,
        Stay,
        inVehicle
    }

    class Canine : BaseScript
    {
        public int dog;

        private int model;

        private ACTION action;

        public Canine()
        {
            this.model = GetHashKey("A_C_Rottweiler");
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

            this.dog = CreatePed(28, (uint)this.model, pPos.X, pPos.Y, pPos.Z, pHead, true, true);

            GiveWeaponToPed(this.dog, (uint)GetHashKey("WEAPON_ANIMAL"), 200, true, true);

            SetBlockingOfNonTemporaryEvents(this.dog, true);

            SetPedFleeAttributes(this.dog, 0, false);

            SetPedCombatAttributes(this.dog, 3, true);

            SetPedCombatAttributes(this.dog, 5, true);

            SetPedCombatAttributes(this.dog, 46, true);

            SetEntityHealth(this.dog, 400);

            SetPedArmour(this.dog, 200);

            SetPedMoveRateOverride(this.dog, 3.0f);
            this.action = ACTION.Stay;
        }

        public async Task<int> Delete()
        {
            DeleteEntity(ref this.dog);
            return this.dog;
        }

        public async Task Heal()
        {
            SetEntityHealth(this.dog, 400);
            SetPedArmour(this.dog, 200);
        }

        public void Follow()
        {
            ClearPedTasks(this.dog);
            Main.ESX.ShowNotification("~y~*HIER*");
            TaskFollowToOffsetOfEntity(this.dog, PlayerPedId(), 0.0f, 0.0f, 0.0f, 10.0f, -1, 0.5f, true);
            this.action = ACTION.Follow;
        }

        public void Stay()
        {
            ClearPedTasks(this.dog);
            Main.ESX.ShowNotification("~y~*BLEIBEN*");
            this.action = ACTION.Stay;
        }

        public async Task EnterVehicle(int veh)
        {
            Vector3 vehCoords = GetEntityCoords(veh, true);
            float forwardX = GetEntityForwardX(veh) * 2.0f;
            float forwardY = GetEntityForwardY(veh) * 2.0f;

            Main.ESX.ShowNotification("~y~*GEH REIN*");

            SetVehicleDoorOpen(veh, 6, true, true);
            TaskFollowNavMeshToCoord(this.dog, vehCoords.X - forwardX, vehCoords.Y - forwardY, vehCoords.Z, 4.0f, -1, 1.0f, true, 1);
            await Delay(5000);
            TaskAchieveHeading(this.dog, GetEntityHeading(veh), -1);
            RequestAnimDict("creatures@rottweiler@in_vehicle@van");
            RequestAnimDict("creatures@rottweiler@amb@world_dog_sitting@base");
            while (!HasAnimDictLoaded("creatures@rottweiler@in_vehicle@van") || !HasAnimDictLoaded("creatures@rottweiler@amb@world_dog_sitting@base"))
            {
                await Delay(5);
            }

            TaskPlayAnim(this.dog, "creatures@rottweiler@in_vehicle@van", "get_in", 8.0f, -4.0f, -1, 2, 0.0f, false, false, false);

            Wait(500);

            ClearPedTasks(this.dog);
            
            AttachEntityToEntity(this.dog, veh, GetEntityBoneIndexByName(veh, "seat_pside_f"), 0.0f, 0.0f, 0.3f, 0.0f, 0.0f, 0.0f, false, false, false, false, 0, true);

            TaskPlayAnim(this.dog, "creatures@rottweiler@amb@world_dog_sitting@base", "base", 8.0f, -4.0f, -1, 1, 0.0f, false, false, false);

            Wait(500);
            SetVehicleDoorShut(veh, 5, false);

            this.action = ACTION.inVehicle;
        }

        public async Task ExitVehicle()
        {
            Vector3 dCoords = GetEntityCoords(this.dog, true);

            int veh = GetClosestVehicle(dCoords.X, dCoords.Y, dCoords.Z, 3.0f, 0, 23);

            Vector3 vehCoords = GetEntityCoords(veh, true);

            float forwardX = GetEntityForwardX(veh) * 3.7f;
            
            float forwardY = GetEntityForwardY(veh) * 3.7f;

            Main.ESX.ShowNotification("~y~*VORAUS*");
            ClearPedTasks(this.dog);

            SetVehicleDoorOpen(veh, 5, false, false);

            DetachEntity(this.dog, true, false);

            SetEntityCoords(this.dog, vehCoords.X - forwardX, vehCoords.Y - forwardY, vehCoords.Z - 1.0f, false, false, false, false);

            SetVehicleDoorShut(veh, 5, false);

            this.action = ACTION.Stay;
        }

        public async Task SearchVehicle(int veh)
        {
            Vector3 vehSideR = GetOffsetFromEntityInWorldCoords(veh, 2.3f, 0.0f, 0.0f);

            Vector3 vehRear = GetOffsetFromEntityInWorldCoords(veh, 0.0f, -3.3f, 0.0f);

            Vector3 vehSideL = GetOffsetFromEntityInWorldCoords(veh, -2.3f, 0.0f, 0.0f);

            float vehHead = GetEntityHeading(veh);


            this.action = ACTION.Search;

            Main.ESX.ShowNotification("~y~*VERLOREN*");

            TaskFollowNavMeshToCoord(this.dog, vehSideL.X, vehSideL.Y, vehSideL.Z, 3.5f, -1, 1.0f, true, 1);

            await Delay(4000);

            TaskAchieveHeading(this.dog, vehHead - 90, -1);

            SetVehicleDoorOpen(veh, 0, false, false);

            SetVehicleDoorOpen(veh, 2, false, false);

            await Delay(5000);

            SetVehicleDoorShut(veh, 0, false);

            SetVehicleDoorShut(veh, 2, false);


            TaskFollowNavMeshToCoord(this.dog, vehRear.X, vehRear.Y, vehRear.Z, 3.0f, -1, 1.0f, true, 1);

            await Delay(3000);

            TaskAchieveHeading(this.dog, vehHead, -1);

            SetVehicleDoorOpen(veh, 5, false, false);

            SetVehicleDoorOpen(veh, 6, false, false);

            SetVehicleDoorOpen(veh, 7, false, false);

            await Delay(5000);

            SetVehicleDoorShut(veh, 5, false);

            SetVehicleDoorShut(veh, 6, false);

            SetVehicleDoorShut(veh, 7, false);


            TaskFollowNavMeshToCoord(this.dog, vehSideR.X, vehSideR.Y, vehSideR.Z, 3.0f, -1, 1.0f, true, 1);

            await Delay(3000);

            TaskAchieveHeading(this.dog, vehHead - 270, -1);

            SetVehicleDoorOpen(veh, 1, false, false);

            SetVehicleDoorOpen(veh, 3, false, false);

            await Delay(5000);

            SetVehicleDoorShut(veh, 1, false);

            SetVehicleDoorShut(veh, 3, false);


            Main.ESX.ShowNotification("~y~K9 ~w~Found: Nothing.");

            TriggerServerEvent("K9:Stay");
        }

        public async Task Attack(string type = "none", int ped = 0)
        {
            if (!DoesEntityExist(ped))
            {   
                // Doesnt work rn.
                /*
                if (!IsPlayerFreeAiming(PlayerId()) && type == "PANIC")
                {
                    // Attack nearest ped
                    ClearPedTasks(this.dog);
                    Vector3 area = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), 0.0f, 10.0f, 0.0f);
                    int enemy = GetEntityInDir;
                    int count = 0;

                    if (enemy != 0)
                    {
                        if (enemy != PlayerPedId())
                        { 
                            Debug.WriteLine(IsEntityAPed(enemy).ToString());
                            TaskCombatPed(this.dog, enemy, 0, 16);
                            
                        }
                    }
                }
                else
                {
                    int enemy = 0;
                    bool foundEnemy = GetEntityPlayerIsFreeAimingAt(PlayerId(), ref enemy);
                    Debug.WriteLine(IsEntityAPed(enemy).ToString());
                    if (enemy != 0)
                    { 
                        // attack entity player is pointed at
                        ClearPedTasks(this.dog);
                        TaskCombatPed(this.dog, enemy, 0, 16);
                        SetPedKeepTask(this.dog, true);
                    }
                    else if (type == "PANIC")
                    {
                        // attack nearest player to dog
                        ClearPedTasks(this.dog);
                        Vector3 area = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), 0.0f, 10.0f, 0.0f);
                        await Delay(4000);

                        int player = GetNearestPlayerToEntity(this.dog);
                        if (GetPlayerPed(player) != PlayerPedId())
                        {
                            TaskCombatPed(this.dog, GetPlayerPed(player), 0, 16);
                            SetPedKeepTask(this.dog, true);
                        }
                    }
                }
                */
                int enemy = 0;
                bool foundEnemy = GetEntityPlayerIsFreeAimingAt(PlayerId(), ref enemy);
                if (enemy != 0)
                {
                    // attack entity player is pointed at
                    Main.ESX.ShowNotification("~y~*FASSEN*");
                    ClearPedTasks(this.dog);
                    TaskCombatPed(this.dog, enemy, 0, 16);
                    SetPedKeepTask(this.dog, true);
                }
                else if (type == "PANIC")
                {
                    // attack nearest player to dog
                    ClearPedTasks(this.dog);
                    Vector3 area = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), 0.0f, 10.0f, 0.0f);
                    await Delay(4000);

                    int player = GetNearestPlayerToEntity(this.dog);
                    if (GetPlayerPed(player) != PlayerPedId())
                    {
                        Main.ESX.ShowNotification("~y~*FASSEN*");
                        TaskCombatPed(this.dog, GetPlayerPed(player), 0, 16);
                        SetPedKeepTask(this.dog, true);
                    }
                }
            }
            else if (DoesEntityExist(ped))
            {
                int enemyped = GetPlayerPed(GetPlayerFromServerId(ped));
                if (enemyped != 0)
                {
                    Main.ESX.ShowNotification("~y~*FASSEN*");
                    // Attack player ped
                    TaskCombatPed(this.dog, enemyped, 0, 16);
                    SetPedKeepTask(this.dog, true);
                }
            }
        }

    }
}
