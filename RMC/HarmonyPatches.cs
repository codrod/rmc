﻿using Verse;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using System.Reflection;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/*
 * flakjacket is incorrectly tagged as not military apparel
 */


namespace RMC
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            Log.Message("RMC: Started");
            Backstory backstory = null;

            var harmony = HarmonyInstance.Create("com.github.RimWorld.RMC");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            foreach(BackstoryDef backstoryDef in DefDatabase<BackstoryDef>.AllDefs)
                if(!BackstoryDatabase.TryGetWithIdentifier(backstoryDef.identifier, out backstory))
                    BackstoryDatabase.AddBackstory(backstoryDef.NewBackstory());

            Log.Message("RMC: Loaded");
        }
    }
}

