using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.Adaptive
{
    [Serializable]
    public class StoryChoice
    {
        public string choiceText;
        public string nextNodeId;
        public string consequenceDescription;
        public int difficultyModifier;
    }

    [Serializable]
    public class StoryNode
    {
        public string nodeId;
        public string title;
        public string narrativeText;
        public string characterSpeaker;
        public List<StoryChoice> choices;
        public string requiredMachineType;
        public bool isCheckpoint;
        public float estimatedDurationSeconds;
    }

    /// <summary>
    /// Branching story/narrative engine for temple exploration.
    /// </summary>
    public class StoryBranchManager : MonoBehaviour
    {
        private static StoryBranchManager instance;
        public static StoryBranchManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<StoryBranchManager>();
                    if (instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("StoryBranchManager");
                        instance = go.AddComponent<StoryBranchManager>();
                    }
                }
                return instance;
            }
            private set { instance = value; }
        }

        [Header("UI References")]
        [SerializeField] private Text narrativeText;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private AudioClip narrativeAudioClip;
        
        private Dictionary<string, StoryNode> storyNodes;
        private StoryNode currentNode;
        private List<string> visitedNodeIds = new List<string>();
        private List<string> decisionLog = new List<string>();

        public event Action<StoryNode> OnNodeLoaded;
        public event Action<StoryChoice> OnChoiceMade;

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                InitializeStory();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeStory()
        {
            storyNodes = new Dictionary<string, StoryNode>();
            
            // Level 1 "Temple Gates"
            storyNodes["node_1"] = new StoryNode {
                nodeId = "node_1",
                title = "The Gates",
                narrativeText = "You stand before the ancient temple gates. Strange mechanisms guard the entrance.",
                characterSpeaker = "Narrator",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Examine closer", nextNodeId = "node_2" }
                }
            };

            storyNodes["node_2"] = new StoryNode {
                nodeId = "node_2",
                title = "Observation",
                narrativeText = "Maya notices ancient inscriptions describing simple machines.",
                characterSpeaker = "Maya",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Fix the lever first", nextNodeId = "node_3" },
                    new StoryChoice { choiceText = "Examine the pulley system", nextNodeId = "node_4" }
                }
            };

            storyNodes["node_3"] = new StoryNode {
                nodeId = "node_3",
                title = "Lever Puzzle",
                narrativeText = "The lever is stuck. You need to apply force correctly.",
                characterSpeaker = "Senuk",
                requiredMachineType = "Lever",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Proceed", nextNodeId = "node_5" }
                }
            };

            storyNodes["node_4"] = new StoryNode {
                nodeId = "node_4",
                title = "Pulley Puzzle",
                narrativeText = "The pulley is unspooled. We must attach the rope.",
                characterSpeaker = "Maya",
                requiredMachineType = "Pulley",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Proceed", nextNodeId = "node_5" }
                }
            };

            storyNodes["node_5"] = new StoryNode {
                nodeId = "node_5",
                title = "Convergence",
                narrativeText = "The mechanisms groan. The gates begin to open.",
                characterSpeaker = "Narrator",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Push through quickly", nextNodeId = "node_6" },
                    new StoryChoice { choiceText = "Wait and observe", nextNodeId = "node_7" }
                }
            };

            storyNodes["node_6"] = new StoryNode {
                nodeId = "node_6",
                title = "Reckless Entry",
                narrativeText = "You rush in, narrowly avoiding a falling stone.",
                characterSpeaker = "Senuk",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Catch breath", nextNodeId = "node_8" }
                }
            };

            storyNodes["node_7"] = new StoryNode {
                nodeId = "node_7",
                title = "Careful Observation",
                narrativeText = "You wait for the dust to settle, spotting a safe path.",
                characterSpeaker = "Maya",
                choices = new List<StoryChoice> {
                    new StoryChoice { choiceText = "Enter slowly", nextNodeId = "node_8" }
                }
            };

            storyNodes["node_8"] = new StoryNode {
                nodeId = "node_8",
                title = "Level Complete",
                narrativeText = "The temple gates fully open.",
                characterSpeaker = "Narrator",
                isCheckpoint = true,
                choices = new List<StoryChoice>()
            };
        }

        public void LoadNode(string nodeId)
        {
            if (storyNodes.TryGetValue(nodeId, out StoryNode node))
            {
                currentNode = node;
                visitedNodeIds.Add(nodeId);
                SecretOfLostTemple.Research.ResearchProgress.SetString("StoryNode", nodeId);
                SecretOfLostTemple.Research.Research.Record("story_node", nodeId);

                if (narrativeText != null) narrativeText.text = node.narrativeText;
                if (speakerNameText != null) speakerNameText.text = node.characterSpeaker;
                
                UpdateChoicesUI();
                OnNodeLoaded?.Invoke(node);
            }
        }

        private void UpdateChoicesUI()
        {
            if (choicePanel == null || choiceButtons == null) return;

            choicePanel.SetActive(currentNode.choices != null && currentNode.choices.Count > 0);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (currentNode.choices != null && i < currentNode.choices.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    Text buttonText = choiceButtons[i].GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = currentNode.choices[i].choiceText;
                    }
                    
                    int choiceIndex = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => MakeChoice(choiceIndex));
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void MakeChoice(int choiceIndex)
        {
            if (currentNode != null && choiceIndex >= 0 && choiceIndex < currentNode.choices.Count)
            {
                StoryChoice choice = currentNode.choices[choiceIndex];
                string logEntry = $"[{DateTime.Now.ToString("o")}] Chose: {choice.choiceText}";
                decisionLog.Add(logEntry);
                SecretOfLostTemple.Research.Research.Record("story_choice", currentNode.nodeId + ":" + choiceIndex);
                
                OnChoiceMade?.Invoke(choice);
                LoadNode(choice.nextNodeId);
            }
        }

        public StoryNode GetCurrentNode()
        {
            return currentNode;
        }

        public void ReloadParticipant()
        {
            visitedNodeIds.Clear();
            decisionLog.Clear();
            currentNode = null;
            string saved = SecretOfLostTemple.Research.ResearchProgress.GetString("StoryNode", "");
            if (!string.IsNullOrEmpty(saved)) LoadNode(saved);
        }

        public List<string> GetStoryPath()
        {
            return new List<string>(visitedNodeIds);
        }

        public List<string> GetDecisionLog()
        {
            return new List<string>(decisionLog);
        }
    }
}
