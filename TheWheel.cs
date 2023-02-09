
using SecretHistories;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using System;
using System.Reflection;
using SecretHistories.UI;
using HarmonyLib;
using SecretHistories.Constants;
using SecretHistories.Infrastructure.Modding;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TheWheel:MonoBehaviour
{
    public static float speedStep = 0.15f;
    public static SettingTracker tracker;
    
    public void Start() => SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

    public void OnDestroy() => SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(AwakePrefix);

    public static void Initialise()
    {
        try
        {
            Harmony harmony = new Harmony("katthefox.thewheel");
            new GameObject().AddComponent<TheWheel>();
            
            harmony.Patch(
                original: GetMethodInvariant(typeof(Heart),"ProcessBeatCounter"),
                prefix: new HarmonyMethod(GetMethodInvariant(typeof(TheWheel),(nameof(TheWheel.ProcessBeatCounterPrefix)))));
        }
        catch(Exception e)
        {
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
        if (scene.name == "S3Menu")
        {
            Setting entityById = Watchman.Get<Compendium>().GetEntityById<Setting>("SpeedMultiplier");
            if (entityById == null)
            {
                NoonUtility.LogWarning("Speed Multiplier Setting Missing");
            }
            else
            {
                entityById.AddSubscriber((ISettingSubscriber)(TheWheel.tracker = new SettingTracker()))
                    ;
                TheWheel.tracker.WhenSettingUpdated(entityById.CurrentValue);
            }
        }
    }

    static private bool ProcessBeatCounterPrefix(Heart __instance, ref float ___timerBetweenBeats,
        ref GameSpeedState ___gameSpeedState)
    {
        ___timerBetweenBeats += Time.deltaTime;
        if ((double)___timerBetweenBeats <= 0.05000000074505806)
            return false;
        ___timerBetweenBeats -= 0.05f;
        if (___gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Fast)
        {
            __instance.Beat(speedStep, 0.05f);
        }
        else if (___gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Normal)
            __instance.Beat(0.05f, 0.05f);
        else if (___gameSpeedState.GetEffectiveGameSpeed() == GameSpeed.Paused)
            __instance.Beat(0.0f, 0.05f);
        else
            NoonUtility.Log("Unknown game speed state: " + ___gameSpeedState.GetEffectiveGameSpeed().ToString());

        return false;
    }
}