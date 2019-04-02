﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RMC
{
    public class ArmyDef : Def
    {
        public FactionDef factionDef = null;
        public IncidentDef deployIncident = null;
        public IncidentDef reinforceIncident = null;
        public bool useDropPods = true;
        public int minDaysOfFood = 0;
        public int daysOfFoodToGive = 0;
        public ThingDef foodToGive = null;
        public List<UnitDef> unitList = new List<UnitDef>();
        public List<RankDef> rankList = new List<RankDef>();

        public ArmyDef()
        {

        }

        public static ArmyDef Named(string defName)
        {
            return DefDatabase<ArmyDef>.GetNamed(defName);
        }

        public static ArmyDef GetFactionArmy(Faction faction)
        {
            ArmyDef army = null;

            foreach (ArmyDef curArmy in DefDatabase<ArmyDef>.AllDefs)
            {
                if (faction.def.defName == curArmy.factionDef.defName)
                {
                    army = curArmy;
                    break;
                }
            }

            return army;
        }

        public UnitDef GetReinforcements(Map map)
        {
            UnitDef maxUnit = null, survivors = null, reinforcements = null;

            survivors = GetAllSoldiersInArmy();
            maxUnit = GetMaxUnitForMap(map);
            reinforcements = maxUnit.SubtractUnit(survivors);

            return reinforcements;
        }

        public RankDef GetPawnRank(Pawn pawn)
        {
            foreach (RankDef rank in rankList)
                if (pawn.kindDef.defName == rank.pawnKindDef.defName)
                    return rank;

            return null;
        }

        public UnitDef GetUnitFromList(IEnumerable<Pawn> pawns)
        {
            UnitDef unit = new UnitDef();

            foreach (Pawn pawn in pawns)
                unit.Add(GetPawnRank(pawn));

            return unit;
        }

        public UnitDef GetAllSoldiersInArmy()
        {
            return GetUnitFromList(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists);
        }

        public UnitDef GetAllSoldiersInMap(Map map)
        {
            return GetUnitFromList(map.mapPawns.FreeColonistsAndPrisoners);
        }

        public UnitDef GetMaxUnitForMap(Map map)
        {
            UnitDef maxUnit = null;

            foreach (UnitDef unitDef in unitList)
            {
                UnitDef newUnit = unitDef.SetRandomUnitSize();

                if (map.wealthWatcher.WealthBuildings > unitDef.minWealth && (maxUnit == null || maxUnit.GetUnitSize() < newUnit.GetUnitSize()))
                    maxUnit = newUnit;
            }

            return maxUnit;
        }

        public void GiveMinFood(int unitSize, Map map, IntVec3 centerCell)
        {
            if (minDaysOfFood == 0 || daysOfFoodToGive == 0 || foodToGive == null || unitSize * 3 * minDaysOfFood <= (int)map.resourceCounter.TotalHumanEdibleNutrition)
                return;

            Thing foodThing = ThingMaker.MakeThing(foodToGive, null);
            foodThing.stackCount = (int)((unitSize * 3 * daysOfFoodToGive) / foodToGive.statBases.GetStatValueFromList(StatDef.Named("Nutrition"), 1.0f));

            List<Thing> things = new List<Thing>();
            things.Add(foodThing);

            SendToMap(things, map, centerCell);

            return;
        }

        public List<Pawn> GenerateUnit(UnitDef unit)
        {
            List<Pawn> pawns = new List<Pawn>();

            foreach (KeyValuePair<RankDef, RankCount> rankCount in unit.soldierList)
                for (int j = 0; j < rankCount.Value.count; j++)
                    pawns.Add(PawnGenerator.GeneratePawnWithRank(rankCount.Key));

            return pawns;
        }

        public IEnumerable<Thing> SendToMap(IEnumerable<Thing> things, Map map, IntVec3 centerCell)
        {
            if (useDropPods == true)
                DropPodUtility.DropThingsNear(centerCell, map, things);
            else
                MapUtilities.SpawnThingsInMap(map, centerCell, things);

            return things;
        }
    }
}
