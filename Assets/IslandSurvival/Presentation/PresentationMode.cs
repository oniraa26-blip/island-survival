namespace IslandSurvival.Presentation
{
    public enum PresentationMode { ThreeD, TwoPointFiveD }

    public static class PresentationRuntime
    {
        public static PresentationMode Mode { get; set; } = PresentationMode.TwoPointFiveD;
        public static bool PlaceholderArtEnabled { get; set; } = true;
        public static bool IsTwoPointFiveD { get { return Mode == PresentationMode.TwoPointFiveD; } }
    }
}
