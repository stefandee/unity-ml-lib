namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    public enum ReactionEmote
    {
        None,
        Fastest,
        BestAverage,
    }

    public interface IEmotable
    {
        void React(ReactionEmote emote);
    }
}