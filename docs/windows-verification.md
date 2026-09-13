# Windows build and startup audit

Baseline: `d96d5d8` on `master`. Work branch: `codex/windows-clean-clone`.

## Inventory before changes

| Area | Findings |
| --- | --- |
| Solution | `Sci-Opolis.sln`, one classic C# project in `PASS3 - Grade 12/`; 18 C# files including assembly metadata. |
| Framework | WinExe, AnyCPU, .NET Framework **4.6.1**, Debug and Release; Visual Studio solution format 12. |
| MonoGame | `MonoGame.Framework.DesktopGL` **3.7.0.1708**, pinned in `packages.config`; OpenGL/SDL2/OpenAL backend. |
| Managed libraries | Tracked `Libraries/Helper.dll` and `Libraries/Animation2D.dll`, both assembly version 1.0.0.0. Helper references MonoGame 3.7.0.1708; Animation2D references 3.6.0.1625. Their source is absent. Existing binaries are retained. |
| Native runtime | MonoGame's NuGet target supplies SDL2 and OpenAL for both x86 and x64, plus its framework configuration. These were absent until restore/build. |
| Content | `Content/Content.mgcb`, DesktopGL/Reach, uncompressed XNB; PNG textures, five spritefonts, MP3 music and WAV effects. Source assets are tracked; compiled content is ignored. |
| Pipeline wiring | The MGCB file was a `None` item, so building C# never compiled assets. Its output was hardcoded to Debug, while the project glob expected a different directory. |
| Missing content | `Can You Just Die Already.wav` and `Im Surprised You Made It This Far.wav` were listed in MGCB but absent. Neither is loaded by game code. |
| Fonts | OriginTech TTF/OTF and Nulshock OTF are tracked. The spritefonts originally named fonts without file extensions. |
| Missing runtime data | `Stage.txt`, `Statistics.txt`, `Upgrades.txt` were absent from the checkout and no matching paths were found in available Git history. Reads returned null; startup dereferenced those results. Upgrades are read from a static initializer. |
| Media | Existing `Content/Sci-Opolis.gif` shows historical gameplay and the stage. It is retained as historical media. |

## Failure reproduction and changes

Before editing, this command failed:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe' .\Sci-Opolis.sln /t:Build /p:Configuration=Debug /v:minimal /nologo
```

The error was `MSB3644`: reference assemblies for `.NETFramework,Version=v4.6.1` were not found. This host had MSBuild 17.14 and .NET Framework 4.8, but no 4.6.1 targeting pack or modern .NET SDK.

Providing the existing MonoGame package and Microsoft's 4.6.1 reference assemblies through a command-line framework path allowed the **unchanged C# source** to compile. The output still contained no compiled game content. This distinguished a C# build from a runnable game.

Changes were checked incrementally:

1. Added pinned reference-assembly and MGCB packages and connected the content project to MSBuild. The first build exposed the legacy task's unquoted executable path; setting `MgcbPath` to the quoted full compiler path fixed paths containing spaces.
2. The content build reproduced the two missing WAV errors. Removed those unused manifest entries and normalized the standalone MGCB output directory. The next build reached the native image importer failure.
3. `dumpbin /dependents` showed `FreeImage.dll` requires `VCOMP120.DLL`, which was absent. Installed Microsoft's signed Visual C++ 2013 x64 Redistributable **12.0.40664**; installer exited 0. Image compilation then succeeded. The newer Visual C++ runtime already on this host did not supply this older DLL.
4. Added embedded default data and executable-relative file paths. The combined game/content Debug build passed.
5. Added `build.ps1` and `NuGet.Config`; the script's build and output checks passed. Set explicit bundled font filenames; rebuilt successfully without installing fonts.

The framework, helper DLLs, weapons, enemy behavior, physics, input, progression and save serialization remain unchanged. `Stage.txt` is a reconstruction from the GIF, including its visible floor/platform positions; it is not a recovered original map.

## Default data and preservation

`RuntimeData` initializes missing files from embedded resources before `FileManager` reads upgrades. Relative paths resolve against the executable directory, regardless of the shell's working directory. `FileMode.CreateNew` prevents replacing existing files; existing malformed files are also left untouched.

- Stage: three header lines, then **22 rows × 38 columns**; tile IDs 0–5, with a continuous ground layer.
- Statistics: six `label,value` rows: best survival time, coins, coins spent, login count, waves survived and enemies killed. Defaults are zero.
- Upgrades: two header lines, then five locked flags (`1,1,1,1,1`).

The templates are embedded rather than copied over output files during builds. Debug and Release have separate save files. Back up existing saves before moving/resetting them. The original parser does not repair corrupt data, and its save writes are not atomic.

## Verification

`build.ps1` checks the executable/configuration, three managed DLLs, x86/x64 SDL2 and OpenAL, every manifest XNB, and all three OGG music sidecars. There are **61 XNB assets** in the final manifest.

`tools/Verify-RuntimeData.ps1` exercises the real readers/writers with isolated temporary files, checks dimensions, initial values, round trips, missing-only restoration, preservation of existing file bytes, and working-directory independence. It does not launch a game window or alter player saves.

The user manually confirmed that the Debug executable launches, the menu and audio work, Single Player starts, and movement/jumping/shooting on platforms work. This is the graphical startup verification; it is not an automated screenshot test.

Final checks on 2026-09-12:

| Check | Result |
| --- | --- |
| Debug build and output check | Passed. |
| Release build and output check | Passed. |
| Isolated data tests, Debug and Release | Passed. |
| Fresh local clone of runtime commit `4f08aa5` | Confirmed no `packages/` or game `bin/` directory existed before building. The documented Windows PowerShell command restored all four pinned packages, built all 61 XNB assets and three OGG sidecars, and passed output checks. |
| Data tests in fresh clone | Passed; clone working tree remained clean after building/testing. |
| Graphical game check | User-confirmed Debug menu/audio and single-player movement, jumping, shooting and platforms. |

The fresh clone used this host's installed prerequisites; it was not a separate clean Windows virtual machine. Network restore required running outside the agent's network sandbox. No dependency or content output was copied into the clone from the original working directory.

The existing `CS0649` warning for `Bullet.gameTime` remains. A first MGCB content build also reports an assembly type-discovery warning (`Unable to load one or more of the requested types`); all assets used by this project compile successfully. Neither warning was suppressed.

## Survival-time follow-up

The follow-up regression runner reproduced the original record bug before editing `Game1`: 1 ms became `0.1`, 65.012 seconds became `5.12`, and a 12.345-second record could be replaced by a later 11.345-second run in the same session. Parsing also failed under `fr-CA`.

The fix compares raw total milliseconds converted to seconds against the current saved record, writes invariant decimal seconds, and labels the Stats value with `s`. It also supplies fractional `ElapsedGameTime.TotalMilliseconds` to the timer. The helper DLL and other gameplay timers remain unchanged.

`tools/Verify-SurvivalTime.ps1` compiles a small runner in a disposable directory and invokes the real game record method without creating a graphical game. It checks subsecond, exact-second, minute/hour, sequential-run and locale cases, then starts a second process to verify persistence. It does not use or change player saves. Previously misformatted historical time cannot be reconstructed from the old save value.

Debug and Release builds, all survival-time regression checks, and the existing runtime-data checks passed after the fix. This includes fractional frame durations, shorter runs under both French Canadian and German number settings, and persistence through a new process. A read-only review found no blocking issues.

## Graphical checks still pending

- Two controllers in local co-op, including independent movement and firing.
- Complete a run through game over, relaunch, and confirm earned coins/statistics persist.
- Buy upgrades, relaunch, and verify the unlocked behavior and sawed-off selection.
- Visually recheck the Stats menu's survival-time value and seconds label after completing a run.

These checks require launching and interacting with the graphical game. They are not prerequisites for compiling, and have not been represented as completed.

## Prerequisite sources

- [Microsoft reference assemblies package](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net461/1.0.3)
- [MonoGame framework 3.7.0.1708](https://www.nuget.org/packages/MonoGame.Framework.DesktopGL/3.7.0.1708)
- [MonoGame content build integration 3.7.0.9](https://www.nuget.org/packages/MonoGame.Content.Builder/3.7.0.9)
- [Windows content compiler 3.7.0.8](https://www.nuget.org/packages/MonoGame.Content.Builder.Windows/3.7.0.8)
- [Microsoft Visual C++ Redistributable downloads, including the legacy 2013 x64 package](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist)
