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
    private List<AssetInfo> unregisteredAssets = new List<AssetInfo>();
    private bool isScanning = false;
    private string targetFolder = "Assets/Resources_moved";

    [System.Serializable]
    private class AssetInfo
    {
        public string path;
        public string assetName;
        public string assetType;
        public Object asset;
        public bool isSelected;
    }

    [MenuItem("Tools/Addressable Checker")]
    public static void ShowWindow()
    {
        GetWindow<AddressableChecker>("Addressable Checker");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("어드레서블 미등록 에셋 검사", EditorStyles.boldLabel);
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

        // 결과 표시
        if (unregisteredAssets.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"미등록 에셋 발견: {unregisteredAssets.Count}개", EditorStyles.boldLabel);

            int selectedCount = unregisteredAssets.Count(a => a.isSelected);
            if (selectedCount > 0)
            {
                EditorGUILayout.LabelField($"(선택: {selectedCount}개)", GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();

            // 전체 선택/해제 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("전체 선택", GUILayout.Height(25)))
            {
                foreach (var asset in unregisteredAssets)
                {
                    asset.isSelected = true;
                }
                Repaint();
            }
            if (GUILayout.Button("전체 해제", GUILayout.Height(25)))
            {
                foreach (var asset in unregisteredAssets)
                {
                    asset.isSelected = false;
                }
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var assetInfo in unregisteredAssets)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                // 체크박스
                assetInfo.isSelected = EditorGUILayout.Toggle(assetInfo.isSelected, GUILayout.Width(20));

                // 에셋 타입과 이름
                EditorGUILayout.LabelField($"[{assetInfo.assetType}] {assetInfo.assetName}", EditorStyles.boldLabel);

                EditorGUILayout.EndHorizontal();

                // 경로
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("경로:", GUILayout.Width(40));
                EditorGUILayout.SelectableLabel(assetInfo.path, GUILayout.Height(18));
                EditorGUILayout.EndHorizontal();

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

                if (GUILayout.Button("등록", GUILayout.Width(60)))
                {
                    RegisterToAddressable(assetInfo);
                }

                if (GUILayout.Button("되돌림", GUILayout.Width(60)))
                {
                    MoveBackToResources(assetInfo);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = selectedCount > 0;
            if (GUILayout.Button($"선택한 항목 어드레서블 등록 ({selectedCount}개)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("확인",
                    $"선택한 {selectedCount}개의 에셋을 어드레서블에 등록할까?",
                    "등록", "취소"))
                {
                    RegisterSelectedToAddressable();
                }
            }

            if (GUILayout.Button($"선택한 항목 Resources로 되돌림 ({selectedCount}개)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("확인",
                    $"선택한 {selectedCount}개의 에셋을 Resources 폴더로 이동할까?",
                    "이동", "취소"))
                {
                    MoveSelectedBackToResources();
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
        else if (!isScanning && unregisteredAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("검사 버튼을 눌러 미등록 에셋을 찾음", MessageType.Info);
        }

        if (isScanning)
        {
            EditorGUILayout.HelpBox("검사 중...", MessageType.Warning);
        }
    }

    private void ScanAssets()
    {
        isScanning = true;
        unregisteredAssets.Clear();

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
            foreach (var guid in allAssetGuids)
            {
                checkedCount++;
                EditorUtility.DisplayProgressBar("검사 중",
                    $"{checkedCount}/{allAssetGuids.Length}",
                    (float)checkedCount / allAssetGuids.Length);

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 폴더는 제외
                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;

                // .meta 파일 제외
                if (assetPath.EndsWith(".meta"))
                    continue;

                // Addressable에 등록되어 있는지 확인
                var entry = settings.FindAssetEntry(guid);

                if (entry == null)
                {
                    // 미등록 에셋 발견
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (asset != null)
                    {
                        unregisteredAssets.Add(new AssetInfo
                        {
                            path = assetPath,
                            assetName = Path.GetFileName(assetPath),
                            assetType = asset.GetType().Name,
                            asset = asset
                        });
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            Debug.Log($"검사 완료: {unregisteredAssets.Count}개 미등록 에셋 발견");

            // 타입별로 정렬
            unregisteredAssets = unregisteredAssets
                .OrderBy(a => a.assetType)
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

        // 기본 그룹에 등록
        var defaultGroup = settings.DefaultGroup;
        var guid = AssetDatabase.AssetPathToGUID(assetInfo.path);

        var entry = settings.CreateOrMoveEntry(guid, defaultGroup);
        entry.address = Path.GetFileNameWithoutExtension(assetInfo.path);

        Debug.Log($"어드레서블 등록 완료: {assetInfo.path}");

        // 리스트에서 제거
        unregisteredAssets.Remove(assetInfo);

        // Addressable 설정 저장
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

        var assetsToRegister = unregisteredAssets.Where(a => a.isSelected).ToList();

        foreach (var assetInfo in assetsToRegister)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetInfo.path);
            var entry = settings.CreateOrMoveEntry(guid, defaultGroup);
            entry.address = Path.GetFileNameWithoutExtension(assetInfo.path);

            registeredCount++;
            EditorUtility.DisplayProgressBar("등록 중",
                $"{registeredCount}/{assetsToRegister.Count}",
                (float)registeredCount / assetsToRegister.Count);
        }

        EditorUtility.ClearProgressBar();

        // 선택된 항목만 리스트에서 제거
        unregisteredAssets.RemoveAll(a => a.isSelected);

        AssetDatabase.SaveAssets();

        Debug.Log($"{registeredCount}개 에셋 어드레서블 등록 완료");

        EditorUtility.DisplayDialog("완료",
            $"{registeredCount}개 에셋이 어드레서블에 등록됨",
            "확인");

        Repaint();
    }

    private void MoveBackToResources(AssetInfo assetInfo)
    {
        // Resources_moved -> Resources 경로 변환
        string newPath = assetInfo.path.Replace("Resources_moved", "Resources");

        // 디렉토리 생성
        string directory = Path.GetDirectoryName(newPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 에셋 이동
        string error = AssetDatabase.MoveAsset(assetInfo.path, newPath);

        if (string.IsNullOrEmpty(error))
        {
            Debug.Log($"에셋 이동 완료: {assetInfo.path} -> {newPath}");
            unregisteredAssets.Remove(assetInfo);
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
        var assetsToMove = unregisteredAssets.Where(a => a.isSelected).ToList();
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

        // 선택된 항목만 리스트에서 제거
        unregisteredAssets.RemoveAll(a => a.isSelected);
        AssetDatabase.Refresh();

        Debug.Log($"이동 완료: {movedCount}개 성공, {failedCount}개 실패");

        EditorUtility.DisplayDialog("완료",
            $"{movedCount}개 에셋 이동 완료\n{failedCount}개 실패",
            "확인");

        Repaint();
    }
}

#endif