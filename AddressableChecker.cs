#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AddressableChecker : EditorWindow
{
    private Vector2 scrollPos;
    private List<AssetInfo> allAssets = new List<AssetInfo>();
    private bool isScanning = false;
    private string targetFolder = "Assets/Resources_moved";

    // 필터링 옵션
    private bool showUnregistered = true;
    private bool showUnlabeled = true;
    private bool showUngrouped = true;

    [System.Serializable]
    private class AssetInfo
    {
        public string path;
        public string assetName;
        public string assetType;
        public Object asset;
        public bool isSelected;

        // 상태 정보
        public bool isRegistered;
        public bool hasLabel;
        public bool hasProperGroup;

        public string addressableGroup;
        public List<string> labels;

        // UI 표시용
        public string GetStatusText()
        {
            List<string> issues = new List<string>();
            if (!isRegistered) issues.Add("미등록");
            if (isRegistered && !hasLabel) issues.Add("라벨 누락");
            if (isRegistered && !hasProperGroup) issues.Add("그룹 누락");

            return issues.Count > 0 ? string.Join(", ", issues) : "정상";
        }

        public bool HasIssue()
        {
            return !isRegistered || (isRegistered && (!hasLabel || !hasProperGroup));
        }
    }

    [MenuItem("Tools/Addressable Checker")]
    public static void ShowWindow()
    {
        GetWindow<AddressableChecker>("Addressable Checker");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("어드레서블 에셋 검사", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // 타겟 폴더 경로 입력
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("검사 폴더:", GUILayout.Width(80));
        targetFolder = EditorGUILayout.TextField(targetFolder);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 검사 버튼
        GUI.enabled = !isScanning;
        if (GUILayout.Button("검사 시작", GUILayout.Height(30)))
        {
            ScanAssets();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        // 필터 옵션
        if (allAssets.Count > 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("필터 옵션", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            showUnregistered = EditorGUILayout.ToggleLeft($"미등록 에셋 ({GetUnregisteredCount()}개)", showUnregistered);
            showUnlabeled = EditorGUILayout.ToggleLeft($"라벨 누락 ({GetUnlabeledCount()}개)", showUnlabeled);
            showUngrouped = EditorGUILayout.ToggleLeft($"그룹 누락 ({GetUngroupedCount()}개)", showUngrouped);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
        }

        // 결과 표시
        DrawAssetList();

        if (isScanning)
        {
            EditorGUILayout.HelpBox("검사 중...", MessageType.Warning);
        }
    }

    private void DrawAssetList()
    {
        var filteredAssets = GetFilteredAssets();

        if (filteredAssets.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"문제가 있는 에셋: {filteredAssets.Count}개", EditorStyles.boldLabel);

            int selectedCount = filteredAssets.Count(a => a.isSelected);
            if (selectedCount > 0)
            {
                EditorGUILayout.LabelField($"(선택: {selectedCount}개)", GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();

            // 전체 선택/해제 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("전체 선택", GUILayout.Height(25)))
            {
                foreach (var asset in filteredAssets)
                {
                    asset.isSelected = true;
                }
                Repaint();
            }
            if (GUILayout.Button("전체 해제", GUILayout.Height(25)))
            {
                foreach (var asset in filteredAssets)
                {
                    asset.isSelected = false;
                }
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var assetInfo in filteredAssets)
            {
                DrawAssetInfoBox(assetInfo);
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            // 하단 액션 버튼
            DrawActionButtons(selectedCount);
        }
        else if (!isScanning && allAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("검사 버튼을 눌러 에셋을 검사함", MessageType.Info);
        }
        else if (!isScanning && filteredAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("필터 조건에 맞는 에셋이 없음", MessageType.Info);
        }
    }

    private void DrawAssetInfoBox(AssetInfo assetInfo)
    {
        // 상태에 따른 색상 설정
        Color originalColor = GUI.backgroundColor;
        if (!assetInfo.isRegistered)
            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f); // 연한 빨강
        else if (!assetInfo.hasLabel || !assetInfo.hasProperGroup)
            GUI.backgroundColor = new Color(1f, 0.9f, 0.6f); // 연한 노랑

        EditorGUILayout.BeginVertical("box");
        GUI.backgroundColor = originalColor;

        EditorGUILayout.BeginHorizontal();

        // 체크박스
        assetInfo.isSelected = EditorGUILayout.Toggle(assetInfo.isSelected, GUILayout.Width(20));

        // 상태 표시
        GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
        statusStyle.normal.textColor = assetInfo.HasIssue() ? Color.red : Color.green;
        EditorGUILayout.LabelField($"[{assetInfo.GetStatusText()}]", statusStyle, GUILayout.Width(120));

        // 에셋 타입과 이름
        EditorGUILayout.LabelField($"[{assetInfo.assetType}] {assetInfo.assetName}", EditorStyles.boldLabel);

        EditorGUILayout.EndHorizontal();

        // 경로
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("경로:", GUILayout.Width(40));
        EditorGUILayout.SelectableLabel(assetInfo.path, GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();

        // 어드레서블 정보 (등록된 경우)
        if (assetInfo.isRegistered)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("그룹:", GUILayout.Width(40));
            EditorGUILayout.LabelField(assetInfo.addressableGroup ?? "없음");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("라벨:", GUILayout.Width(40));
            string labelsText = assetInfo.labels != null && assetInfo.labels.Count > 0
                ? string.Join(", ", assetInfo.labels)
                : "없음";
            EditorGUILayout.LabelField(labelsText);
            EditorGUILayout.EndHorizontal();
        }

        // 에셋 필드와 버튼들
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(assetInfo.asset, typeof(Object), false);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("선택", GUILayout.Width(60)))
        {
            Selection.activeObject = assetInfo.asset;
            EditorGUIUtility.PingObject(assetInfo.asset);
        }

        if (!assetInfo.isRegistered)
        {
            if (GUILayout.Button("등록", GUILayout.Width(60)))
            {
                RegisterToAddressable(assetInfo);
            }

            if (GUILayout.Button("되돌림", GUILayout.Width(60)))
            {
                MoveBackToResources(assetInfo);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.Space(5);
    }

    private void DrawActionButtons(int selectedCount)
    {
        var selectedAssets = allAssets.Where(a => a.isSelected).ToList();
        var unregisteredSelected = selectedAssets.Where(a => !a.isRegistered).ToList();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = unregisteredSelected.Count > 0;
        if (GUILayout.Button($"선택한 미등록 에셋 등록 ({unregisteredSelected.Count}개)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("확인",
                $"선택한 {unregisteredSelected.Count}개의 에셋을 어드레서블에 등록할까?",
                "등록", "취소"))
            {
                RegisterSelectedToAddressable();
            }
        }

        if (GUILayout.Button($"선택한 미등록 에셋 되돌림 ({unregisteredSelected.Count}개)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("확인",
                $"선택한 {unregisteredSelected.Count}개의 에셋을 Resources 폴더로 이동할까?",
                "이동", "취소"))
            {
                MoveSelectedBackToResources();
            }
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private List<AssetInfo> GetFilteredAssets()
    {
        return allAssets.Where(a =>
        {
            if (!a.HasIssue()) return false;

            if (!a.isRegistered && showUnregistered) return true;
            if (a.isRegistered && !a.hasLabel && showUnlabeled) return true;
            if (a.isRegistered && !a.hasProperGroup && showUngrouped) return true;

            return false;
        }).ToList();
    }

    private int GetUnregisteredCount() => allAssets.Count(a => !a.isRegistered);
    private int GetUnlabeledCount() => allAssets.Count(a => a.isRegistered && !a.hasLabel);
    private int GetUngroupedCount() => allAssets.Count(a => a.isRegistered && !a.hasProperGroup);


    private void ScanAssets()
    {
        isScanning = true;
        allAssets.Clear();

        try
        {
            // 폴더 존재 확인
            if (!Directory.Exists(targetFolder))
            {
                EditorUtility.DisplayDialog("오류", $"폴더를 찾을 수 없음: {targetFolder}", "확인");
                return;
            }

            // Addressable 설정 가져오기
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("오류", "Addressable 설정을 찾을 수 없음", "확인");
                return;
            }

            // 모든 에셋 GUID 수집
            var allAssetGuids = AssetDatabase.FindAssets("", new[] { targetFolder });

            Debug.Log($"검사 시작: {allAssetGuids.Length}개 에셋 발견");

            int checkedCount = 0;
            int progressUpdateInterval = Mathf.Max(1, allAssetGuids.Length / 100); // 1% 단위로 업데이트

            foreach (var guid in allAssetGuids)
            {
                checkedCount++;

                // 프로그레스 바 업데이트 빈도 줄임
                if (checkedCount % progressUpdateInterval == 0 || checkedCount == allAssetGuids.Length)
                {
                    EditorUtility.DisplayProgressBar("검사 중",
                        $"{checkedCount}/{allAssetGuids.Length}",
                        (float)checkedCount / allAssetGuids.Length);
                }

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 폴더는 제외
                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;

                // .meta 파일 제외
                if (assetPath.EndsWith(".meta"))
                    continue;

                // Addressable에 등록되어 있는지 확인
                var entry = settings.FindAssetEntry(guid);

                // 문제가 있는 에셋만 체크
                bool isRegistered = entry != null;
                bool hasLabel = entry != null && entry.labels != null && entry.labels.Count > 0;
                bool hasProperGroup = entry != null && entry.parentGroup != null && entry.parentGroup != settings.DefaultGroup;

                // 문제가 없으면 스킵 (성능 최적화)
                bool hasIssue = !isRegistered || !hasLabel || !hasProperGroup;
                if (!hasIssue)
                    continue;

                // 문제가 있는 에셋만 로드
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    var assetInfo = new AssetInfo
                    {
                        path = assetPath,
                        assetName = Path.GetFileName(assetPath),
                        assetType = asset.GetType().Name,
                        asset = asset,
                        isRegistered = isRegistered,
                        hasLabel = hasLabel,
                        hasProperGroup = hasProperGroup
                    };

                    if (entry != null)
                    {
                        assetInfo.addressableGroup = entry.parentGroup != null ? entry.parentGroup.Name : "Unknown";
                        assetInfo.labels = entry.labels != null ? new List<string>(entry.labels) : new List<string>();
                    }

                    allAssets.Add(assetInfo);
                }
            }

            EditorUtility.ClearProgressBar();

            int unregisteredCount = GetUnregisteredCount();
            int unlabeledCount = GetUnlabeledCount();
            int ungroupedCount = GetUngroupedCount();

            Debug.Log($"검사 완료: 미등록 {unregisteredCount}개, 라벨 누락 {unlabeledCount}개, 그룹 누락 {ungroupedCount}개 발견");

            // 타입별로 정렬
            allAssets = allAssets
                .OrderBy(a => a.isRegistered ? 1 : 0) // 미등록을 먼저
                .ThenBy(a => a.assetType)
                .ThenBy(a => a.path)
                .ToList();
        }
        finally
        {
            isScanning = false;
            EditorUtility.ClearProgressBar();
            Repaint();
        }
    }

    private void RegisterToAddressable(AssetInfo assetInfo)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable 설정을 찾을 수 없음");
            return;
        }

        var defaultGroup = settings.DefaultGroup;
        var guid = AssetDatabase.AssetPathToGUID(assetInfo.path);

        var entry = settings.CreateOrMoveEntry(guid, defaultGroup);
        entry.address = Path.GetFileNameWithoutExtension(assetInfo.path);

        // 상태 업데이트
        assetInfo.isRegistered = true;
        assetInfo.hasProperGroup = false; // 기본 그룹이므로
        assetInfo.hasLabel = false;
        assetInfo.addressableGroup = defaultGroup.Name;
        assetInfo.labels = new List<string>();

        Debug.Log($"어드레서블 등록 완료: {assetInfo.path}");

        AssetDatabase.SaveAssets();
        Repaint();
    }

    private void RegisterSelectedToAddressable()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable 설정을 찾을 수 없음");
            return;
        }

        var defaultGroup = settings.DefaultGroup;
        int registeredCount = 0;

        var assetsToRegister = allAssets.Where(a => a.isSelected && !a.isRegistered).ToList();

        foreach (var assetInfo in assetsToRegister)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetInfo.path);
            var entry = settings.CreateOrMoveEntry(guid, defaultGroup);
            entry.address = Path.GetFileNameWithoutExtension(assetInfo.path);

            // 상태 업데이트
            assetInfo.isRegistered = true;
            assetInfo.hasProperGroup = false;
            assetInfo.hasLabel = false;
            assetInfo.addressableGroup = defaultGroup.Name;
            assetInfo.labels = new List<string>();

            registeredCount++;
            EditorUtility.DisplayProgressBar("등록 중",
                $"{registeredCount}/{assetsToRegister.Count}",
                (float)registeredCount / assetsToRegister.Count);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();

        Debug.Log($"{registeredCount}개 에셋 어드레서블 등록 완료");

        EditorUtility.DisplayDialog("완료",
            $"{registeredCount}개 에셋이 어드레서블에 등록됨",
            "확인");

        Repaint();
    }

    private void MoveBackToResources(AssetInfo assetInfo)
    {
        string newPath = assetInfo.path.Replace("Resources_moved", "Resources");

        string directory = Path.GetDirectoryName(newPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string error = AssetDatabase.MoveAsset(assetInfo.path, newPath);

        if (string.IsNullOrEmpty(error))
        {
            Debug.Log($"에셋 이동 완료: {assetInfo.path} -> {newPath}");
            allAssets.Remove(assetInfo);
            AssetDatabase.Refresh();
            Repaint();
        }
        else
        {
            Debug.LogError($"에셋 이동 실패: {error}");
            EditorUtility.DisplayDialog("오류", $"이동 실패: {error}", "확인");
        }
    }

    private void MoveSelectedBackToResources()
    {
        var assetsToMove = allAssets.Where(a => a.isSelected && !a.isRegistered).ToList();
        int movedCount = 0;
        int failedCount = 0;

        foreach (var assetInfo in assetsToMove)
        {
            EditorUtility.DisplayProgressBar("이동 중",
                $"{movedCount + failedCount + 1}/{assetsToMove.Count}",
                (float)(movedCount + failedCount + 1) / assetsToMove.Count);

            string newPath = assetInfo.path.Replace("Resources_moved", "Resources");
            string directory = Path.GetDirectoryName(newPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string error = AssetDatabase.MoveAsset(assetInfo.path, newPath);

            if (string.IsNullOrEmpty(error))
            {
                movedCount++;
            }
            else
            {
                failedCount++;
                Debug.LogError($"이동 실패: {assetInfo.path} - {error}");
            }
        }

        EditorUtility.ClearProgressBar();

        allAssets.RemoveAll(a => a.isSelected && !a.isRegistered);
        AssetDatabase.Refresh();

        Debug.Log($"이동 완료: {movedCount}개 성공, {failedCount}개 실패");

        EditorUtility.DisplayDialog("완료",
            $"{movedCount}개 에셋 이동 완료\n{failedCount}개 실패",
            "확인");

        Repaint();
    }
}

#endif