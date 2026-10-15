using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SecretOfLostTemple.Research;
using Data = SecretOfLostTemple.Research.Research;

public class ResearchSetupUI : MonoBehaviour
{
    GameObject overlay, card;
    Font font;
    Text message;
    InputField code, protocol, score, denominator, source, version, reason, notes;
    string participant="", protocolName="lever-pilot-1", condition="passive", selectedStage="pretest", lastExport;
    bool adaptive=true, embedded=true, confirmed, focused=true, answering;
    float questionSeconds;
    static bool openPanelOnMap;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded-=OnScene; SceneManager.sceneLoaded+=OnScene;
        OnScene(SceneManager.GetActiveScene(),LoadSceneMode.Single);
    }
    static void OnScene(Scene scene,LoadSceneMode mode)
    {
        Data.Record("scene_enter",scene.name);
        if((scene.name=="HomeMapScene" || scene.name=="AuthScene" || scene.name=="MainARScene") && FindFirstObjectByType<ResearchSetupUI>()==null)
            new GameObject("ResearchSetupUI").AddComponent<ResearchSetupUI>();
        if(scene.name=="MainARScene")new GameObject("StudyRuntime").AddComponent<StudyRuntime>();
    }
    void Start()
    {
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=200;
        var scale=gameObject.AddComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=1;
        gameObject.AddComponent<GraphicRaycaster>();
        string scene=SceneManager.GetActiveScene().name;
        var launcher=MakeButton(scene=="AuthScene"?"Research (offline)":scene=="MainARScene"?"Return to research":"Researcher Panel",transform,0,0,380,65,
            ()=>{if(scene=="HomeMapScene")ShowSession();else {openPanelOnMap=true;SceneManager.LoadScene("HomeMapScene");}});
        var rect=(RectTransform)launcher.transform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(0,18);
        overlay=Box("StudyOverlay",transform,0,0,0,0);rect=(RectTransform)overlay.transform;
        rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        overlay.AddComponent<Image>().color=new Color(0,0,0,.92f);
        card=Box("StudyCard",overlay.transform,0,0,1460,950);card.AddComponent<Image>().color=new Color(.10f,.08f,.045f,1);
        overlay.SetActive(false);
        if (scene == "HomeMapScene" && openPanelOnMap) { openPanelOnMap=false; ShowSession(); }
        if (Data.DemoMode) MakeText(ResearchStore.DemoLabel,transform,0,490,1300,45,24);
    }
    void Update(){if(answering&&focused&&Data.Store.PendingTest!=null)questionSeconds+=Time.unscaledDeltaTime;}
    void OnApplicationFocus(bool value){focused=value;if(answering)Data.Record(value?"formal_focus_return":"formal_focus_lost");}
    void OnApplicationPause(bool value){focused=!value;}
    void Page(string title)
    {
        answering=false;overlay.SetActive(true);
        foreach(Transform child in card.transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        Label(title,0,405,1380,65,32);message=Label("",0,-315,1350,140,23);
        Button("Session",-560,-420,220,ShowSession);Button("Assessments",-280,-420,240,ShowAssessments);
        Button("Quick Demo Presets",0,-420,300,ShowDemoPresets);
        Button("Export / notes",280,-420,240,ShowExport);Button("Close",560,-420,220,()=>{answering=false;overlay.SetActive(false);});
        if(Data.DemoMode) Label(ResearchStore.DemoLabel,0,355,1300,35,22);
    }
    void Run(Action action){try{action();}catch(Exception e){if(message!=null)message.text=e.Message;answering=Data.Store.PendingTest!=null;Debug.LogWarning("Study action: "+e.Message);}}
    void Capture(){if(code!=null)participant=code.text;if(protocol!=null)protocolName=protocol.text;}
    void ResetServices()
    {
        SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance?.ReloadParticipant();
        SecretOfLostTemple.Adaptive.StoryBranchManager.Instance?.ReloadParticipant();
        SecretOfLostTemple.Adaptive.HintSystem.Instance?.ResetParticipant();
        SecretOfLostTemple.AR.CognitiveTelemetryLogger.Instance?.StartSession("");
    }
    void ShowSession()
    {
        if(Data.Store.Current != null) {
            var active=Data.Store.Current;participant=active.participant;protocolName=active.protocol;
            condition=active.condition;adaptive=active.adaptive;embedded=active.embeddedAssessment;
        }
        Page("Researcher Panel - participant and session");
        code=Field("Participant (P001)",participant,-360,295,550);protocol=Field("Protocol / version",protocolName,330,295,650);
        Button("Condition: "+condition,-430,185,400,()=>{Capture();condition=condition=="passive"?"interactive":"passive";ShowSession();});
        Button("Adaptation: "+(adaptive?"on":"off"),0,185,400,()=>{Capture();adaptive=!adaptive;ShowSession();});
        Button("Embedded quizzes: "+(embedded?"on":"off"),430,185,400,()=>{Capture();embedded=!embedded;ShowSession();});
        Button("New study",-430,95,400,()=>Run(()=>{Capture();Data.Store.BeginStudy(participant,protocolName,condition,adaptive,embedded);ResetServices();ShowSession();}));
        Button("Resume unfinished",0,95,400,()=>Run(()=>{Capture();Data.Store.Begin(participant,true);ResetServices();ShowSession();}));
        Button("Open follow-up visit",430,95,400,()=>Run(()=>{Capture();Data.Store.OpenFollowUp(participant);ResetServices();ShowSession();}));
        Button("Start / resume learning",-430,5,400,()=>Run(()=>{Data.Store.StartLearning();OpenLever(false);}));
        Button("Finish learning",0,5,400,()=>Run(()=>{Data.Store.CompleteLearning();ShowAssessments();}));
        Button("Review lever",430,5,400,()=>Run(()=>{
            if(Data.Store.Current==null||ResearchStore.LatestTest(Data.Store.Current,"posttest")==null||Data.Store.PendingTest!=null)throw new InvalidOperationException("Complete the post-test before review.");
            OpenLever(true);
        }));
        Button("End visit",-430,-85,400,()=>Run(()=>{Data.Store.End();ShowSession();}));
        Button("Practice (not recorded)",0,-85,400,()=>Run(()=>{Data.Store.Practice();ResetServices();ShowSession();}));
        Button("Refresh results",430,-85,400,ShowSession);
        var s = Data.Store.Current ?? Data.Store.Latest;
        Label(s == null ? "Practice: research recording is off." : "Participant: " + s.participant + " | " + s.condition + " | Stage: " + Data.Store.Stage +
            "\nSession: " + s.id + "\nSaved events: " + s.events.Count + " | Data stored locally",0,-185,1300,130,24);
        message.text="Assignment settings apply only to a NEW study. Resume keeps the original assignment.\nPilot content/rules need research-team review. Verify the device UTC clock.";
    }
    void ShowDemoPresets()
    {
        Page("Quick Demo Presets");
        Label("Demo 1: Kalish (Passive vs Interactive)",0,290,1300,45,27);
        Button("Kalish_Passive",-330,225,620,()=>LoadDemo("kalish_passive",true));
        Button("Kalish_Interactive",330,225,620,()=>LoadDemo("kalish_interactive",true));
        Label("Identical settings: adaptation OFF, quizzes OFF; synthetic baseline ready.",0,170,1300,40,21);
        Button("Demo 2: Abeywickrama (Adaptive + Quizzes)",0,100,1280,()=>LoadDemo("abeywickrama",true));
        Button("Demo 3: Pathirana (Assessment flow)",0,5,1280,()=>LoadDemo("pathirana",false));
        Button("Load [SYNTHETIC DEMO DATA] - 7-day retention",0,-90,1280,()=>LoadDemo("retention_example",false));
        Button("Restart current demo",-330,-185,620,()=>Run(()=>{
            if(!Data.DemoMode || Data.Store.Current==null)throw new InvalidOperationException("Select a demo first.");
            string preset=Data.Store.Current.demoPreset;Data.Store.LoadPresentationPreset(preset,true);ResetServices();ShowSession();
        }));
        Button("Return to participant data",330,-185,620,()=>{
            Data.UsePresentationData(false);ResetServices();participant="";protocolName="lever-pilot-1";ShowSession();
        });
        message.text="Presentation records use a separate dataset and exports. Real visits are preserved.\nPathirana flow starts with a real demo pretest; the separate seven-day example contains generated scores 2/6, 5/6, 4/6.\nRestart current demo resets its difficulty and creates a fresh recorded example.";
    }
    void LoadDemo(string preset,bool jump)
    {
        Run(()=>{
            Data.UsePresentationData(true);Data.Store.LoadPresentationPreset(preset);ResetServices();
            if(jump && string.IsNullOrEmpty(Data.Store.Current.learningCompleted)) {Data.Store.StartLearning();OpenLever(false);}
            else if(preset=="pathirana" && ResearchStore.LatestTest(Data.Store.Current,"pretest")==null) {Data.Store.StartTest("pretest");ShowQuestion();}
            else ShowSession();
        });
    }
    void OpenLever(bool review)
    {
        StudyPolicy.Review=review;PlayerPrefs.SetString("LostTemple_App_Mode","Quest");PlayerPrefs.SetString("Sandbox_Selected_Machine","Lever");PlayerPrefs.Save();
        SceneManager.LoadScene("MainARScene");
    }
    void ShowAssessments()
    {
        Page("Formal assessments - no teaching feedback during tests");Label(Data.FormalStatus,0,180,1350,315,26);
        string[] stages={"pretest","posttest","retention"};
        for(int i=0;i<stages.Length;i++){string stage=stages[i];float x=(i-1)*430;
            Button("In-app "+stage,x,-45,400,()=>Run(()=>{Data.Store.StartTest(stage);ShowQuestion();}));
            Button("Enter "+stage+" score",x,-140,400,()=>{selectedStage=stage;ShowExternal();});
        }
        message.text="In-app test: "+LeverTestBank.Version+". Same six items at each stage; teacher approval required.\nRetention opens after seven days. Partial tests resume at the next unanswered question.";
    }
    void ShowQuestion()
    {
        var a=Data.Store.PendingTest;if(a==null){ShowAssessments();message.text="Test saved. Thank you.";return;}
        Page(a.stage+" - question "+(a.answers.Count+1)+" / "+a.denominator);
        Data.Store.Record("formal_question_presented",a.id+":"+a.answers.Count);
        var q=LeverTestBank.Questions[a.answers.Count];Label(q.text,0,225,1300,220,32);
        for(int i=0;i<q.options.Length;i++){int choice=i;
            Button(q.options[i],0,75-i*95,1240,()=>Run(()=>{if(!answering)return;answering=false;Data.Store.AnswerFormal(choice,questionSeconds);ShowQuestion();}));
        }
        questionSeconds=0;answering=true;
        message.text="Choose one answer. Each answer is saved immediately; correctness is not shown.\nLeaving preserves answers. Time for the unanswered question restarts on return.";
    }
    void ShowExternal()
    {
        Page("Researcher entry - "+selectedStage);
        version=Field("Test version","",-340,285,600);source=Field("Source (paper / form ID)","",340,285,600);
        score=Field("Raw score","",-340,160,600);denominator=Field("Maximum score","",340,160,600);
        reason=Field("Correction reason (required if replacing a score)","",0,30,1280);confirmed=false;
        var confirm=Button("Administrator confirms source and score: NO",0,-95,1260,null);
        confirm.onClick.AddListener(()=>{confirmed=!confirmed;confirm.GetComponentInChildren<Text>().text="Administrator confirms source and score: "+(confirmed?"YES":"NO");});
        Button("Save append-only result",0,-190,1260,()=>Run(()=>{
            if(!int.TryParse(score.text,out int raw)||!int.TryParse(denominator.text,out int max))throw new ArgumentException("Use whole-number raw and maximum scores.");
            Data.Store.EnterExternalScore(selectedStage,version.text,source.text,raw,max,confirmed,reason.text);ShowAssessments();
        }));
        message.text="Enter immediately after administering: collection time is the current device UTC time.\nNo backdating. Corrections retain originals. Do not enter names or contact details.";
    }
    void ShowExport()
    {
        Page("Researcher observations and local export");notes=Field("Observation / instrument responses (anonymous only)","",0,245,1300);
        Button("Save observation",0,130,1260,()=>Run(()=>{
            if(Data.Store.Current==null||string.IsNullOrWhiteSpace(notes.text))throw new InvalidOperationException("Open a visit and enter an observation.");
            Data.Store.Record("researcher_observation",notes.text);notes.text="";message.text="Observation saved.";
        }));
        Button("Export all saved data (ZIP + CSV + JSON)",0,25,1260,()=>Run(()=>{
            string folder=Data.Store.Export();lastExport=folder+".zip";ZipFile.CreateFromDirectory(folder,lastExport);
            message.text="Saved: "+lastExport+"\nCopy the ZIP off the device after each collection day. Cloud sync is not used.";
        }));
        Button("Copy export path",-325,-85,610,()=>{GUIUtility.systemCopyBuffer=lastExport??"";message.text="Path copied.";});
        Button("Open export folder (desktop)",325,-85,610,()=>Run(()=>{
            if(Application.platform==RuntimePlatform.Android)throw new InvalidOperationException("Use USB file transfer or ADB. See the handover guide.");
            if(string.IsNullOrEmpty(lastExport))throw new InvalidOperationException("Export first.");
            Application.OpenURL(new Uri(Path.GetDirectoryName(lastExport)).AbsoluteUri);
        }));
        Label("Exports include sessions, protocols, tests, answers, attempts, raw events and full JSON.\nIncomplete tests stay incomplete; corrections are never erased.",0,-185,1300,130,25);
    }
    GameObject Box(string name,Transform parent,float x,float y,float w,float h)
    {
        var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(parent,false);var rt=(RectTransform)obj.transform;
        rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(w,h);return obj;
    }
    Text MakeText(string value,Transform parent,float x,float y,float w,float h,int size)
    {
        var t=Box("Text",parent,x,y,w,h).AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=new Color(1,.89f,.65f);
        t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;
    }
    Text Label(string v,float x,float y,float w,float h,int size)=>MakeText(v,card.transform,x,y,w,h,size);
    InputField Field(string label,string value,float x,float y,float width)
    {
        Label(label,x,y+42,width,38,22);var obj=Box(label,card.transform,x,y-10,width,65);obj.AddComponent<Image>().color=new Color(.25f,.21f,.14f);
        var input=obj.AddComponent<InputField>();input.textComponent=MakeText("",obj.transform,0,0,width-25,60,25);input.text=value;input.characterLimit=300;return input;
    }
    Button Button(string title,float x,float y,float width,UnityEngine.Events.UnityAction click)=>MakeButton(title,card.transform,x,y,width,65,click);
    Button MakeButton(string title,Transform parent,float x,float y,float w,float h,UnityEngine.Events.UnityAction click)
    {
        var obj=Box(title,parent,x,y,w,h);obj.AddComponent<Image>().color=new Color(.32f,.23f,.10f);var b=obj.AddComponent<Button>();
        if(click!=null)b.onClick.AddListener(click);MakeText(title,obj.transform,0,0,w,h,25);return b;
    }
}
