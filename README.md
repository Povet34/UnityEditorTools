### GradientTextureGenerator
유니티 에디터에서 사용할 수 있는 그라디언트 텍스처 생성기

<img width="527" height="738" alt="image" src="https://github.com/user-attachments/assets/0aaa4c69-ec0e-44af-829a-21cefd8b128e" />


### 주요기능
1. 그라디언트 방향 제어
- 선형: 0-360도 자유로운 방향 설정
- 원형: 중심에서 바깥으로 퍼지는 그라디언트
- 대각선: 대각선 방향 그라디언트
- 다이아몬드: 다이아몬드 형태 그라디언트

2. 그라디언트 시작 지점 조절
- 시작 지점과 끝 지점을 0-1 범위에서 정밀 조절
- 그라디언트 커브를 사용한 비선형 전환 효과

3. PNG 내보내기
- Assets/Textures 폴더에 자동 저장
- 텍스처 임포트 설정 자동 적용
- 알파 채널 지원

추가 기능:
- 실시간 미리보기: 설정 변경 시 즉시 확인
- 다양한 해상도: 64x64부터 2048x2048까지 지원
- 알파 그라디언트: 투명도 그라디언트 별도 제어
- 커브 에디터: 비선형 그라디언트 전환

사용 방법:
- Assets/Editor 폴더에 스크립트 저장
- 유니티 메뉴에서 Tools > 그라디언트 텍스처 생성기 선택
- 원하는 설정으로 조절
- "PNG로 내보내기" 버튼 클릭

------

### AutoKeystoreSHAExtractor
Unity 프로젝트용 Android 키스토어 SHA 핑거프린트 자동 추출 도구

<img width="661" height="589" alt="image" src="https://github.com/user-attachments/assets/5cff8aed-7886-464b-84fe-a90f6fc249c7" />


Firebase Authentication 설정 시 필요한 SHA1/SHA256 해시값을 쉽고 빠르게 추출할 수 있는 Unity Editor 확장 도구입니다.

✨ 주요 기능
- Keytool 자동 탐색: Unity 설치 경로에서 keytool.exe를 자동으로 찾습니다
- 다중 환경 지원: Development, Test, Production 환경을 동시에 처리
- 일괄 처리: 모든 키스토어를 한 번에 처리하거나 개별 처리 가능
- 원클릭 복사: SHA 값을 클립보드에 바로 복사
- Firebase 연동: Firebase Console로 바로 이동
- 파일 관리: 키스토어 폴더 빠른 접근

Unity 에디터에서 Tools > Auto Keystore SHA Extractor 메뉴로 실행

🚀 사용법
1. 도구 실행
Unity 메뉴바에서 Tools > Auto Keystore SHA Extractor 선택
2. 프로젝트 경로 설정
3. 키스토어 파일 준비
다음 경로에 키스토어 파일들을 배치:
Assets/Keystores/
├── Dev.keystore      # 개발용
├── Test.keystore     # 테스트용
└── Prod.keystore     # 배포용
4. SHA 추출 실행

🚀 Extract All SHA Keys: 모든 환경 동시 처리
Dev Only / Test Only / Prod Only: 개별 환경 처리

5. 결과 복사 및 사용

각 환경별 SHA1/SHA256 값이 표시됩니다
- Copy 버튼으로 클립보드에 복사
- Firebase Console 버튼으로 Firebase로 이동

### EnvBuildManager & EnvSwitcher
Unity 환경별 Firebase 포함 Andorid 종속성 해결 /Addressable 환경 설정 자동설정 후 빌드 해주는 에디터 확장 도구입니다. 

<img width="387" height="576" alt="image" src="https://github.com/user-attachments/assets/46bba639-e946-457d-b18f-82f51e47b26e" />




### Shader Changer
1. Runtime/Editor Mode 둘 다 가능한 Shader 및 RenderingMode 변경가능한 Tool

<img width="1183" height="854" alt="image" src="https://github.com/user-attachments/assets/ec9ff874-c9fa-4b64-83eb-71e4dca13549" />




### Object Profiler
1. 선택된 오브젝트의 전체적인 메시 정보
2. 사용중인 셰이더 정보
3. 지정된 카메라로부터의 DrawCall 빈도

<img width="890" height="645" alt="image" src="https://github.com/user-attachments/assets/a42fe19c-f2c1-47b0-b105-48ee696c9ad8" />



### Poiyomi Shader Converter
1.  Runtime/Editor Mode 둘 다 가능한 Shader Converter 2
2.  Poiyomi Oldversion에서 현재 깔려있는 버전의 pro 버전으로 업그레이드 할 때, 누락 되는 부분 수정
   - _ClippingMask -> _AlphaMask
   - _RGBMaskEnabled 유무 확인후, Texture 추가, agba masking세팅
   - RenderQueue 설정
   - Skine Outline ZOffset 설정
   - ColorSpace HSV로 설정

<img width="656" height="1196" alt="image" src="https://github.com/user-attachments/assets/2c1ac98b-b121-4d7b-968c-509ac802a653" />


<details>
<summary>Code</summary>
<div markdown="1">

      #if UNITY_EDITOR
      
      using System;
      using System.Collections.Generic;
      using System.Linq;
      using System.Text.RegularExpressions;
      using UnityEditor;
      using UnityEngine;
      
      [System.Serializable]
      public class RenderQueueRule
      {
          public int renderQueue = 2500;
          public List<string> keywords = new List<string>();
          public bool foldout = true;
      }
      
      public class PoiyomiShaderConverter : EditorWindow
      {
          private Vector2 _scrollPosition;
          private Vector2 _renderQueueScrollPosition;
          private List<Material> _foundMaterials = new List<Material>();
          private bool _showMaterialList = false;
          private string _searchFilter = "";
          private bool _autoFixRGBMask = true;
          private bool _autoFixAlphaMask = true;
          private bool _autoFixRenderQueue = true;
          private bool _autoFixSkinOutline = true;
          private bool _autoColorSpaceToHSV = true;
      
          // RenderQueue 규칙 리스트
          private List<RenderQueueRule> _renderQueueRules = new List<RenderQueueRule>();
          private bool _showRenderQueueSettings = false;
      
          [MenuItem("Window/Poiyomi Shader Converter", priority = 200)]
          public static void ShowWindow()
          {
              PoiyomiShaderConverter window = GetWindow<PoiyomiShaderConverter>("Poiyomi Shader Converter");
              window.minSize = new Vector2(400, 800);
              window.Show();
          }
      
          private void OnEnable()
          {
              LoadRenderQueueRules();
          }
      
          private void LoadRenderQueueRules()
          {
              // 기본값 설정
              if (_renderQueueRules.Count == 0)
              {
                  _renderQueueRules.Add(new RenderQueueRule
                  {
                      renderQueue = 2500,
                      keywords = new List<string> { "hair", "highlight_poi", "iris" }
                  });
                  _renderQueueRules.Add(new RenderQueueRule
                  {
                      renderQueue = 3000,
                      keywords = new List<string> { "head_emotion", "head_effect" }
                  });
              }
          }
      
          private void OnGUI()
          {
              EditorGUILayout.Space(10);
      
              // 헤더
              EditorGUILayout.LabelField("Poiyomi Shader Converter", EditorStyles.boldLabel);
              EditorGUILayout.LabelField("8.x/7.x → 9.3 버전 변환 도구", EditorStyles.miniLabel);
              EditorGUILayout.Space(10);
      
              EditorGUILayout.HelpBox("이 도구는 Poiyomi 8.1, 8.0, 7.3 버전 셰이더를 9.3 버전으로 변환합니다.", MessageType.Info);
              EditorGUILayout.Space(5);
      
              // 변환 옵션
              EditorGUILayout.LabelField("변환 옵션", EditorStyles.boldLabel);
              _autoFixRGBMask = EditorGUILayout.ToggleLeft("RGB Mask 자동 설정", _autoFixRGBMask);
              _autoFixAlphaMask = EditorGUILayout.ToggleLeft("Alpha Mask 자동 설정", _autoFixAlphaMask);
              _autoFixRenderQueue = EditorGUILayout.ToggleLeft("RenderQueue 자동 설정", _autoFixRenderQueue);
              _autoFixSkinOutline = EditorGUILayout.ToggleLeft("Skin Outline ZOffset 자동 설정", _autoFixSkinOutline);
              _autoColorSpaceToHSV = EditorGUILayout.ToggleLeft("ColorSpace를 HSV로 자동 설정", _autoColorSpaceToHSV);
      
              // RenderQueue 설정
              if (_autoFixRenderQueue)
              {
                  EditorGUI.indentLevel++;
                  _showRenderQueueSettings = EditorGUILayout.Foldout(_showRenderQueueSettings, "RenderQueue 규칙 설정", true);
      
                  if (_showRenderQueueSettings)
                  {
                      EditorGUILayout.BeginVertical("box");
                      _renderQueueScrollPosition = EditorGUILayout.BeginScrollView(_renderQueueScrollPosition, GUILayout.MaxHeight(500));
      
                      for (int i = 0; i < _renderQueueRules.Count; i++)
                      {
                          DrawRenderQueueRule(i);
                      }
      
                      EditorGUILayout.EndScrollView();
      
                      EditorGUILayout.Space(5);
                      EditorGUILayout.BeginHorizontal();
                      if (GUILayout.Button("+ 규칙 추가", GUILayout.Height(25)))
                      {
                          _renderQueueRules.Add(new RenderQueueRule
                          {
                              renderQueue = 2000,
                              keywords = new List<string> { "keyword" }
                          });
                      }
                      if (GUILayout.Button("기본값 복원", GUILayout.Height(25)))
                      {
                          _renderQueueRules.Clear();
                          LoadRenderQueueRules();
                      }
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.EndVertical();
                  }
                  EditorGUI.indentLevel--;
              }
      
              EditorGUILayout.Space(10);
      
              // 머티리얼 검색
              EditorGUILayout.LabelField("머티리얼 검색", EditorStyles.boldLabel);
      
              EditorGUILayout.BeginHorizontal();
              if (GUILayout.Button("프로젝트 전체 검색", GUILayout.Height(30)))
              {
                  _foundMaterials = FindMaterialsInProject();
                  _showMaterialList = true;
              }
              if (GUILayout.Button("선택한 오브젝트에서 검색", GUILayout.Height(30)))
              {
                  _foundMaterials = FindMaterialsInSelection();
                  _showMaterialList = true;
              }
              EditorGUILayout.EndHorizontal();
      
              EditorGUILayout.Space(10);
      
              // 검색 결과
              if (_showMaterialList)
              {
                  EditorGUILayout.LabelField($"검색 결과: {_foundMaterials.Count}개", EditorStyles.boldLabel);
      
                  // 검색 필터
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.LabelField("필터:", GUILayout.Width(40));
                  _searchFilter = EditorGUILayout.TextField(_searchFilter);
                  if (GUILayout.Button("×", GUILayout.Width(25)))
                  {
                      _searchFilter = "";
                      GUI.FocusControl(null);
                  }
                  EditorGUILayout.EndHorizontal();
      
                  EditorGUILayout.Space(5);
      
                  // 머티리얼 리스트
                  EditorGUILayout.BeginVertical("box");
                  _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
      
                  var filteredMaterials = string.IsNullOrEmpty(_searchFilter)
                      ? _foundMaterials
                      : _foundMaterials.Where(m => m.name.ToLower().Contains(_searchFilter.ToLower())).ToList();
      
                  foreach (var mat in filteredMaterials)
                  {
                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.ObjectField(mat, typeof(Material), false);
                      EditorGUILayout.LabelField(mat.shader.name, EditorStyles.miniLabel, GUILayout.Width(200));
                      EditorGUILayout.EndHorizontal();
                  }
      
                  EditorGUILayout.EndScrollView();
                  EditorGUILayout.EndVertical();
      
                  EditorGUILayout.Space(10);
      
                  // 변환 버튼
                  GUI.backgroundColor = Color.green;
                  if (GUILayout.Button($"변환 시작 ({filteredMaterials.Count}개)", GUILayout.Height(40)))
                  {
                      ConvertMaterials(filteredMaterials);
                  }
                  GUI.backgroundColor = Color.white;
              }
      
              EditorGUILayout.Space(10);
      
              // 하단 정보
              EditorGUILayout.HelpBox(
                  "변환 과정:\n" +
                  "1. 셰이더를 .poiyomi/Poiyomi Pro로 변경\n" +
                  "2. Alpha Mask 자동 설정 (옵션)\n" +
                  "3. RGB Mask 채널 자동 활성화 (옵션)\n" +
                  "4. RenderQueue 설정 (옵션)\n" +
                  "5. Skin Outline ZOffset 설정 (옵션)\n" +
                  "6. ColorSpace를 HSV로 설정 (옵션)",
                  MessageType.None);
          }
      
          private void DrawRenderQueueRule(int index)
          {
              RenderQueueRule rule = _renderQueueRules[index];
      
              EditorGUILayout.BeginVertical("box");
      
              EditorGUILayout.BeginHorizontal();
              rule.foldout = EditorGUILayout.Foldout(rule.foldout, $"RenderQueue: {rule.renderQueue}", true);
      
              GUI.backgroundColor = Color.red;
              if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(18)))
              {
                  _renderQueueRules.RemoveAt(index);
                  GUI.backgroundColor = Color.white;
                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.EndVertical();
                  return;
              }
              GUI.backgroundColor = Color.white;
              EditorGUILayout.EndHorizontal();
      
              if (rule.foldout)
              {
                  EditorGUI.indentLevel++;
      
                  // RenderQueue 값 설정
                  rule.renderQueue = EditorGUILayout.IntField("RenderQueue 값", rule.renderQueue);
      
                  EditorGUILayout.Space(3);
                  EditorGUILayout.LabelField("키워드 (머티리얼 이름에 포함)", EditorStyles.miniLabel);
      
                  // 키워드 리스트
                  for (int i = 0; i < rule.keywords.Count; i++)
                  {
                      EditorGUILayout.BeginHorizontal();
                      rule.keywords[i] = EditorGUILayout.TextField(rule.keywords[i]);
      
                      if (GUILayout.Button("−", GUILayout.Width(25)))
                      {
                          rule.keywords.RemoveAt(i);
                          break;
                      }
                      EditorGUILayout.EndHorizontal();
                  }
      
                  // 키워드 추가 버튼
                  if (GUILayout.Button("+ 키워드 추가", GUILayout.Height(20)))
                  {
                      rule.keywords.Add("");
                  }
      
                  EditorGUI.indentLevel--;
              }
      
              EditorGUILayout.EndVertical();
          }
      
          private void ConvertMaterials(List<Material> materialsToConvert)
          {
              if (materialsToConvert.Count == 0)
              {
                  EditorUtility.DisplayDialog("변환", "변환할 머티리얼이 없습니다.", "확인");
                  return;
              }
      
              if (!EditorUtility.DisplayDialog("일괄 변환",
                  $"{materialsToConvert.Count}개의 머티리얼을 9.3 버전으로 변환하시겠습니까?",
                  "예", "아니오"))
              {
                  return;
              }
      
              int successCount = 0;
              int failCount = 0;
              int renderQueueChangedCount = 0;
              int rgbMaskCount = 0;
              int alphaMaskCount = 0;
              int skinOutlineCount = 0;
              int colorSpaceCount = 0;
      
              AssetDatabase.StartAssetEditing();
      
              try
              {
                  for (int i = 0; i < materialsToConvert.Count; i++)
                  {
                      Material mat = materialsToConvert[i];
      
                      if (EditorUtility.DisplayCancelableProgressBar(
                          "머티리얼 변환 중",
                          $"{mat.name} ({i + 1}/{materialsToConvert.Count})",
                          (float)i / materialsToConvert.Count))
                      {
                          break;
                      }
      
                      try
                      {
                          // 락 해제
                          bool wasLocked = Thry.ThryEditor.ShaderOptimizer.IsMaterialLocked(mat);
                          if (wasLocked)
                          {
                              Thry.ThryEditor.ShaderOptimizer.UnlockMaterials(new[] { mat }, Thry.ThryEditor.ShaderOptimizer.ProgressBar.None);
                          }
      
                          // === 1단계: 셰이더 변경 ===
                          Shader newShader = Shader.Find(".poiyomi/Poiyomi Pro");
                          if (newShader == null)
                          {
                              Debug.LogError($"9.3 셰이더를 찾을 수 없습니다: {mat.name}");
                              failCount++;
                              continue;
                          }
                          mat.shader = newShader;
      
                          // === 2단계: Alpha Mask 자동 설정 ===
                          if (_autoFixAlphaMask)
                          {
                              if (mat.HasProperty("_ClippingMask") || mat.HasProperty("_AlphaMask"))
                              {
                                  Texture clippingMask = mat.GetTexture("_ClippingMask");
                                  if (clippingMask != null)
                                  {
                                      mat.SetTexture("_AlphaMask", clippingMask);
                                      alphaMaskCount++;
                                  }
                              }
                          }
      
                          // === 3단계: RGB Mask 자동 설정 ===
                          if (_autoFixRGBMask)
                          {
                              if (mat.HasProperty("_RGBMaskEnabled") && mat.GetFloat("_RGBMaskEnabled") > 0.5f)
                              {
                                  Texture rgbMask = mat.GetTexture("_RGBMask");
                                  if (rgbMask != null)
                                  {
                                      string textureName = rgbMask.name.ToLower();
                                      var match = Regex.Match(textureName, @"_([rgba]+)$");
      
                                      if (match.Success)
                                      {
                                          string channels = match.Groups[1].Value;
                                          Texture mainTex = mat.GetTexture("_MainTex");
      
                                          SetRGBMaskChannels(mat, channels, mainTex);
                                          rgbMaskCount++;
      
                                          Debug.Log($"RGB Mask 설정: {mat.name} - Channels: {channels}");
                                      }
                                  }
                              }
                          }
      
                          // === 4단계: RenderQueue 설정 (동적 규칙) ===
                          if (_autoFixRenderQueue)
                          {
                              string matNameLower = mat.name.ToLower();
      
                              foreach (var rule in _renderQueueRules)
                              {
                                  foreach (var keyword in rule.keywords)
                                  {
                                      if (!string.IsNullOrEmpty(keyword) && matNameLower.Contains(keyword.ToLower()))
                                      {
                                          mat.renderQueue = rule.renderQueue;
                                          renderQueueChangedCount++;
                                          Debug.Log($"RenderQueue 변경: {mat.name} -> {rule.renderQueue} (키워드: '{keyword}')");
                                          break;
                                      }
                                  }
                              }
                          }
      
                          // === 5단계: Skin Outline ZOffset 설정 ===
                          if (_autoFixSkinOutline)
                          {
                              string matNameLower = mat.name.ToLower();
                              if (matNameLower.Contains("skin"))
                              {
                                  // 아웃라인 활성화 확인
                                  bool outlineEnabled = false;
                                  if (mat.HasProperty("_OutlineMode"))
                                  {
                                      float outlineMode = mat.GetFloat("_OutlineMode");
                                      outlineEnabled = outlineMode > 0.5f;
                                  }
      
                                  if (outlineEnabled && mat.HasProperty("_OutlineZOffsetMaskStrength"))
                                  {
                                      mat.SetFloat("_OutlineZOffsetMaskStrength", 0.99f);
                                      skinOutlineCount++;
                                      Debug.Log($"Skin Outline ZOffset 설정: {mat.name} -> 0.99");
                                  }
                              }
                          }
      
                          // === 6단계: ColorSpace를 HSV로 설정 ===
                          if (_autoColorSpaceToHSV)
                          {
                              if (mat.HasProperty("_MainHueShiftColorSpace"))
                              {
                                  mat.SetFloat("_MainHueShiftColorSpace", 1f);
                                  colorSpaceCount++;
                                  Debug.Log($"ColorSpace 설정: {mat.name} -> HSV (1)");
                              }
                          }
      
                          EditorUtility.SetDirty(mat);
                          Thry.ShaderEditor.FixKeywords(new[] { mat });
      
                          // 락 복원
                          if (wasLocked)
                          {
                              Thry.ThryEditor.ShaderOptimizer.LockMaterials(new[] { mat }, Thry.ThryEditor.ShaderOptimizer.ProgressBar.None);
                          }
      
                          successCount++;
                      }
                      catch (Exception e)
                      {
                          Debug.LogError($"변환 실패: {mat.name}\n{e.Message}");
                          failCount++;
                      }
                  }
              }
              finally
              {
                  AssetDatabase.StopAssetEditing();
                  EditorUtility.ClearProgressBar();
                  AssetDatabase.SaveAssets();
                  AssetDatabase.Refresh();
              }
      
              string resultMessage = $"변환 완료!\n\n" +
                                     $"성공: {successCount}개\n" +
                                     $"실패: {failCount}개\n" +
                                     $"RGB Mask 설정: {rgbMaskCount}개\n" +
                                     $"Alpha Mask 설정: {alphaMaskCount}개\n" +
                                     $"RenderQueue 변경: {renderQueueChangedCount}개\n" +
                                     $"Skin Outline 설정: {skinOutlineCount}개\n" +
                                     $"ColorSpace HSV 설정: {colorSpaceCount}개";
      
              EditorUtility.DisplayDialog("변환 완료", resultMessage, "확인");
      
              // 검색 결과 업데이트
              _foundMaterials = FindMaterialsInProject();
          }
      
          private void SetRGBMaskChannels(Material mat, string channels, Texture mainTex)
          {
              const int REPLACE = 0;
      
              if (channels.Contains('r'))
              {
                  if (mat.HasProperty("_RGBARedEnable"))
                      mat.SetFloat("_RGBARedEnable", 1f);
                  if (mat.HasProperty("_RGBARedBlendType"))
                      mat.SetFloat("_RGBARedBlendType", REPLACE);
                  if (mainTex != null && mat.HasProperty("_RedTexture"))
                      mat.SetTexture("_RedTexture", mainTex);
              }
      
              if (channels.Contains('g'))
              {
                  if (mat.HasProperty("_RGBAGreenEnable"))
                      mat.SetFloat("_RGBAGreenEnable", 1f);
                  if (mat.HasProperty("_RGBAGreenBlendType"))
                      mat.SetFloat("_RGBAGreenBlendType", REPLACE);
                  if (mainTex != null && mat.HasProperty("_GreenTexture"))
                      mat.SetTexture("_GreenTexture", mainTex);
              }
      
              if (channels.Contains('b'))
              {
                  if (mat.HasProperty("_RGBABlueEnable"))
                      mat.SetFloat("_RGBABlueEnable", 1f);
                  if (mat.HasProperty("_RGBABlueBlendType"))
                      mat.SetFloat("_RGBABlueType", REPLACE);
                  if (mainTex != null && mat.HasProperty("_BlueTexture"))
                      mat.SetTexture("_BlueTexture", mainTex);
              }
      
              if (channels.Contains('a'))
              {
                  if (mat.HasProperty("_RGBAAlphaEnable"))
                      mat.SetFloat("_RGBAAlphaEnable", 1f);
                  if (mat.HasProperty("_RGBAAlphaBlendType"))
                      mat.SetFloat("_RGBAAlphaBlendType", REPLACE);
                  if (mainTex != null && mat.HasProperty("_AlphaTexture"))
                      mat.SetTexture("_AlphaTexture", mainTex);
              }
          }
      
          private List<Material> FindMaterialsInProject()
          {
              string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
              List<Material> materials = new List<Material>();
      
              foreach (string guid in materialGUIDs)
              {
                  string path = AssetDatabase.GUIDToAssetPath(guid);
                  Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
      
                  if (mat != null && mat.shader != null &&
                      (mat.shader.name.Contains("8.1") || mat.shader.name.Contains("8.0") || mat.shader.name.Contains("7.3")))
                  {
                      materials.Add(mat);
                  }
              }
      
              return materials;
          }
      
          private List<Material> FindMaterialsInSelection()
          {
              List<Material> materials = new List<Material>();
      
              // GameObject에서 찾기
              foreach (GameObject go in Selection.gameObjects)
              {
                  Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                  foreach (Renderer renderer in renderers)
                  {
                      foreach (Material mat in renderer.sharedMaterials)
                      {
                          if (mat != null && mat.shader != null &&
                              (mat.shader.name.Contains("8.1") || mat.shader.name.Contains("8.0") || mat.shader.name.Contains("7.3")))
                          {
                              if (!materials.Contains(mat))
                                  materials.Add(mat);
                          }
                      }
                  }
              }
      
              // Material 에셋에서 직접 찾기
              foreach (Material mat in Selection.GetFiltered<Material>(SelectionMode.Assets))
              {
                  if (mat != null && mat.shader != null &&
                      (mat.shader.name.Contains("8.1") || mat.shader.name.Contains("8.0") || mat.shader.name.Contains("7.3")))
                  {
                      if (!materials.Contains(mat))
                          materials.Add(mat);
                  }
              }
      
              return materials;
          }
      }
      
      #endif

</div>
</details>

### Addresable Checker
<img width="706" height="911" alt="image" src="https://github.com/user-attachments/assets/a4717bb3-9a1a-4ca2-9ea0-391a8929b113" />



1. Assets/Resources_moved 경로에 어드레서블 체크가 누락된 아이템을 찾고 제어할 수 있는 툴
2. 선택한 항목을 어드레서블로 재등록할 수 있으나, Reource폴더로 이동시켜서, 다시 어드레서블을 등록할 수 있게 함.
3. 체크가능한 목록
  - 라벨
  - 그룹
  - 미등록
