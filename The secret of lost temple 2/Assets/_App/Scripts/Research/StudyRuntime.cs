using UnityEngine;
using UnityEngine.SceneManagement;
using SecretOfLostTemple.AR;
using SecretOfLostTemple.Research;
using Data = SecretOfLostTemple.Research.Research;

public static class StudyPolicy
{
    public static bool Review, Tracking, SimulatedTracking;
    public static bool Measured => Data.Store.IsStudy;
    public static bool Passive => Measured && Data.Store.Current.condition=="passive";
    public static bool InputAllowed => Data.Store.CanManipulate && (!Measured||Tracking);
    public static InteractionMode Resolve(InteractionMode requested) => Measured ? (Passive?InteractionMode.ViewOnly:InteractionMode.Interactive) : requested;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset(){Review=false;Tracking=false;SimulatedTracking=false;}
}
public class StudyRuntime : MonoBehaviour
{
    float seconds;
    bool foreground=true;
    void Start()
    {
        StudyPolicy.Tracking=false;
        if(!StudyPolicy.Measured && !Data.DemoMode)
        {
            Data.Store.EnsureAutoParticipantSession("interactive");
        }
        if(!StudyPolicy.Measured)return;
        var s=Data.Store.Current;
        if(Data.Store.PendingTest!=null)
        {SceneManager.LoadScene("HomeMapScene");return;}
        if(string.IsNullOrEmpty(s.learningStarted))
        {
            s.learningStarted = System.DateTime.UtcNow.ToString("O");
        }
        Data.Record(StudyPolicy.Review?"review_start":"learning_scene_start","lever");
    }
    void Update()
    {
        if(!StudyPolicy.Measured||!foreground||!StudyPolicy.Tracking)return;
        if(!StudyPolicy.Review && Data.Store.Stage != "learning") return;
        seconds+=Time.unscaledDeltaTime;if(seconds>=10)Flush();
    }
    void Flush()
    {
        if(seconds<=0)return;
        Data.Store.Record(StudyPolicy.Review?"review_exposure":"learning_exposure",StudyPolicy.SimulatedTracking?"foreground_and_camera_locked":"foreground_and_marker_tracked",false,false,seconds);seconds=0;
    }
    public static void FlushActive()
    {
        var runtime = FindFirstObjectByType<StudyRuntime>();
        if (runtime != null) runtime.Flush();
    }
    void OnApplicationPause(bool paused){Flush();foreground=!paused;Data.Record(paused?"app_background":"app_foreground");}
    void OnApplicationFocus(bool focus){Flush();foreground=focus;}
    void OnDestroy(){Flush();Data.Record(StudyPolicy.Review?"review_end":"learning_scene_end","lever");StudyPolicy.Review=false;StudyPolicy.Tracking=false;}
}
