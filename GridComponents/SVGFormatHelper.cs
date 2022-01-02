using System.Text;
using SVGElements;
using SVGHelper;

namespace GridComponents;
public enum DownloadFormat
{
    SVG//, BMP
};

public static class SVGFormatHelper
{
    public static byte[] GetImageData(SVG svg, DownloadFormat downloadFormat)
    {
        if (downloadFormat == DownloadFormat.SVG)
        {
            var stringData = svg.RenderString();
            return Encoding.UTF8.GetBytes(stringData);
        }


        throw new ArgumentOutOfRangeException(nameof(downloadFormat), downloadFormat, null);
    }
}
