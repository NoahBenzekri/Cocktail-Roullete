public interface IPlayerBrain
{
    // Interfaces define properties with getters and setters instead of raw fields
    bool IsDrinking { get; set; }

    void TakeAction();
}