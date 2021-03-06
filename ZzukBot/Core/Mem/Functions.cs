﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ZzukBot.Core.Constants;
using ZzukBot.Core.Game.Statics;
using ZzukBot.Core.Mem.Hooks;
using ZzukBot.Core.Game.Objects;
using ZzukBot.Core.Utilities.Extensions;
using ZzukBot.Core.Utilities.Helpers;
using funcs = ZzukBot.Core.Constants.Offsets.Functions;

namespace ZzukBot.Core.Mem
{
    internal static class Functions
    {
        private static RepopMeDelegate SelectCharacterFunction;
        private static RepopMeDelegate RepopMeFunction;
        private static UnmanagedNoParamsDelegate RetrieveCorpseFunction;
        private static UnmanagedNoParamsDelegate GetLootSlotsFunction;
        private static SetControlBitDelegate SetControlBitFunction;
        private static SetFacingDelegate SetFacingFunction;
        private static SendMovementUpdateDelegate SendMovementUpdateFunction;
        private static OnRightClickUnitDelegate OnRightClickUnitFunction;
        private static OnRightClickObjectDelegate OnRightClickObjectFunction;
        private static LootAllDelegate LootAllFunction;
        private static UnitReactionDelegate UnitReactionFunction;
        private static SetTargetDelegate SetTargetFunction;
        private static ItemCacheGetRowDelegate ItemCacheGetRowFunction;
        private static QuestCacheGetRowDelegate QuestCacheGetRowFunction;
        private static GetSpellCooldownDelegate GetSpellCooldownFunction;
        private static UseItemDelegate UseItemFunction;
        private static CtmDelegate CtmFunction;
        private static AcceptQuestDelegate AcceptQuestFunction;
        private static AcceptQuestDelegate CompleteQuestFunction;
        private static NetClientSendDelegate NetClientSendFunction;
        private static ClientConnectionDelegate ClientConnectionFunction;
        private static GetCreatureRankDelegate GetCreatureRankFunction;
        private static GetCreatureTypeDelegate GetCreatureTypeFunction;
        private static GetCreatureFamilyDelegate GetCreatureFamilyFunction;
        private static LuaGetArgCountDelegate LuaGetArgCountFunction;
        //private static HandleSpellTerrainDelegate HandleSpellTerrainFunction;
        private static GetPlayerGuidDelegate GetPlayerGuidFunction;
        private static ClntObjMgrObjectPtr GetPtrForGuidFunction;
        private static CanCompleteQuestDelegate CanCompleteQuestFunction;
        private static CanUseItemDelegate CanUseItemFunction;
        private static CastAtPosDelegate CastAtPosFunction;
        //private static LootSlotDelegate LootSlotFunction;
        private static GameObjectGetPositionDelegate GameObjectGetPositionFunction;
        private static AbandonQuestDelegate AbandonQuestFunction;
        private static GetGameObjectPositionDelegate GetGameObjectPositionFunction;


        [DllImport(Libs.FastCall, EntryPoint= "_MultiplyTransformWithFacingMatrix", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr _MultiplyTransformWithFacingMatrix(IntPtr returnMatrix, IntPtr facingMatrix, IntPtr posMatrix, IntPtr funcPtr);


        [DllImport(Libs.FastCall, EntryPoint = "_RegFunc", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppRegFunc(string parFuncName, uint parFuncPtr, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_LuaPushString", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppLuaPushString(IntPtr parLuaState, string parString, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_UnregFunc", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppUnregFunc(string parFuncName, uint parFuncPtr, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_LuaToString", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr _CppLuaToString(IntPtr parLuaState, int number, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_GetText", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr _CppGetText(string varName, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_DoString", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppDoString(string parLuaCode, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_SellItem", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppSellItem(uint parLuaCode, ulong parVendorGuid, ulong parItemGuid, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_Intersect", CallingConvention = CallingConvention.StdCall)]
        private static extern byte _Intersect(ref XYZXYZ points, ref float distance, ref Intersection intersection,
            uint flags, IntPtr Ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_LootSlot", CallingConvention = CallingConvention.StdCall)]
        private static extern byte _LootSlot(int parSlot, IntPtr Ptr);

        internal static void GetPosition(IntPtr ptr, IntPtr bytes)
        {
            if (GameObjectGetPositionFunction == null)
                GameObjectGetPositionFunction = Mem.Memory.Reader.RegisterDelegate<GameObjectGetPositionDelegate>((IntPtr)0x005F9F50);
            MainThread.Instance.Invoke(() => GameObjectGetPositionFunction(ptr, bytes));
        }

        internal static void AbandonQuest(int questRealQuestIndex)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (AbandonQuestFunction == null)
                AbandonQuestFunction = Memory.Reader.RegisterDelegate<AbandonQuestDelegate>(funcs.AbandonQuest);
            MainThread.Instance.Invoke(() => AbandonQuestFunction(questRealQuestIndex));
        }

        internal static Intersection Intersect(Location parStart, Location parEnd)
        {
            if (!ObjectManager.Instance.IsIngame) new Intersection();
            var points = new XYZXYZ(parStart.X, parStart.Y, parStart.Z,
                parEnd.X, parEnd.Y, parEnd.Z);
            points.Z1 += 2;
            points.Z2 += 2;
            var intersection = new Intersection();
            var distance = parStart.GetDistanceTo(parEnd);
            return MainThread.Instance.Invoke(() =>
            {
                _Intersect(ref points, ref distance, ref intersection, 0x100111, funcs.Intersect);
                return intersection;
            });
        }

        internal static unsafe Location GetGameObjectPosition(WoWGameObject obj)
        {
            if (!ObjectManager.Instance.IsIngame) return new Location(0, 0, 0);
            if (GetGameObjectPositionFunction == null)
                GetGameObjectPositionFunction = Memory.Reader.RegisterDelegate<GetGameObjectPositionDelegate>(funcs.GetGameObjectPosition);
            var xyzStruct = new XYZ();
            GetGameObjectPositionFunction(obj.Pointer, &xyzStruct);
            return new Location(ref xyzStruct);
        }

        internal static void LootSlot(int parSlot)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            //if (LootSlotFunction == null)
            //    LootSlotFunction = Mem.Memory.Reader.RegisterDelegate<LootSlotDelegate>(Offsets.Functions.LootSlotAt);
            //MainThread.Instance.Invoke(() => LootSlotFunction(parSlot));
            MainThread.Instance.Invoke(
                () => _LootSlot(parSlot, funcs.LootSlotAt)
            );
        }

        internal static void CastAtPos(string parSpell, Location parPos, int parRank = -1)
        {
            if (CastAtPosFunction == null)
                CastAtPosFunction = Memory.Reader.RegisterDelegate<CastAtPosDelegate>(funcs.CastAtPos);
            if (!ObjectManager.Instance.IsIngame) return;
            MainThread.Instance.Invoke(() =>
            {
                Memory.Reader.Write((IntPtr) 0xCECAC0, 0);
                Spell.Instance.Cast(parSpell, parRank);
                var pos = parPos.ToStruct;
                CastAtPosFunction(ref pos);
            });
        }

        //internal static void RegisterFunction(string parFuncName, uint parFuncPtr)
        //{
        //    MainThread.Instance.Invoke(() => _CppRegFunc(parFuncName, parFuncPtr, funcs.LuaRegisterFunc));
        //}

        //internal static void UnregisterFunction(string parFuncName, uint parFuncPtr)
        //{
        //    MainThread.Instance.Invoke(() => _CppUnregFunc(parFuncName, parFuncPtr, funcs.LuaUnregFunc));
        //}

        internal static string GetText(string parVarName)
        {
            return MainThread.Instance.Invoke(delegate
            {
                var addr = _CppGetText(parVarName, funcs.GetText);
                return addr.ReadString();
            });
        }

        internal static bool CanCompleteQuest(int parQuestEntry)
        {
            if (!ObjectManager.Instance.IsIngame) return false;
            if (CanCompleteQuestFunction == null)
                CanCompleteQuestFunction =
                    Memory.Reader.RegisterDelegate<CanCompleteQuestDelegate>(funcs.CanCompleteQuest);
            var ret = MainThread.Instance.Invoke(() =>
            {
                var result = CanCompleteQuestFunction(parQuestEntry);
                return result;
            });
            return ret == 1;
        }

        internal static bool CanUseItem(int parItemId, PrivateEnums.ItemCacheLookupType parType)
        {
            if (!ObjectManager.Instance.IsIngame) return false;
            if (CanUseItemFunction == null)
                CanUseItemFunction = Memory.Reader.RegisterDelegate<CanUseItemDelegate>(funcs.CanUseItem);
            var ret = MainThread.Instance.Invoke(() =>
            {
                var ptr1 = ObjectManager.Instance.Player.Pointer;
                if (ptr1 == IntPtr.Zero) return 0;
                var ptr2 = ObjectManager.Instance.LookupItemCachePtr(parItemId, parType);
                if (ptr2 == IntPtr.Zero) return 0;
                var randomInt = 1;
                var result = CanUseItemFunction(ptr1, ptr2, ref randomInt);
                return result;
            });
            return ret == 1;
        }

        internal static string[] GetText(string[] parVarName)
        {
            return MainThread.Instance.Invoke(delegate
            {
                var ret = new List<string>();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var x in parVarName)
                {
                    var addr = _CppGetText(x, funcs.GetText);
                    ret.Add(addr.ReadString());
                }
                return ret.ToArray();
            });
        }

        internal static int GetLootSlots()
        {
            if (!ObjectManager.Instance.IsIngame) return 0;
            if (GetLootSlotsFunction == null)
                GetLootSlotsFunction = Memory.Reader.RegisterDelegate<UnmanagedNoParamsDelegate>(funcs.GetLootSlots);
            return MainThread.Instance.Invoke(() => GetLootSlotsFunction());
        }

        internal static void RetrieveCorpse()
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (RetrieveCorpseFunction == null)
                RetrieveCorpseFunction = Memory.Reader.RegisterDelegate<UnmanagedNoParamsDelegate>(funcs.RetrieveCorpse);
            MainThread.Instance.Invoke(() => RetrieveCorpseFunction());
        }

        internal static void RepopMe()
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (RepopMeFunction == null)
                RepopMeFunction = Memory.Reader.RegisterDelegate<RepopMeDelegate>(funcs.RepopMe);
            var player = ObjectManager.Instance.Player;
            if (player == null) return;
            MainThread.Instance.Invoke(() => RepopMeFunction(player.Pointer));
        }

        internal static void DoString(string parLuaCode)
        {
            MainThread.Instance.Invoke(() => _CppDoString(parLuaCode, funcs.DoString));
        }

        internal static void SellItem(uint parItemCount, ulong parItemGuid)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            MainThread.Instance.Invoke(
                () => _CppSellItem(parItemCount, ObjectManager.Instance.Player.VendorGuid, parItemGuid, funcs.SellItem));
        }

        internal static ulong GetPlayerGuid()
        {
            if (!ObjectManager.Instance.IsIngame) return 0;
            if (GetPlayerGuidFunction == null)
                GetPlayerGuidFunction =
                    Memory.Reader.RegisterDelegate<GetPlayerGuidDelegate>(funcs.ClntObjMgrGetActivePlayer);

            return MainThread.Instance.Invoke(() => GetPlayerGuidFunction());
        }

        internal static IntPtr GetPtrForGuid(ulong parGuid)
        {
            if (!ObjectManager.Instance.IsIngame) return IntPtr.Zero;
            if (GetPtrForGuidFunction == null)
                GetPtrForGuidFunction = Memory.Reader.RegisterDelegate<ClntObjMgrObjectPtr>(funcs.GetPtrForGuid);

            return MainThread.Instance.Invoke(() => GetPtrForGuidFunction(parGuid));
        }

        internal static void EnumVisibleObjects(IntPtr callback, int filter)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            MainThread.Instance.Invoke(() => _CppEnumVisibleObjects(callback, filter, funcs.EnumVisibleObjects));
        }

        internal static void BuyVendorItem(int parItemIndex, int parQuantity)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (parItemIndex < 1) return;

            var ptr1 = 0xBDD118 + 0x1C * (parItemIndex - 1);
            if (ptr1.ReadAs<int>() == 0) return;

            var itemId = (ptr1 + 4).ReadAs<int>();

            MainThread.Instance.Invoke(
                () =>
                    _CppBuyVendorItem(itemId, parQuantity, ObjectManager.Instance.Player.VendorGuid, funcs.BuyVendorItem));
        }

        internal static void SelectCharacterAtIndex(int index)
        {
            if (ObjectManager.Instance.IsIngame) return;
            if (SelectCharacterFunction == null)
                SelectCharacterFunction =
                    Memory.Reader.RegisterDelegate<RepopMeDelegate>(funcs.SelectCharacter);
            MainThread.Instance.Invoke(() => SelectCharacterFunction((IntPtr) index));
        }

        internal static void EnterWorld()
        {
            if (ObjectManager.Instance.IsIngame) return;
            const string str = "if CharSelectEnterWorldButton ~= nil then CharSelectEnterWorldButton:Click()  end";
            DoString(str);
        }

        internal static void SetControlBit(int parBit, int parState, int parTickCount)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (SetControlBitFunction == null)
                SetControlBitFunction =
                    Memory.Reader.RegisterDelegate<SetControlBitDelegate>(funcs.CGInputControl__SetControlBit);
            var ptr = Memory.Reader.Read<IntPtr>(Offsets.Misc.CGInputControlActive);

            MainThread.Instance.Invoke(() => SetControlBitFunction(ptr, parBit, parState, parTickCount));
        }

        internal static void SetFacing(IntPtr parPlayerPtr, float parFacing)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (SetFacingFunction == null)
                SetFacingFunction = Memory.Reader.RegisterDelegate<SetFacingDelegate>(funcs.SetFacing);
            MainThread.Instance.Invoke(() => SetFacingFunction(parPlayerPtr, parFacing));
        }

        internal static void SendMovementUpdate(IntPtr parPlayerPtr, int parTimeStamp, int parOpcode)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (SendMovementUpdateFunction == null)
                SendMovementUpdateFunction =
                    Memory.Reader.RegisterDelegate<SendMovementUpdateDelegate>(funcs.SendMovementPacket);

            MainThread.Instance.Invoke(() => SendMovementUpdateFunction(parPlayerPtr, parTimeStamp, parOpcode, 0, 0));
        }

        internal static void OnRightClickUnit(IntPtr parPlayerPtr, int parAutoLoot)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (OnRightClickUnitFunction == null)
                OnRightClickUnitFunction =
                    Memory.Reader.RegisterDelegate<OnRightClickUnitDelegate>(funcs.OnRightClickUnit);

            MainThread.Instance.Invoke(() => OnRightClickUnitFunction(parPlayerPtr, parAutoLoot));
        }

        internal static void OnRightClickObject(IntPtr parPlayerPtr, int parAutoLoot)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (OnRightClickObjectFunction == null)
                OnRightClickObjectFunction =
                    Memory.Reader.RegisterDelegate<OnRightClickObjectDelegate>(funcs.OnRightClickObject);

            MainThread.Instance.Invoke(() => OnRightClickObjectFunction(parPlayerPtr, parAutoLoot));
        }

        internal static void LootAll()
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (LootAllFunction == null)
                LootAllFunction = Memory.Reader.RegisterDelegate<LootAllDelegate>(funcs.AutoLoot);
            MainThread.Instance.Invoke(() => LootAllFunction());
        }

        internal static Enums.UnitReaction UnitReaction(IntPtr unitPtr1, IntPtr unitPtr2)
        {
            if (!ObjectManager.Instance.IsIngame) return Enums.UnitReaction.Neutral;
            if (UnitReactionFunction == null)
                UnitReactionFunction = Memory.Reader.RegisterDelegate<UnitReactionDelegate>(funcs.UnitReaction);

            var ret = UnitReactionFunction(unitPtr1, unitPtr2);
            if (Enum.IsDefined(typeof(Enums.UnitReaction), ret))
                return (Enums.UnitReaction) ret;
            return Enums.UnitReaction.Neutral;
        }

        internal static void SetTarget(ulong parGuid)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (SetTargetFunction == null)
                SetTargetFunction = Memory.Reader.RegisterDelegate<SetTargetDelegate>(funcs.SetTarget);
            MainThread.Instance.Invoke(() => SetTargetFunction(parGuid));
        }

        internal static IntPtr QuestCacheGetRow(int parQuestId)
        {
            if (!ObjectManager.Instance.IsIngame) return IntPtr.Zero;
            if (QuestCacheGetRowFunction == null)
                QuestCacheGetRowFunction =
                    Memory.Reader.RegisterDelegate<QuestCacheGetRowDelegate>(funcs.QuestCacheGetRow);

            ulong guid = 0;
            return
                MainThread.Instance.Invoke(
                    () =>
                        QuestCacheGetRowFunction(funcs.QuestCacheBasePtr, parQuestId, ref guid,
                            CacheCallbacks.Instance.QuestCallbackPtr, 0,
                            0));
        }

        internal static IntPtr ItemCacheGetRow(int parItemId, PrivateEnums.ItemCacheLookupType parLookupType)
        {
            if (!ObjectManager.Instance.IsIngame) return IntPtr.Zero;
            if (ItemCacheGetRowFunction == null)
                ItemCacheGetRowFunction = Memory.Reader.RegisterDelegate<ItemCacheGetRowDelegate>(funcs.ItemCacheGetRow);

            ulong val = 0;
            switch (parLookupType)
            {
                case PrivateEnums.ItemCacheLookupType.None:
                    return
                        MainThread.Instance.Invoke(
                            () =>
                                ItemCacheGetRowFunction(funcs.ItemCacheBasePtr, parItemId, ref val,
                                    CacheCallbacks.Instance.ItemCallbackPtr,
                                    0, 0x0));

                case PrivateEnums.ItemCacheLookupType.Vendor:
                    val = ObjectManager.Instance.Player.VendorGuid;
                    return
                        MainThread.Instance.Invoke(
                            () => ItemCacheGetRowFunction(funcs.ItemCacheBasePtr, parItemId, ref val,
                                CacheCallbacks.Instance.ItemCallbackPtr,
                                0, 0x0));

                case PrivateEnums.ItemCacheLookupType.Quest:
                    val = ObjectManager.Instance.Player.QuestNpcGuid;
                    return
                        MainThread.Instance.Invoke(
                            () => ItemCacheGetRowFunction(funcs.ItemCacheBasePtr, parItemId, ref val,
                                CacheCallbacks.Instance.ItemCallbackPtr,
                                0, 0x0));
                default:
                    return IntPtr.Zero;
            }
        }

        internal static bool IsSpellReady(int spellId)
        {
            if (!ObjectManager.Instance.IsIngame) return false;
            if (GetSpellCooldownFunction == null)
                GetSpellCooldownFunction =
                    Memory.Reader.RegisterDelegate<GetSpellCooldownDelegate>(funcs.GetSpellCooldown);

            var CdDuration = 0;
            var CdStartedAt = 0;
            var third = false;
            MainThread.Instance.Invoke(
                () =>
                    GetSpellCooldownFunction(funcs.GetSpellCooldownPtr1, spellId, 0, ref CdDuration, ref CdStartedAt,
                        ref third));
            return CdDuration == 0 || CdStartedAt == 0;
        }

        internal static void UseItem(IntPtr ptr, ulong guidOfOtherItem = 0)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (UseItemFunction == null)
                UseItemFunction = Memory.Reader.RegisterDelegate<UseItemDelegate>(funcs.UseItem);
            var ptrToGuid = guidOfOtherItem;

            MainThread.Instance.Invoke(() => UseItemFunction(ptr, ref ptrToGuid, 0));
        }

        internal static void UseItemAtPos(IntPtr ptr, Location parPos)
        {
            if (CastAtPosFunction == null)
                CastAtPosFunction = Memory.Reader.RegisterDelegate<CastAtPosDelegate>(funcs.CastAtPos);
            if (!ObjectManager.Instance.IsIngame) return;
            MainThread.Instance.Invoke(() =>
            {
                Memory.Reader.Write((IntPtr) 0xCECAC0, 64);
                UseItem(ptr);
                var pos = parPos.ToStruct;
                CastAtPosFunction(ref pos);
            });
        }

        internal static void Ctm(IntPtr parPlayerPtr, PrivateEnums.CtmType parType, Location parPosition, ulong parGuid)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (CtmFunction == null)
                CtmFunction = Memory.Reader.RegisterDelegate<CtmDelegate>(funcs.ClickToMove);
            var guid = parGuid;
            var xyz = parPosition.ToStruct;
            MainThread.Instance.Invoke(() =>
            {
                CtmFunction(parPlayerPtr, (uint) parType, ref guid,
                    ref xyz, 2);
            });
        }

        internal static void AcceptQuest(ulong parNpcGuid, int parQuestId)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (AcceptQuestFunction == null)
                AcceptQuestFunction = Memory.Reader.RegisterDelegate<AcceptQuestDelegate>(funcs.AcceptQuest);

            MainThread.Instance.Invoke(() => AcceptQuestFunction(ref parNpcGuid, parQuestId));
        }

        internal static void CompleteQuest(ulong parNpcGuid, int parQuestId)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (CompleteQuestFunction == null)
                CompleteQuestFunction = Memory.Reader.RegisterDelegate<AcceptQuestDelegate>(funcs.CompleteQuest);
            MainThread.Instance.Invoke(() => CompleteQuestFunction(ref parNpcGuid, parQuestId));
        }

        internal static void NetClientSend(IntPtr pDataStore)
        {
            if (!ObjectManager.Instance.IsIngame) return;
            if (NetClientSendFunction == null)
                NetClientSendFunction = Memory.Reader.RegisterDelegate<NetClientSendDelegate>(funcs.NetClientSend);
            MainThread.Instance.Invoke(() => NetClientSendFunction(ClientConnection(), pDataStore.ToInt32()));
        }

        internal static IntPtr ClientConnection()
        {
            if (!ObjectManager.Instance.IsIngame) return IntPtr.Zero;
            if (ClientConnectionFunction == null)
                ClientConnectionFunction =
                    Memory.Reader.RegisterDelegate<ClientConnectionDelegate>(funcs.ClientConnection);
            return MainThread.Instance.Invoke(() => ClientConnectionFunction());
        }

        internal static int GetCreatureRank(IntPtr parUnitPtr)
        {
            if (!ObjectManager.Instance.IsIngame) return 0;
            if (GetCreatureRankFunction == null)
                GetCreatureRankFunction = Memory.Reader.RegisterDelegate<GetCreatureRankDelegate>(funcs.GetCreatureRank);
            return MainThread.Instance.Invoke(() => GetCreatureRankFunction(parUnitPtr));
        }

        internal static Enums.CreatureType GetCreatureType(IntPtr parUnitPtr)
        {
            if (!ObjectManager.Instance.IsIngame) return 0;
            if (GetCreatureTypeFunction == null)
                GetCreatureTypeFunction = Memory.Reader.RegisterDelegate<GetCreatureTypeDelegate>(funcs.GetCreatureType);
            return (Enums.CreatureType)GetCreatureTypeFunction(parUnitPtr);
        }

        internal static Enums.CreatureFamily GetCreatureFamily(IntPtr parUnitPtr)
        {
            if (!ObjectManager.Instance.IsIngame) return 0;
            if (GetCreatureFamilyFunction == null)
                GetCreatureFamilyFunction = Memory.Reader.RegisterDelegate<GetCreatureFamilyDelegate>(funcs.GetCreatureFamily);
            return (Enums.CreatureFamily)GetCreatureFamilyFunction(parUnitPtr);
        }

        internal static int LuaGetArgCount(IntPtr parLuaState)
        {
            if (LuaGetArgCountFunction == null)
                LuaGetArgCountFunction = Memory.Reader.RegisterDelegate<LuaGetArgCountDelegate>(funcs.LuaGetArgCount);
            return MainThread.Instance.Invoke(() => LuaGetArgCountFunction(parLuaState));
        }

        internal static string LuaToString(IntPtr parLuaState, int number)
        {
            var ptr = MainThread.Instance.Invoke(() => _CppLuaToString(parLuaState, number, funcs.LuaToString));
            return ptr.ReadString();
        }

        [DllImport(Libs.FastCall, EntryPoint = "_EnumVisibleObjects", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppEnumVisibleObjects(IntPtr callback, int filter, IntPtr ptr);

        [DllImport(Libs.FastCall, EntryPoint = "_BuyVendorItem", CallingConvention = CallingConvention.StdCall)]
        private static extern void _CppBuyVendorItem(int parItemIndex, int parQuantity, ulong parVendorGuid, IntPtr ptr);

        
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int RepopMeDelegate(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int UnmanagedNoParamsDelegate();
        
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetControlBitDelegate(IntPtr device, int bit, int state, int tickCount);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetFacingDelegate(IntPtr playerPtr, float facing);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SendMovementUpdateDelegate(
            IntPtr playerPtr, int timestamp, int opcode, float zero, int zero2);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void OnRightClickUnitDelegate(IntPtr unitPtr, int autoLoot);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void OnRightClickObjectDelegate(IntPtr unitPtr, int autoLoot);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void LootAllDelegate();

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int UnitReactionDelegate(IntPtr unitPtr1, IntPtr unitPtr2);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SetTargetDelegate(ulong guid);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private unsafe delegate void GetGameObjectPositionDelegate(IntPtr pointer, XYZ* pos);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr ItemCacheGetRowDelegate(
            IntPtr fixedPtr, int itemId, ref ulong guid, IntPtr callbackPtr, int _zero, int __zero);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr QuestCacheGetRowDelegate(
            IntPtr fixedPtr, int itemId, ref ulong guid, IntPtr callbackPtr, int __zero, int ___zero);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void GetSpellCooldownDelegate(
            IntPtr spellCooldownPtr, int spellId, int zero, ref int first, ref int second, ref bool third);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void UseItemDelegate(IntPtr ptr, ref ulong guidOfOtherItem, int zero);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void CtmDelegate
            (IntPtr playerPtr, uint clickType, ref ulong interactGuidPtr, ref XYZ posPtr, float precision);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void AcceptQuestDelegate
            (ref ulong parPtrToNpcGuid, int parQuestId);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NetClientSendDelegate
            (IntPtr clientConn, int pDataStore);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr ClientConnectionDelegate
            ();

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetCreatureRankDelegate
            (IntPtr parUnitPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetCreatureTypeDelegate
            (IntPtr parUnitPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetCreatureFamilyDelegate
            (IntPtr parUnitPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int LuaGetArgCountDelegate
            (IntPtr parLuaState);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int HandleSpellTerrainDelegate
            (ref XYZ parPos);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ulong GetPlayerGuidDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr ClntObjMgrObjectPtr(ulong guid);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int CanCompleteQuestDelegate(int parQuestEntry);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int CanUseItemDelegate(IntPtr parPlayerPtr, IntPtr parItemCacheEntryPtr, ref int parPtr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int CastAtPosDelegate(ref XYZ parPos);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int LootSlotDelegate(int slotNumber);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void GameObjectGetPositionDelegate(IntPtr objPtr, IntPtr xyzStruct);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void AbandonQuestDelegate
            (int realQuestId);
    }
}