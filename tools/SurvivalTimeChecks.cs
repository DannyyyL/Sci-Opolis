using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using SciOpolis;

// Runs in a disposable directory, without constructing a graphical Game1.
internal static class SurvivalTimeChecks
{
    private static int failures;
    private static readonly MethodInfo SaveBest = typeof(Game1).GetMethod("BestSurvivalTime");

    private static string[,] ReadStats()
    {
        Type type = typeof(Game1).Assembly.GetType("SciOpolis.FileManager", true);
        object manager = Activator.CreateInstance(type, true);
        return (string[,])type.GetMethod("ReadStatsAndInventory").Invoke(manager, new object[] { "Statistics.txt", 6 });
    }

    private static void Record(int elapsedMilliseconds, double bestAtLaunch)
    {
        var timer = new Helper.Timer(Helper.Timer.INFINITE_TIMER, true);
        timer.Update(elapsedMilliseconds);
        RecordTimer(timer, bestAtLaunch);
    }

    private static void RecordTimer(Helper.Timer timer, double bestAtLaunch)
    {
        // The two-argument path also lets this regression reproduce the original bug.
        object[] arguments = SaveBest.GetParameters().Length == 2
            ? new object[] { timer, bestAtLaunch }
            : new object[] { timer };
        SaveBest.Invoke(null, arguments);
    }

    private static void AssertRecord(double expected)
    {
        string value = Game1.playerData[0, 1];
        double actual = double.Parse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        if (Math.Abs(actual - expected) > 0.0000001)
            throw new Exception("Expected " + expected.ToString(CultureInfo.InvariantCulture) + " seconds, got " + value);
        string[,] saved = ReadStats();
        if (saved[0, 1] != value || File.ReadAllLines("Statistics.txt")[0].Split(',').Length != 2)
            throw new Exception("The saved seconds must match the displayed record and remain valid CSV.");
    }

    private static void Scenario(string name, double initial, int[] runs, double expected)
    {
        try
        {
            Game1.playerData = new string[,]
            {
                { "Best Survival Time: ", initial.ToString(CultureInfo.InvariantCulture) },
                { "Coins: ", "17" }, { "Coins Spent: ", "0" },
                { "Times Logged In: ", "1" }, { "Waves Survived: ", "0" }, { "Enemies Killed: ", "0" }
            };
            Game1.SaveStatsAndInventory();
            foreach (int run in runs) Record(run, initial);
            AssertRecord(expected);
            if (ReadStats()[1, 1] != "17") throw new Exception("Other statistics changed.");
            Console.WriteLine("PASS: " + name);
        }
        catch (Exception error)
        {
            failures++;
            Console.WriteLine("FAIL: " + name + " - " + error.GetBaseException().Message);
        }
    }

    private static int Main(string[] args)
    {
        // ResolvePath uses the process base; all test files are owned by this runner.
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        if (args.Length == 1 && args[0] == "--reload")
        {
            Game1.playerData = ReadStats();
            Record(45000, 65.012);
            AssertRecord(65.012);
            Record(70001, 65.012);
            AssertRecord(70.001);
            Console.WriteLine("PASS: persisted record survives a new process and a shorter run, then improves.");
            return 0;
        }

        Scenario("one millisecond", 0, new[] { 1 }, 0.001);
        Scenario("ten milliseconds", 0, new[] { 10 }, 0.010);
        Scenario("exact second", 0, new[] { 1000 }, 1.000);
        Scenario("fractional seconds", 0, new[] { 1234 }, 1.234);
        Scenario("over one minute", 0, new[] { 65012 }, 65.012);
        Scenario("over one hour", 0, new[] { 3600001 }, 3600.001);
        Scenario("shorter second run cannot lower a new record", 10, new[] { 12345, 11345 }, 12.345);
        Scenario("equal or shorter runs preserve existing record", 12.345, new[] { 12345, 11000 }, 12.345);
        Scenario("later longer run improves record", 10, new[] { 12345, 11345, 14001 }, 14.001);
        try
        {
            Game1.playerData[0, 1] = "0";
            Game1.SaveStatsAndInventory();
            var frameTimer = new Helper.Timer(Helper.Timer.INFINITE_TIMER, true);
            for (int frame = 0; frame < 60; frame++) frameTimer.Update(16.6667);
            RecordTimer(frameTimer, 0);
            AssertRecord(1.000);
            Console.WriteLine("PASS: fractional frame durations retain total elapsed time.");
        }
        catch (Exception error)
        {
            failures++;
            Console.WriteLine("FAIL: fractional frame durations - " + error.GetBaseException().Message);
        }
        foreach (string culture in new[] { "fr-CA", "de-DE" })
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Scenario("culture-independent seconds in " + culture, 1.200, new[] { 1234 }, 1.234);
            Scenario("shorter run in " + culture, 1.200, new[] { 1100 }, 1.200);
        }
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        Scenario("record for restart verification", 0, new[] { 65012 }, 65.012);
        return failures == 0 ? 0 : 1;
    }
}
