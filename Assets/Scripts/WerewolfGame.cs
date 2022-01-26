using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfGame : MonoBehaviour
{
    public enum TOD
    {
        Day,
        Night
    }

    public enum GameState
    {
        GeneratePopulation,         // We generate all of the population, ready for the game
        IntroStorySegment,          // Little story segment about a previous (out of game) murder

        // \/ Start of game loop \/
        TransitionToDay,            // Graphic wheel transition to day
        VictimFoundAnnouncement,    // Game announces a found victim in the morning (if any)
        PhaseGenerationDay,         // We generate the story data for the next day time phase
        ClueGenerationDay,          // We generate the clue data from the day phase generated
        PlayerInvestigateDay,       // Player is walking around investigating the town (daytime)
        
        TransitionToNight,          // Graphic wheel transition to night
        PhaseGenerationNight,       // We generate the story data for the next night time phase
        ClueGenerationNight,        // We generate the clue data from the night phase generated
        PlayerInvestigateNight,     // Player is walking around investigating the town (nighttime)

        PlayerInformationReview,    // Corkboard scene where the player can organise their clues and thoughts
        // /\ End of game loop /\

        PlayerChoseToStake,         // Scene change to the player having chosen to stake a towns-person
        GameSummary,                // End of game screen, with results on win/lose and statistics
    }

    public static GameState InvalidState => (GameState)(-1);

    public enum SubState
    {
        Start,
        Update,
        Finish
    }

    // public vars

#if UNITY_EDITOR
    public bool DisplayDebug = true;
#endif

    public GameState CurrentState = GameState.GeneratePopulation;
    private GameState NextState = InvalidState;

    public SubState CurrentSubState = SubState.Start;

    public TOD CurrentTimeOfDay = TOD.Day;
    public int CurrentDay = 0;
    public bool IsGamePaused = true;

    [SerializeField]
    public float TimeTransitionDuration = 5.0f;

    // Private vars

    private float fStateTimer = 0.0f;
    private bool canCurrentStateBeProgressed = false;

    public bool CanUpdatePopulation()
    {
        if(IsGamePaused)
        {
            return false;
        }

        if(CurrentState != GameState.PlayerInvestigateDay && CurrentState != GameState.PlayerInvestigateNight)
        {
            return false;
        }

        if(CurrentSubState != SubState.Update)
        {
            return false;
        }

        return true;
    }

    void Awake()
    {
        Service.Game = this;
    }

    // Start is called before the first frame update
    void Update()
    {
        #region DEBUG
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Service.PhaseSolve.bShowDebug = false;
            Service.Population.bShowDebug = false;
            DisplayDebug = !DisplayDebug;
        }

        if (DisplayDebug && Input.GetKeyDown(KeyCode.F5))
        {
            ProgressGameFromExternal();
        }
        if (DisplayDebug && Input.GetKeyDown(KeyCode.F6))
        {
            IsGamePaused = !IsGamePaused;
        }
#endif
        #endregion

        if (IsGamePaused)
        {
            return;
        }

        fStateTimer += Time.deltaTime;

        switch (CurrentState)
        {
            case GameState.GeneratePopulation:
                ProcessStateGeneratePopulation();
                break;
            case GameState.IntroStorySegment:
                ProcessStateIntroStorySegment();
                break;

            case GameState.TransitionToDay:
            case GameState.TransitionToNight:
                ProcessTimeTransitionState();
                break;

            case GameState.VictimFoundAnnouncement:
                ProcessVictimFoundAnnouncement();
                break;

            case GameState.PhaseGenerationDay:
            case GameState.PhaseGenerationNight:
                ProcessStatePhaseGeneration();
                break;

            case GameState.ClueGenerationDay:
            case GameState.ClueGenerationNight:
                ProcessStateClueGeneration();
                break;

            case GameState.PlayerInvestigateDay:
            case GameState.PlayerInvestigateNight:
                ProcessStatePlayerInvestigate();
                break;
            
            case GameState.PlayerInformationReview:
                ProcessStatePlayerInformationReview();
                break;

            case GameState.PlayerChoseToStake:
                ProcessStatePlayerChoseToStake();
                break;

            case GameState.GameSummary:
                ProcessStateGameSummary();
                break;
        }

        if(CurrentSubState == SubState.Finish)
        {
            // This means we're naturally progressing, rather than something external pushing us through
            if (NextState == InvalidState)
            {
                ProgressGame(true);
            }

            CurrentState = NextState;
            NextState = InvalidState;
        }
    }

    void ProcessStateGeneratePopulation()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    Service.Population.Init();
                    CurrentSubState++;
                    break;
                }
            case SubState.Update:
                {
                    if(Service.Population.CharacterCreationDone)
                    {
                        CurrentSubState++;
                    }
                    break;
                }
        }
    }

    void ProcessStateIntroStorySegment()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    CurrentSubState = SubState.Finish;
                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessTimeTransitionState()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    // If there is a victim to kill, try and do so
                    Service.PhaseSolve.TryKillOffCurrentVictim();

                    CurrentTimeOfDay = CurrentState == GameState.TransitionToDay ? TOD.Day : TOD.Night;

                    // (To day from night, increment the day counter)
                    if(CurrentState == GameState.TransitionToDay)
                    {
                        CurrentDay++;
                    }

                    CurrentSubState++;
                    break;
                }
            case SubState.Update:
                {
                    if(fStateTimer >= TimeTransitionDuration)
                    {
                        CurrentSubState++;

                        foreach (var c in Service.Population.ActiveCharacters)
                        {
                            c.OnTimeOfDayPhaseShift();
                        }
                    }
                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessVictimFoundAnnouncement()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    Character v = Service.PhaseSolve.GetVictimFromDay(CurrentDay - 1);
                    if (v != null)
                    {
                        // Setup announcement

                        CurrentSubState = SubState.Finish; // Don't actually finish when we have a way to announce
                    }
                    else
                    {
                        CurrentSubState = SubState.Finish;
                    }

                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessStatePhaseGeneration()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    if(Service.PhaseSolve.CanGeneratePhase())
                    {
                        Service.PhaseSolve.StartPhaseGeneration();
                        CurrentSubState++;
                    }
                    else
                    {
                        // TODO: Go to fail screen?
                    }
                    break;
                }
            case SubState.Update:
                {
                    if(!Service.PhaseSolve.IsGeneratingAPhase)
                    {
                        CurrentSubState++;
                    }
                    break;
                }
        }
    }

    void ProcessStateClueGeneration()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    CurrentSubState = SubState.Finish;
                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessStatePlayerInvestigate()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    canCurrentStateBeProgressed = true;
                    CurrentSubState++;
                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessStatePlayerInformationReview()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {
                    CurrentSubState = SubState.Finish;
                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessStatePlayerChoseToStake()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {

                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }
    }

    void ProcessStateGameSummary()
    {
        switch (CurrentSubState)
        {
            case SubState.Start:
                {

                    break;
                }
            case SubState.Update:
                {

                    break;
                }
            case SubState.Finish:
                {

                    break;
                }
        }

    }

    public void ProgressGameFromExternal()
    {
        ProgressGame();
    }
    void ProgressGame(bool bProgressFromFinish = false)
    {
        if (!bProgressFromFinish)
        {
            if (!canCurrentStateBeProgressed)
            {
                return;
            }
        }

        if (bProgressFromFinish)
        {
            switch (CurrentState)
            {
                case GameState.GeneratePopulation:
                case GameState.IntroStorySegment:
                case GameState.TransitionToDay:
                case GameState.VictimFoundAnnouncement:
                case GameState.PhaseGenerationDay:
                case GameState.ClueGenerationDay:
                case GameState.PlayerInvestigateDay:
                case GameState.TransitionToNight:
                case GameState.PhaseGenerationNight:
                case GameState.ClueGenerationNight:
                case GameState.PlayerInvestigateNight:
                    NextState = CurrentState + 1;
                    break;

                case GameState.PlayerInformationReview:
                    NextState = GameState.TransitionToDay;
                    break;

                case GameState.PlayerChoseToStake:
                    NextState = CurrentState + 1;
                    break;

                case GameState.GameSummary:
                    break;
            }

            canCurrentStateBeProgressed = false;
            fStateTimer = 0.0f;
        }

        CurrentSubState = bProgressFromFinish ? SubState.Start : SubState.Finish;
    }

    

    void ShowPhaseShiftAnimation(TOD eFrom, TOD eTo)
    {

    }

    void BeginPhase()
    {

    }

    void ProcessPhaseShiftOnAllCharacters()
    {
        foreach(var c in Service.Population.ActiveCharacters)
        {
            c.OnTimeOfDayPhaseShift();
        }
    }

    #region DEBUG
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!DisplayDebug && !Service.PhaseSolve.bShowDebug && !Service.Population.bShowDebug)
        {
            GUI.Label(new Rect(5, 5, 200, 24), "F1 - Population Debug");
            GUI.Label(new Rect(5, 22, 200, 24), "F2 - PhaseSolver Debug");
            GUI.Label(new Rect(5, 39, 200, 24), "F3 - GameManager Debug");
        }

        if (!DisplayDebug)
        {
            return;
        }

        GUI.Label(new Rect(6, 5, 200, 24), "F5 - Step game forward.");
        GUI.Label(new Rect(6, 22, 300, 24), string.Format("F6 - Pause/Unpause{0}", IsGamePaused ? " (Currently Paused)" : ""));
        GUI.Label(new Rect(410, 5, 400, 24), string.Format("{0} ({1}) : {2} - Day {3}, {4}", 
            CurrentState.ToString(),
            fStateTimer.ToString("0.0"),
            CurrentSubState.ToString(), 
            CurrentDay, 
            CurrentTimeOfDay.ToString()));
    }
#endif
    #endregion
}