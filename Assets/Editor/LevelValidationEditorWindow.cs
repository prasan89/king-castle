#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using KingSmash.Levels;

namespace KingSmash.Editor
{
    public class LevelValidationEditorWindow : EditorWindow
    {
        private Vector2 _scroll;
        private List<LevelValidationReport> _reports;
        private bool _worldsValid;
        private string _summary;

        [MenuItem("KingSmash/Level Tools/Validate All Levels")]
        public static void ShowWindow()
        {
            var win = GetWindow<LevelValidationEditorWindow>("Level Validator");
            win.minSize = new Vector2(500, 400);
            win.RunValidation();
            win.Show();
        }

        [MenuItem("KingSmash/Level Tools/Validate Worlds")]
        public static void ValidateWorldsMenu()
        {
            bool ok = WorldValidator.ValidateAll();
            EditorUtility.DisplayDialog("World Validation",
                ok ? "All 5 worlds are valid." : "World validation FAILED — check Console.", "OK");
        }

        [MenuItem("KingSmash/Level Tools/Check Level ID Uniqueness")]
        public static void CheckIdUniqueness()
        {
            var seen = new HashSet<int>();
            var dupes = new List<int>();
            foreach (var def in LevelConfigFactory.All)
                if (!seen.Add(def.LevelIndex)) dupes.Add(def.LevelIndex);
            if (dupes.Count == 0)
                EditorUtility.DisplayDialog("ID Check", $"All {LevelConfigFactory.All.Count} level IDs are unique.", "OK");
            else
                EditorUtility.DisplayDialog("ID Check", $"DUPLICATE IDs found: {string.Join(", ", dupes)}", "OK");
        }

        private void RunValidation()
        {
            _reports = LevelValidator.ValidateAll();
            _worldsValid = WorldValidator.ValidateAll();
            int errors = 0, warnings = 0;
            foreach (var r in _reports) { errors += r.Errors.Count; warnings += r.Warnings.Count; }
            _summary = $"Levels: {LevelConfigFactory.All.Count}/100 | Errors: {errors} | Warnings: {warnings} | Worlds: {(_worldsValid ? "OK" : "FAIL")}";
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            if (GUILayout.Button("Re-run Validation", GUILayout.Height(30)))
                RunValidation();

            EditorGUILayout.Space(4);
            var summaryStyle = new GUIStyle(EditorStyles.boldLabel);
            summaryStyle.normal.textColor = (_reports != null && _reports.TrueForAll(r => r.IsValid)) ? Color.green : Color.red;
            EditorGUILayout.LabelField(_summary ?? "Click Re-run to validate.", summaryStyle);
            EditorGUILayout.Space(4);

            if (_reports == null) return;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var r in _reports)
            {
                if (r.IsValid && r.Warnings.Count == 0) continue; // hide passing levels
                var color = r.IsValid ? Color.yellow : Color.red;
                var style = new GUIStyle(EditorStyles.foldout);
                style.normal.textColor = color;
                EditorGUILayout.LabelField(r.ToString(), EditorStyles.helpBox);
            }

            // Show count of passing levels
            int passing = 0;
            foreach (var r in _reports) if (r.IsValid && r.Warnings.Count == 0) passing++;
            EditorGUILayout.LabelField($"({passing} levels passed with no issues)", EditorStyles.miniLabel);

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
