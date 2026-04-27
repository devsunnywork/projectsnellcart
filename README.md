# Building Tracker - Project  Documentation

Welcome to the comprehensive handover documentation for **Building Tracker**. This application is a fully integrated 3D visual management engine built in Unity, designed to offer facility managers, technical staff, and administrative personnel an interactive, data-driven way to manage real estate assets, monitor equipment lifecycles, track maintenance tasks, and perform digital tours.

---

## 1. Project Overview

Building Tracker is essentially a **"Digital Twin"** interface for buildings. Instead of scrolling through 2D spreadsheets to find asset data, users can fly through a realistic 3D representation of their building, drill down into specific rooms, and interact with individual assets to view and edit live data panels. 

**Key Features:**
*   **3D Spatial Navigation:** Move seamlessly from an aerial city map down to specific floor plans and interior rooms.
*   **Interactive Assets:** Every tracked asset (furniture, electronics, appliances) has an interactive 3D marker.
*   **Comprehensive Data Models:** Fully integrated data layers encompassing Lifecycle, Warranty, Cost, Quality, and Task/Maintenance management.
*   **Dynamic UI Panels:** Data-rich dashboards, summary tables, and side panels for immediate insight.
*   **Search & Filtering:** Robust search mechanics to locate any task or asset by name or ID.

---

## 2. User Application Hierarchy & Flow

Understanding how to use the app is best explained through its navigation hierarchy. The system follows a top-down structural flow:

### Level 1: Global & Aerial View
*   **Starting Point:** The app boots into an aerial, map-like perspective over the target location (via `RealWorldMapEngine`).
*   **Actions:** Users can click on a specific 3D Building Marker to initiate the transition into the building.
*   **Global Dashboards:** From the top-level UI, users can open the "Global Dashboard", "Insights Panel", or global task search.

### Level 2: Building & Floor View (Interior Mode)
*   **Entering the Building:** Once a building is selected, the camera dives into the structure. Exterior walls are faded or hidden (`AerialUIManager.SetExteriorColliders`), revealing the interior layout.
*   **Navigation:** The user is now in "First Person / Drone" mode. They can fly freely using WASD keys to inspect floor layouts.

### Level 3: Room View
*   **Entering a Room:** Clicking on a room label or navigating through the doorway transitions the UI focus to that specific space. 
*   **UI Update:** The left panel updates to show room-specific tasks or information.

### Level 4: Asset Interaction
*   **Focusing:** Clicking on an asset's 3D indicator (like an arrow or dot) causes the camera to smoothly zoom in and focus on that specific object.
*   **Data Panels:** The `AssetDetailPanel` (Right Panel) opens, displaying multiple tabs: Identity, Lifecycle, Cost, Warranty, Quality, and Gallery.
*   **Actioning:** Users can edit asset data, mark tasks as complete, or view attached images.
*   **Exiting:** Pressing the `ESC` key or the top back arrow steps backward up the hierarchy (Asset -> Room -> Aerial).

---

## 3. Camera Controls

The camera system (`SmartCameraController.cs`) is the backbone of the 3D experience, designed to mimic a human "eye-level" tour combined with a drone's freedom.

| Action | Control Input | Description |
| :--- | :--- | :--- |
| **Move Forward/Backward** | `W` / `S` | Moves the camera relative to where it is currently facing. |
| **Strafe Left/Right** | `A` / `D` | Slides the camera sideways. |
| **Look Around** | `Right Mouse Button (Hold & Drag)` | Smooth mouselook to rotate the camera pitch and yaw. |
| **Focus Object** | `Left Click` on Asset | Triggers the `FocusOnAsset` method, automatically flying the camera to an optimal viewing angle in front of the item. |
| **Go Back / Zoom Out** | `ESC` Key or Back Arrow | Steps backward through the camera's history stack, returning to the previous zoom level or reverting to Aerial view. |
| **Zoom in/out** | `Mouse Scroll Wheel` | Adjusts FOV or distance incrementally. |

---

## 4. Searchable & Clickable Assets Reference

The building is populated with various interactive assets loaded from the JSON data store (`BLD_001.json`). Below is the complete manifest of the searchable assets and their unique system IDs. You can type these into the Left Panel Search to instantly locate them.

### Living Room / Main Hall
*   **LV_DINING_TBL_01** - Premium Dining table *(Furniture)*
*   **LV_CHAIR_SET_01** - Dining Chairs (6-Set) *(Furniture)*
*   **LV_SOFA_MAIN_01** - Primary L-Shape Sofa *(Furniture)*
*   **LV_TV_4K_SMART_01** - Sony Bravia 55' 4K TV *(Electronics)*
*   **LV_SPKR_BIG_01** - JBL Cinema Big Subwoofer *(Electronics)*
*   **LV_BOOK_SHELF_VOL_01** - Hardcover Book Set *(Decor)*
*   **LV_TABLE_LAMP_01** - Designer Table Lamp *(Electrical)*
*   **LV_CARPET_AREA_01** - Luxury Floor Carpet *(Decor)*
*   **LV_PAINTING_HERO_01** - Large Hero Painting *(Decor)*
*   **LV_SOFA_SIDE_01** - Sofa Side Table *(Furniture)*
*   **LV_FUSE_BOX_01** - Main Living Room Fuse Box *(Electrical)*

### Master Bedroom (BR1)
*   **BR1_BED_01** - King Size Bed *(Furniture)*
*   **BR1_AC_01** - Split AC - 1.5 Ton *(Electrical)*
*   **BR1_TV_01** - 4K Smart TV *(Electronics)*
*   **BR1_WARDROBE_01** - Integrated Working-Wardrobe Unit *(Furniture)*
*   **BR1_CURTAIN_01** - Premium Window Curtains *(Linen)*

### Guest Bedroom (BR2)
*   **BR2_BED_01** - Queen Size Bed *(Furniture)*
*   **BR2_AC_01** - Split AC - 1 Ton *(Electrical)*
*   **BR2_WARDROBE_01** - Standard Wardrobe *(Furniture)*

### Kitchen
*   **KT_MCW_01** - Convection Microwave *(Appliances)*
*   **KT_OVN_01** - Electric Oven *(Appliances)*
*   **KT_REF_01** - Double Door Refrigerator *(Appliances)*
*   **KT_WP_01** - RO Water Purifier *(Appliances)*
*   **KT_SINK_01** - Stainless Steel Sink *(Fixture)*
*   **KT_ISLAND_01** - Kitchen Island *(Furniture)*
*   **KT_STOVE_01** - Gas Stove *(Appliances)*

### Bathroom
*   **BT_SINK_01** - Bathroom Sink *(Fixture)*
*   **BT_SHOWER_01** - Rain Shower Head *(Fixture)*
*   **BT_TOILET_01** - Ceramic Toilet Commode *(Fixture)*
*   **BT_GEYSER_01** - Electric Water Geyser (25L) *(Appliances)*

---

## 5. Complete Codebase Reference

The following section explains every single C# file, its classes, and all methods, specifying exactly where they are located and what they do. This is your definitive handover map.

### 5.1 Scene & Engine Controllers (Core 3D Logistics)

**`Assets\Scripts\BMS_v2\Scene\SmartCameraController.cs`**
The brain behind user movement and visual transitions.
*   `Awake()` / `Start()` / `Update()`: Unity lifecycle methods initializing camera caches, base positions, and listening for frame-by-frame user inputs.
*   `HandleWASDMovement()`: Reads keyboard input to translate the camera across the X and Z axes.
*   `HandleMouseLook()`: Captures mouse drag data to rotate the camera pitch/yaw.
*   `HandleScrollZoom()`: Uses the scroll wheel to zoom the camera.
*   `ApplySmoothRotation()`: Lerps camera rotation values to ensure silky smooth movement without jarring snaps.
*   `HandleEscapeBack()` / `GoBack()`: Pops the camera state off the stack (returning from focused asset view to room view, etc.).
*   `FocusOnAsset(string targetId)`: Receives an asset ID, locates its 3D coordinates, and triggers an animation sequence to fly to it.
*   `FocusInFrontOfTransform(Transform target)`: Calculates the math needed to position the camera a set distance in *front* of an object, rather than inside it.
*   `PushCurrentState()`: Saves the current camera position/rotation to a stack so `GoBack()` can recall it.
*   `AnimateToState()` & `IntroZoomSequence()`: Coroutines handling the time-over-distance interpolation (flying animations).

**`Assets\Scripts\BMS_v2\Scene\RealWorldMapEngine.cs`**
Manages the aerial map tiles (fetching OpenStreetMap/Mapbox tiles) when outside the building.
*   `EnsureInitialized()`: Checks if map tile systems are ready.
*   `LoadMapForBuilding(double lat, double lon, int zoom)`: Given GPS coordinates, figures out which map tiles to download and display.
*   `RefreshMap()` / `ClearTiles()`: Forces a redownload or wipes the existing tile cache to free memory.
*   `CreateTile()` & `DownloadTileRoutine()`: Core engine functions that hit map APIs asynchronously and map textures onto planes.

**`Assets\Scripts\BMS_v2\Scene\Building3DEngine.cs`**
Generates primitive 3D geometry based on OSM data if actual 3D models aren't available.
*   `Load3DBuildings()` / `FetchOSMBuildingData()`: Queries API for adjacent building footprint polygons.
*   `ParseOSMData()`: Deserializes the JSON map data.
*   `CreateBuildingMesh()`: Takes 2D polygon arrays and extrudes them into 3D Unity Meshes to simulate a city skyline.

**`Assets\Scripts\BMS_v2\Scene\TourStateManager.cs`**
Keeps track of what state the application is currently in (Aerial vs. Interior vs. Detail).
*   `CategorizeAllInteractables()`: Scans the scene on load and categorizes all objects with a `ThinAssetInfo` tag.
*   `ProcessInteraction(string clickedId)`: Central router; when a user clicks something, this method decides if it's a building, room, or asset, and triggers the UI managers accordingly.
*   `ForceState()` & `SetInteractionState()`: Hard-switches the application mode and disables colliders/scripts on non-relevant objects to save performance.

**`Assets\Scripts\BMS_v2\Scene\AssetInteractionManager.cs`**
*   `Update()`: Runs raycasts every frame from the mouse position. If it hits an object with an `InteractableObjectInfo` or `ThinAssetInfo` script, it triggers outline highlights or click events.

**`Assets\Scripts\BMS_v2\Scene\ThinAssetInfo.cs`** & **`Assets\Scripts\BMS_v2\Scene\IgnoreDoorCollision.cs`**
*   `InitializeData()` & `CreateDotIndicator()`: Attached directly to 3D models (like the TV or Sofa). Sets up a floating UI dot indicator over the object.
*   `ApplyIgnore()`: Disables collision between the player camera and doors, ensuring seamless walkthroughs without getting stuck on physics geometry.

---

### 5.2 UI Managers (Data Display & Dashboards)

**`Assets\Scripts\BMS_v2\UI\LeftPanelManager.cs`**
Controls the search bar, task list, and side-navigation accordion.
*   `OnSearchChanged(string query)`: Fired on every keystroke in the search bar. Filters assets and tasks immediately.
*   `RefreshGlobalTasks()`: Pulls all tasks (Service, Maintenance) from the `DataStore` and lists them.
*   `FilterTasks()` & `CreateSearchResultBtn()`: Instantiates UI button prefabs dynamically based on search results.
*   `ToggleAccordion()`: Expands or collapses the side menu sections with an animation.
*   `OnTaskClicked()` / `OnResultClicked()`: Passes the selected ID to the `TourStateManager` and Camera Controller to fly the user to the physical object.

**`Assets\Scripts\BMS_v2\UI\AssetDetailPanel.cs`**
The massive Right Panel that handles editing and viewing all JSON data (Lifecycle, Cost, etc.).
*   `ToggleTab(GameObject tab)`: Switches between Warranty, Identity, and Cost tabs.
*   `ShowAsset(string id)` / `RenderSingleAssetContext()`: Queries the `DataStore` for the specific ID and binds the JSON data to the TextMeshPro fields.
*   `ToggleEditMode()` & `SaveDataToFile()`: Flips the UI from view-only Text to InputFields, allowing users to modify data. Flushes changes back to `DataStore`.
*   `PopulateHeader()` & `PopulateAllTabs()`: Heavy lifting UI binding functions that update every single text label.
*   `ShowCenterGalleryPanel()` & `AddImageRow()`: Renders the images tied to a `GalleryData` object into the UI image previewer.

**`Assets\Scripts\BMS_v2\UI\GlobalDashboardManager.cs`** & **`BuildingInsightManager.cs`**
*   `BindDashboardData()`: Aggregates total building costs, active task counts, and warranty expires, feeding them into the top-level pie charts and statistic blocks.
*   `ShowPanel()` / `HidePanel()`: Toggles the visibility of the fullscreen table views for insights.

**`Assets\Scripts\BMS_v2\UI\SummaryManager.cs`**
*   `OpenSummary(string id)` / `CloseSummary()`: Triggers the slide-up summary table at the bottom of the screen.
*   `PopulateVerticalTable()` / `AddTableRow()`: Dynamically instantiates grid rows for high-level overviews of rooms or floors without opening the full right panel.

---

### 5.3 Data Pipeline (JSON & State Storage)

**`Assets\Scripts\BMS_v2\Data\DataLoader.cs`** & **`DataStore.cs`**
*   `LoadAllDataRoutine()` (DataLoader): Coroutine that runs on app start. Uses `StreamingAssets` path to locate `BLD_001.json` and parses it into memory.
*   `RegisterBuilding()`, `GetAsset()`, `GetAllAssets()` (DataStore): The central in-memory repository (Singleton). All UI scripts ask `DataStore` for information rather than reading files themselves.
*   `AddOrUpdateAsset()`: Mutates the active memory state when a user edits data in the `AssetDetailPanel`.

**`Assets\Scripts\BMS_v2\Data\BMSDataModels.cs`**
*   *Contains no methods*. This file strictly holds standard C# Classes (`AssetData`, `LifecycleData`, `CostData`, etc.) that mimic the JSON structure perfectly for standard serialization.

**`Assets\Scripts\BMS_v2\Data\PatchProcessor.cs`**
*   `ApplyPatches(string patchesJson)`: Utility method designed to take partial JSON updates (patches) from a server and merge them into the local `DataStore` to ensure data remains up to date without reloading the whole building file.

---

### 5.4 Unity Editor Custom Tools (For Developers)

These scripts only execute inside the Unity Editor, providing custom inspectors and UI auto-generation, streamlining future development.

**`Assets\Scripts\BMS_v2\Editor\PremiumUIBuilder.cs`**
*   `GenerateUI()`, `GenerateInsightTable()`, `AutoSpawnLabels()`: Menu items (found at the top of the Unity Editor). When clicked, they automatically build perfectly aligned, pixel-perfect UI hierarchies using Unity's GUI API. Extremely useful for avoiding manual UI drag-and-drop.
*   `CreateAccordionSection()`, `CreateTableCell()`: Helper factory methods that instantiate GameObjects, add `VerticalLayoutGroups`, assign fonts, and set colors.

**`Assets\Scripts\BMS_v2\Editor\BMS_UI_Updater.cs`** & **`BuildingLabelGenerator.cs`**
*   `UpdateRightPanelUI()` / `GenerateLabels()`: Bulk updates text configurations or spawns 3D text meshes for newly imported floorplans.

---

### End of Documentation
*This system has been built focusing on modularity. Data is separated from UI, and UI is separated from the 3D Engine. All future modifications should respect this Model-View-Controller (MVC) architecture.*
