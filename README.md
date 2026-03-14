BattleBuck Match Simulation – Technical Documentation

Overview
This project simulates a 10-player deathmatch and was built with a strong focus on clean architecture,
performance, and scalability.
The core goal was to keep game logic independent from Unity components, making the system easier
to maintain and scale later for multiplayer or larger matches.

1. Architecture and Design Patterns
The project mainly follows a Model–View–Controller (MVC) structure combined with an event-driven
approach.
This separation ensures that gameplay logic, visual representation, and user interface remain
independent from each other.

Event-Driven System (Observer Pattern) :-

Implementation
A static class called `GameEvents.cs` works as a central event dispatcher.
Purpose :-
Gameplay systems broadcast events instead of directly interacting with other components.
For example, when a player kills another player, the system triggers a kill event rather than directly
updating UI or effects.
This keeps systems loosely coupled and avoids situations where gameplay scripts depend on multiple
UI or visual scripts.
Classes such as:
● `PlayerData`
● `PlayerManager`
● `MatchTimer`
● `ScoreSystem`
are implemented as standard C# classes, not MonoBehaviours.
Purpose :-
MonoBehaviours include additional Unity engine overhead.
Keeping core logic in pure C# classes allows the simulation to run more efficiently and remain
independent from Unity-specific systems.
The simulation runs using a controlled update loop via a single `Tick(deltaTime)` call.

a. Central Match Controller
Implementation :-
`MatchController.cs` acts as the main MonoBehaviour responsible for driving the simulation.
Purpose :-
Instead of having multiple `Update()` calls across different scripts, the controller runs a single update
loop and manually updates systems using `Tick()`.
Benefits include:
● Reduced CPU overhead
● Clear execution order
● Easier debugging
For example, the match timer can update before player movement or combat calculations.

b. Object Pooling
Implementation :-
Object pools are used for:
● `ProjectilePool.cs`
● Leaderboard UI entries
Purpose :-
Creating and destroying objects repeatedly can cause performance issues and garbage collection
spikes, especially on mobile devices.
Object pooling solves this by creating objects once and reusing them by enabling or disabling them as
needed.
This ensures smoother performance during combat situations where many projectiles may appear.

c. ScriptableObject Configuration
Implementation :-
Gameplay settings are stored inside `MatchConfig.cs` using a ScriptableObject.
Purpose :-
This allows designers to tweak gameplay values such as:
● Player health
● Movement speed
● Match duration
without modifying code.
Using ScriptableObjects also keeps configuration data organized and easily editable in the Unity
Inspector.

3. Scaling the Project to Multiplayer
Because the gameplay logic is separated from Unity GameObjects, adapting this system for multiplayer
networking becomes much easier.

a. Server Authority :-
The core simulation systems (`PlayerManager`, `MatchTimer`, etc.) can run on a dedicated server.
The server runs the main `Tick()` loop and handles:
● Player movement
● Combat detection
● Score updates
This ensures the server remains the authoritative source of truth.

b. Client State Updates :-
Instead of directly moving GameObjects, the server sends logical player states to clients at a fixed rate
(for example 10–30 updates per second).
Each update contains information such as player positions and relevant gameplay events.
Interpolation :-
On the client side, the visual player objects smoothly interpolate toward the latest position received
from the server.
This helps hide network latency and keeps movement looking smooth.

c. Event Synchronization :-
Gameplay events like kills are triggered by the server and sent to clients.
Clients then raise the same event locally through the event system.
UI elements such as the kill feed update automatically when these events occur.

5. Scaling to Larger Mobile Matches (50+ Players)
For larger player counts, additional optimizations would be required.

a. Spatial Partitioning :-
Currently, enemy detection checks the distance between every player pair.
This results in O(N²) comparisons.
To optimize this, a grid-based spatial partitioning system or quadtree structure could be introduced.
Players would then only check nearby grid cells, greatly reducing the number of distance checks.

b. Unity Job System (Burst / DOTS)
Movement and projectile calculations can be processed in parallel using Unity’s Job System.
By converting these loops into `IJobParallelFor` jobs and compiling them with Burst, calculations can
run across multiple CPU cores, improving performance and reducing battery usage on mobile devices.

c. UI Virtualization
Displaying large leaderboards requires UI optimization.
Instead of creating UI entries for all players, a virtualized scrolling list can be used.
Only the visible rows are instantiated, and elements are recycled as the user scrolls.
This keeps UI performance stable even with large player counts.

d. Asset Streaming
In a full production environment, player models and other assets would be loaded using Unity
Addressables.
This allows assets to be loaded asynchronously rather than all at once, preventing long loading times or
memory spikes.
Configuration for these assets could be stored inside `MatchConfig` using `AssetReference` fields.
