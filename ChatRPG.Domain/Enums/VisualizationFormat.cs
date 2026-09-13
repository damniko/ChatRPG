namespace ChatRPG.Domain.Enums;

public enum VisualizationFormat
{
    Svg, Png, Pdf
}

public static class VisualizationFormatExtensions
{
    extension(VisualizationFormat format)
    {
        public string Extension() => format switch
        {
            VisualizationFormat.Pdf => "pdf",
            VisualizationFormat.Png => "png",
            VisualizationFormat.Svg => "svg",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        public string ContentType() => format switch
        {
            VisualizationFormat.Svg => "image/svg+xml",
            VisualizationFormat.Png => "image/png",
            VisualizationFormat.Pdf => "application/pdf",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}
