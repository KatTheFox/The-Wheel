using System;
using System.Threading.Tasks;
using SecretHistories.UI;
using UnityEngine.SceneManagement;


public class Settler
{
    /// <summary>
    /// Waits for the tabletop scene to be loaded.
    /// </summary>
    /// <returns>A task that resolves when the tabletop scene has loaded.</returns>
    public static async Task AwaitGameReady()
    {
        while (await IsGameStarted() == false)
        {
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Waits for the game to settle.
    /// </summary>
    /// <returns>A task that resolves when the game has no ongoing tasks that are incomplete.</returns>
    /// <remarks>
    /// Settling is defined as having no ongoing token itineraries.
    /// </remarks>
    public static async Task AwaitSettled()
    {
        while (await IsSettled() == false)
        {
            await Task.Delay(100);
        }

        // Would be great to do this, but it looks like this wont stop the animations, and the animation will also call Arrive on completed.
        // var itineraries = Watchman.Get<Xamanek>().CurrentItineraries.Values.ToArray();
        // foreach (var itinerary in itineraries)
        // {
        //     itinerary.Arrive(itinerary.GetToken(), new Context(Context.ActionSource.Debug));
        // }
    }

    private static Task<bool> IsSettled()
    {
        return DispatchRead(() =>
        {
            var xamanek = Watchman.Get<Xamanek>();
            if (xamanek == null)
            {
                // Game is not active, that's settled enough.
                return true;
            }

            return xamanek.CurrentItineraries.Count == 0;
        });
    }

    public static Task<T> DispatchRead<T>(Func<T> function)
    {
        // This might be dangerous, but nothing bad seems to have happened yet.
        return Task.FromResult(function());
    }

    private static Task<bool> IsGameStarted()
    {
        return DispatchRead(() => { return SceneManager.GetSceneByName("S4Tabletop").isLoaded; });
    }
}