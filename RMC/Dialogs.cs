﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace RMC
{
    public class Dialog_Recruit : Window
    {
        protected Pawn negotiator;
        protected static readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);
        protected static readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);
        private Vector2 scrollPosition;
        ArmyDef army;
        UnitDef reinforcements = new UnitDef();

        static Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow", true);

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(720f, 600f);
            }
        }

        public Dialog_Recruit(Pawn negotiator, bool radioMode)
        {
            this.negotiator = negotiator;
            this.army = ArmyDef.GetFactionArmy(negotiator.Faction);

            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.soundAppear = SoundDefOf.CommsWindow_Open;
            this.soundClose = SoundDefOf.CommsWindow_Close;

            for (int i = 0; i < army.rankList.Count; i++)
                reinforcements.Add(army.rankList[i]);
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            inRect = inRect.AtZero();

            Rect position = new Rect(0f, 0f, inRect.width, 58f);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Medium;

            Rect rect = new Rect(0f, 0f, position.width / 2f, position.height);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect, Faction.OfPlayer.Name.Truncate(rect.width, null));

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(0f, 27f, position.width / 2f, position.height / 2f);
            Widgets.Label(rect3, "Negotiator".Translate() + ": " + negotiator.LabelShort);

            GUI.EndGroup();

            Rect rect6 = new Rect(0f, 58f, 116f, 30f);
            Widgets.Label(rect6, negotiator.Map.resourceCounter.Silver.ToString());

            Rect rect62 = new Rect(inRect.width - 143f, 58f, 100f, 30f);
            GUI.color = Color.red;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect62, "-" + reinforcements.GetUnitCost().ToString());

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Color.white;

            float num3 = inRect.width - 16f;
            Widgets.DrawLineHorizontal(0f, 87f, num3);
            float num2 = 30f;

            Rect mainRect = new Rect(0f, 58f + num2, inRect.width, inRect.height - 58f - 38f - num2 - 20f);

            this.FillMainRect(mainRect);

            Rect rect7 = new Rect(inRect.width / 2f - Dialog_Recruit.AcceptButtonSize.x / 2f, inRect.height - 55f, Dialog_Recruit.AcceptButtonSize.x, Dialog_Recruit.AcceptButtonSize.y);
            if (Widgets.ButtonText(rect7, "AcceptButton".Translate(), true, false, true))
            {
                Action action = delegate
                {
                    MapUtilities.DestroyThingsInMap(negotiator.Map, ThingDef.Named("Silver"), (int)reinforcements.GetUnitCost());

                    int arrivalTick = Find.TickManager.TicksGame + reinforcements.GetUnitSpawnTime();

                    IncidentParms_Deploy incidentParms = new IncidentParms_Deploy();
                    incidentParms.target = negotiator.Map;
                    incidentParms.faction = negotiator.Faction;
                    incidentParms.forced = true;
                    incidentParms.reinforcements = reinforcements;
                    Find.Storyteller.incidentQueue.Add(DefDatabase<IncidentDef>.GetNamed("RMC_IncidentDef_Deploy"), arrivalTick, incidentParms, 240000);

                    this.Close(true);
                };

                if (negotiator.Map.resourceCounter.Silver >= reinforcements.GetUnitCost())
                {
                    action();
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
                    Messages.Message("MessageColonyCannotAfford".Translate(), MessageTypeDefOf.RejectInput, false);
                }

                Event.current.Use();
            }

            Rect rect8 = new Rect(rect7.x - 10f - Dialog_Recruit.OtherBottomButtonSize.x, rect7.y, Dialog_Recruit.OtherBottomButtonSize.x, Dialog_Recruit.OtherBottomButtonSize.y);
            if (Widgets.ButtonText(rect8, "ResetButton".Translate(), true, false, true))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);

                reinforcements.soldierList.Clear();

                for (int i = 0; i < army.rankList.Count; i++)
                    reinforcements.Add(army.rankList[i]);
            }

            Rect rect9 = new Rect(rect7.xMax + 10f, rect7.y, Dialog_Recruit.OtherBottomButtonSize.x, Dialog_Recruit.OtherBottomButtonSize.y);
            if (Widgets.ButtonText(rect9, "CancelButton".Translate(), true, false, true))
            {
                this.Close(true);
                Event.current.Use();
            }

            GUI.EndGroup();
        }

        private void FillMainRect(Rect mainRect)
        {
            Text.Font = GameFont.Small;
            float height = 6f + army.rankList.Count * 30f;
            Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
            Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
            float num = 6f;
            float num2 = this.scrollPosition.y - 30f;
            float num3 = this.scrollPosition.y + mainRect.height;
            int i = 0;

            foreach (RankDef rank in reinforcements.soldierList.Keys)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect = new Rect(0f, num, viewRect.width, 30f);
                    DrawTradeableRow(rect, i, rank, reinforcements.soldierList[rank]);
                }

                num += 30f;
                i++;
            }

            Widgets.EndScrollView();
        }

        public void DrawTradeableRow(Rect rect, int index, RankDef rank, RankCount rankCount)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Text.Font = GameFont.Small;
            GUI.BeginGroup(rect);

            float num = rect.width;

            Rect rect2 = new Rect(0f, 0f, 150f, rect.height);
            if (Mouse.IsOver(rect2))
            {
                Widgets.DrawHighlight(rect2);
            }

            Rect rect3 = new Rect(num - 75f, 0f, 75f, rect.height);
            rect3.xMin += 5f;
            rect3.xMax -= 5f;
            Widgets.Label(rect2, rank.label);

            Rect rect4 = new Rect(150f, 0f, 75f, rect.height);
            Widgets.Label(rect4, GenText.ToStringMoney(rank.cost));

            num -= 175f;
            Rect rect5 = new Rect(num - 240f, 0f, 240f, rect.height);

            Rect rect6 = rect5.ContractedBy(2f);
            rect3.xMax -= 15f;
            rect3.xMin += 16f;

            Widgets.TextFieldNumeric<int>(rect3, ref rankCount.count, ref rankCount.editBuffer, 0.0f, 99.0f);

            num -= 240f;
            num -= 175f;
            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }
    }
}
