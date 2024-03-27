using SecretHistories.Entities;
using SecretHistories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecretHistories.UI;
using HarmonyLib;
using SecretHistories.Constants;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// ReSharper disable InconsistentNaming

public class TheWheel : MonoBehaviour
{
    private static ValueTracker<float> trackerSpeedMultiplier;
    private static ValueTracker<int> skipSpeed;
    public static KeybindTracker trackerBeat1,trackerBeat10,trackerNextVerb,trackerNextCard,trackerSpeedUp,trackerSpeedDown;
    private int _skipTicks;

    // Register the SceneLoaded callback when the mod is loaded, and remove it on destroy to prevent unload crashes.
    public void Start() => SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

    public void OnDestroy() => SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

    public async void Update()
    {
        await Settler.AwaitSettled();
        if (_skipTicks <= 0)
            return;
        NoonUtility.LogWarning($"{_skipTicks}");
        while (_skipTicks > 0)
        {
            var num = this.TrySkip(400) || this.TrySkip(100) || this.TrySkip(25) ? 1 : (this.TrySkip(1) ? 1 : 0);
            if (skipSpeed.current > 0.0)
                break;
            await Settler.AwaitSettled();
        }
    }

    // Gets the time until the next card decay or recipe completion, whichever is sooner. Returns 0.01 second 'ticks'.
    private static int GetNextCardTime()
    {
        var elementStacks = Watchman.Get<HornedAxe>().GetExteriorSpheres()
            .Where((x => x.TokenHeartbeatIntervalMultiplier > 0.0f)).SelectMany(x => x.GetTokens())
            .Select(x => x.Payload).OfType<ElementStack>();
        var lowest = float.PositiveInfinity;
        foreach (var stack in elementStacks)
        {
            if (stack.Decays && stack.LifetimeRemaining < lowest && stack.LifetimeRemaining > 0.0f)
                lowest = stack.LifetimeRemaining;
        }

        var verbTime = GetNextVerbTime();
        NoonUtility.Log("TheWheel: Next card time is " + lowest + " seconds");
        if (float.IsPositiveInfinity(lowest) || (SecondsToTicks(lowest) > verbTime && verbTime>0))
        {
            return verbTime;
        } 
        return SecondsToTicks(lowest);
        
    }

    // Gets the time until the next recipe completion. Returns 0.01 second 'ticks'.
    private static int GetNextVerbTime()
    {
        var verbList = Watchman.Get<HornedAxe>().GetRegisteredSituations();
        if (verbList.Count == 0)
            return 0;
        var lowest = float.PositiveInfinity;
        foreach (var verb in verbList.Where(verb => verb.TimeRemaining < lowest && verb.TimeRemaining > 0.0f))
        {
            lowest = verb.TimeRemaining;
        }
        
        return float.IsPositiveInfinity(lowest) ? 0 : SecondsToTicks(lowest);
    }
    public bool TrySkip(int ticks)
    {
        if (ticks == 1)
        {
            Watchman.Get<Heart>().Beat(1f / 64f, 0.0f);
        }
        else
        {
            if (_skipTicks - skipSpeed.current < ticks)
                return false;
            NoonUtility.Log($"The Wheel: Skip {(object)(0.01 * (double)ticks)}");
            Watchman.Get<Heart>().Beat(0.01f * (float) ticks, 0.5f);
        }
        _skipTicks -= ticks;
        return true;
    }
    // Called when the mod loads. Prints out a debug message and patches the required functions.
    public static void Initialise()
    {
        NoonUtility.Log("TheWheel: Initialising");

        var harmony = new Harmony("katthefoxthewheel");
        new GameObject().AddComponent<TheWheel>();
        try
        {
            harmony.Patch(
                original: GetMethodInvariant(typeof(Heart), "GetTimerMultiplierForSpeed"),
                prefix: new HarmonyMethod(GetMethodInvariant(typeof(TheWheel),
                    (nameof(GetTimerMultiplierForSpeedPrefix)))));
        }
        catch (Exception e)
        {
            NoonUtility.Log(e.ToString());
            NoonUtility.LogException(e);
        }
    }

    // Blatant copy of a function from The Roost Machine. Gets the MethodInfo of a function without needing to specify
    // its BindingFlags. For general cases; for overloads and generics etc a more specialized approach is needed.
    private static MethodInfo GetMethodInvariant(Type definingClass, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            NoonUtility.LogWarning($"Trying to find whitespace method for class {definingClass.Name} (don't!)");

        try
        {
            var method = definingClass.GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

            if (method == null)
                throw new Exception("Method not found");

            return method;
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Failed to find method '{name}'  in '{definingClass.Name}', reason: {ex.FormatException()}");
        }
    }

    // When the menu scene loads, register the ISettingSubscribers and InputActions.
    private void AwakePrefix(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "S3Menu") return;
        try
        {
            NoonUtility.LogWarning("The Wheel: Menu Loaded");
            trackerSpeedMultiplier = new ValueTracker<float>("SpeedMultiplier", new float[]
            {
                0.2f,
                0.5f,
                2f,
                3f,
                5f,
                8f,
                13f,
                21f,
                34f
            });
            skipSpeed = new ValueTracker<int>("SkipSpeed", new int[]
            {
                0,
                10,
                70
            });
            trackerBeat1 = new KeybindTracker("ff1sec",Beat1);
            trackerBeat10 = new KeybindTracker("ff10sec",Beat10);
            trackerNextVerb = new KeybindTracker("ffNextVerb",NextVerb);
            trackerNextCard = new KeybindTracker("ffNextCard",NextCard);
            trackerSpeedUp = new KeybindTracker("speedUp",SpeedUp);
            trackerSpeedDown = new KeybindTracker("speedDown",SpeedDown);
        }
        catch (Exception e)
        {
            NoonUtility.LogException(e);
        }
    }
    
    // Beat 1 second
    private void Beat1(InputAction.CallbackContext _callbackContext)
    {
        NoonUtility.LogWarning("beating 1");
        _skipTicks = 100;
    }

    // Convert seconds to 0.01 second ticks. Thanks to RobynTheDevil on Steam for the logic here.
    private static int SecondsToTicks(float seconds)
    {
        NoonUtility.LogWarning($"seconds={seconds} ticks={(int)(seconds * 100.0 + 0.01)}");
        return (int)(seconds * 100.0 + 0.01);
    }

    // Beat 10 seconds
    private void Beat10(InputAction.CallbackContext _callbackContext)
    {
        _skipTicks = 1000;
    }
    
    // Fast forward until the next card decay or recipe completion
    private void NextCard(InputAction.CallbackContext _callbackContext)
    {
        NoonUtility.Log("TheWheel: Fast forwarding to next card");
        var nextCardTime = GetNextCardTime();
        if (nextCardTime== 0) return;
        _skipTicks = nextCardTime;
    }

    // Fast forward until the next recipe completion
    private void NextVerb(InputAction.CallbackContext _callbackContext)
    {
        var nextVerbTime = GetNextVerbTime();
        NoonUtility.LogWarning("TheWheel: Fast forwarding " + nextVerbTime + " seconds");
        if (nextVerbTime==0) return;
        _skipTicks=nextVerbTime;
    }

    // Increase the speed of the Fast Forward setting. Known issues: Does not update the slider in the options menu
    private static void SpeedUp(InputAction.CallbackContext _callbackContext)
    {
        if (Array.IndexOf(trackerSpeedMultiplier.values,trackerSpeedMultiplier.current)!=8)
        {
            trackerSpeedMultiplier.SetCurrent(Array.IndexOf(trackerSpeedMultiplier.values,trackerSpeedMultiplier.current)+1);
        }
    }

    // Decrease the speed of the Fast Forward setting. Known issues: Does not update the slider in the options menu
    private static void SpeedDown(InputAction.CallbackContext _callbackContext)
    {
        if (Array.IndexOf(trackerSpeedMultiplier.values,trackerSpeedMultiplier.current)!=0)
        {
            trackerSpeedMultiplier.SetCurrent(Array.IndexOf(trackerSpeedMultiplier.values,trackerSpeedMultiplier.current)-1);
        }
    }

    // The override function for the Heard.GetTimerMultiplierForSpeed function. Replaces the Fast Forward speed with
    // our custom multiplier.
    private static bool GetTimerMultiplierForSpeedPrefix(Heart __instance, GameSpeed speed,
        ref float ___veryFastMultiplier, ref float ___veryVeryfastMultiplier, ref GameSpeedState ___gameSpeedState,
        ref float __result)
    {
        switch (speed)
        {
            case GameSpeed.Paused:
                __result = 0.0f;
                break;
            case GameSpeed.Normal:
                __result = 1f;
                break;
            case GameSpeed.Fast:
                __result = trackerSpeedMultiplier.current;
                break;
            case GameSpeed.VeryFast:
                __result = ___veryFastMultiplier;
                break;
            case GameSpeed.VeryVeryFast:
                __result = ___veryVeryfastMultiplier;
                break;
            default:
                NoonUtility.Log("Unknown game speed state: " + ___gameSpeedState.GetEffectiveGameSpeed().ToString());
                __result = 0.0f;
                break;
        }

        return false;
    }
}