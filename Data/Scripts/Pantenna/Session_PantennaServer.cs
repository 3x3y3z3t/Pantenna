// ;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SharedCore;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Pantenna
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class Session_PantennaServer : MySessionComponentBase
    {
        public bool IsServer { get; private set; }
        public bool IsDedicated { get; private set; }
        public bool IsSetupDone { get; private set; }

        private List<IMyPlayer> m_Players = null;
        private HashSet<MyDataBroadcaster> m_Broadcasters = null;

        private Logger m_Logger = null;
        private int m_Ticks = 0;

        public override void LoadData()
        {
            m_Logger = new Logger("Pantenna", LoggerSide.SERVER);
            m_Logger.Init("debug_server.log");
            //m_Logger.Log("Starting LoadData()");

            ServerConfig.Init();

            m_Players = new List<IMyPlayer>();
            m_Broadcasters = new HashSet<MyDataBroadcaster>();



        }

        protected override void UnloadData()
        {
            Shutdown();

            m_Logger.DeInit();
        }

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            ServerConfig config = ServerConfig.GetInstance();

            if (m_Ticks % config.ServerUpdateInterval == 0)
            {
                if (!IsSetupDone)
                {
                    Setup();
                }

                m_Players.Clear();

                MyAPIGateway.Multiplayer.Players.GetPlayers(m_Players);
                foreach (IMyPlayer player in m_Players)
                {
                    if (player.Character == null)
                    {
                        m_Logger.Log(0, "Player <" + player.SteamUserId + ">[" + player.DisplayName + "] doesn't have character.");
                        continue;
                    }

                    string data = GetVisibleSignalAsStringFor(player);
                    bool isSuccess = SendSyncDataToPlayer(player, data);

                }

            }

            // clear ticks count;
            if (m_Ticks >= 2000000000)
                m_Ticks = 0;

            ++m_Ticks;
            // end of methods;
            return;
        }

        public override void SaveData()
        {

        }

        private string GetVisibleSignalAsStringFor(IMyPlayer _player)
        {
            MyDataReceiver receiver = _player.Character.Components.Get<MyDataReceiver>();

            if (receiver == null)
            {
                m_Logger.Log(0, "Character [" + _player.Character + "] doesn't have receiver.");
                return "Error! Character doesn't have receiver.";
            }

            m_Logger.Log(0, "Character [" + _player.Character + "] sees " + receiver.BroadcastersInRange.Count + " signals");
            m_Broadcasters.Clear();
            foreach (MyDataBroadcaster broadcaster in receiver.BroadcastersInRange)
            {
                string strDistance = GetDistanceAsString(_player.GetPosition(), broadcaster.BroadcastPosition);
                
                MyEntity broadcasterEntity = broadcaster.Entity as MyEntity;
                if (broadcasterEntity == null)
                {
                    m_Logger.Log(0, "  (" + strDistance + ") Signal with no Entity");
                    continue;
                }

                m_Logger.Log(0, "  (" + strDistance + ") Signal [" + broadcasterEntity.DisplayName + "]");

                continue;

                IMyCharacter charEnt = broadcasterEntity as IMyCharacter;
                if (charEnt != null)
                {
                    m_Logger.Log(0, "  Signal as Character [" + charEnt.DisplayName + "]");
#if true
                    var relation = _player.GetRelationTo(charEnt.ControllerInfo.ControllingIdentityId);
#else
                    var relation = GetRelationsBetweenPlayers(_player.IdentityId, charEnt.ControllerInfo.ControllingIdentityId);
#endif

                    switch (relation)
                    {

                    }




                    continue;
                }

                MyCubeGrid gridEnt = broadcasterEntity.GetTopMostParent() as MyCubeGrid;
                if (gridEnt == null || gridEnt.IsPreview)
                    continue;

                if (gridEnt != null)
                {

                }




            }



            string str = "";


            return str;
        }

        private bool SendSyncDataToPlayer(IMyPlayer _player, string _data)
        {
            return false;
            //PlayerShieldSyncData data;
            //if (m_Shields.ContainsKey(_player.Character.EntityId))
            //{
            //    data = ShieldEmitterV2.GenerateSyncData(m_Shields[_player.Character.EntityId]);
            //}
            //else
            //{
            //    data = ShieldEmitterV2.GenerateSyncData();
            //}
            //string serializedData = MyAPIGateway.Utilities.SerializeToXML(data);
            //MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID_SYNC, System.Text.Encoding.Unicode.GetBytes(serializedData), _player.SteamUserId);
            //m_Logger.Log(0, "Sent PlayerShieldSyncData to player <" + _player.SteamUserId + ">[" + _player.Character.DisplayName + "].");
        }

        public static string GetDistanceAsString(Vector3D _position1, Vector3D _position2)
        {
            double distance = Vector3D.Distance(_position1, _position2);

            if (distance > 1000.0f)
            {
                return string.Format("{0:F1} Km", distance / 1000.0f);
            }

            return string.Format("{0:F1} m", distance);
        }

        //public static MyRelationsBetweenPlayers GetRelationsBetweenPlayers(long _playerId1, long _playerId2)
        //{
        //    if (_playerId1 == _playerId2)
        //        return MyRelationsBetweenPlayers.Self;

        //    IMyFaction faction1 = MyAPIGateway.Session.Factions.TryGetPlayerFaction(_playerId1);
        //    IMyFaction faction2 = MyAPIGateway.Session.Factions.TryGetPlayerFaction(_playerId2);

        //    if (faction1 == null || faction2 == null)
        //        return MyRelationsBetweenPlayers.Enemies;

        //    if (faction1 == faction2)
        //        return MyRelationsBetweenPlayers.Self;

        //    if (MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction1.FactionId, faction2.FactionId) == MyRelationsBetweenFactions.Neutral)
        //        return MyRelationsBetweenPlayers.Neutral;

        //    return MyRelationsBetweenPlayers.Enemies;
        //}



        //private void GetAllRelayedBroadcasters(MyDataReceiver receiver, long identityId, bool mutual, HashSet<MyDataBroadcaster> output = null)
        //{
        //    if (output == null)
        //    {
        //        output = radioBroadcasters;
        //        output.Clear();
        //    }

        //    foreach (MyDataBroadcaster current in receiver.BroadcastersInRange)
        //    {
        //        if (!output.Contains(current) && !current.Closed && (!mutual || (current.Receiver != null && receiver.Broadcaster != null && current.Receiver.BroadcastersInRange.Contains(receiver.Broadcaster))))
        //        {
        //            output.Add(current);

        //            if (current.Receiver != null && current.CanBeUsedByPlayer(identityId))
        //            {
        //                GetAllRelayedBroadcasters(current.Receiver, identityId, mutual, output);
        //            }
        //        }
        //    }
        //}


        //private void Entities_OnEntityAdd(VRage.ModAPI.IMyEntity _entity)
        //{
        //    IMyCharacter character = _entity as IMyCharacter;
        //    if (character == null)
        //    {
        //        return;
        //    }

        //    character.CharacterDied += Character_CharacterDied;
        //    if (character.IsBot)
        //    {
        //        m_Logger.Log(0, "Character [" + character.DisplayName + "] is Bot.");
        //        /// Bot supports may come soon...
        //        // TODO: handle bot character;
        //        return;
        //    }
        //    else if (!character.IsPlayer)
        //    {
        //        m_Logger.Log(0, "Character [" + character.DisplayName + "] is neither Player nor Bot.");
        //        return;
        //    }

        //    MyInventory inventory = character.GetInventory() as MyInventory;
        //    if (inventory == null)
        //    {
        //        m_Logger.Log(0, "Character [" + character.DisplayName + "] doesn't have inventory.");
        //    }

        //    inventory.ContentsChanged += Inventory_ContentsChanged;
        //}

        //private void Character_CharacterDied(VRage.Game.ModAPI.IMyCharacter _character)
        //{
        //    if (!m_Shields.ContainsKey(_character.EntityId))
        //        return;

        //    foreach (IMyPlayer player in m_Players)
        //    {
        //        if (player.Character != null && player.Character == _character)
        //        {
        //            SendSyncDataToPlayer(player);
        //            break;
        //        }
        //    }

        //    m_Shields.Remove(_character.EntityId);
        //}

        //private void Inventory_ContentsChanged(VRage.Game.Entity.MyInventoryBase _inventory)
        //{
        //    RefreshInventory(_inventory);

        //    foreach (IMyPlayer player in m_Players)
        //    {
        //        if (player.Character != null && player.Character.EntityId == _inventory.Container.Entity.EntityId)
        //        {
        //            SendSyncDataToPlayer(player);
        //            break;
        //        }
        //    }
        //}

        //private void DamageHandler(object _target, ref MyDamageInformation _damageInfo)
        //{
        //    if (_damageInfo.IsDeformation)
        //    {
        //        return;
        //    }

        //    IMyCharacter character = _target as IMyCharacter;
        //    if (character == null)
        //    {
        //        return;
        //    }

        //    if (!m_Shields.ContainsKey(character.EntityId))
        //    {
        //        return;
        //    }

        //    ShieldEmitterV2 emitter = m_Shields[character.EntityId];
        //    emitter.TakeDamage(ref _damageInfo);

        //    foreach (IMyPlayer player in m_Players)
        //    {
        //        if (player.Character != null && player.Character == character)
        //        {
        //            SendSyncDataToPlayer(player);
        //            break;
        //        }
        //    }
        //}

        ////
        //// Summary:
        ////     Allows you do reliable checks WHO have sent message to you.
        ////
        //// Parameters:
        ////   id:
        ////     Uniq handler id
        ////
        ////   messageHandler:
        ////     Call function
        ////     ushort:
        ////       HandlerId
        ////     byte[][]:
        ////       Package
        ////     ulong:
        ////       Player SteamID or 0 for Dedicated server
        ////     bool:
        ////       Sent message comes from server
        ////void RegisterSecureMessageHandler(ushort id, Action<ushort, byte[], ulong, bool> messageHandler);
        //public void HandleInitialSyncRequest(ushort _handlerId, byte[] _package, ulong _playerId, bool _sentMsg)
        //{
        //    string decodedPackage = System.Text.Encoding.Unicode.GetString(_package);
        //    //m_Logger.Log(0, "  _handlerId = " + _handlerId + ", _package = " + decodedPackage + ", _playerId = " + _playerId + ", _sentMsg = " + _sentMsg);
        //    m_Logger.Log(0, "> Recieved message from client <" + _playerId + ">: " + decodedPackage);

        //    if (decodedPackage == Constants.MSG_REQ_INITIAL_VAL)
        //    {
        //        m_Logger.Log(0, ">  Request acknowledged.");
        //        foreach (IMyPlayer player in m_Players)
        //        {
        //            if (player != null && player.SteamUserId == _playerId)
        //            {
        //                SendSyncDataToPlayer(player);
        //                //if (m_Shields.ContainsKey(player.Character.EntityId))
        //                //{
        //                //    SendSyncDataToPlayer(player);
        //                //    //PlayerShieldData data = m_PlayerShieldDataManager.GetData(player);
        //                //    //string syncData = MyAPIGateway.Utilities.SerializeToXML(data);
        //                //    //m_Logger.Log("    Sending PlayerShieldData to player <" + player.SteamUserId + ">.");
        //                //    //MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID, Encoding.Unicode.GetBytes(syncData), player.SteamUserId);
        //                //}
        //                //else
        //                //{
        //                //    m_Logger.Log("    Player <" + _playerId + "> doesn't have shield.");
        //                //}
        //                ////break;
        //                return;
        //            }
        //        }

        //        m_Logger.Log(0, ">  Player <" + _playerId + "> not found on server."); // this should not happens;

        //    }
        //    else
        //    {
        //        m_Logger.Log(0, ">  Unknown message."); // this should not happens;
        //    }
        //}

        public void Setup()
        {
        //    IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
        //    if (!IsServer)
        //        return;
        //    IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;

        //    MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(51, DamageHandler);

        //    //m_Logger.Log("  Registering Message Handler (id " + Constants.MSG_HANDLER_ID_2 + ")...");
        //    MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID_INITIAL_SYNC, HandleInitialSyncRequest);

        //    LoadPlayerShieldData();

        //    MyAPIGateway.Players.GetPlayers(m_Players);
        //    m_Logger.Log(0, "  Player count: " + m_Players.Count);
        //    foreach (IMyPlayer player in m_Players)
        //    {
        //        m_Logger.Log(0, "  Player <" + player.SteamUserId + ">[" + player.DisplayName + "]");
        //        if (player.Character == null)
        //            continue;

        //        MyInventory inventory = player.Character.GetInventory() as MyInventory;
        //        if (inventory == null)
        //            continue;

        //        RefreshInventory(inventory);

        //        if (!m_Shields.ContainsKey(player.Character.EntityId))
        //            continue;

        //        if (m_PlayerShieldSaveData.ContainsKey(player.SteamUserId))
        //        {
        //            ShieldEmitterV2 emitter = m_Shields[player.Character.EntityId];
        //            PlayerShieldSaveData saveData = m_PlayerShieldSaveData[player.SteamUserId];
        //            emitter.Energy = saveData.Energy;
        //            m_Shields[player.Character.EntityId] = emitter;
        //        }
        //        SendSyncDataToPlayer(player);

        //        //ShieldEmitterV2 emitter = m_Shields[player.Character.EntityId];
        //        //PlayerShieldSyncData data = emitter.GenerateSyncData();

        //        //if (IsServer && !IsDedicated)
        //        //{
        //        //    // single player, update directly;
        //        //}
        //        //else
        //        //{
        //        //    // send message to player;
        //        //}
        //    }

        //    m_Logger.Log(0, "  IsServer = " + IsServer);
        //    m_Logger.Log(0, "  IsDedicated = " + IsDedicated);

        //    m_Logger.Log(0, "  Setup Done.");
        //    IsSetupDone = true;
        }

        private void Shutdown()
        {
        //    m_Players.Clear();
        //    m_Shields.Clear();

        //    //m_Logger.Log("    UnRegistering Message Handler (id " + Constants.MSG_HANDLER_ID_2 + ")...");
        //    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.MSG_HANDLER_ID_INITIAL_SYNC, HandleInitialSyncRequest);

        //    ShieldLogger.DeInit();
        //}

        //private HashSet<MyDataBroadcaster> m_Broadcasters = new HashSet<MyDataBroadcaster>();

        //private void GetAllBroadcasters(MyDataReceiver _receiver, long _playerId, bool _mutual)
        //{
        //    m_Broadcasters.Clear();

        //    foreach (MyDataBroadcaster broadcaster in _receiver.BroadcastersInRange)
        //    {
        //        if (m_Broadcasters.Contains(broadcaster) || broadcaster.Closed)
        //            continue;
        //        if (!(_mutual || (broadcaster.Receiver != null && _receiver.Broadcaster != null && broadcaster.Receiver.BroadcastersInRange.Contains(_receiver.Broadcaster))))
        //            continue;

        //        m_Broadcasters.Add(broadcaster);





        //        if ((!mutual || (current.Receiver != null && receiver.Broadcaster != null && current.Receiver.BroadcastersInRange.Contains(receiver.Broadcaster))))
        //        {
        //            output.Add(current);

        //            if (current.Receiver != null && current.CanBeUsedByPlayer(identityId))
        //            {
        //                GetAllRelayedBroadcasters(current.Receiver, identityId, mutual, output);
        //            }
        //        }

        //    }



        }

        //private void RefreshInventory(MyInventoryBase _inventory)
        //{
        //    long characterEntityId = _inventory.Container.Entity.EntityId;
        //    string characterDisplayName = _inventory.Container.Entity.DisplayName;

        //    List<MyPhysicalInventoryItem> inventoryItems = _inventory.GetItems();
        //    m_Logger.Log(0, "  [" + characterDisplayName + "]'s inventory now contains " + inventoryItems.Count + " items.");

        //    {
        //        /// Peek items.
        //        int index = 0;
        //        foreach (MyPhysicalInventoryItem item in inventoryItems)
        //        {
        //            m_Logger.Log(0, "    Found '" + item.Content.SubtypeId + "' at slot " + index + ".");
        //            ++index;
        //        }
        //    }

        //    uint pluginsCount = 0;
        //    uint capPluginsCount = 0;
        //    uint defPluginsCount = 0;
        //    uint kiPluginsCount = 0;
        //    uint exPluginsCount = 0;
        //    const uint MaxPluginsCount = 8;
        //    ShieldItemType shieldTypeFound = ShieldItemType.None;
        //    foreach (MyPhysicalInventoryItem item in inventoryItems)
        //    {
        //        MyStringHash subtypeId = item.Content.SubtypeId;

        //        if (shieldTypeFound == ShieldItemType.None && subtypeId == MyStringHash.GetOrCompute("PocketShieldBasicEmitter"))
        //        {
        //            shieldTypeFound = ShieldItemType.BasicEmitter;
        //        }
        //        else if (shieldTypeFound == ShieldItemType.None && subtypeId == MyStringHash.GetOrCompute("PocketShieldAdvancedEmitter"))
        //        {
        //            shieldTypeFound = ShieldItemType.AdvancedEmitter;
        //        }
        //        else if (pluginsCount < MaxPluginsCount)
        //        {
        //            if (subtypeId == MyStringHash.GetOrCompute("PocketShieldCapacityPlugin"))
        //            {
        //                ++pluginsCount;
        //                ++capPluginsCount;
        //            }
        //            else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldDefensePlugin"))
        //            {
        //                ++pluginsCount;
        //                ++defPluginsCount;
        //            }
        //            else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldBulletResistancePlugin"))
        //            {
        //                ++pluginsCount;
        //                ++kiPluginsCount;
        //            }
        //            else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldExplosiveResistancePlugin"))
        //            {
        //                ++pluginsCount;
        //                ++exPluginsCount;
        //            }
        //        }

        //        if (pluginsCount >= MaxPluginsCount && shieldTypeFound != ShieldItemType.None)
        //            break;
        //    }

        //    if (shieldTypeFound == ShieldItemType.None)
        //    {
        //        if (m_Shields.ContainsKey(characterEntityId))
        //        {
        //            m_Logger.Log(0, "  Character [" + characterDisplayName + "] has lost shield item.");
        //            m_Shields.Remove(characterEntityId);
        //            m_Logger.Log(0, "  Removed shield from list. List now contains " + m_Shields.Count + " shields.");
        //        }
        //        return;
        //    }

        //    Func<IMyCharacter, IMyPlayer> FindPlayerOfThisCharacter = (_character) =>
        //    {
        //        foreach (IMyPlayer player in m_Players)
        //        {
        //            if (player.Character != null && player.Character == _character)
        //                return player;
        //        }

        //        m_Logger.Log(0, "  Cannot find player of character [" + _character.DisplayName + "].");
        //        return null;
        //    };

        //    IMyCharacter character = _inventory.Container.Entity as IMyCharacter;
        //    if (m_Shields.ContainsKey(characterEntityId))
        //    {
        //        // shield item found, and shield exist in list;
        //        ShieldEmitterV2 emitter = m_Shields[characterEntityId];
        //        if (emitter.Type != shieldTypeFound)
        //        {
        //            // different shield: reconstruct new shield;
        //            emitter = ShieldEmitterV2.ConstructEmitter(FindPlayerOfThisCharacter(character), character, shieldTypeFound);
        //            if (emitter == null)
        //            {
        //                m_Logger.Log(0, "  Cannot construct emitter of type '" + shieldTypeFound + "'.");
        //                return;
        //            }
        //        }

        //        emitter.SetCapacityPlugins(capPluginsCount);
        //        emitter.SetDefensePlugins(defPluginsCount);
        //        emitter.SetBulletResPlugins(kiPluginsCount);
        //        emitter.SetExplosionResPlugins(exPluginsCount);
        //        m_Shields[characterEntityId] = emitter;
        //        m_Logger.Log(0, "  Modified shield to type " + shieldTypeFound + " from list. List now contains " + m_Shields.Count + " shields.");
        //    }
        //    else
        //    {
        //        // shield item found, but shield does not exist in list;
        //        // construct new shield;
        //        ShieldEmitterV2 emitter = ShieldEmitterV2.ConstructEmitter(FindPlayerOfThisCharacter(character), character, shieldTypeFound);
        //        if (emitter == null)
        //        {
        //            m_Logger.Log(0, "  Cannot construct emitter of type '" + shieldTypeFound + "'.");
        //            return;
        //        }
        //        emitter.SetCapacityPlugins(capPluginsCount);
        //        emitter.SetDefensePlugins(defPluginsCount);
        //        emitter.SetBulletResPlugins(kiPluginsCount);
        //        emitter.SetExplosionResPlugins(exPluginsCount);
        //        m_Shields.Add(characterEntityId, emitter);
        //        m_Logger.Log(0, "  Added shield to list. List now contains " + m_Shields.Count + " shields.");
        //    }
        //}



        //private void SavePlayerShieldData()
        //{
        //    try
        //    {
        //        System.IO.TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(Constants.SAVEDATA_FILENAME, GetType());
        //        foreach (ShieldEmitterV2 emitter in m_Shields.Values)
        //        {
        //            PlayerShieldSaveData data = emitter.GenerateSaveData();
        //            writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
        //            writer.Flush();
        //        }
        //        writer.Close();
        //    }
        //    catch (Exception _e)
        //    {
        //        m_Logger.Log(0, "Failed to save player shield data (Data is not saved!).");
        //    }
        //}

        //private void LoadPlayerShieldData()
        //{
        //    if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(Constants.SAVEDATA_FILENAME, GetType()))
        //    {
        //        m_Logger.Log(0, "Player shield data save file not found in world storage.");
        //        return;
        //    }

        //    try
        //    {
        //        System.IO.TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Constants.SAVEDATA_FILENAME, typeof(PlayerShieldDataManager));
        //        string xmlText = reader.ReadToEnd();
        //        reader.Close();


        //        PlayerShieldSaveData[] datas = MyAPIGateway.Utilities.SerializeFromXML<PlayerShieldSaveData[]>(xmlText);
        //        m_PlayerShieldSaveData = new Dictionary<ulong, PlayerShieldSaveData>(datas.Length);
        //        foreach (PlayerShieldSaveData data in datas)
        //        {
        //            m_PlayerShieldSaveData.Add(data.SteamUId, data);
        //        }

        //    }
        //    catch (Exception _e)
        //    {
        //        m_Logger.Log(0, "Failed to load player shield data (Data is not loaded!).");
        //    }
        //}

        //private IMyPlayer FindPlayerOfThisCharacterA(IMyCharacter _character)
        //{
        //    foreach (IMyPlayer player in m_Players)
        //    {
        //        if (player.Character != null && player.Character == _character)
        //        {
        //            return player;
        //        }
        //    }

        //    return null;
        //}









    }
}
