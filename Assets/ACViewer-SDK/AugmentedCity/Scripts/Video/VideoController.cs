
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
    bool haveclip;
    int reasonStart, reasonStop;
    bool pausedByHand, startedPlaying, scaled;
    #region PRIVATE_MEMBERS

    private VideoPlayer videoPlayer;

    #endregion //PRIVATE_MEMBERS


    #region PUBLIC_MEMBERS

    public Button m_PlayButton;
    public RectTransform m_ProgressBar;
	public bool playOnAwake;

    bool videoPrepared;
   public bool playV;
    #endregion //PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        reasonStart = 0;
        reasonStop = 0;
        // Setup Delegates
        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.started += HandleStartedEvent;
        videoPlayer.prepareCompleted += HandlePrepareCompleted;
        videoPlayer.seekCompleted += HandleSeekCompleted;
        videoPlayer.loopPointReached += HandleLoopPointReached;
        PrepareVid();
        LogClipInfo();
    }
		
	public void PlayFirstTime(){
		if (playOnAwake) {
			Play ();
		}
	}

    void Update()
    {
        if (!haveclip) {
            LogClipInfo();
        }
        if (videoPlayer.isPlaying)
        {
            if (!scaled && (videoPlayer.isPrepared)) {
                float сoef = (float) videoPlayer.height / (float) videoPlayer.width;
                /*
                Debug.Log("videoPlayer.width = " + videoPlayer.width);
                Debug.Log("videoPlayer.height = " + videoPlayer.height);
                Debug.Log("koef = " + koef);
                */
                if (сoef > 0)
                {
                    scaled = true;
                    this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,
                                                                       this.gameObject.transform.localScale.y,
                                                                       this.gameObject.transform.localScale.z * сoef);
                }
            }
            ToggleButton(false);
            startedPlaying = true;
            if (videoPlayer.frameCount < float.MaxValue)
            {
                float frame = (float)videoPlayer.frame;
                float count = (float)videoPlayer.frameCount;

                float progressPercentage = 0;

                if (count > 0)
                    progressPercentage = (frame / count) * 100.0f;

				if (m_ProgressBar != null && !playOnAwake)
                    m_ProgressBar.sizeDelta = new Vector2((float)progressPercentage, m_ProgressBar.sizeDelta.y);
            }

        }
        else
        {
            if ((!pausedByHand)&&(startedPlaying))
            {
                reasonStop = 2;
                reasonStop = 0;
                startedPlaying = false;
            }
            m_PlayButton.enabled = true;
        }

        if (playV && videoPlayer.isPrepared)
        {
            Debug.Log("prepared");

            PlayFirstTime();
            playV = false;
        }
    }


    public void PrepareVid()
    {
        videoPlayer.Prepare();
        Debug.Log("preparing video");
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            reasonStop = 3;
            Pause();
        }
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS

    public void Play()
    {
        Debug.Log("Pressed 'Play'");
        pausedByHand = false;
        videoPlayer.Play();
        ToggleButton(false);
        reasonStart = 0;
    }

    public void Pause()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
            ToggleButton(true);
        }
        pausedByHand = true;
        reasonStop = 0;
    }

    public void SetReasonStartButton() {
        reasonStart = 1;
    }

    public void SetReasonStopButton()
    {
        reasonStop = 1;
    }

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS
    void OnBecameVisible()
    {
        Play();
    }

    void OnBecameInvisible()
    {
        Pause();
    }



    private void ToggleButton(bool enable)
    {
        m_PlayButton.enabled = enable;
        m_PlayButton.GetComponent<Image>().enabled = enable;
    }

    private void LogClipInfo()
    {
        if (videoPlayer.clip != null)
        {
            string stats =
                "\nName: " + videoPlayer.clip.name +
                "\nAudioTracks: " + videoPlayer.clip.audioTrackCount +
                "\nFrames: " + videoPlayer.clip.frameCount +
                "\nFPS: " + videoPlayer.clip.frameRate +
                "\nHeight: " + videoPlayer.clip.height +
                "\nWidth: " + videoPlayer.clip.width +
                "\nLength: " + videoPlayer.clip.length +
                "\nPath: " + videoPlayer.clip.originalPath;
            haveclip = true;

            Debug.Log(stats);
        }
        else
        {
        }
    }

    #endregion // PRIVATE_METHODS


    #region DELEGATES

    void HandleVideoError(VideoPlayer video, string errorMsg)
    {
        //Debug.LogError("Error: " + video.clip.name + "\nError Message: " + errorMsg);
    }

    void HandleStartedEvent(VideoPlayer video)
    {
        //Debug.Log("Started: " + video.clip.name);
    }

    void HandlePrepareCompleted(VideoPlayer video)
    {
        //Debug.Log("Prepare Completed: " + video.clip.name);
    }

    void HandleSeekCompleted(VideoPlayer video)
    {
        //Debug.Log("Seek Completed: " + video.clip.name);
    }

    void HandleLoopPointReached(VideoPlayer video)
    {
        //Debug.Log("Loop Point Reached: " + video.clip.name);
        ToggleButton(true);
    }

    #endregion //DELEGATES
}
