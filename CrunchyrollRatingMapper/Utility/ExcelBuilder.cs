using ClosedXML.Excel;
using CrunchyrollRatingMapper.DTOs;

namespace CrunchyrollRatingMapper.Utility;

public class ExcelBuilder
{
    public static void BuildExcel(
    List<AnimeInfo> currentAnimes)
    {
        using var wbook = new XLWorkbook();
        var ws = wbook.AddWorksheet("Sheet1");

        ws.Cell("A1").Value = "CrunchyRoll Id";
        ws.Cell("B1").Value = "Title";
        ws.Cell("C1").Value = "Genres";
        ws.Cell("D1").Value = "Availability";
        ws.Cell("E1").Value = "Average Rating";
        ws.Cell("F1").Value = "Total Ratings";
        ws.Cell("G1").Value = "Mal Id";
        ws.Cell("H1").Value = "Mal Ratings";
        ws.Cell("I1").Value = "Mal Genres";
        foreach (var anime in currentAnimes)
        {
            ws.Cell(i, 1).Value = anime.Id;
            ws.Cell(i, 2).Value = anime.Title;
            ws.Cell(i, 3).Value = anime.Genres;
            ws.Cell(i, 4).Value = anime.Availability;
            ws.Cell(i, 5).Value = anime.AverageRating;
            ws.Cell(i, 6).Value = anime.TotalRatings;
        }
        wbook.SaveAs("Animes.xlsx");
    }
}
