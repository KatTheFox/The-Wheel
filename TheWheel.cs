using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretHistories.UI;
using HarmonyLib;
using SecretHistories.Constants;
using SecretHistories.Infrastructure;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TheWheel: MonoBehaviour
{
    public static float speedStep = 0.15f;
    public static string KB1Str= "P";
    public static string KB10Str= "Z";
    public static string KBNextVerb="Slash";
    public static string KBNextCard = "H";
    public static SpeedSettingTracker tracker;
    public static KB1SettingTracker tracker1;
    public static KB10SettingTracker tracker10;
    public static KBNextVerbSettingTracker trackerNextVerb;
    public static KBNextCardSettingTracker trackerNextCard;
    
    public void Start() => SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(AwakePrefix);
    
    public void OnDestroy() => SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

    public void Update()
    {
        
        if (((ButtonControl)Keyboard.current[(Key) Enum.Parse(typeof (Key), KB10Str)]).wasPressedThisFrame && !Watchman.Get<LocalNexus>().PlayerInputDisabled())
        {
            
            
            for (int index = 0; index < 10; ++index)
                Watchman.Get<Heart>().Beat(1f, 0.0f);
            
        }
        if (((ButtonControl)Keyboard.current[(Key) Enum.Parse(typeof (Key), KBNextVerb)]).wasPressedThisFrame && !Watchman.Get<LocalNexus>().PlayerInputDisabled())
        {
                
                
                float timeToFF=GetNextVerbTime();
                NoonUtility.LogWarning("TheWheel: Fast forwarding "+timeToFF+" seconds");
                if (timeToFF >= 0.1f)
                {
                    Watchman.Get<Heart>().Beat(timeToFF - 0.1f, 0.0f);
                    Watchman.Get<Heart>().Beat(0.2f,0.0f);
                    
                }

        }
        if (((ButtonControl)Keyboard.current[(Key) Enum.Parse(typeof (Key), KB1Str)]).wasPressedThisFrame && !Watchman.Get<LocalNexus>().PlayerInputDisabled())
        {
            
            
                Watchman.Get<Heart>().Beat(1f, 0.0f);
            
        }
        if (((ButtonControl)Keyboard.current[(Key) Enum.Parse(typeof (Key), KBNextCard)]).wasPressedThisFrame && !Watchman.Get<LocalNexus>().PlayerInputDisabled())
        {
            NoonUtility.Log("TheWheel: Fast forwarding to next card");
            float nextCardTime=GetNextCardTime();
            
                Watchman.Get<Heart>().Beat(nextCardTime - 0.1f, 0.0f);
                Watchman.Get<Heart>().Beat(0.2f,0.0f);
               
        }
    }

    public static float GetNextCardTime()
    {
        var elementStacks=Watchman.Get<HornedAxe>(). GetExteriorSpheres().Where((x => x.TokenHeartbeatIntervalMultiplier>0.0f)).SelectMany(x => x.GetTokens()).Select(x=>x.Payload).OfType<ElementStack>();
        var lowest = float.PositiveInfinity;
        foreach (var stack in elementStacks)
        {
            if(stack.Decays && stack.LifetimeRemaining<lowest && stack.LifetimeRemaining>=0.2f)
                lowest = stack.LifetimeRemaining;
        }
        NoonUtility.Log("TheWheel: Next card time is "+lowest+" seconds");
        lowest=Math.Min(lowest,GetNextVerbTime());
        
        return float.IsPositiveInfinity(lowest)?0.0f:lowest;
    }
    
    public static float GetNextVerbTime()
    {
        List<Situation> verbList=Watchman.Get<HornedAxe>().GetRegisteredSituations();
        if (verbList.Count == 0)
            return 0.0f;
        float lowest=float.PositiveInfinity;
        foreach (Situation verb in verbList)
        {
            if(verb.TimeRemaining<lowest && verb.TimeRemaining>=0.1f)
                lowest = verb.TimeRemaining;
            
        };
        if(float.IsPositiveInfinity(lowest))
            return 0.0f;
        NoonUtility.Log("TheWheel: Next verb time is "+lowest+" seconds");
        return lowest;
    }
    public static void Initialise()
    {
        NoonUtility.Log("TheWheel: Initialising");
        
            Harmony harmony = new Harmony("katthefoxthewheel");
            new GameObject().AddComponent<TheWheel>();
            try
            {
                harmony.Patch(
                    original: GetMethodInvariant(typeof(Heart), "GetTimerMultiplierForSpeed"),
                    prefix: new HarmonyMethod(GetMethodInvariant(typeof(TheWheel),
                        (nameof(TheWheel.GetTimerMultiplierForSpeedPrefix)))));
            }catch(Exception e)
            {
                NoonUtility.Log(e.ToString());
                NoonUtility.LogException(e);
            }
            
        
    }
    static MethodInfo GetMethodInvariant(Type definingClass, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            NoonUtility.LogWarning($"Trying to find whitespace method for class {definingClass.Name} (don't!)");

        try
        {
            MethodInfo method = definingClass.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

            if (method == null)
                throw new Exception("Method not found");

            return method;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to find method '{name}'  in '{definingClass.Name}', reason: {ex.FormatException()}");
        }
    }
    void AwakePrefix(Scene scene, LoadSceneMode mode)
    {
        NoonUtility.LogWarning("wkeprefix loded");
        if (scene.name == "S3Menu")
        {
            try
            {
                Setting speedMultSetting = Watchman.Get<Compendium>().GetEntityById<Setting>("SpeedMultiplier");
                if (speedMultSetting == null)
                {
                    NoonUtility.LogWarning("Speed Multiplier Setting Missing");
                }
                else
                {
                    speedMultSetting.AddSubscriber((ISettingSubscriber)(TheWheel.tracker = new SpeedSettingTracker()))
                        ;
                    TheWheel.tracker.WhenSettingUpdated(speedMultSetting.CurrentValue);
                }

                Setting kb1sec = Watchman.Get<Compendium>().GetEntityById<Setting>("ff1sec");
                if (kb1sec == null)
                {
                    NoonUtility.LogWarning("one second keybind missing");
                }
                else
                {
                    kb1sec.AddSubscriber((ISettingSubscriber)(TheWheel.tracker1 = new KB1SettingTracker()))
                        ;
                    TheWheel.tracker1.WhenSettingUpdated(kb1sec.CurrentValue);
                }

                Setting kb10sec = Watchman.Get<Compendium>().GetEntityById<Setting>("ff10sec");
                if (kb10sec == null)
                {
                    NoonUtility.LogWarning("ten second keybind missing");
                }
                else
                {
                    kb10sec.AddSubscriber((ISettingSubscriber)(TheWheel.tracker10 = new KB10SettingTracker()))
                        ;
                    TheWheel.tracker10.WhenSettingUpdated(kb10sec.CurrentValue);
                }

                Setting kbNextVerb = Watchman.Get<Compendium>().GetEntityById<Setting>("ffNextVerb");
                if (kbNextVerb == null)
                {
                    NoonUtility.LogWarning("next verb keybind missing");
                }
                else
                {
                    kbNextVerb.AddSubscriber(trackerNextVerb = new KBNextVerbSettingTracker())
                        ;
                    trackerNextVerb.WhenSettingUpdated(kbNextVerb.CurrentValue);
                }
                Setting kbNextCard = Watchman.Get<Compendium>().GetEntityById<Setting>("ffNextCard");
                if (kbNextCard == null)
                {
                    NoonUtility.LogWarning("next card keybind missing");
                }
                else
                {
                    kbNextCard.AddSubscriber(trackerNextCard = new KBNextCardSettingTracker())
                        ;
                    trackerNextCard.WhenSettingUpdated(kbNextCard.CurrentValue);
                }
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }
        }
    }

    private static bool GetTimerMultiplierForSpeedPrefix(Heart __instance,GameSpeed speed,
        ref float ___veryFastMultiplier, ref float ___veryVeryfastMultiplier, ref GameSpeedState ___gameSpeedState, ref float __result)
    {
        switch (speed)
        {
            case GameSpeed.Paused:
                __result= 0.0f;
                break;
            case GameSpeed.Normal:
                __result=1f;
                break;
            case GameSpeed.Fast:
                __result=speedStep;
                break;
            case GameSpeed.VeryFast:
                __result=___veryFastMultiplier;
                break;
            case GameSpeed.VeryVeryFast:
                __result=___veryVeryfastMultiplier;
                break;
            default:
                NoonUtility.Log("Unknown game speed state: " + ___gameSpeedState.GetEffectiveGameSpeed().ToString());
                __result=0.0f;
                break;
        }
        return false;
    }
}