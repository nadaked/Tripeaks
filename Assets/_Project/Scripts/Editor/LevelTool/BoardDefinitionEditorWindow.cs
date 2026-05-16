using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Scripts.Application.LevelData;
using _Project.Scripts.Core.Cards;
using _Project.Scripts.Presentation.Views.Board;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Project.Scripts.Editor.LevelTool
{
    public sealed class BoardDefinitionEditorWindow : EditorWindow
    {
        private const string DefaultFolder = "Assets/_Project/Data/Boards";

        private BoardDefinition _boardDefinition;
        private Transform _boardRoot;
        private BoardCardView _cardPrefab;
        private int _bonusValue = 3;
        private BoardCardAuthoring _blockedCard;
        private readonly List<BoardCardAuthoring> _blockingCards = new();
        private Vector2 _scrollPosition;
        private bool _showAdvanced;
        private bool _showDuplicatePatterns = true;
        private bool _showDaisyPattern;
        private bool _showStackPattern;
        private bool _showFanPattern;
        private bool _showPyramidPattern;
        private bool _showTutorialArcPattern;
        private float _duplicateOffsetX = 2f;
        private float _mirrorDistanceX = 6f;
        private Vector2 _daisyCardSize = new(7.29589939f, 10.2501965f);
        private int _daisyPetalCount = 8;
        private float _daisyRadius = 4.6f;
        private bool _daisyRotatePetals = true;
        private bool _daisyCreateBlockers = true;
        private int _stackCardCount = 6;
        private float _stackStepY = 0.7f;
        private bool _stackCreateBlockers = true;
        private float _fanOffsetX = 2.6f;
        private float _fanOffsetY = -0.25f;
        private float _fanRotation = 12f;
        private bool _fanCreateBlockers = true;
        private int _pyramidBottomCount = 4;
        private float _pyramidStepX = 2.8f;
        private float _pyramidStepY = 2.25f;
        private int _pyramidRows = 3;
        private bool _pyramidCreateBlockers = true;
        private int _tutorialArcCardCount = 5;
        private int _tutorialArcBottomCardCount = 5;
        private float _tutorialArcStepX = 2.45f;
        private float _tutorialArcRowOffsetX = 1.225f;
        private float _tutorialArcRowGapY = 2.55f;
        private float _tutorialArcCurveY = 0.24f;
        private float _tutorialArcMaxRotation = 12f;
        private bool _tutorialArcCreateBlockers = true;
        private bool _tutorialArcSetOpeningDeckCard = true;

        [MenuItem("Tripeaks/Board Builder")]
        public static void Open()
        {
            GetWindow<BoardDefinitionEditorWindow>("Board Builder");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Board Builder", EditorStyles.boldLabel);
            DrawSetup();

            EditorGUILayout.Space(8f);
            DrawAddCards();

            EditorGUILayout.Space(8f);
            DrawBlockerTools();

            EditorGUILayout.Space(8f);
            DrawPatternTools();

            EditorGUILayout.Space(8f);
            DrawSave();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSetup()
        {
            EditorGUILayout.LabelField("1. Setup", EditorStyles.boldLabel);

            if (_boardDefinition == null)
            {
                if (GUILayout.Button("Create New Board", GUILayout.Height(34f)))
                    CreateNewAsset();
            }

            _boardDefinition = (BoardDefinition)EditorGUILayout.ObjectField("Board", _boardDefinition, typeof(BoardDefinition), false);
            _boardRoot = (Transform)EditorGUILayout.ObjectField("Card Parent", _boardRoot, typeof(Transform), true);
            _cardPrefab = (BoardCardView)EditorGUILayout.ObjectField("Card Prefab", _cardPrefab, typeof(BoardCardView), false);

            DrawBoardAssetSettings();

            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced");
            if (_showAdvanced)
            {
                EditorGUILayout.HelpBox("Card Parent is the scene object that will contain designer-placed cards. Board is the asset saved from those scene cards.", MessageType.None);
            }
        }

        private void DrawBoardAssetSettings()
        {
            if (_boardDefinition == null)
                return;

            Undo.RecordObject(_boardDefinition, "Apply Tutorial Opening Cards");

            var serializedBoard = new SerializedObject(_boardDefinition);
            serializedBoard.Update();

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(serializedBoard.FindProperty("cardGenerationMode"), new GUIContent("Board Card Values"));
            EditorGUILayout.PropertyField(serializedBoard.FindProperty("randomSeed"), new GUIContent("Seed"));
            EditorGUILayout.PropertyField(serializedBoard.FindProperty("deckGenerationMode"), new GUIContent("Deck Mode"));
            EditorGUILayout.PropertyField(serializedBoard.FindProperty("initialDeckCardCount"), new GUIContent("Initial Deck Count"));
            EditorGUILayout.PropertyField(serializedBoard.FindProperty("useOpeningDeckCard"), new GUIContent("Fixed First Deck Card"));

            if (serializedBoard.FindProperty("useOpeningDeckCard").boolValue)
                EditorGUILayout.PropertyField(serializedBoard.FindProperty("openingDeckCard"), new GUIContent("First Deck Card"), true);

            serializedBoard.ApplyModifiedProperties();
        }

        private void DrawAddCards()
        {
            EditorGUILayout.LabelField("2. Add Cards", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_boardRoot == null || _cardPrefab == null))
            {
                if (GUILayout.Button("Add Closed Normal Card", GUILayout.Height(40f)))
                    AddCard(new SerializableCardData { type = CardType.Normal }, "Normal");

                EditorGUILayout.BeginHorizontal();
                _bonusValue = Mathf.Max(1, EditorGUILayout.IntField("+ Card Value", _bonusValue));

                if (GUILayout.Button($"Add +{_bonusValue} Card", GUILayout.Height(32f)))
                    AddCard(new SerializableCardData { type = CardType.AddDeckCards, value = _bonusValue }, $"AddDeck_{_bonusValue}");
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Add Wild Card", GUILayout.Height(34f)))
                    AddCard(new SerializableCardData { type = CardType.Wild }, "Wild");
            }

            EditorGUILayout.HelpBox("After adding a card, move it directly in the Scene view.", MessageType.None);
        }

        private void DrawBlockerTools()
        {
            EditorGUILayout.LabelField("3. Blockers", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_boardRoot == null))
            {
                if (GUILayout.Button("A) Selected Card Is Blocked", GUILayout.Height(34f)))
                    UseSelectedAsBlockedCard();

                EditorGUILayout.LabelField("Blocked:", _blockedCard == null ? "None" : _blockedCard.name);

                if (GUILayout.Button("B) Selected Cards Block It", GUILayout.Height(34f)))
                {
                    _blockingCards.Clear();
                    AddSelectedAsBlockers();
                }

                EditorGUILayout.LabelField("Blockers:", _blockingCards.Count == 0
                    ? "None"
                    : string.Join(", ", _blockingCards.Where(card => card != null).Select(card => card.name)));

                using (new EditorGUI.DisabledScope(_blockedCard == null))
                {
                    if (GUILayout.Button("C) Save Blockers On Card", GUILayout.Height(40f)))
                        ApplyBlockers();
                }

                if (GUILayout.Button("Clear Blocker Setup"))
                {
                    _blockedCard = null;
                    _blockingCards.Clear();
                }
            }

            EditorGUILayout.HelpBox("Simple flow: select the lower/locked card and press A. Select the upper blocking cards and press B. Press C. This only saves blocker links on the scene card.", MessageType.None);
        }

        private void DrawSave()
        {
            EditorGUILayout.LabelField("5. Generate Board Asset", EditorStyles.boldLabel);

            var foundCards = GetAuthoringCards();
            EditorGUILayout.LabelField("Scene Cards Found:", foundCards.Length.ToString());

            using (new EditorGUI.DisabledScope(_boardDefinition == null || _boardRoot == null))
            {
                if (GUILayout.Button("Fix Scene Draw Order", GUILayout.Height(34f)))
                    ApplySceneDrawOrder(foundCards);

                if (GUILayout.Button("Generate SO From Scene Cards", GUILayout.Height(44f)))
                    SaveBoardFromScene();
            }

            EditorGUILayout.HelpBox("Fix draw order makes upper cards render over lower cards in Game View. Generate writes the card list into the selected Board asset.", MessageType.None);
        }

        private void DrawPatternTools()
        {
            EditorGUILayout.LabelField("4. Pattern Tools", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_boardRoot == null))
            {
                DrawDuplicatePatternFoldout();
                DrawDaisyPatternFoldout();
                DrawStackPatternFoldout();
                DrawFanPatternFoldout();
                DrawPyramidPatternFoldout();
                DrawTutorialArcPatternFoldout();
            }

            EditorGUILayout.HelpBox("Select all cards in a pattern, then duplicate or mirror-copy them. Pattern buttons use the selected card position, or the last card position, as their anchor.", MessageType.None);
        }

        private void DrawDuplicatePatternFoldout()
        {
            _showDuplicatePatterns = EditorGUILayout.Foldout(_showDuplicatePatterns, "Duplicate / Mirror", true);
            if (!_showDuplicatePatterns)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            _duplicateOffsetX = EditorGUILayout.FloatField("Duplicate X Offset", _duplicateOffsetX);
            if (GUILayout.Button("Duplicate Selected", GUILayout.Height(32f)))
                DuplicateSelectedPattern(new Vector3(_duplicateOffsetX, 0f, 0f), false);
            EditorGUILayout.EndHorizontal();

            _mirrorDistanceX = Mathf.Max(0f, EditorGUILayout.FloatField("Mirror Center Distance", _mirrorDistanceX));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Mirror Copy Left", GUILayout.Height(34f)))
                MirrorSelectedPattern(-1);

            if (GUILayout.Button("Mirror Copy Right", GUILayout.Height(34f)))
                MirrorSelectedPattern(1);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private void DrawDaisyPatternFoldout()
        {
            _showDaisyPattern = EditorGUILayout.Foldout(_showDaisyPattern, "Daisy Pattern", true);
            if (!_showDaisyPattern)
                return;

            EditorGUI.indentLevel++;
            _daisyCardSize = EditorGUILayout.Vector2Field("Card Collider Size", _daisyCardSize);
            _daisyPetalCount = Mathf.Max(3, EditorGUILayout.IntField("Petal Count", _daisyPetalCount));
            EditorGUILayout.BeginHorizontal();
            _daisyRadius = Mathf.Max(0f, EditorGUILayout.FloatField("Radius", _daisyRadius));
            if (GUILayout.Button("From Size", GUILayout.Width(86f)))
                _daisyRadius = CalculateDaisyRadius(_daisyCardSize);
            EditorGUILayout.EndHorizontal();
            _daisyRotatePetals = EditorGUILayout.Toggle("Rotate Petals Outward", _daisyRotatePetals);
            _daisyCreateBlockers = EditorGUILayout.Toggle("Create Blockers", _daisyCreateBlockers);

            if (GUILayout.Button("Add Daisy Pattern", GUILayout.Height(40f)))
                AddDaisyPattern();

            if (GUILayout.Button("Fix Selected Daisy Rotations", GUILayout.Height(30f)))
                FixSelectedDaisyRotations();
            EditorGUI.indentLevel--;
        }

        private void DrawStackPatternFoldout()
        {
            _showStackPattern = EditorGUILayout.Foldout(_showStackPattern, "Stack Pattern", true);
            if (!_showStackPattern)
                return;

            EditorGUI.indentLevel++;
            _stackCardCount = Mathf.Max(2, EditorGUILayout.IntField("Card Count", _stackCardCount));
            EditorGUILayout.BeginHorizontal();
            _stackStepY = Mathf.Max(0f, EditorGUILayout.FloatField("Visible Step Y", _stackStepY));
            if (GUILayout.Button("From Size", GUILayout.Width(86f)))
                _stackStepY = CalculateStackStep(_daisyCardSize);
            EditorGUILayout.EndHorizontal();
            _stackCreateBlockers = EditorGUILayout.Toggle("Create Blockers", _stackCreateBlockers);

            if (GUILayout.Button("Add Stack Pattern", GUILayout.Height(40f)))
                AddStackPattern();
            EditorGUI.indentLevel--;
        }

        private void DrawFanPatternFoldout()
        {
            _showFanPattern = EditorGUILayout.Foldout(_showFanPattern, "Fan Pattern", true);
            if (!_showFanPattern)
                return;

            EditorGUI.indentLevel++;
            _fanOffsetX = Mathf.Max(0f, EditorGUILayout.FloatField("Side Offset X", _fanOffsetX));
            _fanOffsetY = EditorGUILayout.FloatField("Side Offset Y", _fanOffsetY);
            _fanRotation = Mathf.Max(0f, EditorGUILayout.FloatField("Side Rotation", _fanRotation));
            _fanCreateBlockers = EditorGUILayout.Toggle("Create Blockers", _fanCreateBlockers);

            if (GUILayout.Button("Add 3 Card Fan Pattern", GUILayout.Height(40f)))
                AddFanPattern();
            EditorGUI.indentLevel--;
        }

        private void DrawPyramidPatternFoldout()
        {
            _showPyramidPattern = EditorGUILayout.Foldout(_showPyramidPattern, "Pyramid Pattern", true);
            if (!_showPyramidPattern)
                return;

            EditorGUI.indentLevel++;
            _pyramidBottomCount = Mathf.Max(2, EditorGUILayout.IntField("Bottom Count", _pyramidBottomCount));
            _pyramidRows = Mathf.Clamp(EditorGUILayout.IntField("Rows", _pyramidRows), 2, _pyramidBottomCount);
            EditorGUILayout.BeginHorizontal();
            _pyramidStepX = Mathf.Max(0f, EditorGUILayout.FloatField("Step X", _pyramidStepX));
            if (GUILayout.Button("From Size", GUILayout.Width(86f)))
                _pyramidStepX = CalculatePyramidStepX(_daisyCardSize);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _pyramidStepY = Mathf.Max(0f, EditorGUILayout.FloatField("Step Y", _pyramidStepY));
            if (GUILayout.Button("From Size", GUILayout.Width(86f)))
                _pyramidStepY = CalculatePyramidStepY(_daisyCardSize);
            EditorGUILayout.EndHorizontal();
            _pyramidCreateBlockers = EditorGUILayout.Toggle("Create Blockers", _pyramidCreateBlockers);

            if (GUILayout.Button("Add Pyramid Pattern", GUILayout.Height(40f)))
                AddPyramidPattern();
            EditorGUI.indentLevel--;
        }

        private void DrawTutorialArcPatternFoldout()
        {
            _showTutorialArcPattern = EditorGUILayout.Foldout(_showTutorialArcPattern, "Tutorial Arc Pattern", true);
            if (!_showTutorialArcPattern)
                return;

            EditorGUI.indentLevel++;
            _tutorialArcCardCount = Mathf.Max(2, EditorGUILayout.IntField("Top Card Count", _tutorialArcCardCount));
            _tutorialArcBottomCardCount = Mathf.Clamp(
                EditorGUILayout.IntField("Bottom Card Count", _tutorialArcBottomCardCount),
                1,
                _tutorialArcCardCount);
            EditorGUILayout.BeginHorizontal();
            _tutorialArcStepX = Mathf.Max(0f, EditorGUILayout.FloatField("Step X", _tutorialArcStepX));
            if (GUILayout.Button("From Size", GUILayout.Width(86f)))
                _tutorialArcStepX = CalculateArcStepX(_daisyCardSize);
            EditorGUILayout.EndHorizontal();
            _tutorialArcRowOffsetX = EditorGUILayout.FloatField("Bottom Offset X", _tutorialArcRowOffsetX);
            _tutorialArcRowGapY = Mathf.Max(0f, EditorGUILayout.FloatField("Row Gap Y", _tutorialArcRowGapY));
            _tutorialArcCurveY = EditorGUILayout.FloatField("Curve Y", _tutorialArcCurveY);
            _tutorialArcMaxRotation = Mathf.Max(0f, EditorGUILayout.FloatField("Max Rotation", _tutorialArcMaxRotation));
            _tutorialArcCreateBlockers = EditorGUILayout.Toggle("Create Blockers", _tutorialArcCreateBlockers);
            _tutorialArcSetOpeningDeckCard = EditorGUILayout.Toggle("Set Deck First Card 4", _tutorialArcSetOpeningDeckCard);

            if (GUILayout.Button("Add Tutorial Arc 5-9 / Hidden 10-A", GUILayout.Height(40f)))
                AddTutorialArcPattern();
            EditorGUI.indentLevel--;
        }

        private void CreateNewAsset()
        {
            Directory.CreateDirectory(DefaultFolder);

            var path = AssetDatabase.GenerateUniqueAssetPath($"{DefaultFolder}/BoardDefinition.asset");
            var asset = CreateInstance<BoardDefinition>();

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _boardDefinition = asset;
            Selection.activeObject = asset;
        }

        private void AddCard(SerializableCardData cardData, string cardName)
        {
            var authoring = CreateCardObject(cardData, cardName, GetNextCardPosition(), Quaternion.identity);
            if (authoring == null)
                return;

            Selection.activeGameObject = authoring.gameObject;
            AppendCardToDrawOrder(authoring);
        }

        private BoardCardAuthoring CreateCardObject(
            SerializableCardData cardData,
            string cardName,
            Vector3 localPosition,
            Quaternion localRotation,
            bool showNormalFaceUp = false)
        {
            var instance = (BoardCardView)PrefabUtility.InstantiatePrefab(_cardPrefab, _boardRoot);
            if (instance == null)
                instance = Instantiate(_cardPrefab, _boardRoot);

            Undo.RegisterCreatedObjectUndo(instance.gameObject, $"Add {cardName} Card");

            instance.name = $"BoardCard_{cardName}";
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = Vector3.one;

            if (cardData.type == CardType.Normal && !showNormalFaceUp)
                instance.ShowBack();
            else
                instance.ShowCard(cardData.ToCardData(), true);

            var authoring = instance.GetComponent<BoardCardAuthoring>();
            if (authoring == null)
                authoring = Undo.AddComponent<BoardCardAuthoring>(instance.gameObject);

            authoring.Card = cardData;
            authoring.Blockers = Array.Empty<BoardCardAuthoring>();

            EditorUtility.SetDirty(instance.gameObject);
            EditorSceneManager.MarkSceneDirty(instance.gameObject.scene);

            return authoring;
        }

        private void SaveBoardFromScene()
        {
            if (_boardDefinition == null || _boardRoot == null)
                return;

            var cards = GetAuthoringCards()
                .OrderBy(card => card.transform.GetSiblingIndex())
                .ToArray();

            if (cards.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Board Builder",
                    "No board cards were found under Card Parent. Add cards with this tool or choose the parent object that contains the cards.",
                    "OK");
                return;
            }

            var indexByCard = new Dictionary<BoardCardAuthoring, int>();
            for (var i = 0; i < cards.Length; i++)
                indexByCard[cards[i]] = i;

            var slots = new BoardSlotDefinition[cards.Length];

            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                var blockers = card.Blockers
                    .Where(blocker => blocker != null && indexByCard.ContainsKey(blocker))
                    .Select(blocker => indexByCard[blocker])
                    .Distinct()
                    .ToArray();

                slots[i] = new BoardSlotDefinition
                {
                    index = i,
                    localPosition = card.transform.localPosition,
                    localEulerAngles = card.transform.localEulerAngles,
                    sortingOrder = GetCardSortingOrder(card, i + 1),
                    card = card.Card,
                    blockedBy = blockers,
                    unlockAction = CreateUnlockAction(card.Card)
                };
            }

            Undo.RecordObject(_boardDefinition, "Save Board From Scene");
            _boardDefinition.SetSlots(slots);
            EditorUtility.SetDirty(_boardDefinition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Board Builder",
                $"Generated {slots.Length} board slots into {_boardDefinition.name}.",
                "OK");
        }

        private void UseSelectedAsBlockedCard()
        {
            var selected = Selection.activeGameObject == null
                ? null
                : Selection.activeGameObject.GetComponentInParent<BoardCardAuthoring>();

            if (selected == null)
            {
                EditorUtility.DisplayDialog("Board Builder", "Select one card in the scene first.", "OK");
                return;
            }

            _blockedCard = selected;

            foreach (var blocker in _blockedCard.Blockers.Where(card => card != null))
            {
                if (!_blockingCards.Contains(blocker))
                    _blockingCards.Add(blocker);
            }
        }

        private void AddSelectedAsBlockers()
        {
            var selectedBlockers = Selection.gameObjects
                .Select(go => go.GetComponentInParent<BoardCardAuthoring>())
                .Where(card => card != null && card != _blockedCard)
                .Distinct();

            foreach (var blocker in selectedBlockers)
            {
                if (!_blockingCards.Contains(blocker))
                    _blockingCards.Add(blocker);
            }
        }

        private void ApplyBlockers()
        {
            if (_blockedCard == null)
                return;

            var blockers = _blockingCards
                .Where(card => card != null && card != _blockedCard)
                .Distinct()
                .ToArray();

            Undo.RecordObject(_blockedCard, "Apply Card Blockers");
            _blockedCard.Blockers = blockers;
            EditorUtility.SetDirty(_blockedCard);
            EditorSceneManager.MarkSceneDirty(_blockedCard.gameObject.scene);
        }

        private void DuplicateSelectedPattern(Vector3 offset, bool mirrorX)
        {
            var selectedCards = GetSelectedAuthoringCards();
            if (selectedCards.Length == 0)
            {
                EditorUtility.DisplayDialog("Board Builder", "Select one or more board cards in the scene first.", "OK");
                return;
            }

            var centerX = selectedCards.Average(card => card.transform.localPosition.x);
            var map = new Dictionary<BoardCardAuthoring, BoardCardAuthoring>();
            var newObjects = new List<GameObject>();

            foreach (var source in selectedCards)
            {
                var copy = DuplicateCardObject(source);
                var sourcePosition = source.transform.localPosition;

                if (mirrorX)
                {
                    var localDeltaX = sourcePosition.x - centerX;
                    sourcePosition.x = centerX + offset.x - localDeltaX;
                }
                else
                {
                    sourcePosition += offset;
                }

                copy.transform.localPosition = sourcePosition;
                copy.transform.localRotation = source.transform.localRotation;
                if (mirrorX)
                {
                    var euler = copy.transform.localEulerAngles;
                    euler.z = NormalizeMirroredZRotation(euler.z);
                    copy.transform.localEulerAngles = euler;
                }
                copy.transform.localScale = source.transform.localScale;

                map[source] = copy;
                newObjects.Add(copy.gameObject);
            }

            foreach (var pair in map)
            {
                var source = pair.Key;
                var copy = pair.Value;
                copy.Blockers = source.Blockers
                    .Where(blocker => blocker != null && map.ContainsKey(blocker))
                    .Select(blocker => map[blocker])
                    .ToArray();
                EditorUtility.SetDirty(copy);
            }

            Selection.objects = newObjects.Cast<UnityEngine.Object>().ToArray();
            AppendCardsToDrawOrder(map.Values);
            EditorSceneManager.MarkSceneDirty(_boardRoot.gameObject.scene);
        }

        private void MirrorSelectedPattern(int direction)
        {
            DuplicateSelectedPattern(new Vector3(direction * _mirrorDistanceX, 0f, 0f), true);
        }

        private void AddDaisyPattern()
        {
            if (_boardRoot == null || _cardPrefab == null)
                return;

            var centerPosition = GetNextCardPosition();
            var normalCard = new SerializableCardData { type = CardType.Normal };
            var petals = new List<BoardCardAuthoring>(_daisyPetalCount);
            var newObjects = new List<GameObject>(_daisyPetalCount + 1);

            for (var i = 0; i < _daisyPetalCount; i++)
            {
                var angleDegrees = 360f * i / _daisyPetalCount;
                var angleRadians = angleDegrees * Mathf.Deg2Rad;
                var localOffset = new Vector3(
                    Mathf.Cos(angleRadians) * _daisyRadius,
                    Mathf.Sin(angleRadians) * _daisyRadius,
                    0f);
                var petalRotationZ = KeepCardUpright(angleDegrees - 90f);
                var rotation = _daisyRotatePetals
                    ? Quaternion.Euler(0f, 0f, petalRotationZ)
                    : Quaternion.identity;

                var petal = CreateCardObject(normalCard, $"Daisy_Petal_{i + 1:00}", centerPosition + localOffset, rotation);
                if (petal == null)
                    continue;

                petals.Add(petal);
                newObjects.Add(petal.gameObject);
            }

            var center = CreateCardObject(normalCard, "Daisy_Center", centerPosition, Quaternion.identity);
            if (center != null)
                newObjects.Add(center.gameObject);

            if (_daisyCreateBlockers && center != null)
                ApplyDaisyBlockers(petals, center);

            Selection.objects = newObjects.Cast<UnityEngine.Object>().ToArray();
            AppendCardsToDrawOrder(newObjects
                .Select(go => go.GetComponent<BoardCardAuthoring>())
                .Where(card => card != null));
            EditorSceneManager.MarkSceneDirty(_boardRoot.gameObject.scene);
        }

        private void FixSelectedDaisyRotations()
        {
            var selectedCards = GetSelectedAuthoringCards();
            if (selectedCards.Length == 0)
            {
                EditorUtility.DisplayDialog("Board Builder", "Select one or more daisy cards in the scene first.", "OK");
                return;
            }

            foreach (var card in selectedCards)
            {
                Undo.RecordObject(card.transform, "Fix Daisy Card Rotation");
                var euler = card.transform.localEulerAngles;
                euler.z = KeepCardUpright(euler.z);
                card.transform.localEulerAngles = euler;
                EditorUtility.SetDirty(card.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(selectedCards[0].gameObject.scene);
        }

        private static void ApplyDaisyBlockers(IReadOnlyList<BoardCardAuthoring> petals, BoardCardAuthoring center)
        {
            for (var i = 0; i < petals.Count; i++)
            {
                var blockers = new List<BoardCardAuthoring> { center };

                if (i + 1 < petals.Count)
                    blockers.Add(petals[i + 1]);

                Undo.RecordObject(petals[i], "Apply Daisy Blockers");
                petals[i].Blockers = blockers.ToArray();
                EditorUtility.SetDirty(petals[i]);
            }
        }

        private void AddStackPattern()
        {
            if (_boardRoot == null || _cardPrefab == null)
                return;

            var topPosition = GetNextCardPosition();
            var normalCard = new SerializableCardData { type = CardType.Normal };
            var cards = new BoardCardAuthoring[_stackCardCount];
            var newObjects = new List<GameObject>(_stackCardCount);

            for (var i = _stackCardCount - 1; i >= 0; i--)
            {
                var localPosition = topPosition + new Vector3(0f, -_stackStepY * i, 0f);
                var card = CreateCardObject(normalCard, $"Stack_{_stackCardCount - i:00}", localPosition, Quaternion.identity);
                if (card == null)
                    continue;

                cards[i] = card;
                newObjects.Add(card.gameObject);
            }

            if (_stackCreateBlockers)
                ApplyStackBlockers(cards);

            Selection.objects = newObjects.Cast<UnityEngine.Object>().ToArray();
            AppendCardsToDrawOrder(newObjects
                .Select(go => go.GetComponent<BoardCardAuthoring>())
                .Where(card => card != null));
            EditorSceneManager.MarkSceneDirty(_boardRoot.gameObject.scene);
        }

        private static void ApplyStackBlockers(IReadOnlyList<BoardCardAuthoring> cards)
        {
            for (var i = 1; i < cards.Count; i++)
            {
                if (cards[i] == null || cards[i - 1] == null)
                    continue;

                Undo.RecordObject(cards[i], "Apply Stack Blockers");
                cards[i].Blockers = new[] { cards[i - 1] };
                EditorUtility.SetDirty(cards[i]);
            }
        }

        private static float CalculateDaisyRadius(Vector2 cardSize)
        {
            var widthRadius = Mathf.Abs(cardSize.x) * 0.63f;
            var heightRadius = Mathf.Abs(cardSize.y) * 0.45f;
            return Mathf.Max(0.5f, Mathf.Min(widthRadius, heightRadius));
        }

        private static float CalculateStackStep(Vector2 cardSize)
        {
            return Mathf.Max(0.2f, Mathf.Abs(cardSize.y) * 0.07f);
        }

        private void AddFanPattern()
        {
            if (_boardRoot == null || _cardPrefab == null)
                return;

            var centerPosition = GetNextCardPosition();
            var normalCard = new SerializableCardData { type = CardType.Normal };
            var left = CreateCardObject(
                normalCard,
                "Fan_Left",
                centerPosition + new Vector3(-_fanOffsetX, _fanOffsetY, 0f),
                Quaternion.Euler(0f, 0f, _fanRotation));
            var right = CreateCardObject(
                normalCard,
                "Fan_Right",
                centerPosition + new Vector3(_fanOffsetX, _fanOffsetY, 0f),
                Quaternion.Euler(0f, 0f, -_fanRotation));
            var center = CreateCardObject(normalCard, "Fan_Center", centerPosition, Quaternion.identity);
            var cards = new[] { left, right, center }.Where(card => card != null).ToArray();

            if (_fanCreateBlockers && center != null)
            {
                ApplySingleBlocker(left, center);
                ApplySingleBlocker(right, center);
            }

            Selection.objects = cards.Select(card => card.gameObject).Cast<UnityEngine.Object>().ToArray();
            AppendCardsToDrawOrder(cards);
            EditorSceneManager.MarkSceneDirty(_boardRoot.gameObject.scene);
        }

        private void AddPyramidPattern()
        {
            if (_boardRoot == null || _cardPrefab == null)
                return;

            var anchor = GetNextCardPosition();
            var normalCard = new SerializableCardData { type = CardType.Normal };
            var rows = new List<List<BoardCardAuthoring>>(_pyramidRows);
            var allCards = new List<BoardCardAuthoring>();

            for (var row = 0; row < _pyramidRows; row++)
            {
                var count = _pyramidBottomCount - row;
                var rowCards = new List<BoardCardAuthoring>(count);
                var rowWidth = (count - 1) * _pyramidStepX;
                var y = row * _pyramidStepY;

                for (var column = 0; column < count; column++)
                {
                    var x = column * _pyramidStepX - rowWidth * 0.5f;
                    var card = CreateCardObject(
                        normalCard,
                        $"Pyramid_R{row + 1}_C{column + 1}",
                        anchor + new Vector3(x, y, 0f),
                        Quaternion.identity);
                    if (card == null)
                        continue;

                    rowCards.Add(card);
                    allCards.Add(card);
                }

                rows.Add(rowCards);
            }

            if (_pyramidCreateBlockers)
                ApplyPyramidBlockers(rows);

            Selection.objects = allCards.Select(card => card.gameObject).Cast<UnityEngine.Object>().ToArray();
            AppendCardsToDrawOrder(allCards);
            EditorSceneManager.MarkSceneDirty(_boardRoot.gameObject.scene);
        }

        private void AddTutorialArcPattern()
        {
            if (_boardRoot == null || _cardPrefab == null)
                return;

            var anchor = GetNextCardPosition();
            var topCards = new List<BoardCardAuthoring>(_tutorialArcCardCount);
            var bottomCards = new List<BoardCardAuthoring>(_tutorialArcBottomCardCount);
            var cards = new List<BoardCardAuthoring>(_tutorialArcCardCount + _tutorialArcBottomCardCount);

            AddTutorialArcRow(
                bottomCards,
                anchor + Vector3.right * _tutorialArcRowOffsetX,
                _tutorialArcBottomCardCount,
                CardRank.Ten,
                "Bottom",
                false);

            cards.AddRange(bottomCards);

            AddTutorialArcRow(
                topCards,
                anchor + Vector3.up * _tutorialArcRowGapY,
                _tutorialArcCardCount,
                CardRank.Five,
                "Top",
                true);

            cards.AddRange(topCards);

            if (_tutorialArcCreateBlockers)
                ApplyTutorialArcBlockers(topCards, bottomCards);

            if (_tutorialArcSetOpeningDeckCard)
                ApplyTutorialOpeningDeckSettings();

            Selection.objects = cards.Select(card => card.gameObject).Cast<UnityEngine.Object>().ToArray();
            AppendCardsToDrawOrder(cards);
            EditorSceneManager.MarkSceneDirty(_boardRoot.gameObject.scene);
        }

        private void AddTutorialArcRow(
            ICollection<BoardCardAuthoring> cards,
            Vector3 rowAnchor,
            int count,
            CardRank startRank,
            string rowName,
            bool showFaceUp)
        {
            var center = (count - 1) * 0.5f;

            for (var i = 0; i < count; i++)
            {
                var normalized = center <= 0f ? 0f : (i - center) / center;
                var rank = StepRank(startRank, i);
                var cardData = new SerializableCardData
                {
                    type = CardType.Normal,
                    rank = rank,
                    suit = GetTutorialSuit(i)
                };
                var position = rowAnchor + new Vector3(
                    (i - center) * _tutorialArcStepX,
                    -Mathf.Abs(normalized) * _tutorialArcCurveY,
                    0f);
                var rotation = Quaternion.Euler(0f, 0f, -normalized * _tutorialArcMaxRotation);
                var card = CreateCardObject(cardData, $"TutorialArc_{rowName}_{rank}", position, rotation, showFaceUp);

                if (card != null)
                    cards.Add(card);
            }
        }

        private static void ApplyTutorialArcBlockers(
            IReadOnlyList<BoardCardAuthoring> topCards,
            IReadOnlyList<BoardCardAuthoring> bottomCards)
        {
            for (var i = 0; i < bottomCards.Count; i++)
            {
                if (bottomCards[i] == null)
                    continue;

                var blockers = new List<BoardCardAuthoring>(2);

                if (i < topCards.Count && topCards[i] != null)
                    blockers.Add(topCards[i]);

                if (i + 1 < topCards.Count && topCards[i + 1] != null)
                    blockers.Add(topCards[i + 1]);

                if (blockers.Count == 0)
                    continue;

                Undo.RecordObject(bottomCards[i], "Apply Tutorial Arc Blockers");
                bottomCards[i].Blockers = blockers.ToArray();
                EditorUtility.SetDirty(bottomCards[i]);
            }
        }

        private static void ApplyPyramidBlockers(IReadOnlyList<List<BoardCardAuthoring>> rows)
        {
            for (var row = 0; row < rows.Count - 1; row++)
            {
                var lowerRow = rows[row];
                var upperRow = rows[row + 1];

                for (var column = 0; column < lowerRow.Count; column++)
                {
                    var blockers = new List<BoardCardAuthoring>(2);

                    if (column - 1 >= 0 && column - 1 < upperRow.Count)
                        blockers.Add(upperRow[column - 1]);

                    if (column < upperRow.Count)
                        blockers.Add(upperRow[column]);

                    if (blockers.Count == 0)
                        continue;

                    Undo.RecordObject(lowerRow[column], "Apply Pyramid Blockers");
                    lowerRow[column].Blockers = blockers.ToArray();
                    EditorUtility.SetDirty(lowerRow[column]);
                }
            }
        }

        private static void ApplySingleBlocker(BoardCardAuthoring blockedCard, BoardCardAuthoring blocker)
        {
            if (blockedCard == null || blocker == null)
                return;

            Undo.RecordObject(blockedCard, "Apply Pattern Blocker");
            blockedCard.Blockers = new[] { blocker };
            EditorUtility.SetDirty(blockedCard);
        }

        private static float CalculatePyramidStepX(Vector2 cardSize)
        {
            return Mathf.Max(0.5f, Mathf.Abs(cardSize.x) * 0.58f);
        }

        private static float CalculatePyramidStepY(Vector2 cardSize)
        {
            return Mathf.Max(0.5f, Mathf.Abs(cardSize.y) * 0.28f);
        }

        private static float CalculateArcStepX(Vector2 cardSize)
        {
            return Mathf.Max(0.5f, Mathf.Abs(cardSize.x) * 0.34f);
        }

        private static CardRank StepRank(CardRank rank, int delta)
        {
            var value = (int)rank + delta;

            while (value > (int)CardRank.King)
                value -= (int)CardRank.King;

            while (value < (int)CardRank.Ace)
                value += (int)CardRank.King;

            return (CardRank)value;
        }

        private static CardSuit GetTutorialSuit(int index)
        {
            return (index % 4) switch
            {
                0 => CardSuit.Hearts,
                1 => CardSuit.Clubs,
                2 => CardSuit.Diamonds,
                _ => CardSuit.Spades
            };
        }

        private void ApplyTutorialOpeningDeckSettings()
        {
            if (_boardDefinition == null)
                return;

            var serializedBoard = new SerializedObject(_boardDefinition);
            serializedBoard.Update();

            serializedBoard.FindProperty("cardGenerationMode").enumValueIndex = (int)BoardCardGenerationMode.Fixed;
            serializedBoard.FindProperty("useOpeningDeckCard").boolValue = true;

            var openingCard = serializedBoard.FindProperty("openingDeckCard");
            openingCard.FindPropertyRelative("type").enumValueIndex = (int)CardType.Normal;
            openingCard.FindPropertyRelative("rank").enumValueIndex = (int)CardRank.Four;
            openingCard.FindPropertyRelative("secondRank").enumValueIndex = (int)CardRank.None;
            openingCard.FindPropertyRelative("suit").enumValueIndex = (int)CardSuit.Spades;
            openingCard.FindPropertyRelative("value").intValue = 0;

            serializedBoard.ApplyModifiedProperties();
            EditorUtility.SetDirty(_boardDefinition);
            AssetDatabase.SaveAssets();
        }

        private BoardCardAuthoring DuplicateCardObject(BoardCardAuthoring source)
        {
            GameObject copyObject;
            var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(source.gameObject);
            if (prefabSource != null)
            {
                copyObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabSource, _boardRoot);
                if (copyObject == null)
                    copyObject = Instantiate(source.gameObject, _boardRoot);
            }
            else
            {
                copyObject = Instantiate(source.gameObject, _boardRoot);
            }

            Undo.RegisterCreatedObjectUndo(copyObject, $"Duplicate {source.name}");
            copyObject.name = $"{source.name}_Copy";

            var copy = copyObject.GetComponent<BoardCardAuthoring>();
            if (copy == null)
                copy = Undo.AddComponent<BoardCardAuthoring>(copyObject);

            copy.Card = source.Card;
            copy.Blockers = Array.Empty<BoardCardAuthoring>();

            var view = copyObject.GetComponent<BoardCardView>();
            if (view != null)
            {
                if (copy.Card.type == CardType.Normal)
                    view.ShowBack();
                else
                    view.ShowCard(copy.Card.ToCardData(), true);
            }

            EditorUtility.SetDirty(copyObject);
            return copy;
        }

        private static float NormalizeMirroredZRotation(float zRotation)
        {
            return -NormalizeSignedZRotation(zRotation);
        }

        private static float KeepCardUpright(float zRotation)
        {
            var normalized = NormalizeSignedZRotation(zRotation);

            if (normalized > 90f)
                normalized -= 180f;
            else if (normalized < -90f)
                normalized += 180f;

            return normalized;
        }

        private static float NormalizeSignedZRotation(float zRotation)
        {
            zRotation %= 360f;

            if (zRotation > 180f)
                zRotation -= 360f;
            else if (zRotation < -180f)
                zRotation += 360f;

            return zRotation;
        }

        private static void ApplySceneDrawOrder(IEnumerable<BoardCardAuthoring> cards)
        {
            var orderedCards = OrderCardsByBlockers(cards);

            for (var i = 0; i < orderedCards.Length; i++)
            {
                var card = orderedCards[i];
                var view = card.BoardCardView != null
                    ? card.BoardCardView
                    : card.GetComponent<BoardCardView>();

                if (view != null)
                    view.SetSortingOrder(i + 1);

                card.transform.SetSiblingIndex(i);
                EditorUtility.SetDirty(card.gameObject);
            }
        }

        private static BoardCardAuthoring[] OrderCardsByBlockers(IEnumerable<BoardCardAuthoring> cards)
        {
            var allCards = cards
                .Where(card => card != null)
                .Distinct()
                .ToArray();

            var result = new List<BoardCardAuthoring>();
            var visited = new HashSet<BoardCardAuthoring>();
            var visiting = new HashSet<BoardCardAuthoring>();

            foreach (var card in allCards
                         .OrderBy(card => card.transform.localPosition.y)
                         .ThenBy(card => card.transform.localPosition.x)
                         .ThenBy(card => card.transform.GetSiblingIndex()))
            {
                VisitBlockedCardFirst(card, allCards, visited, visiting, result);
            }

            return result.ToArray();
        }

        private static void VisitBlockedCardFirst(
            BoardCardAuthoring card,
            BoardCardAuthoring[] allCards,
            HashSet<BoardCardAuthoring> visited,
            HashSet<BoardCardAuthoring> visiting,
            List<BoardCardAuthoring> result)
        {
            if (visited.Contains(card))
                return;

            if (!visiting.Add(card))
                return;

            var cardsBlockedByThisCard = allCards
                .Where(other => other != null &&
                                other != card &&
                                other.Blockers != null &&
                                other.Blockers.Contains(card))
                .OrderBy(other => other.transform.localPosition.y)
                .ThenBy(other => other.transform.localPosition.x)
                .ThenBy(other => other.transform.GetSiblingIndex());

            foreach (var blockedCard in cardsBlockedByThisCard)
                VisitBlockedCardFirst(blockedCard, allCards, visited, visiting, result);

            visited.Add(card);
            result.Add(card);

            visiting.Remove(card);
        }

        private static int GetCardSortingOrder(BoardCardAuthoring card, int fallback)
        {
            var renderer = card == null
                ? null
                : card.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault();

            return renderer == null ? fallback : renderer.sortingOrder;
        }

        private void AppendCardToDrawOrder(BoardCardAuthoring card)
        {
            if (card == null)
                return;

            var maxSortingOrder = GetMaxSortingOrder(GetAuthoringCards().Where(other => other != card));

            var view = card.BoardCardView != null
                ? card.BoardCardView
                : card.GetComponent<BoardCardView>();

            if (view != null)
                view.SetSortingOrder(maxSortingOrder + 1);

            card.transform.SetAsLastSibling();
            EditorUtility.SetDirty(card.gameObject);
        }

        private void AppendCardsToDrawOrder(IEnumerable<BoardCardAuthoring> cards)
        {
            var cardsToAppend = cards
                .Where(card => card != null)
                .OrderBy(card => card.transform.GetSiblingIndex())
                .ToArray();

            var maxSortingOrder = GetMaxSortingOrder(GetAuthoringCards().Except(cardsToAppend));

            for (var i = 0; i < cardsToAppend.Length; i++)
            {
                var card = cardsToAppend[i];
                var view = card.BoardCardView != null
                    ? card.BoardCardView
                    : card.GetComponent<BoardCardView>();

                if (view != null)
                    view.SetSortingOrder(maxSortingOrder + i + 1);

                card.transform.SetAsLastSibling();
                EditorUtility.SetDirty(card.gameObject);
            }
        }

        private static int GetMaxSortingOrder(IEnumerable<BoardCardAuthoring> cards)
        {
            return cards
                .Where(other => other != null)
                .SelectMany(other => other.GetComponentsInChildren<SpriteRenderer>(true))
                .Select(renderer => renderer.sortingOrder)
                .DefaultIfEmpty(0)
                .Max();
        }

        private static SerializableGameAction CreateUnlockAction(SerializableCardData card)
        {
            return card.type switch
            {
                CardType.AddDeckCards => new SerializableGameAction
                {
                    type = Core.Actions.GameActionType.AddDeckCards,
                    value = Math.Max(1, card.value)
                },
                CardType.Wild => new SerializableGameAction
                {
                    type = Core.Actions.GameActionType.AddWildToDeck,
                    value = 1
                },
                _ => default
            };
        }

        private void LoadBlockersFromBlockedCard()
        {
            _blockingCards.Clear();

            if (_blockedCard == null)
                return;

            _blockingCards.AddRange(_blockedCard.Blockers.Where(card => card != null));
        }

        private BoardCardAuthoring[] GetAuthoringCards()
        {
            return _boardRoot == null
                ? Array.Empty<BoardCardAuthoring>()
                : _boardRoot.GetComponentsInChildren<BoardCardAuthoring>(true);
        }

        private Vector3 GetNextCardPosition()
        {
            var selectedCard = Selection.activeGameObject == null
                ? null
                : Selection.activeGameObject.GetComponentInParent<BoardCardAuthoring>();

            if (selectedCard != null)
                return selectedCard.transform.localPosition;

            var lastCard = GetAuthoringCards()
                .Where(card => card != null)
                .OrderByDescending(card => card.transform.GetSiblingIndex())
                .FirstOrDefault();

            return lastCard == null ? Vector3.zero : lastCard.transform.localPosition;
        }

        private static BoardCardAuthoring[] GetSelectedAuthoringCards()
        {
            return Selection.gameObjects
                .Select(go => go.GetComponentInParent<BoardCardAuthoring>())
                .Where(card => card != null)
                .Distinct()
                .ToArray();
        }
    }
}
