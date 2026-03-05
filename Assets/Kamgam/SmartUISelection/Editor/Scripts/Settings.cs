#if UNITY_EDITOR
#define KAMGAM_SMART_UI_SELECTION
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;

namespace Kamgam.SmartUISelection
{
    // Using this because InitializeOnLoad is unreliable if assets need to be created during that callback.
    // see: https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html
    // Asset operations such as asset loading should be avoided in InitializeOnLoad methods. InitializeOnLoad methods are
    // called before asset importing is completed and therefore the asset loading can fail resulting in a null object.
    // To do initialization after a domain reload which requires asset operations use the
    // AssetPostprocessor.OnPostprocessAllAssets callback
    class EditorInitializer : AssetPostprocessor
    {
        private static bool processed = false;
            
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // Trigger setting creation.
            var _ = Settings.Instance;
            
            if (processed)
                return;
            
            processed = true;

            Selection.Init();
            AutoHide.Init();
        }
    }
    
    [System.Serializable]
    public class Settings : ScriptableObject
    {
        private static Settings _instance;

        /// <summary>
        /// Gets or creates a single instance of the settings.
        /// </summary>
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = getOrCreate();
                }

                return _instance;
            }
        }

        private static Settings getOrCreate()
        {
            Settings settings = findSettingsInAssetDatabase();

            // no settings from file, create an instance
            if (settings == null)
            {
                settings = createSettingsInstance();
            }

            if (settings == null)
            {
                Debug.LogWarning("SmartUISettings file not found. Please create it in Assets/Editor/Resources with Right-Click > Create > Smart UI Selection > Settings.");
            }
            
            return settings;
        }

        static Settings findSettingsInAssetDatabase()
        {
            Settings settings = null;

            string[] foundPathGuids = AssetDatabase.FindAssets("t:Kamgam.SmartUISelection.Settings");
            if (foundPathGuids.Length > 0)
            {
                settings = AssetDatabase.LoadAssetAtPath<Settings>(AssetDatabase.GUIDToAssetPath(foundPathGuids[0]));
            }

            return settings;
        }

        static Settings createSettingsInstance()
        {
            Settings settings = CreateInstance<Settings>();
            settings._enablePlugin = true;
            settings._multiClickTimeThreshold = 1f;
            settings._selectOnlyEditableObjects = true;
            settings._excludeByName = new List<string>();
            settings._excludeByTag = new List<string>();
            settings._excludeByType = new List<MonoScript>();
            settings._select3dObjectsBehindCanvas = true;
            settings._select3dObjectsByMesh = true;
            settings._select3dColliders = true;
            settings._highPrecisionSpriteSelection = true;
            settings._maxDistanceFor3DSelection = 9000;
            settings._pushKeyToUseUISelection = false;
            settings._enableSmartUIKeyCode = KeyCode.Space;
            settings._pushKeyToDisableExcludeLists = false;
            settings._disableExcludeListsKeyCode = KeyCode.Escape;
            settings._limitSelectionToGraphics = true;
            settings._graphicsAlphaMinThreshold = 0f;
            settings._ignoreSelectionBaseAttributes = true;
            settings._selectionBaseToggleKeyCode = KeyCode.LeftAlt;
            settings._highlightSelection = false;
            settings._highlightColor = Color.yellow;
            settings._highlightThickness = 6;
            
            settings._ignoreScrollRects = true;
            settings._ignoreMaskImages = true;
            settings._enableAutoHide = false;
            settings._autoHideAlways = false;
            settings._dontAudoHideIfUISelected = false;
            settings._autoHideDistanceThreshold = 400;
            settings._autoHideDuringPlayback = false;
            settings._showAutoHideWarningGizmo = true;
            settings._autoHideWarningTextColor = new Color(1f, 0.7f, 0f, 1f);
            
            // select asset file location
            AssetDatabase.CreateAsset(settings, "Assets/SmartUISelectionSettings.asset");
            
            return settings;
        }

        public void ForceSave()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
        
        // settings
        [MenuItem("Tools/Smart UI Selection/Settings", priority = 102)]
        public static void SelectSettingsFile()
        {
            var settings = getOrCreate();
            if (settings != null)
            {
                UnityEditor.Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
        }

        static void updateMenuCheckedStates()
        {
            Menu.SetChecked("Tools/Smart UI Selection/Turn On", Instance._enablePlugin);
            Menu.SetChecked("Tools/Smart UI Selection/Turn 'Click Through Canvas' On", Instance._select3dObjectsBehindCanvas);
            Menu.SetChecked("Tools/Smart UI Selection/Turn 'Canvas auto-hide' On", Instance._enableAutoHide);
            Menu.SetChecked("Tools/Smart UI Selection/Turn 'Push a key to use Smart Ui' ON", Instance._pushKeyToUseUISelection);
        }

        [MenuItem("Tools/Smart UI Selection/Turn On", priority = 201, validate = true)]
        public static bool TurnPluginOnValidate()
        {
            updateMenuCheckedStates();
            return true;
        }
        
        // Plugin On/Off
        [MenuItem("Tools/Smart UI Selection/Turn On", priority = 201)]
        public static void TurnPluginOn()
        {
            Instance._enablePlugin = !Instance._enablePlugin;
            Instance.ForceSave();
            updateMenuCheckedStates();
        }

        // Click Through Canvas On/Off
        [MenuItem("Tools/Smart UI Selection/Turn 'Click Through Canvas' On", priority = 301)]
        public static void TurnClickThroughCanvasOn()
        {
            Instance._select3dObjectsBehindCanvas = !Instance._select3dObjectsBehindCanvas;
            Instance.ForceSave();
            updateMenuCheckedStates();
        }

        // Auto Hide On/Off
        [MenuItem("Tools/Smart UI Selection/Turn 'Canvas auto-hide' On", priority = 302)]
        public static void TurnAutoHideOn()
        {
            Instance._enableAutoHide = !Instance._enableAutoHide;
            Instance.ForceSave();
            updateMenuCheckedStates();
        }

        // Push Key
        [MenuItem("Tools/Smart UI Selection/Turn 'Push a key to use Smart Ui' ON", priority = 303)]
        public static void TurnPushKeyToUseSmartUiOn()
        {
            Instance._pushKeyToUseUISelection = !Instance._pushKeyToUseUISelection;
            Instance.ForceSave();
            updateMenuCheckedStates();
        }

        

        // SETTINGS

        // enabled
        public static bool enablePlugin
        {
            get { return Instance != null && Instance._enablePlugin; }
        }
        [Header("Smart UI Selection - v" + Installer.Version)]
        [Tooltip("Enables or disables the whole plugin.")]
        [SerializeField]
        private bool _enablePlugin = true;

        // multiClickTimeThreshold
        public static float multiClickTimeThreshold
        {
            get { return Instance._multiClickTimeThreshold; }
        }
        [Tooltip("If you click twice within this time then the selection will cycle through all found ui elements. Time is in seconds.")]
        [SerializeField]
        [Range(0, 2)]
        private float _multiClickTimeThreshold = 1.0f;

        // selectOnlyEditableObjects
        public static bool selectOnlyEditableObjects
        {
            get { return Instance != null && Instance._selectOnlyEditableObjects; }
        }
        [Tooltip("If checked then objects whose hideFlags are set to HideFlags.NotEditable will be ignored.")]
        [SerializeField]
        private bool _selectOnlyEditableObjects = true;

        // exludeByNameList
        public static List<string> excludeByName
        {
            get { return Instance._excludeByName; }
        }
        [Tooltip("Add names of game objects which should not be selectable.")]
        [UnityEngine.Serialization.FormerlySerializedAs("_excudeByName")]
        [SerializeField]
        private List<string> _excludeByName;

        // exludeByTagList
        public static List<string> excludeByTag
        {
            get { return Instance._excludeByTag; }
        }
        [Tooltip("Add tags of game objects which should not be selectable.")]
        [UnityEngine.Serialization.FormerlySerializedAs("_excludeByTag")]
        [SerializeField]
        private List<string> _excludeByTag;

        // exludeByTypeList
        public static List<MonoScript> excludeByType
        {
            get { return Instance._excludeByType; }
        }
        [Tooltip("Add Types (MonoScripts/components) which should not be selectable.Keep the list short because a lot of entries may impact performance.")]
        [SerializeField]
        private List<MonoScript> _excludeByType;

        // CLICK THROUGH CANVAS

        // select3dObjectsBehindCanvas
        public static bool select3dObjectsBehindCanvas
        {
            get { return Instance._select3dObjectsBehindCanvas; }
        }
        [Header("Click Through Canvas")]
        [Tooltip("If no ui element has been selected then try to click through the canvas and select 3D objects behind it. Uses the 3d objects colliders or a bounding box if there is no collider.")]
        [SerializeField]
        private bool _select3dObjectsBehindCanvas = true;

        // select3dObjectsByMesh
        public static bool select3dObjectsByMesh
        {
            get { return Instance._select3dObjectsByMesh; }
        }
        [Tooltip("Enabling this will make the click-through feature more accurate for meshes without colliders. It will do a raycast on all triangles of each mesh (even those without a collider). It may slow down the click handling (if clicked through a canvas in big scenes), turn it off if you have any issues.")]
        [SerializeField]
        private bool _select3dObjectsByMesh = true;

        // select3dColliders
        public static bool select3dColliders
        {
            get { return Instance._select3dColliders; }
        }
        [Tooltip("Select 3D objects based on their colliders too. Useful for invisible objects which solely consist of colliders (like a trigger). Usually 3D objects are only selected based on their mesh.")]
        [SerializeField]
        private bool _select3dColliders = true;

        // highPrecisionSpriteSelection
        public static bool highPrecisionSpriteSelection
        {
            get { return Instance._highPrecisionSpriteSelection; }
        }
        [Tooltip("Turn off if selection is slow in scenes with a lot of SpriteRenderers. If turned on then clicks are checked against the actual sprite mesh.")]
        [SerializeField]
        private bool _highPrecisionSpriteSelection = true;

        // maxDistanceFor3DSelection
        public static float maxDistanceFor3DSelection
        {
            get { return Instance._maxDistanceFor3DSelection; }
        }
        [Tooltip("A raycast is used to detect 3D objects. This sets the maximum distance for the raycast in world units.")]
        [SerializeField]
        private float _maxDistanceFor3DSelection = 9000;

        // UI SETTINGS

        // Smart UI Selection
        
        // pushKeyToUseUiSelection
        public static bool pushKeyToUseUiSelection
        {
            get { return Instance != null && Instance._pushKeyToUseUISelection; }
        }
        [FormerlySerializedAs("_pushKeyToUseUiSelection")]
        [Space(10)]
        [Header("Smart Selection")]
        [Tooltip("If checked then Smart UI Selection is only enabled if you press the 'Enable Smart Ui Key Code' key.")]
        [SerializeField]
        private bool _pushKeyToUseUISelection = false;

        // This is the key which you need to press if "pushKeyToUseUiSelection" is checked.
        public static KeyCode EnableSmartUiKeyCode
        {
            get { return Instance._enableSmartUIKeyCode; }
        }
        [FormerlySerializedAs("_enableSmartUiKeyCode")]
        [Tooltip("Push and HOLD this key to disable Smart UI Selection (works only if pushKeyToUseUiSelection is turned on).")]
        [SerializeField]
        public KeyCode _enableSmartUIKeyCode = KeyCode.Space;

        // pushKeyToDisableExcludeLists
        public static bool pushKeyToDisableExcludeLists
        {
            get { return Instance != null && Instance._pushKeyToDisableExcludeLists; }
        }
        [Space(10)]
        [Tooltip("If checked and if the key is pressed then Smart UI Selection will ignore your exclude lists (act as if they are empty).")]
        [SerializeField]
        private bool _pushKeyToDisableExcludeLists = false;

        // This is the key which you need to press if "pushKeyToDisableExcludeLists" is checked.
        public static KeyCode DisableExcludeListsKeyCode
        {
            get { return Instance._disableExcludeListsKeyCode; }
        }
        [Tooltip("Push and HOLD this key to disable the exclude lists (works only if \"Push Key To Disable Exclude Lists\" is turned on).")]
        [SerializeField]
        public KeyCode _disableExcludeListsKeyCode = KeyCode.Escape;


        // limitSelectionToGraphics
        public static bool limitSelectionToGraphics
        {
            get { return Instance._limitSelectionToGraphics; }
        }
        [Space(10)]
        [Tooltip("Limit ui selection to elements with graphics (objects which have a 'Graphic' component).")]
        [SerializeField]
        private bool _limitSelectionToGraphics = true;

        // alphaThreshold
        public static float alphaMinThreshold
        {
            get { return Instance._graphicsAlphaMinThreshold; }
        }
        [Tooltip("Select elements only if they have an alpha value above the threshold. 'Limit Selection To Graphics' needs to be turned on for this to have any effect.")]
        [SerializeField]
        [Range(0, 1)]
        private float _graphicsAlphaMinThreshold = 0;

        // ignoreSelectionBaseAttributes
        public static bool ignoreSelectionBaseAttributes
        {
            get { return Instance._ignoreSelectionBaseAttributes; }
        }
        [Tooltip("Check to completely ignore the [SelectionBase] Attributes. Hint: Buttons have this Attribute by default.")]
        [SerializeField]
        private bool _ignoreSelectionBaseAttributes = true;

        // This is the key which you need to press if "SelectionBaseToggleKeyCode" is checked.
        public static KeyCode SelectionBaseToggleKeyCode
        {
            get { return Instance._selectionBaseToggleKeyCode; }
        }
        [Tooltip("Push and HOLD this key while selecting to invert the ignoreSelectionBaseAttributes setting.")]
        [SerializeField]
        public KeyCode _selectionBaseToggleKeyCode = KeyCode.LeftAlt;
        

        // ignoreScrolRects
        public static bool ignoreScrollRects
        {
            get { return Instance._ignoreScrollRects; }
        }
        [Tooltip("Check to completely ignore ScrolRect transforms. Usually you just want the content, not the scroll rect.")]
        [SerializeField]
        private bool _ignoreScrollRects = true;

        // ignoreMaskImages
        public static bool ignoreMaskImages
        {
            get { return Instance._ignoreMaskImages; }
        }
        [Tooltip("Check to completely ignore mask images (if the are hidden). Usually you just want the content of the masked area, not the image that defines the mask.")]
        [SerializeField]
        private bool _ignoreMaskImages = true;


        // SCREEN SPACE OVERLAY SETTINGS

        // hideScreenSpaceOverlaysOnCloseUp
        public static bool enableAutoHide
        {
            get { return Instance._enableAutoHide; }
        }
        [Header("Canvas auto-hide")]
        [Tooltip("Should ScreenSpaceOverlay canvases be hidden in the scene view if the editor camera gets very close? Usefull to prohibit unwanted canvas selections while you edit the 3d scene. They will only be hidden if your mouse cursor is in the scene view.")]
        [SerializeField]
        private bool _enableAutoHide = false;

        // autoHideAlways
        public static bool autoHideAlways
        {
            get { return Instance._autoHideAlways; }
        }
        [Tooltip("USE WITH CAUTION! If enabled then auto hide will always affect the canvases independently of where the mouse cursor is (this may confuse users). Enabling this works only in Unity 2019.2+. ")]
        [SerializeField]
        private bool _autoHideAlways = false;

        // dontAudoHideIfUiSelected
        public static bool dontAudoHideIfUiSelected
        {
            get { return Instance._dontAudoHideIfUISelected; }
        }
        [FormerlySerializedAs("_dontAudoHideIfUiSelected")]
        [Tooltip("If enabled then canvases will not be auto-hidden if a GameObject with a RectTransform is selected. Useful if switching often between 3D and UI. Off by default.")]
        [SerializeField]
        private bool _dontAudoHideIfUISelected = false;

        // autoHideDistanceThreshold
        public static float autoHideDistanceThreshold
        {
            get { return Instance._autoHideDistanceThreshold; }
        }
        [Tooltip("ScreenSpaceOverlay canvases will be hidden if the editor camera distance to the XY plane is less than X.")]
        [SerializeField]
        [Range(0, 1000)]
        private float _autoHideDistanceThreshold = 400f;

        // autoHideDuringPlayback
        public static bool autoHideDuringPlayback
        {
            get { return Instance._autoHideDuringPlayback; }
        }
        [Tooltip("Should ScreenSpaceOverlay canvases be hidden in play mode too? - BETA")]
        [SerializeField]
        private bool _autoHideDuringPlayback = false;

        // showAutoHideWarningGizmo
        public static bool showAutoHideWarningGizmo
        {
            get { return Instance._showAutoHideWarningGizmo; }
        }
        [Tooltip("Show a warning text next to canvases to indicate that auto-hide is turned on?")]
        [SerializeField]
        private bool _showAutoHideWarningGizmo = true;

        // autoHideWarningTextColor
        public static Color autoHideWarningTextColor
        {
            get { return Instance._autoHideWarningTextColor; }
        }
        [Tooltip("Color of the Text shown beneath a canvas which will be hidden.")]
        [SerializeField]
        private Color _autoHideWarningTextColor = new Color(1.0f, 0.7f, 0.0f);
        
        
        [Header("Highlight")]
        [Tooltip("Color of the selection highlight. NOTICE: It only highlights what you are selecting in the scene view, not the hierarchy.")]
        [SerializeField]
        private bool _highlightSelection = false;
        public static bool highlightSelection
        {
            get { return Instance._highlightSelection; }
            set
            {
                Instance._highlightSelection = value;
                if (UnityEditor.Selection.gameObjects.Length > 0)
                {
                    RectTransformHighlighter.Highlight(UnityEditor.Selection.gameObjects.ToList().Where(g => g.transform is RectTransform).Select(g => g.transform as RectTransform).ToList());
                }
            }
        }
        
        
        [Tooltip("Color of the selection highlight")]
        [SerializeField]
        private Color _highlightColor = new Color(1.0f, 1.0f, 0.0f, 1f);
        public static Color highlightColor
        {
            get { return Instance._highlightColor; }
        }

        [Tooltip("How thick the highlight outline should be.")]
        [SerializeField][Range(1, 10)]
        private int _highlightThickness = 6;
        public static int highlightThickness
        {
            get { return Instance._highlightThickness; }
        }
        

        public void OnValidate()
        {
#if !UNITY_2019_2_OR_NEWER
            if (_autoHideAlways == true)
            {
                _autoHideAlways = false;
                Debug.LogWarning("Smart Ui: Auto hide canvas 'Auto Hide Always' is only supported in Unity 2019.2.0 or newer. Resetting it to disabled now.");
            }
#endif
        }
    }
}
#endif
